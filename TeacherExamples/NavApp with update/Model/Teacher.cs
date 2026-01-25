using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public class Teacher : User
    {
        private DateTime startWorkDate;

        public DateTime StartWorkDate { get => startWorkDate; set => startWorkDate = value; }

        
    }
}
