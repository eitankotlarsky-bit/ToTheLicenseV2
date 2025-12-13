using System;
using Model;

namespace ViewModel
{
    public abstract class UserDB : BaseDB
    {
        protected override void CreateModel(BaseEntity entity)
        {
            User user = entity as User;
            user.Id = (int)reader["id"];
            user.UserName = reader["UserName"].ToString();
            user.Password = reader["Password"].ToString();
            user.Email = reader["Email"].ToString();
            user.FirstName = reader["FirstName"].ToString();
            user.LastName = reader["LastName"].ToString();
        }

        public override string CreateInsertSQL(BaseEntity entity)
        {
            User user = entity as User;
            return $"INSERT INTO tblUsers (UserName, [Password], Email, FirstName, LastName) VALUES ('{user.UserName}', '{user.Password}', '{user.Email}', '{user.FirstName}', '{user.LastName}')";
        }
    }
}