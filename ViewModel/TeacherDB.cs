using Model;
using System;
using System.Globalization;

namespace ViewModel
{
    public class TeacherDB : UserDB
    {
        protected override BaseEntity NewEntity() => new Teacher();

        // פונקציית עזר חכמה שממירה כל סוג למספר עשרוני
        private double SafeConvertToDouble(object value)
        {
            if (value == null || value == DBNull.Value) return 0;

            // אם זה כבר מספר, פשוט נחזיר אותו
            if (value is double d) return d;
            if (value is float f) return (double)f;
            if (value is int i) return (double)i;
            if (value is decimal dec) return (double)dec;

            // אם זה טקסט או משהו מוזר אחר, ננסה להמיר בכוח
            string s = value.ToString();
            // מנסה גם נקודה וגם פסיק
            if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out double res1)) return res1;
            if (double.TryParse(s, out double res2)) return res2; // ברירת מחדל של המחשב

            return 0; // נכשלנו
        }

        protected override void CreateModel(BaseEntity entity)
        {
            base.CreateModel(entity);
            Teacher teacher = entity as Teacher;

            try { if (reader["Location"] != DBNull.Value) teacher.Location = reader["Location"].ToString(); } catch { }
            try { if (reader["VehicleType"] != DBNull.Value) teacher.VehicleType = reader["VehicleType"].ToString(); } catch { }

            // שימוש בפונקציה הבטוחה למחיר
            try { teacher.LessonPrice = SafeConvertToDouble(reader["LessonPrice"]); } catch { }

            // שימוש בפונקציה הבטוחה לדירוג
            try { teacher.Rating = SafeConvertToDouble(reader["Rating"]); } catch { }
        }

        public Teacher Login(string username, string password)
        {
            this.command.CommandText = $"SELECT tblUsers.*, tblTeacher.Location, tblTeacher.VehicleType, tblTeacher.LessonPrice, tblTeacher.Rating FROM tblUsers INNER JOIN tblTeacher ON tblUsers.id = tblTeacher.id WHERE (tblUsers.UserName = '{username}') AND (tblUsers.[Password] = '{password}')";
            TeacherList list = new TeacherList(base.Select());
            return list.Count > 0 ? list[0] : null;
        }

        public Teacher GetTeacherById(int id)
        {
            this.command.CommandText = $"SELECT tblUsers.*, tblTeacher.Location, tblTeacher.VehicleType, tblTeacher.LessonPrice, tblTeacher.Rating FROM tblUsers INNER JOIN tblTeacher ON tblUsers.id = tblTeacher.id WHERE tblUsers.id = {id}";
            TeacherList list = new TeacherList(base.Select());
            return list.Count > 0 ? list[0] : null;
        }

        public TeacherList Search(string location, string maxPrice)
        {
            string sql = "SELECT tblUsers.*, tblTeacher.Location, tblTeacher.VehicleType, tblTeacher.LessonPrice, tblTeacher.Rating FROM tblUsers INNER JOIN tblTeacher ON tblUsers.id = tblTeacher.id WHERE 1=1";
            if (!string.IsNullOrEmpty(location)) sql += $" AND tblTeacher.Location LIKE '%{location}%'";
            if (!string.IsNullOrEmpty(maxPrice) && double.TryParse(maxPrice, out double price)) sql += $" AND tblTeacher.LessonPrice <= {price}";

            this.command.CommandText = sql;
            return new TeacherList(base.Select());
        }

        // פונקציית "פטיש" לשליפת הדירוג בלבד - הכי יציב שיש
        public double GetRatingDirectly(int teacherId)
        {
            string sql = $"SELECT Rating FROM tblTeacher WHERE id={teacherId}";
            this.command.CommandText = sql;

            try
            {
                if (connection.State != System.Data.ConnectionState.Open) connection.Open();

                object result = command.ExecuteScalar();

                if (result != null && result != DBNull.Value)
                {
                    // המרה בטוחה שיודעת להתמודד עם הכל
                    return Convert.ToDouble(result);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error fetching rating directly: " + ex.Message);
            }
            finally
            {
                if (connection.State == System.Data.ConnectionState.Open) connection.Close();
            }

            return 0; // אם נכשל
        }
        public override void Insert(BaseEntity entity)
        {
            Teacher teacher = entity as Teacher;
            if (teacher != null)
            {
                this.inserted.Add(new ChangeEntity(base.CreateInsertSQL, entity));
                this.inserted.Add(new ChangeEntity(this.CreateInsertSQL, entity));
            }
        }

        public override string CreateInsertSQL(BaseEntity entity)
        {
            Teacher teacher = entity as Teacher;
            return $"INSERT INTO tblTeacher (id, Location, VehicleType, LessonPrice, Rating) VALUES ({teacher.Id}, '{teacher.Location}', '{teacher.VehicleType}', {teacher.LessonPrice.ToString(CultureInfo.InvariantCulture)}, 0)";
        }

        public override void Update(BaseEntity entity)
        {
            Teacher teacher = entity as Teacher;
            if (teacher != null)
            {
                this.updated.Add(new ChangeEntity(base.CreateUpdateSQL, entity));
                this.updated.Add(new ChangeEntity(this.CreateUpdateSQL, entity));
            }
        }

        public override string CreateUpdateSQL(BaseEntity entity)
        {
            Teacher teacher = entity as Teacher;
            string ratingStr = teacher.Rating.ToString(CultureInfo.InvariantCulture);
            string priceStr = teacher.LessonPrice.ToString(CultureInfo.InvariantCulture);

            return $"UPDATE tblTeacher SET Location='{teacher.Location}', VehicleType='{teacher.VehicleType}', LessonPrice={priceStr}, Rating={ratingStr} WHERE id={teacher.Id}";
        }

        public override string CreateDeleteSQL(BaseEntity entity) => throw new NotImplementedException();
    }
}