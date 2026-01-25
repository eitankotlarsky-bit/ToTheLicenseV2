using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class StudentList : List<Student>
    {
        public StudentList()
        {
            
        }

        public StudentList(IEnumerable<Student> list) : base(list) { }
        public StudentList(IEnumerable<BaseEntity> list) 
            : base(list.Cast<Student>().ToList()) { }


    }
}
