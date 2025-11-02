using System.Collections.Generic;
using System.Linq;

namespace Model
{
    public class LessonList : List<Lesson>
    {
        public LessonList() { }
        public LessonList(IEnumerable<Lesson> items) : base(items) { }
        public LessonList(IEnumerable<BaseEntity> items) : base(items.Cast<Lesson>().ToList()) { }
    }
}
