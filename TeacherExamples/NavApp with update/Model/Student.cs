using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Student : User
    {
        private int classNumber;
        private string majorsA;
        private string majorsB;
        private int level;

        public int ClassNumber { get => classNumber; set => classNumber = value; }
        public string MajorsA { get => majorsA; set => majorsA = value; }
        public string MajorsB { get => majorsB; set => majorsB = value; }
        public int StartYear { get => level; set => level = value; }
    }
}
