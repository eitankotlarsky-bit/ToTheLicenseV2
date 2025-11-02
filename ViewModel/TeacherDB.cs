using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModel
{
    public class TeacherDB : UserDB
    {
        public override string CreateDeleteSQL(BaseEntity entity)
        {
            throw new NotImplementedException();
        }

        public override string CreateInsertSQL(BaseEntity entity)
        {
            throw new NotImplementedException();
        }

        public override string CreateUpdateSQL(BaseEntity entity)
        {
            throw new NotImplementedException();
        }

        protected override BaseEntity NewEntity()
        {
            return new Teacher();
        }

        protected override void CreateModel(BaseEntity entity)
        {
            Teacher teacher = entity as Teacher;

            base.CreateModel(entity);

        }

        public Teacher Login(string username, string password)
        {
            this.command.CommandText = $"SELECT        tblUsers.id, tblUsers.Username, tblUsers.[Password], tblUsers.Email, tblUsers.FirstName, tblUsers.LastName, tblTeacher.StartWorkDate\r\nFROM            (tblTeacher INNER JOIN\r\n                         tblUsers ON tblTeacher.id = tblUsers.id)\r\nWHERE        (tblUsers.Username = '{username}') AND (tblUsers.[Password] = '{password}')";

            TeacherList teachers = new TeacherList(base.Select());

            if (teachers.Count > 0)
                return teachers[0];

            return null;
        }
    }
}
