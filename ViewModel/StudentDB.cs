using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModel
{
    public class StudentDB : UserDB
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

        protected override void CreateModel(BaseEntity entity)
        {
            Student student = entity as Student;

            base.CreateModel(student);

            // --Student

            student.LicenseType = (string)this.reader["licenseType"];
            student.Lessons = (int)this.reader["lessons"];

        }

        protected override BaseEntity NewEntity()
        {
            return new Student();
        }


        public StudentList SelectAll()
        {
            this.command.CommandText = $@"SELECT        tblUsers.id, tblUsers.Username, tblUsers.[Password], tblUsers.Email, tblUsers.FirstName, tblUsers.LastName, tblStudent.ClassNumber, tblStudent.MajorA, tblStudent.MajorB, tblStudent.[StartYear]
FROM            (tblStudent INNER JOIN
                         tblUsers ON tblStudent.id = tblUsers.id)";

            return new StudentList(base.Select());
        }
    }
}
