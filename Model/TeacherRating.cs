using System;

namespace Model
{
    public class TeacherRating : BaseEntity
    {
        private int teacherId;
        private int studentId;
        private int ratingValue;
        private string comment;
        private DateTime ratingDate;

        public int TeacherId { get => teacherId; set => teacherId = value; }
        public int StudentId { get => studentId; set => studentId = value; }
        public int RatingValue { get => ratingValue; set => ratingValue = value; }
        public string Comment { get => comment; set => comment = value; }
        public DateTime RatingDate { get => ratingDate; set => ratingDate = value; }
    }
}