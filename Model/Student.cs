using System;

namespace Model
{
    public class Student : User
    {
        private string licenseType;
        private int lessonsCount;
        private bool passedTheory; // שדה חדש

        public string LicenseType { get => licenseType; set => licenseType = value; }
        public int LessonsCount { get => lessonsCount; set => lessonsCount = value; }
        public bool PassedTheory { get => passedTheory; set => passedTheory = value; }
    }
}