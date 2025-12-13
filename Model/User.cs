using System;

namespace Model
{
    public abstract class User : BaseEntity
    {
        private string firstName;
        private string lastName;
        private string email;
        private string userName;
        private string password;

        public string FirstName { get => firstName; set => firstName = value; }
        public string LastName { get => lastName; set => lastName = value; }
        public string Email { get => email; set => email = value; }
        public string UserName { get => userName; set => userName = value; }
        public string Password { get => password; set => password = value; }

        // מאפיין עזר לתצוגה ב-XAML
        public string FullName => $"{FirstName} {LastName}";
    }
}