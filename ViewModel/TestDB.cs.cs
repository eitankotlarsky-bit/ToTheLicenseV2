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
        }

        public TestEntity GetLastTest(int studentId, int testType)
        {
            // שליפת הטסט האחרון מסוג מסוים (כדי לדעת אם עבר או נכשל)
            command.CommandText = $"SELECT TOP 1 * FROM tblTests WHERE StudentId={studentId} AND TestType={testType} ORDER BY TestDate DESC, id DESC";
            var list = base.Select();
            return list.Count > 0 ? (TestEntity)list[0] : null;
        }

        public override string CreateInsertSQL(BaseEntity entity)
        {
            TestEntity t = entity as TestEntity;
            string dateStr = t.TestDate.ToString("yyyy-MM-dd HH:mm:ss");
            string safeNotes = t.Notes != null ? t.Notes.Replace("'", "''") : "";

            return $"INSERT INTO tblTests (StudentId, TeacherId, TestType, Status, TestDate, Notes) " +
                   $"VALUES ({t.StudentId}, {t.TeacherId}, {t.TestType}, {t.Status}, #{dateStr}#, '{safeNotes}')";
        }

        public override string CreateUpdateSQL(BaseEntity entity) => ""; // לבינתיים
        public override string CreateDeleteSQL(BaseEntity entity) => "";
    }
}