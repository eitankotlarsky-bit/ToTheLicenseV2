using Model;
using System;
using System.Globalization;

namespace ViewModel
{
    public class TeacherDB : UserDB
    {
        protected override BaseEntity NewEntity() => new Teacher();

        protected override void CreateModel(BaseEntity entity)
        {
            base.CreateModel(entity);
            Teacher teacher = entity as Teacher;
            try { teacher.Location = this.reader["Location"].ToString(); } catch { }
            try { teacher.VehicleType = this.reader["VehicleType"].ToString(); } catch { }
            try { teacher.LessonPrice = double.Parse(this.reader["LessonPrice"].ToString()); } catch { }
            if (this.reader["Rating"] != DBNull.Value) try { teacher.Rating = double.Parse(this.reader["Rating"].ToString()); } catch { }
        }

        // התחברות עם שאילתה מתוקנת (למניעת כפילות ID)
        public Teacher Login(string username, string password)
        {
            this.command.CommandText = $"SELECT tblUsers.*, tblTeacher.Location, tblTeacher.VehicleType, tblTeacher.LessonPrice, tblTeacher.Rating FROM tblUsers INNER JOIN tblTeacher ON tblUsers.id = tblTeacher.id WHERE (tblUsers.UserName = '{username}') AND (tblUsers.[Password] = '{password}')";
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

        // === לוגיקת הכנסה כפולה (כמו אצל המורה) ===
        public override void Insert(BaseEntity entity)
        {
            Teacher teacher = entity as Teacher;
            if (teacher != null)
            {
                this.inserted.Add(new ChangeEntity(base.CreateInsertSQL, entity)); // לטבלת המשתמשים
                this.inserted.Add(new ChangeEntity(this.CreateInsertSQL, entity)); // לטבלת המורים
            }
        }

        public override string CreateInsertSQL(BaseEntity entity)
        {
            Teacher teacher = entity as Teacher;
            // ה-ID כבר יהיה מעודכן בשלב הזה בזכות הטרנזקציה ב-BaseDB
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
            return $"UPDATE tblTeacher SET Location='{teacher.Location}', VehicleType='{teacher.VehicleType}', LessonPrice={teacher.LessonPrice.ToString(CultureInfo.InvariantCulture)} WHERE id={teacher.Id}";
        }

        public override string CreateDeleteSQL(BaseEntity entity) => throw new NotImplementedException();
    }
}