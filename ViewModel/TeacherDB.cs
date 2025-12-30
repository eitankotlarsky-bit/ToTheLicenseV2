using Model;
using System;

namespace ViewModel
{
    public class TeacherDB : UserDB
    {
        protected override BaseEntity NewEntity()
        {
            return new Teacher();
        }

        protected override void CreateModel(BaseEntity entity)
        {
            // ממלאים את ה-User
            base.CreateModel(entity);

            Teacher teacher = entity as Teacher;

            // ממלאים את ה-Teacher בזהירות
            try { teacher.Location = this.reader["Location"].ToString(); } catch { }
            try { teacher.VehicleType = this.reader["VehicleType"].ToString(); } catch { }

            // המרה בטוחה למחיר
            try
            {
                if (this.reader["LessonPrice"] != DBNull.Value)
                    teacher.LessonPrice = double.Parse(this.reader["LessonPrice"].ToString());
            }
            catch { }

            // המרה בטוחה לדירוג
            try
            {
                if (this.reader["Rating"] != DBNull.Value)
                    teacher.Rating = double.Parse(this.reader["Rating"].ToString());
                else
                    teacher.Rating = 0;
            }
            catch { teacher.Rating = 0; }
        }

        public Teacher Login(string username, string password)
        {
            // שאילתה מתוקנת: בוחרת ספציפית עמודות כדי למנוע את שגיאת "id ambiguous"
            this.command.CommandText = $"SELECT tblUsers.*, tblTeacher.Location, tblTeacher.VehicleType, tblTeacher.LessonPrice, tblTeacher.Rating FROM tblUsers INNER JOIN tblTeacher ON tblUsers.id = tblTeacher.id WHERE (tblUsers.UserName = '{username}') AND (tblUsers.[Password] = '{password}')";

            TeacherList teachers = new TeacherList(base.Select());

            if (teachers.Count > 0)
                return teachers[0];

            return null;
        }

        public TeacherList Search(string location, string maxPrice)
        {
            // שאילתה מתוקנת גם בחיפוש
            string sql = "SELECT tblUsers.*, tblTeacher.Location, tblTeacher.VehicleType, tblTeacher.LessonPrice, tblTeacher.Rating FROM tblUsers INNER JOIN tblTeacher ON tblUsers.id = tblTeacher.id WHERE 1=1";

            if (!string.IsNullOrEmpty(location))
            {
                sql += $" AND tblTeacher.Location LIKE '%{location}%'";
            }

            if (!string.IsNullOrEmpty(maxPrice))
            {
                if (double.TryParse(maxPrice, out double price))
                {
                    sql += $" AND tblTeacher.LessonPrice <= {price}";
                }
            }

            this.command.CommandText = sql;
            return new TeacherList(base.Select());
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
            return $"INSERT INTO tblTeacher (id, Location, VehicleType, LessonPrice, Rating) VALUES ({teacher.Id}, '{teacher.Location}', '{teacher.VehicleType}', {teacher.LessonPrice}, 0)";
        }

        public override void Update(BaseEntity entity)
        {
            Teacher teacher = entity as Teacher;
            if (teacher != null)
            {
                // עדכון טבלת המשתמשים (דרך מחלקת האב)
                this.updated.Add(new ChangeEntity(base.CreateUpdateSQL, entity));
                // עדכון טבלת המורים
                this.updated.Add(new ChangeEntity(this.CreateUpdateSQL, entity));
            }
        }

        public override string CreateUpdateSQL(BaseEntity entity)
        {
            Teacher teacher = entity as Teacher;
            return $"UPDATE tblTeacher SET Location='{teacher.Location}', VehicleType='{teacher.VehicleType}', LessonPrice={teacher.LessonPrice} WHERE id={teacher.Id}";
        }
        public override string CreateDeleteSQL(BaseEntity entity) => throw new NotImplementedException();
    }
}