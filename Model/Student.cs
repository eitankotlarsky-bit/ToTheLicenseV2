using System;

namespace Model
{
    public class Student : User
    {
        private string licenseType;
        private int lessonsCount; // שדה Lessons בטבלה

        public string LicenseType { get => licenseType; set => licenseType = value; }
        public int LessonsCount { get => lessonsCount; set => lessonsCount = value; }
    }
}