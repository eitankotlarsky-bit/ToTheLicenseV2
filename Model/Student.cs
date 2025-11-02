using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Student : User
    {
        private string licenseType;
        private int lessons;

        public string LicenseType { get => licenseType; set => licenseType = value; }
        public int Lessons { get => lessons; set => lessons = value; }
    }
}
