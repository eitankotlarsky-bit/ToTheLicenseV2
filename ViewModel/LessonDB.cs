using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Globalization;
using Model;

namespace ViewModel
{
    public class LessonDB : BaseDB
    {
        // 5 = ממתין, 6 = בוצע/מאושר, 7 = בוטל
        private const int STATUS_PENDING = 5;
        private const int STATUS_DONE = 6;
        private const int STATUS_CANCELLED = 7;

        private int _bookingStudentId;

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

            try { l.StatusName = reader["StatusName"].ToString(); } catch { }
            try { l.TeacherName = reader["FirstName"].ToString() + " " + reader["LastName"].ToString(); } catch { }
            try { l.StudentName = reader["FirstName"].ToString() + " " + reader["LastName"].ToString(); } catch { }
        }

        // --- הפונקציה עם הדיבאג המלא ---
        public Lesson GetNextLessonForStudent(int studentId)
        {
            System.Diagnostics.Debug.WriteLine($"--- DEBUG START: GetNextLessonFor StudentID {studentId} ---");

            // שליפת כל השיעורים של התלמיד ללא סינון תאריך בשאילתה
            command.CommandText = $@"
                SELECT tblLessons.*, tblStatus.StatusName, tblUsers.FirstName, tblUsers.LastName
                FROM ((tblLessons 
                LEFT JOIN tblStatus ON tblLessons.Status = tblStatus.id)
                INNER JOIN tblStudentLessonReq ON tblLessons.id = tblStudentLessonReq.LessonId)
                INNER JOIN (tblTeacher INNER JOIN tblUsers ON tblTeacher.id = tblUsers.id) 
                ON tblLessons.TeacherID = tblTeacher.id
                WHERE tblStudentLessonReq.StudentId={studentId} 
                ORDER BY tblLessons.StartTime ASC";

            LessonList allLessons = new LessonList(base.Select());

            System.Diagnostics.Debug.WriteLine($"DEBUG: Found {allLessons.Count} total lessons for this student in DB.");

            DateTime now = DateTime.Now;
            System.Diagnostics.Debug.WriteLine($"DEBUG: Current Computer Time: {now}");

            // לולאת בדיקה לכל שיעור
            foreach (var l in allLessons)
            {
                bool isFuture = l.StartTime > now;
                bool isNotCancelled = l.Status != STATUS_CANCELLED;

                System.Diagnostics.Debug.WriteLine($"Checking Lesson ID: {l.Id} | Date: {l.StartTime} | Status: {l.Status}");
                System.Diagnostics.Debug.WriteLine($"   -> Is Future? {isFuture}");
                System.Diagnostics.Debug.WriteLine($"   -> Is Not Cancelled? {isNotCancelled}");

                if (isFuture && isNotCancelled)
                {
                    System.Diagnostics.Debug.WriteLine("   -> MATCH! This lesson is a candidate.");
                }
            }

            // הסינון האמיתי ב-C#
            var nextLesson = allLessons
                .Where(l => l.StartTime > now && l.Status != STATUS_CANCELLED)
                .OrderBy(l => l.StartTime)
                .FirstOrDefault();

            if (nextLesson == null)
                System.Diagnostics.Debug.WriteLine("DEBUG: Result is NULL (No suitable lesson found)");
            else
                System.Diagnostics.Debug.WriteLine($"DEBUG: Selected Lesson ID: {nextLesson.Id}");

            System.Diagnostics.Debug.WriteLine("--- DEBUG END ---");

            return nextLesson;
        }

        // --- שאר הפונקציות (ללא שינוי) ---

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

        public int GetCompletedLessonsCount(int studentId)
        {
            command.CommandText = $@"
                SELECT COUNT(*) 
                FROM tblLessons 
                INNER JOIN tblStudentLessonReq ON tblLessons.id = tblStudentLessonReq.LessonId
                WHERE tblStudentLessonReq.StudentId={studentId} AND tblLessons.Status={STATUS_DONE}";

            try
            {
                if (connection.State != ConnectionState.Open) connection.Open();
                object result = command.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch { return 0; }
            finally { if (connection.State == ConnectionState.Open) connection.Close(); }
        }

        public LessonList GetLessonsByDate(int teacherId, DateTime date)
        {
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
            foreach (Lesson lesson in allLessons)
            {
                if (lesson.StartTime.Date == date.Date) filtered.Add(lesson);
            }
            return filtered;
        }

        public bool IsSlotAvailable(int teacherId, DateTime start, DateTime end)
        {
            command.CommandText = $"SELECT * FROM tblLessons WHERE TeacherID={teacherId}";
            List<BaseEntity> allLessons = base.Select();

            foreach (Lesson l in allLessons)
            {
                if (l.Status != STATUS_CANCELLED && l.StartTime.Date == start.Date)
                {
                    if (start < l.EndTime && end > l.StartTime) return false;
                }
            }
            return true;
        }

        public void BookLesson(Lesson lesson, int studentId)
        {
            _bookingStudentId = studentId;
            if (lesson.Status == 0) lesson.Status = STATUS_PENDING;
            this.Insert(lesson);
        }

        public override void Insert(BaseEntity entity)
        {
            if (entity is Lesson)
            {
                this.inserted.Add(new ChangeEntity(this.CreateInsertSQL, entity));
                this.inserted.Add(new ChangeEntity(this.CreateLinkSQL, entity));
            }
        }

        public override string CreateInsertSQL(BaseEntity entity)
        {
            Lesson l = entity as Lesson;
            int validStatus = l.Status > 0 ? l.Status : STATUS_PENDING;

            string startStr = l.StartTime.ToString("yyyy-MM-dd HH:mm:ss");
            string endStr = l.EndTime.ToString("yyyy-MM-dd HH:mm:ss");
            string safeLocation = l.Location.Replace("'", "''");
            string safeNotes = l.Notes.Replace("'", "''");
            string safePrice = l.Price.ToString(CultureInfo.InvariantCulture);

            return $"INSERT INTO tblLessons ([TeacherID], [Location], [Price], [Status], [Notes], [StartTime], [EndTime], [VehicleType]) " +
                   $"VALUES ({l.TeacherId}, '{safeLocation}', {safePrice}, {validStatus}, '{safeNotes}', #{startStr}#, #{endStr}#, '{l.VehicleType}')";
        }

        private string CreateLinkSQL(BaseEntity entity)
        {
            Lesson l = entity as Lesson;
            int validStatus = l.Status > 0 ? l.Status : STATUS_PENDING;
            return $"INSERT INTO tblStudentLessonReq ([LessonId], [StudentId], [Status]) VALUES ({l.Id}, {_bookingStudentId}, {validStatus})";
        }

        public override void Update(BaseEntity entity)
        {
            if (entity is Lesson) this.updated.Add(new ChangeEntity(this.CreateUpdateSQL, entity));
        }

        public override string CreateUpdateSQL(BaseEntity entity)
        {
            Lesson l = entity as Lesson;
            string safeNotes = l.Notes.Replace("'", "''");
            return $"UPDATE tblLessons SET [Status]={l.Status}, [Notes]='{safeNotes}' WHERE id={l.Id}";
        }

        public override string CreateDeleteSQL(BaseEntity entity) => "";
    }
}