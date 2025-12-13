using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Model;
using ViewModel;

namespace ToTheLicense
{
    public partial class MyStudents : Window
    {
        private Teacher currentTeacher;
        private StudentDB studentDB;
        private StudentList allStudents;

        public MyStudents(Teacher teacher)
        {
            InitializeComponent();
            this.currentTeacher = teacher;
            this.studentDB = new StudentDB();

            LoadStudents();
        }

        private void LoadStudents()
        {
            // שליפת התלמידים של המורה הנוכחי
            allStudents = studentDB.GetStudentsByTeacher(currentTeacher.Id);

            // עדכון הטבלה
            StudentsGrid.ItemsSource = allStudents;

            // עדכון כותרת משנה
            SubtitleText.Text = $"סה\"כ {allStudents.Count} תלמידים פעילים";
        }

        // חיפוש בזמן אמת
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = SearchBox.Text.ToLower();

            if (allStudents == null) return;

            var filtered = allStudents.Where(s =>
                s.FirstName.ToLower().Contains(query) ||
                s.LastName.ToLower().Contains(query) ||
                s.Email.ToLower().Contains(query)
            ).ToList();

            StudentsGrid.ItemsSource = filtered;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            HomePage home = new HomePage(currentTeacher);
            home.Show();
            this.Close();
        }
    }
}