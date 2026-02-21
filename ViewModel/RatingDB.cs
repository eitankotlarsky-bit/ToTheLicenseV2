using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Globalization;

namespace ViewModel
{
    public class RatingDB : BaseDB
    {
        protected override BaseEntity NewEntity() => new TeacherRating();

        protected override void CreateModel(BaseEntity entity)
        {
            TeacherRating r = entity as TeacherRating;
            r.Id = (int)reader["id"];
            r.TeacherId = (int)reader["TeacherId"];
            r.StudentId = (int)reader["StudentId"];
            r.RatingValue = (int)reader["RatingValue"];
            r.Comment = reader["Comment"].ToString();
            r.RatingDate = Convert.ToDateTime(reader["RatingDate"]);
        }

        public void AddRating(TeacherRating rating)
        {
            this.Insert(rating);
            this.SaveChanges();
            UpdateTeacherAverage(rating.TeacherId);
        }

        private void UpdateTeacherAverage(int teacherId)
        {
            string sql = $"SELECT * FROM tblRatings WHERE TeacherId={teacherId}";
            this.command.CommandText = sql;
            List<BaseEntity> ratings = base.Select();

            if (ratings.Count > 0)
            {
                double avg = ratings.Cast<TeacherRating>().Average(r => r.RatingValue);
                // שימוש ב-CultureInfo.InvariantCulture כדי שהנקודה העשרונית תהיה נקודה ולא פסיק
                string updateSql = $"UPDATE tblTeacher SET Rating={avg.ToString(CultureInfo.InvariantCulture)} WHERE id={teacherId}";

                try
                {
                    if (connection.State != ConnectionState.Open) connection.Open();
                    command.CommandText = updateSql;
                    command.ExecuteNonQuery();
                }
                finally
                {
                    if (connection.State == ConnectionState.Open) connection.Close();
                }
            }
        }

        // =================================================================
        // הנה הפונקציה שהייתה חסרה לך - תעתיק את זה בבקשה
        // =================================================================
        public bool HasRatedRecently(int teacherId, int studentId)
        {
            // נבדוק אם קיים דירוג מאותו תלמיד לאותו מורה מהיום
            // (כדי למנוע הצפת דירוגים על אותו שיעור)
            string today = DateTime.Today.ToString("yyyy-MM-dd");
            string sql = $"SELECT COUNT(*) FROM tblRatings WHERE TeacherId={teacherId} AND StudentId={studentId} AND RatingDate >= #{today}#";

            this.command.CommandText = sql;
            try
            {
                if (connection.State != ConnectionState.Open) connection.Open();
                int count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
            catch { return false; }
            finally
            {
                if (connection.State == ConnectionState.Open) connection.Close();
            }
        }
        public bool HasRated(int teacherId, int studentId)
        {
            string sql = $"SELECT COUNT(*) FROM tblRatings WHERE TeacherId={teacherId} AND StudentId={studentId}";

            this.command.CommandText = sql;
            try
            {
                if (connection.State != System.Data.ConnectionState.Open) connection.Open();
                int count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
            catch { return false; }
            finally
            {
                if (connection.State == System.Data.ConnectionState.Open) connection.Close();
            }
        }
        // =================================================================

        public override string CreateInsertSQL(BaseEntity entity)
        {
            TeacherRating r = entity as TeacherRating;
            string dateStr = r.RatingDate.ToString("yyyy-MM-dd HH:mm:ss");
            string safeComment = r.Comment != null ? r.Comment.Replace("'", "''") : "";

            return $"INSERT INTO tblRatings (TeacherId, StudentId, RatingValue, Comment, RatingDate) " +
                   $"VALUES ({r.TeacherId}, {r.StudentId}, {r.RatingValue}, '{safeComment}', #{dateStr}#)";
        }

        public override string CreateUpdateSQL(BaseEntity entity) => "";
        public override string CreateDeleteSQL(BaseEntity entity) => "";
    }
}