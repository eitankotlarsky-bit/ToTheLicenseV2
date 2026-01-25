using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModel
{
    public abstract class UserDB : BaseDB
    {
        protected override void CreateModel(BaseEntity entity)
        {
            User user = entity as User;

            // -- Users
            user.Id = (int)this.reader["id"];
            user.FirstName = this.reader["FirstName"].ToString();
            user.LastName = this.reader["LastName"].ToString();
            user.UserName = this.reader["UserName"].ToString();
            user.Email = this.reader["Email"].ToString();
            user.Password = this.reader["Password"] != DBNull.Value ? this.reader["Password"].ToString() : string.Empty;
        }

        public override string CreateInsertSQL(BaseEntity entity)
        {
            User user = entity as User;
            string sqlStr = $@"INSERT INTO tblUsers
                                        (Username, [Password], Email, FirstName, LastName)
                                VALUES
                                        ('{user.FirstName}', 
                                        '{user.Password}', 
                                        '{user.Email}',
                                        '{user.FirstName}',
                                        '{user.LastName}')";

            return sqlStr;
        }

        public override string CreateUpdateSQL(BaseEntity entity)
        {
            User user = entity as User;

            string sqlStr = $@"UPDATE tblUsers SET
                                    Username = '{user.UserName}',
                                    [Password] = '{user.Password}',
                                    Email = '{user.Email}',
                                    FirstName = '{user.FirstName}',
                                    LastName = '{user.LastName}'
                               WHERE id = {user.Id}";

            return sqlStr;
        }
    }
}
