using Model;
using System;

namespace ViewModel
{
    public class StudentDB : UserDB
    {
        protected override BaseEntity NewEntity()
        {
            return new Student();
        }

        protected override void CreateModel(BaseEntity entity)
        {
            base.CreateModel(entity);

            Student student = entity as Student;

            try { student.LicenseType = this.reader["LicenseType"].ToString(); } catch { }

            try
            {
                if (this.reader["Lessons"] != DBNull.Value)
                    student.LessonsCount = int.Parse(this.reader["Lessons"].ToString());
                else
                    student.LessonsCount = 0;
            }
            catch { student.LessonsCount = 0; }
        }

        public Student Login(string username, string password)
        {
            // שאילתה מתוקנת: בוחרים ספציפית LicenseType ו-Lessons כדי לא ליצור כפילות id
            this.command.CommandText = $"SELECT tblUsers.*, tblStudents.LicenseType, tblStudents.Lessons FROM tblUsers INNER JOIN tblStudents ON tblUsers.id = tblStudents.id WHERE (tblUsers.UserName = '{username}') AND (tblUsers.[Password] = '{password}')";

            StudentList students = new StudentList(base.Select());

            if (students.Count > 0)
                return students[0];

            return null;
        }

        public override void Insert(BaseEntity entity)
        {
            Student student = entity as Student;
            if (student != null)
            {
                this.inserted.Add(new ChangeEntity(base.CreateInsertSQL, entity));
                this.inserted.Add(new ChangeEntity(this.CreateInsertSQL, entity));
            }
        }

        public override string CreateInsertSQL(BaseEntity entity)
        {
            Student student = entity as Student;
            return $"INSERT INTO tblStudents (id, LicenseType, Lessons) VALUES ({student.Id}, '{student.LicenseType}', 0)";
        }

        public override string CreateUpdateSQL(BaseEntity entity) => throw new NotImplementedException();
        public override string CreateDeleteSQL(BaseEntity entity) => throw new NotImplementedException();
        public StudentList GetStudentsByTeacher(int teacherId)
        {
            // שאילתה שמביאה תלמידים שיש להם שיעורים או בקשות שיעור עם המורה הזה
            // אנחנו עושים JOIN בין המשתמשים, התלמידים, והבקשות לשיעורים שמקושרות לשיעורים של המורה

            // הערה: זו שאילתה חכמה שמונעת כפילויות (DISTINCT)
            string sql = $@"
        SELECT DISTINCT tblUsers.*, tblStudents.LicenseType, tblStudents.Lessons
        FROM (((tblUsers 
        INNER JOIN tblStudents ON tblUsers.id = tblStudents.id)
        INNER JOIN tblStudentLessonReq ON tblStudents.id = tblStudentLessonReq.StudentId)
        INNER JOIN tblLessons ON tblStudentLessonReq.LessonId = tblLessons.id)
        WHERE tblLessons.TeacherID = {teacherId}";

            this.command.CommandText = sql;

            // אם השאילתה מורכבת מדי לאקסס שלך או מחזירה שגיאה, 
            // אפשר זמנית להשתמש ב-SelectAll() כדי לראות שהמסך עובד:
            // this.command.CommandText = "SELECT tblUsers.*, tblStudents.* FROM tblUsers INNER JOIN tblStudents ON tblUsers.id = tblStudents.id";

            return new StudentList(base.Select());
        }
    }
}