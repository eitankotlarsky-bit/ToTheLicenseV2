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

        
        public override string CreateUpdateSQL(BaseEntity entity)
        {
            Teacher teacher = entity as Teacher;

            string sqlStr = $@"UPDATE tblTeacher SET
                                    StartWorkDate = #{teacher.StartWorkDate}#
                               WHERE id = {teacher.Id}";

            return sqlStr;
        }

        protected override BaseEntity NewEntity()
        {
            return new Teacher();
        }

        protected override void CreateModel(BaseEntity entity)
        {
            Teacher teacher = entity as Teacher;
            
            base.CreateModel(entity);

            teacher.StartWorkDate = Convert.ToDateTime(this.reader["StartWorkDate"]);
        }

        public Teacher Login(string username,string password)
        {
            this.command.CommandText = $"SELECT        tblUsers.id, tblUsers.Username, tblUsers.[Password], tblUsers.Email, tblUsers.FirstName, tblUsers.LastName, tblTeacher.StartWorkDate\r\nFROM            (tblTeacher INNER JOIN\r\n                         tblUsers ON tblTeacher.id = tblUsers.id)\r\nWHERE        (tblUsers.Username = '{username}') AND (tblUsers.[Password] = '{password}')";

            TeacherList teachers =  new TeacherList(base.Select());

            if (teachers.Count > 0)
                return teachers[0];

            return null;
        }
        // this case require multiple inputs
        // so we need to override the insert method
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

            string sqlStr = $@"INSERT INTO tblTeacher
                                    (id, StartWorkDate)
                            VALUES        ({teacher.Id}, #{teacher.StartWorkDate}#)";

            return sqlStr;
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
    }
}
