using Model;
using System;

namespace ViewModel
{
    public class StudentDB : UserDB
    {
        protected override BaseEntity NewEntity() => new Student();

        protected override void CreateModel(BaseEntity entity)
        {
            base.CreateModel(entity);
            Student student = entity as Student;
            try { student.LicenseType = this.reader["LicenseType"].ToString(); } catch { }
            try { if (this.reader["Lessons"] != DBNull.Value) student.LessonsCount = int.Parse(this.reader["Lessons"].ToString()); } catch { }
        }

        public Student Login(string username, string password)
        {
            this.command.CommandText = $"SELECT tblUsers.*, tblStudents.LicenseType, tblStudents.Lessons FROM tblUsers INNER JOIN tblStudents ON tblUsers.id = tblStudents.id WHERE (tblUsers.UserName = '{username}') AND (tblUsers.[Password] = '{password}')";
            StudentList list = new StudentList(base.Select());
            return list.Count > 0 ? list[0] : null;
        }

        public StudentList GetStudentsByTeacher(int teacherId)
        {
            string sql = $@"SELECT DISTINCT tblUsers.*, tblStudents.LicenseType, tblStudents.Lessons
                            FROM (((tblUsers 
                            INNER JOIN tblStudents ON tblUsers.id = tblStudents.id)
                            INNER JOIN tblStudentLessonReq ON tblStudents.id = tblStudentLessonReq.StudentId)
                            INNER JOIN tblLessons ON tblStudentLessonReq.LessonId = tblLessons.id)
                            WHERE tblLessons.TeacherID = {teacherId}";
            this.command.CommandText = sql;
            return new StudentList(base.Select());
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

        public override void Update(BaseEntity entity)
        {
            Student student = entity as Student;
            if (student != null)
            {
                this.updated.Add(new ChangeEntity(base.CreateUpdateSQL, entity));
                this.updated.Add(new ChangeEntity(this.CreateUpdateSQL, entity));
            }
        }

        public override string CreateUpdateSQL(BaseEntity entity)
        {
            Student student = entity as Student;
            return $"UPDATE tblStudents SET LicenseType='{student.LicenseType}' WHERE id={student.Id}";
        }

        public override string CreateDeleteSQL(BaseEntity entity) => throw new NotImplementedException();
    }
}