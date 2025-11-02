using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Teacher : User
    {
        private string location;
        private string vehicleType;
        private int lessonPrice;
        private double rating;

        public string Location { get => location; set => location = value; }
        public string VehicleType { get => vehicleType; set => vehicleType = value; }
        public int LessonPrice { get => lessonPrice; set => lessonPrice = value; }
        public double Rating { get => rating; set => rating = value; }
    }
}
