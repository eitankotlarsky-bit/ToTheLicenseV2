using System.Windows;
using Model;
using ViewModel;

namespace ToTheLicense
{
    public partial class FindTeacher : Window
    {
        private Student currentStudent;
        private TeacherDB teacherDB;

        public FindTeacher(Student student)
        {
            InitializeComponent();
            this.currentStudent = student;
            this.teacherDB = new TeacherDB();

            LoadTeachers();
        }

        // שינינו את הפרמטר השני מ-vehicle ל-maxPrice
        private void LoadTeachers(string location = "", string maxPrice = "")
        {
            // שולחים ל-DB את המיקום ואת המחיר
            TeacherList teachers = teacherDB.Search(location, maxPrice);

            TeachersList.ItemsSource = teachers;

            if (teachers.Count == 0)
            {
                // אופציונלי: הודעה שאין תוצאות
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string location = LocationTextBox.Text.Trim();
            // לוקחים את הערך מתיבת המחיר החדשה
            string price = PriceTextBox.Text.Trim();

            LoadTeachers(location, price);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            HomePage home = new HomePage(currentStudent);
            home.Show();
            this.Close();
        }
    }
}