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

        public Lesson GetLastCompletedLesson(int studentId)
        {
            DateTime now = DateTime.Now;
            string nowStr = now.ToString("yyyy-MM-dd HH:mm:ss");

            command.CommandText = $@"
                SELECT TOP 1 tblLessons.*, tblStatus.StatusName, tblUsers.FirstName, tblUsers.LastName
                FROM ((tblLessons 
                LEFT JOIN tblStatus ON tblLessons.Status = tblStatus.id)
                INNER JOIN tblStudentLessonReq ON tblLessons.id = tblStudentLessonReq.LessonId)
                INNER JOIN (tblTeacher INNER JOIN tblUsers ON tblTeacher.id = tblUsers.id) 
                ON tblLessons.TeacherID = tblTeacher.id
                WHERE tblStudentLessonReq.StudentId={studentId} 
                AND tblLessons.Status={STATUS_DONE} 
                AND tblLessons.EndTime < #{nowStr}#
                ORDER BY tblLessons.EndTime DESC";

            LessonList list = new LessonList(base.Select());
            return list.Count > 0 ? list[0] : null;
        }

        public Lesson GetNextLessonForStudent(int studentId)
        {
            DateTime now = DateTime.Now;
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

            return allLessons
                .Where(l => l.StartTime > now && l.Status != STATUS_CANCELLED)
                .OrderBy(l => l.StartTime)
                .FirstOrDefault();
        }

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