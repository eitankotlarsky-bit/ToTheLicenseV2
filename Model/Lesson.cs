using System;

namespace Model
{
    public class Lesson : BaseEntity
    {
        private int teacherId;
        private string location;
        private double price;
        private int status;
        private string statusName;
        private string notes;
        private DateTime startTime;
        private DateTime endTime;
        private string vehicleType;

        // שדות עזר לתצוגה
        private string teacherName;
        private string studentName; // הוספנו את זה עבור ManageLessons

        public int TeacherId { get => teacherId; set => teacherId = value; }
        public string Location { get => location; set => location = value; }
        public double Price { get => price; set => price = value; }
        public int Status { get => status; set => status = value; }
        public string StatusName { get => statusName; set => statusName = value; }
        public string Notes { get => notes; set => notes = value; }
        public DateTime StartTime { get => startTime; set => startTime = value; }
        public DateTime EndTime { get => endTime; set => endTime = value; }
        public string VehicleType { get => vehicleType; set => vehicleType = value; }
        public string TeacherName { get => teacherName; set => teacherName = value; }
        public string StudentName { get => studentName; set => studentName = value; }
    }
}