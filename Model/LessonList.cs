using System.Collections.Generic;
using System.Linq;

namespace Model
{
    public class LessonList : List<Lesson>
    {
        public LessonList() { }
        public LessonList(IEnumerable<Lesson> list) : base(list) { }
        public LessonList(IEnumerable<BaseEntity> list) : base(list.Cast<Lesson>().ToList()) { }
    }
}