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
        }
    }
}
