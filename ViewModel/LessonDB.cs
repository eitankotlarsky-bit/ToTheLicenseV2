using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb; // חשוב ל-OleDbCommand
using System.Linq; // חשוב ל-LINQ
using Model;

namespace ViewModel
{
    public class LessonDB : BaseDB
    {
        // משתנה עזר לשמירת ה-ID של התלמיד שמזמין (לשימוש בתוך ה-Insert)
        private int _bookingStudentId;
        
        // Cache ל-Status IDs (כדי לא לשאול כל פעם)
        private static int? _pendingStatusId = null;
        private static int? _completedStatusId = null;
        private static int? _cancelledStatusId = null;

        protected override BaseEntity NewEntity() => new Lesson();

        protected override void CreateModel(BaseEntity entity)
        {
            Lesson l = entity as Lesson;
            l.Id = (int)reader["id"];
            l.TeacherId = (int)reader["TeacherID"];
            l.Location = reader["Location"].ToString();
            l.Price = Convert.ToDouble(reader["Price"]);
            l.Status = (int)reader["Status"];
            l.Notes = reader["Notes"].ToString();
            l.StartTime = Convert.ToDateTime(reader["StartTime"]);
            l.EndTime = Convert.ToDateTime(reader["EndTime"]);
            l.VehicleType = reader["VehicleType"].ToString();

            // שדות לתצוגה (מגיעים מה-JOIN)
            try { l.StatusName = reader["StatusName"].ToString(); } catch { }
            try { l.TeacherName = reader["FirstName"].ToString() + " " + reader["LastName"].ToString(); } catch { }
            try { l.StudentName = reader["FirstName"].ToString() + " " + reader["LastName"].ToString(); } catch { }
        }

        // --- קריאת נתונים ---
        public LessonList GetLessonsByTeacher(int teacherId)
        {
            command.CommandText = $@"
                SELECT tblLessons.*, tblStatus.StatusName, tblUsers.FirstName, tblUsers.LastName
                FROM ((tblLessons 
                LEFT JOIN tblStatus ON tblLessons.Status = tblStatus.id)
                LEFT JOIN tblStudentLessonReq ON tblLessons.id = tblStudentLessonReq.LessonId)
                LEFT JOIN tblUsers ON tblStudentLessonReq.StudentId = tblUsers.id
                WHERE tblLessons.TeacherID={teacherId} ORDER BY tblLessons.StartTime DESC";
            return new LessonList(base.Select());
        }

        public LessonList GetLessonsByStudent(int studentId)
        {
            command.CommandText = $@"
                SELECT tblLessons.*, tblStatus.StatusName, tblUsers.FirstName, tblUsers.LastName
                FROM ((tblLessons 
                LEFT JOIN tblStatus ON tblLessons.Status = tblStatus.id)
                INNER JOIN tblStudentLessonReq ON tblLessons.id = tblStudentLessonReq.LessonId)
                INNER JOIN (tblTeacher INNER JOIN tblUsers ON tblTeacher.id = tblUsers.id) 
                ON tblLessons.TeacherID = tblTeacher.id
                WHERE tblStudentLessonReq.StudentId={studentId} ORDER BY tblLessons.StartTime DESC";
            return new LessonList(base.Select());
        }

        // --- פונקציות נוספות לתלמידים ---
        public int GetCompletedLessonsCount(int studentId)
        {
            // שליפת ה-ID של "בוצע" מהטבלה (דינמי)
            int completedId = GetCompletedStatusId();
            command.CommandText = $@"
                SELECT COUNT(*) 
                FROM tblLessons 
                INNER JOIN tblStudentLessonReq ON tblLessons.id = tblStudentLessonReq.LessonId
                WHERE tblStudentLessonReq.StudentId={studentId} AND tblLessons.Status={completedId}";
            
            try
            {
                if (connection.State != ConnectionState.Open) connection.Open();
                object result = command.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error in GetCompletedLessonsCount: " + ex.Message);
                return 0;
            }
            finally
            {
                if (connection.State == ConnectionState.Open) connection.Close();
            }
        }

        public Lesson GetNextLessonForStudent(int studentId)
        {
            // שיעור הבא = השיעור הקרוב ביותר שעדיין לא עבר (StartTime > עכשיו) ולא מבוטל
            int cancelledId = GetCancelledStatusId();
            command.CommandText = $@"
                SELECT TOP 1 tblLessons.*, tblStatus.StatusName, tblUsers.FirstName, tblUsers.LastName
                FROM ((tblLessons 
                LEFT JOIN tblStatus ON tblLessons.Status = tblStatus.id)
                INNER JOIN tblStudentLessonReq ON tblLessons.id = tblStudentLessonReq.LessonId)
                INNER JOIN (tblTeacher INNER JOIN tblUsers ON tblTeacher.id = tblUsers.id) 
                ON tblLessons.TeacherID = tblTeacher.id
                WHERE tblStudentLessonReq.StudentId={studentId} 
                AND tblLessons.StartTime > #{DateTime.Now}#
                AND tblLessons.Status != {cancelledId}
                ORDER BY tblLessons.StartTime ASC";
            
            LessonList list = new LessonList(base.Select());
            return list.Count > 0 ? list[0] : null;
        }

        public LessonList GetLessonsByDate(int teacherId, DateTime date)
        {
            // שליפת כל השיעורים של המורה וסינון לפי תאריך בזיכרון (כמו IsSlotAvailable)
            command.CommandText = $@"
                SELECT tblLessons.*, tblStatus.StatusName, tblUsers.FirstName, tblUsers.LastName
                FROM ((tblLessons 
                LEFT JOIN tblStatus ON tblLessons.Status = tblStatus.id)
                LEFT JOIN tblStudentLessonReq ON tblLessons.id = tblStudentLessonReq.LessonId)
                LEFT JOIN tblUsers ON tblStudentLessonReq.StudentId = tblUsers.id
                WHERE tblLessons.TeacherID={teacherId}
                ORDER BY tblLessons.StartTime ASC";
            
            LessonList allLessons = new LessonList(base.Select());
            LessonList filtered = new LessonList();
            
            // סינון לפי תאריך בזיכרון
            foreach (Lesson lesson in allLessons)
            {
                if (lesson.StartTime.Date == date.Date)
                {
                    filtered.Add(lesson);
                }
            }
            
            return filtered;
        }

        // --- בדיקת זמינות (מונע כפילויות) ---
        public bool IsSlotAvailable(int teacherId, DateTime start, DateTime end)
        {
            // שולף את כל השיעורים של המורה ליום הספציפי
            // אנחנו עושים את הסינון בזיכרון כדי למנוע בעיות תאריכים ב-SQL של אקסס
            command.CommandText = $"SELECT * FROM tblLessons WHERE TeacherID={teacherId}";
            List<BaseEntity> allLessons = base.Select();

            int cancelledId = GetCancelledStatusId();
            foreach (Lesson l in allLessons)
            {
                // אם השיעור לא מבוטל וגם הוא באותו יום
                if (l.Status != cancelledId && l.StartTime.Date == start.Date)
                {
                    // בדיקת חפיפה בזמנים
                    if (start < l.EndTime && end > l.StartTime)
                        return false; // תפוס
                }
            }
            return true; // פנוי
        }

        // --- הזמנת שיעור (Transaction Logic) ---
        public void BookLesson(Lesson lesson, int studentId)
        {
            _bookingStudentId = studentId;
            
            // שליפת Status ID לפני הטרנזקציה (כדי למנוע בעיות עם טרנזקציה פעילה)
            if (lesson.Status == 0) // אם לא הוגדר Status, נשלוף את "ממתין לאישור"
            {
                lesson.Status = GetPendingStatusId();
                
                // אם לא מצאנו Status ID, נזרוק שגיאה ברורה
                if (lesson.Status == 0)
                {
                    throw new Exception("לא נמצא Status 'ממתין לאישור' בטבלת tblStatus. אנא וודא שהטבלה מכילה את הרשומה הנדרשת.");
                }
            }
            
            // קורא ל-Insert הרגיל שמוסיף את שתי הפעולות לרשימה
            this.Insert(lesson);
        }

        public override void Insert(BaseEntity entity)
        {
            if (entity is Lesson)
            {
                // 1. הוספת השיעור לטבלת השיעורים
                this.inserted.Add(new ChangeEntity(this.CreateInsertSQL, entity));

                // 2. הוספת הקישור לתלמיד (ישתמש ב-ID החדש שיוצר בשלב 1)
                this.inserted.Add(new ChangeEntity(this.CreateLinkSQL, entity));
            }
        }

        // פונקציה עזר לשליפת Status ID לפי שם (ללא סגירת חיבור - משתמש בחיבור קיים)
        private int GetStatusIdByName(string statusName)
        {
            bool wasOpen = connection.State == ConnectionState.Open;
            OleDbCommand tempCmd = null;
            
            try
            {
                // אם החיבור לא פתוח, נפתח אותו
                if (!wasOpen) 
                {
                    connection.Open();
                }
                
                // יצירת command חדש שלא קשור לטרנזקציה (אם יש אחת)
                // שימוש ב-LIKE במקום = כדי להיות יותר גמיש
                tempCmd = new OleDbCommand($"SELECT id FROM tblStatus WHERE StatusName LIKE '%{statusName}%'", connection);
                object result = tempCmd.ExecuteScalar();
                
                if (result != null)
                {
                    int id = Convert.ToInt32(result);
                    System.Diagnostics.Debug.WriteLine($"Found Status ID: {id} for '{statusName}'");
                    return id;
                }
                else
                {
                    // נסה לשלוף את כל ה-Statuses כדי לראות מה יש
                    System.Diagnostics.Debug.WriteLine($"Status '{statusName}' not found. Checking available statuses...");
                    tempCmd.CommandText = "SELECT id, StatusName FROM tblStatus";
                    using (var reader = tempCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            System.Diagnostics.Debug.WriteLine($"Available Status: id={reader[0]}, Name='{reader[1]}'");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error getting Status ID: " + ex.Message);
            }
            finally
            {
                if (tempCmd != null) tempCmd.Dispose();
                // סוגרים רק אם אנחנו פתחנו (לא היה פתוח לפני)
                if (!wasOpen && connection.State == ConnectionState.Open) 
                {
                    connection.Close();
                }
            }
            return 0; // fallback
        }
        
        // פונקציה לשליפת Status ID של "ממתין לאישור" (עם cache)
        private int GetPendingStatusId()
        {
            if (_pendingStatusId == null)
            {
                // נסה כמה וריאציות של השם
                _pendingStatusId = GetStatusIdByName("ממתין לאישור");
                if (_pendingStatusId == 0)
                    _pendingStatusId = GetStatusIdByName("ממתין");
                if (_pendingStatusId == 0)
                    _pendingStatusId = GetStatusIdByName("Pending");
                if (_pendingStatusId == 0)
                    _pendingStatusId = GetStatusIdByName("מאושר");
            }
            return _pendingStatusId.Value;
        }
        
        private int GetCompletedStatusId()
        {
            if (_completedStatusId == null)
            {
                _completedStatusId = GetStatusIdByName("בוצע");
                if (_completedStatusId == 0)
                    _completedStatusId = GetStatusIdByName("Completed");
            }
            return _completedStatusId.Value;
        }
        
        private int GetCancelledStatusId()
        {
            if (_cancelledStatusId == null)
            {
                _cancelledStatusId = GetStatusIdByName("בוטל");
                if (_cancelledStatusId == 0)
                    _cancelledStatusId = GetStatusIdByName("Cancelled");
            }
            return _cancelledStatusId.Value;
        }

        public override string CreateInsertSQL(BaseEntity entity)
        {
            Lesson l = entity as Lesson;
            // ה-Status כבר נקבע ב-BookLesson() לפני הטרנזקציה
            // אם עדיין 0, נשתמש ב-5 כגיבוי (אבל זה לא אמור לקרות)
            int statusId = l.Status > 0 ? l.Status : 5;
            return $"INSERT INTO tblLessons (TeacherID, Location, Price, Status, Notes, StartTime, EndTime, VehicleType) VALUES ({l.TeacherId}, '{l.Location}', {l.Price}, {statusId}, '{l.Notes}', #{l.StartTime}#, #{l.EndTime}#, '{l.VehicleType}')";
        }

        private string CreateLinkSQL(BaseEntity entity)
        {
            Lesson l = entity as Lesson;
            // בשלב זה l.Id כבר עודכן ע"י BaseDB
            return $"INSERT INTO tblStudentLessonReq (LessonId, StudentId, Status) VALUES ({l.Id}, {_bookingStudentId}, 0)";
        }

        // --- עדכון סטטוס ---
        public override void Update(BaseEntity entity)
        {
            if (entity is Lesson) this.updated.Add(new ChangeEntity(this.CreateUpdateSQL, entity));
        }

        public override string CreateUpdateSQL(BaseEntity entity)
        {
            Lesson l = entity as Lesson;
            return $"UPDATE tblLessons SET Status={l.Status}, Notes='{l.Notes}' WHERE id={l.Id}";
        }

        public override string CreateDeleteSQL(BaseEntity entity) => "";
    }
}