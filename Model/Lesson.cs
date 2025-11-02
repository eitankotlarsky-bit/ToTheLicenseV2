using System;

namespace Model
{
    public class Lesson : BaseEntity
    {
        public int StudentID { get; set; }
        public int TeacherID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Location { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; } // "Scheduled" / "Completed" / "Canceled"
        public string Notes { get; set; }
        public string VehicleType { get; set; }
    }
}
