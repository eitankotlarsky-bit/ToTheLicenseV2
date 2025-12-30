using System;
using System.Linq;
using System.Windows;
using Model;
using ViewModel;

namespace ToTheLicense
{
    public partial class HomePage : Window
    {
        private User currentUser;
        private LessonDB lessonDB;

        public HomePage(User user)
        {
            InitializeComponent();
            this.currentUser = user;
            this.lessonDB = new LessonDB();

            WelcomeText.Text = $"שלום, {user.FirstName} {user.LastName}";

            if (currentUser is Student student)
            {
                LoadStudentDashboard(student);
            }
            else if (currentUser is Teacher teacher)
            {
                LoadTeacherDashboard(teacher);
            }
        }

        private void LoadStudentDashboard(Student student)
        {
            StudentDashboard.Visibility = Visibility.Visible;
            TeacherDashboard.Visibility = Visibility.Collapsed;

            int lessonsDone = lessonDB.GetCompletedLessonsCount(student.Id);
            LicenseProgress.Value = lessonsDone;
            ProgressText.Text = $"{lessonsDone}/28 שיעורים בוצעו";

            Lesson nextLesson = lessonDB.GetNextLessonForStudent(student.Id);
            if (nextLesson != null)
            {
                NextLessonDate.Text = nextLesson.StartTime.ToString("dd/MM בשעה HH:mm");
                NextLessonDetails.Text = $"עם המורה {nextLesson.TeacherName} ב-{nextLesson.Location}";
            }
            else
            {
                NextLessonDate.Text = "אין שיעורים קרובים";
                NextLessonDetails.Text = "לחץ על 'חפש מורה' כדי לקבוע!";
            }
        }

        private void LoadTeacherDashboard(Teacher teacher)
        {
            TeacherDashboard.Visibility = Visibility.Visible;
            StudentDashboard.Visibility = Visibility.Collapsed;

            TodaysDateText.Text = DateTime.Now.ToString("(dd/MM/yyyy)");

            // שליפת כל השיעורים וסינון להיום בלבד
            var allLessons = lessonDB.GetLessonsByTeacher(teacher.Id);
            var todaysLessons = allLessons.Where(l => l.StartTime.Date == DateTime.Today).OrderBy(l => l.StartTime).ToList();

            TodaysLessonsGrid.ItemsSource = todaysLessons;

            // --- לוגיקה להספק יומי ---
            int totalToday = todaysLessons.Count;
            // נניח שסטטוס 2 זה "בוצע", או שנבדוק אם השעה עברה
            int completedToday = todaysLessons.Count(l => l.Status == 2 || l.StartTime < DateTime.Now);

            DailyProgressText.Text = $"{completedToday}/{totalToday}";

            // עדכון ה-ProgressBar (מונע חלוקה באפס)
            DailyProgressBar.Maximum = totalToday > 0 ? totalToday : 1;
            DailyProgressBar.Value = completedToday;
        }

        private void FindTeachers_Click(object sender, RoutedEventArgs e)
        {
            FindTeacher page = new FindTeacher((Student)currentUser);
            page.Show();
            this.Close();
        }

        private void ManageLessons_Click(object sender, RoutedEventArgs e)
        {
            ManageLessons page = new ManageLessons((Teacher)currentUser);
            page.Show();
            this.Close();
        }

        private void MyStudents_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser is Teacher teacher)
            {
                MyStudents page = new MyStudents(teacher);
                page.Show();
                this.Close();
            }
        }

        // בקובץ HomePage.xaml.cs, עדכן את הפונקציה הזו:
        private void Profile_Click(object sender, RoutedEventArgs e)
        {
            Profile profilePage = new Profile(currentUser);
            profilePage.Show();
            this.Close();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            Login login = new Login();
            login.Show();
            this.Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}