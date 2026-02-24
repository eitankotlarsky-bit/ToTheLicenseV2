using Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ViewModel
{
    public class TestDB : BaseDB
    {
        protected override BaseEntity NewEntity() => new TestEntity();

        protected override void CreateModel(BaseEntity entity)
        {
            TestEntity t = entity as TestEntity;
            t.Id = (int)reader["id"];
            t.StudentId = (int)reader["StudentId"];
            t.TeacherId = (int)reader["TeacherId"];
            t.TestType = (int)reader["TestType"];
            t.Status = (int)reader["Status"];
            try { t.TestDate = Convert.ToDateTime(reader["TestDate"]); } catch { }
            try { t.Notes = reader["Notes"].ToString(); } catch { }

            // קריאת שם התלמיד בעזרת השאילתה המשותפת (JOIN)
            try { t.StudentName = reader["FirstName"].ToString() + " " + reader["LastName"].ToString(); } catch { }
        }

        public TestEntity GetLastTest(int studentId, int testType)
        {
            command.CommandText = $"SELECT TOP 1 * FROM tblTests WHERE StudentId={studentId} AND TestType={testType} ORDER BY TestDate DESC, id DESC";
            var list = base.Select();
            return list.Count > 0 ? (TestEntity)list[0] : null;
        }

        // שליפת כל הטסטים (פנימיים וחיצוניים) שקשורים למורה מסוים
        public TestList GetTestsByTeacher(int teacherId)
        {
            command.CommandText = $@"
                SELECT tblTests.*, tblUsers.FirstName, tblUsers.LastName 
                FROM tblTests 
                INNER JOIN tblUsers ON tblTests.StudentId = tblUsers.id 
                WHERE tblTests.TeacherId = {teacherId} 
                ORDER BY tblTests.TestDate DESC, tblTests.id DESC";

            return new TestList(base.Select());
        }

        public override void Insert(BaseEntity entity)
        {
            if (entity is TestEntity) this.inserted.Add(new ChangeEntity(this.CreateInsertSQL, entity));
        }

        public override string CreateInsertSQL(BaseEntity entity)
        {
            TestEntity t = entity as TestEntity;
            string dateStr = t.TestDate.ToString("yyyy-MM-dd HH:mm:ss");
            string safeNotes = t.Notes != null ? t.Notes.Replace("'", "''") : "";

            return $"INSERT INTO tblTests (StudentId, TeacherId, TestType, Status, TestDate, Notes) " +
                   $"VALUES ({t.StudentId}, {t.TeacherId}, {t.TestType}, {t.Status}, #{dateStr}#, '{safeNotes}')";
        }

        public override void Update(BaseEntity entity)
        {
            if (entity is TestEntity) this.updated.Add(new ChangeEntity(this.CreateUpdateSQL, entity));
        }

        public override string CreateUpdateSQL(BaseEntity entity)
        {
            TestEntity t = entity as TestEntity;
            string dateStr = t.TestDate.ToString("yyyy-MM-dd HH:mm:ss");
            string safeNotes = t.Notes != null ? t.Notes.Replace("'", "''") : "";

            return $"UPDATE tblTests SET Status={t.Status}, TestDate=#{dateStr}#, Notes='{safeNotes}' WHERE id={t.Id}";
        }

        public override string CreateDeleteSQL(BaseEntity entity) => "";
    }
}