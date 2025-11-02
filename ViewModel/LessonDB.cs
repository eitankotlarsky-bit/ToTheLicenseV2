using System;
using System.Data;
using System.Data.OleDb;
using Model;

namespace ViewModel
{
    public class LessonDB : BaseDB
    {
        protected override BaseEntity NewEntity() => new Lesson();

        protected override void CreateModel(BaseEntity entity)
        {
            Lesson l = entity as Lesson;

            l.Id = Convert.ToInt32(this.reader["id"]);
            l.StudentID = Convert.ToInt32(this.reader["StudentID"]);
            l.TeacherID = Convert.ToInt32(this.reader["TeacherID"]);
            l.Location = this.reader["Location"].ToString();
            l.Price = (decimal)Convert.ToDouble(this.reader["Price"]);
            l.Status = this.reader["Status"].ToString();
            l.Notes = this.reader["Notes"].ToString();
            l.StartTime = Convert.ToDateTime(this.reader["StartTime"]);
            l.EndTime = Convert.ToDateTime(this.reader["EndTime"]);
            l.VehicleType = this.reader["VehicleType"].ToString();
        }


        // --- SELECTs ---
        public LessonList SelectAll()
        {
            this.command.Parameters.Clear();
            this.command.CommandText = @"SELECT * FROM tblLessons ORDER BY StartTime DESC";
            return new LessonList(base.Select());
        }

        public LessonList SelectForStudent(int studentId)
        {
            this.command.Parameters.Clear();
            this.command.CommandText = @"SELECT * FROM tblLessons WHERE StudentID = ? ORDER BY StartTime DESC";
            this.command.Parameters.AddWithValue("?", studentId);
            return new LessonList(base.Select());
        }

        public LessonList SelectForTeacher(int teacherId)
        {
            this.command.Parameters.Clear();
            this.command.CommandText = @"SELECT * FROM tblLessons WHERE TeacherID = ? ORDER BY StartTime DESC";
            this.command.Parameters.AddWithValue("?", teacherId);
            return new LessonList(base.Select());
        }

        // --- CRUD ישירים (כי SaveChanges שלך כרגע מושבת) ---

        public int Insert(Lesson l)
        {
            using (var cmd = new OleDbCommand(
                @"INSERT INTO tblLessons (StudentID, TeacherID, StartTime, EndTime, Location, Price, Status, Notes)
                  VALUES (?,?,?,?,?,?,?,?)", this.connection))
            {
                cmd.Parameters.AddWithValue("?", l.StudentID);
                cmd.Parameters.AddWithValue("?", l.TeacherID);
                cmd.Parameters.AddWithValue("?", l.StartTime);
                cmd.Parameters.AddWithValue("?", l.EndTime);
                cmd.Parameters.AddWithValue("?", (object)l.Location ?? DBNull.Value);
                cmd.Parameters.AddWithValue("?", l.Price);
                cmd.Parameters.AddWithValue("?", l.Status ?? "Scheduled");
                cmd.Parameters.AddWithValue("?", (object)l.Notes ?? DBNull.Value);

                try
                {
                    if (this.connection.State != ConnectionState.Open)
                        this.connection.Open();

                    int rows = cmd.ExecuteNonQuery();

                    // קבלת המזהה שנוצר (Access)
                    cmd.CommandText = "SELECT @@IDENTITY";
                    cmd.Parameters.Clear();
                    l.Id = Convert.ToInt32(cmd.ExecuteScalar());

                    return rows;
                }
                finally
                {
                    if (this.connection.State == ConnectionState.Open)
                        this.connection.Close();
                }
            }
        }

        public int Update(Lesson l)
        {
            using (var cmd = new OleDbCommand(
                @"UPDATE tblLessons
                  SET StudentID = ?, TeacherID = ?, StartTime = ?, EndTime = ?, Location = ?, Price = ?, Status = ?, Notes = ?
                  WHERE ID = ?", this.connection))
            {
                cmd.Parameters.AddWithValue("?", l.StudentID);
                cmd.Parameters.AddWithValue("?", l.TeacherID);
                cmd.Parameters.AddWithValue("?", l.StartTime);
                cmd.Parameters.AddWithValue("?", l.EndTime);
                cmd.Parameters.AddWithValue("?", (object)l.Location ?? DBNull.Value);
                cmd.Parameters.AddWithValue("?", l.Price);
                cmd.Parameters.AddWithValue("?", l.Status ?? "Scheduled");
                cmd.Parameters.AddWithValue("?", (object)l.Notes ?? DBNull.Value);
                cmd.Parameters.AddWithValue("?", l.Id);

                try
                {
                    if (this.connection.State != ConnectionState.Open)
                        this.connection.Open();

                    return cmd.ExecuteNonQuery();
                }
                finally
                {
                    if (this.connection.State == ConnectionState.Open)
                        this.connection.Close();
                }
            }
        }

        public int Delete(int id)
        {
            using (var cmd = new OleDbCommand(
                @"DELETE FROM tblLessons WHERE ID = ?", this.connection))
            {
                cmd.Parameters.AddWithValue("?", id);

                try
                {
                    if (this.connection.State != ConnectionState.Open)
                        this.connection.Open();

                    return cmd.ExecuteNonQuery();
                }
                finally
                {
                    if (this.connection.State == ConnectionState.Open)
                        this.connection.Close();
                }
            }
        }

        // בדיקה פשוטה לחפיפה (אופציונלי – תוספת טובה):
        public bool HasOverlapForTeacher(int teacherId, DateTime start, DateTime end)
        {
            using (var cmd = new OleDbCommand(
                @"SELECT COUNT(*) FROM tblLessons
                  WHERE TeacherID = ?
                    AND NOT (EndTime <= ? OR StartTime >= ?)", this.connection))
            {
                cmd.Parameters.AddWithValue("?", teacherId);
                cmd.Parameters.AddWithValue("?", start);
                cmd.Parameters.AddWithValue("?", end);

                try
                {
                    if (this.connection.State != ConnectionState.Open)
                        this.connection.Open();

                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
                finally
                {
                    if (this.connection.State == ConnectionState.Open)
                        this.connection.Close();
                }
            }
        }

        public override string CreateInsertSQL(BaseEntity entity)
        {
            throw new NotImplementedException();
        }

        public override string CreateUpdateSQL(BaseEntity entity)
        {
            throw new NotImplementedException();
        }

        public override string CreateDeleteSQL(BaseEntity entity)
        {
            throw new NotImplementedException();
        }
    }
}
