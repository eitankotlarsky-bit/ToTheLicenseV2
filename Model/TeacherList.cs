using System.Collections.Generic;
using System.Linq;

namespace Model
{
    public class TeacherList : List<Teacher>
    {
        public TeacherList() { }
        public TeacherList(IEnumerable<Teacher> list) : base(list) { }
        public TeacherList(IEnumerable<BaseEntity> list) : base(list.Cast<Teacher>().ToList()) { }
    }
}