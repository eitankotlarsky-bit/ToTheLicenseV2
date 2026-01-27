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
        private string studentName;

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

        // ==========================================
        // תוספות חדשות עבור העיצוב (הלוגיקה הפשוטה)
        // ==========================================

        // 1. קובע האם להציג את הכפתורים (רק אם סטטוס 5-ממתין)
        public string ButtonsVisibility
        {
            get
            {
                return Status == 5 ? "Visible" : "Collapsed";
            }
        }

        // 2. צבע הטקסט (כתום, ירוק או אדום)
        public string StatusColor
        {
            get
            {
                if (Status == 5) return "#FF9800"; // כתום
                if (Status == 6) return "#4CAF50"; // ירוק
                if (Status == 7) return "#F44336"; // אדום
                return "#000000";
            }
        }

        // 3. צבע הרקע של התווית
        public string StatusBackground
        {
            get
            {
                if (Status == 5) return "#FFF3E0"; // כתום בהיר
                if (Status == 6) return "#E8F5E9"; // ירוק בהיר
                if (Status == 7) return "#FFEBEE"; // אדום בהיר
                return "#FFFFFF";
            }
        }

        // 4. טקסט מעוצב לסטטוס
        public string DisplayStatus
        {
            get
            {
                if (Status == 5) return "ממתין לאישור";
                if (Status == 6) return "מאושר ✔";
                if (Status == 7) return "נדחה / בוטל ✖";
                return StatusName; // ברירת מחדל
            }
        }
    }
}