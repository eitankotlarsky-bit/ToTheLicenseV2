using System.Windows;
using Model;
using ViewModel;

namespace ToTheLicense
{
    /// <summary>
    /// לוגיקה לניהול שיעורים (עבור מורים)
    /// </summary>
    public partial class ManageLessons : Window
    {
        private Teacher currentTeacher;
        private LessonDB lessonDB;

        public ManageLessons(Teacher teacher)
        {
            InitializeComponent();
            this.currentTeacher = teacher;
            this.lessonDB = new LessonDB();

            // טעינת רשימת השיעורים מיד עם פתיחת החלון
            LoadLessons();
        }

        private void LoadLessons()
        {
            // שליפת השיעורים של המורה הנוכחי ממסד הנתונים
            // הפונקציה GetLessonsByTeacher ב-ViewModel דואגת למלא גם את השדה StudentName
            // שבו השתמשנו ב-Binding ב-XAML ({Binding StudentName})
            LessonList lessons = lessonDB.GetLessonsByTeacher(currentTeacher.Id);

            // חיבור הנתונים ל-ItemsControl ב-XAML שנקרא "LessonsList"
            LessonsList.ItemsSource = lessons;
        }

        // כפתור חזור בפינה הימנית של ה-Header
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // חזרה לדף הבית עם פרטי המורה הנוכחי
            HomePage home = new HomePage(currentTeacher);
            home.Show();
            this.Close();
        }
    }
}