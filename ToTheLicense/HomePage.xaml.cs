using System.Windows;

namespace ToTheLicense
{
    public partial class HomePage : Window
    {
        private readonly string _displayName;

        // אפשר לקרוא: new HomePage("איתן") – ואז נראה "ברוך הבא, איתן"
        public HomePage(string displayName = "משתמש")
        {
            InitializeComponent();
            _displayName = displayName;
            WelcomeNameText.Text = $"ברוך הבא, {_displayName}";
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            // חזרה למסך התחברות
            var login = new Login();
            login.Show();
            Close();
        }

        private void ManageLessons_Click(object sender, RoutedEventArgs e)
        {
            ManageLessons manageLessons = new ManageLessons(); // יצירת המסך
            manageLessons.Show(); // פתיחה של המסך
            this.Close(); // סגירת דף הבית (לא חובה, אבל מומלץ כדי שלא יהיו שני חלונות פתוחים)
        }



        private void FindTeachers_Click(object sender, RoutedEventArgs e)
        {
            // פותח את חלון חיפוש המורים
            var find = new FindTeacher();
            find.Show();

            // אם אתה רוצה לסגור את דף הבית:
            this.Close();
            // ואם מעדיף להשאיר ברקע במקום לסגור:  this.Hide();
        }


        private void Payments_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("כאן נפתח מסך תשלומים וקבלות.");
        }

        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("כאן נפתח מסך פרופיל.");
        }
    }
}
