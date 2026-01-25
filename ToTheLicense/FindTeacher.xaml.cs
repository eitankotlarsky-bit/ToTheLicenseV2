using System.Windows;
using System.Windows.Controls; // חשוב עבור Button
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

        private void LoadTeachers(string location = "", string maxPrice = "")
        {
            // שליפת מורים עם סינון (הפונקציה Search קיימת ב-TeacherDB המתוקן)
            TeacherList teachers = teacherDB.Search(location, maxPrice);
            TeachersList.ItemsSource = teachers;

            if (teachers.Count == 0)
            {
                // אפשר להוסיף הודעה או טקסט "לא נמצאו תוצאות" ב-XAML
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string location = LocationTextBox.Text.Trim();
            string price = PriceTextBox.Text.Trim();
            LoadTeachers(location, price);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            HomePage home = new HomePage(currentStudent);
            home.Show();
            this.Close();
        }

        // === הפונקציה החדשה לכפתור "הזמן שיעור" ===
        private void BookTeacher_Click(object sender, RoutedEventArgs e)
        {
            // 1. זיהוי הכפתור שנלחץ
            Button btn = sender as Button;

            // 2. שליפת אובייקט המורה מתוך ה-Tag של הכפתור (הגדרנו Tag="{Binding}" ב-XAML)
            Teacher selectedTeacher = btn.Tag as Teacher;

            if (selectedTeacher != null)
            {
                // 3. פתיחת חלון ההזמנה עם המורה שנבחר
                BookLessonWindow bookWindow = new BookLessonWindow(selectedTeacher, currentStudent);

                // שימוש ב-ShowDialog כדי לחסום את החלון הראשי עד לסיום ההזמנה
                bookWindow.ShowDialog();

                // אופציונלי: כאן אפשר להוסיף לוגיקה אחרי שההזמנה בוצעה (למשל רענון)
            }
        }
    }
}