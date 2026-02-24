using System.Collections.Generic;
using System.Linq;

namespace Model
{
    public class TestList : List<TestEntity>
    {
        public TestList() { }
        public TestList(IEnumerable<TestEntity> list) : base(list) { }
        public TestList(IEnumerable<BaseEntity> list) : base(list.Cast<TestEntity>().ToList()) { }
    }
}