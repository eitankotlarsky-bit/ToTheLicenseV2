using System;

namespace Model
{
    public class TestEntity : BaseEntity
    {
        // סוגי טסט
        public const int TYPE_INTERNAL = 1;
        public const int TYPE_EXTERNAL = 2;

        // סטטוסים
        public const int STATUS_REQUESTED = 0; // ממתין לאישור מורה
        public const int STATUS_APPROVED = 1;  // נקבע תאריך
        public const int STATUS_PASSED = 2;    // עבר
        public const int STATUS_FAILED = 3;    // נכשל
        public const int STATUS_REJECTED = 4;  // המורה דחה את הבקשה

        private int studentId;
        private int teacherId;
        private int testType;
        private int status;
        private DateTime testDate;
        private string notes;

        public int StudentId { get => studentId; set => studentId = value; }
        public int TeacherId { get => teacherId; set => teacherId = value; }
        public int TestType { get => testType; set => testType = value; }
        public int Status { get => status; set => status = value; }
        public DateTime TestDate { get => testDate; set => testDate = value; }
        public string Notes { get => notes; set => notes = value; }

        // מאפייני עזר לתצוגה
        public string TestTypeName => TestType == TYPE_INTERNAL ? "טסט פנימי" : "טסט חיצוני";
        public string StatusColor
        {
            get
            {
                if (Status == STATUS_PASSED) return "#4CAF50"; // ירוק
                if (Status == STATUS_FAILED || Status == STATUS_REJECTED) return "#F44336"; // אדום
                if (Status == STATUS_APPROVED) return "#2196F3"; // כחול
                return "#FF9800"; // כתום (ממתין)
            }
        }
    }
}