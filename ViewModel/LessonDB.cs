using System;
using System.Collections.Generic;
using Model;

namespace ViewModel
{
    public class LessonDB : BaseDB
    {
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

            // נסיון לקרוא שדות מחושבים
            try { l.StatusName = reader["StatusName"].ToString(); } catch { }
            try { l.TeacherName = reader["FirstName"].ToString() + " " + reader["LastName"].ToString(); } catch { }
            try { l.StudentName = reader["FirstName"].ToString() + " " + reader["LastName"].ToString(); } catch { }
        }

        // --- פונקציות חדשות ל-Smart Dashboard ---

        // מביא את השיעור העתידי הקרוב ביותר של התלמיד
        public Lesson GetNextLessonForStudent(int studentId)
        {
            // שולף שיעורים עתידיים, ממוין לפי תאריך עולה, לוקח את הראשון
            // (הלוגיקה הפשוטה: נשלוף הכל ונמיין בקוד כדי לא להסתבך עם SQL של תאריכים באקסס)
            LessonList allLessons = GetLessonsByStudent(studentId);

            Lesson nextLesson = null;
            DateTime now = DateTime.Now;
            DateTime closest = DateTime.MaxValue;

            foreach (Lesson l in allLessons)
            {
                // אם השיעור בעתיד וגם קרוב יותר ממה שמצאנו עד עכשיו
                if (l.StartTime > now && l.StartTime < closest)
                {
                    closest = l.StartTime;
                    nextLesson = l;
                }
            }
            return nextLesson;
        }

        // ספירת כמה שיעורים התלמיד כבר ביצע (סטטוס "בוצע" נניח שזה 2 או 3)
        public int GetCompletedLessonsCount(int studentId)
        {
            LessonList allLessons = GetLessonsByStudent(studentId);
            int count = 0;
            foreach (Lesson l in allLessons)
            {
                // נניח שסטטוסים 2 ו-3 נחשבים "בוצע" או "עבר"
                // תתאים את המספרים לטבלת הסטטוסים שלך
                if (l.Status == 2 || l.Status == 3)
                    count++;
            }
            return count;
        }

        // --- סוף פונקציות חדשות ---

        public LessonList GetLessonsByTeacher(int teacherId)
        {
            command.CommandText = $@"
                SELECT tblLessons.*, tblStatus.StatusName, tblUsers.FirstName, tblUsers.LastName
                FROM ((tblLessons 
                LEFT JOIN tblStatus ON tblLessons.Status = tblStatus.id)
                LEFT JOIN tblStudentLessonReq ON tblLessons.id = tblStudentLessonReq.LessonId)
                LEFT JOIN tblUsers ON tblStudentLessonReq.StudentId = tblUsers.id
                WHERE tblLessons.TeacherID={teacherId}";

            return new LessonList(base.Select());
        }

        public LessonList GetLessonsByStudent(int studentId)
        {
            command.CommandText = $@"
                SELECT tblLessons.*, tblStatus.StatusName, tblUsers.FirstName, tblUsers.LastName
                FROM ((tblLessons 
                LEFT JOIN tblStatus ON tblLessons.Status = tblStatus.id)
                LEFT JOIN tblStudentLessonReq ON tblLessons.id = tblStudentLessonReq.LessonId)
                INNER JOIN (tblTeacher INNER JOIN tblUsers ON tblTeacher.id = tblUsers.id) 
                ON tblLessons.TeacherID = tblTeacher.id
                WHERE tblStudentLessonReq.StudentId={studentId}";

            return new LessonList(base.Select());
        }

        // ... שאר הפונקציות (Insert, Update, Delete) נשארות אותו דבר ...
        public override string CreateInsertSQL(BaseEntity entity) { /* הקוד הקיים */ return ""; }
        public override string CreateUpdateSQL(BaseEntity entity) { /* הקוד הקיים */ return ""; }
        public override string CreateDeleteSQL(BaseEntity entity) { /* הקוד הקיים */ return ""; }
    }
}