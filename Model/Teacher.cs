using System;

namespace Model
{
    public class Teacher : User
    {
        private string location;
        private string vehicleType;
        private double lessonPrice;
        private double rating;

        public string Location { get => location; set => location = value; }
        public string VehicleType { get => vehicleType; set => vehicleType = value; }
        public double LessonPrice { get => lessonPrice; set => lessonPrice = value; }
        public double Rating { get => rating; set => rating = value; }
    }
}