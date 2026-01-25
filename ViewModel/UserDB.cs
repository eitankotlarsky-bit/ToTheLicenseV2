using Model;

namespace ViewModel
{
    public abstract class UserDB : BaseDB
    {
        protected override void CreateModel(BaseEntity entity)
        {
            User user = entity as User;
            user.Id = (int)this.reader["id"];
            user.UserName = this.reader["UserName"].ToString();
            user.Password = this.reader["Password"].ToString();
            user.Email = this.reader["Email"].ToString();
            user.FirstName = this.reader["FirstName"].ToString();
            user.LastName = this.reader["LastName"].ToString();
        }

        public override string CreateInsertSQL(BaseEntity entity)
        {
            User user = entity as User;
            // טיפול בגרשיים
            string uName = user.UserName.Replace("'", "''");
            string pass = user.Password.Replace("'", "''");
            string email = user.Email.Replace("'", "''");
            string fName = user.FirstName.Replace("'", "''");
            string lName = user.LastName.Replace("'", "''");

            return $"INSERT INTO tblUsers (UserName, [Password], Email, FirstName, LastName) VALUES ('{uName}', '{pass}', '{email}', '{fName}', '{lName}')";
        }

        public override string CreateUpdateSQL(BaseEntity entity)
        {
            User user = entity as User;
            string fName = user.FirstName.Replace("'", "''");
            string lName = user.LastName.Replace("'", "''");
            string email = user.Email.Replace("'", "''");
            string pass = user.Password.Replace("'", "''");

            return $"UPDATE tblUsers SET FirstName='{fName}', LastName='{lName}', Email='{email}', [Password]='{pass}' WHERE id={user.Id}";
        }
    }
}