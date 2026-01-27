using System;
using System.Linq;
using System.Windows;
using System.Windows.Media; // הוספתי בשביל הצבעים
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

            // 1. עדכון בר התקדמות
            int lessonsDone = lessonDB.GetCompletedLessonsCount(student.Id);
            LicenseProgress.Value = lessonsDone;
            ProgressText.Text = $"{lessonsDone}/28 שיעורים בוצעו";

            // 2. עדכון השיעור הבא
            Lesson nextLesson = lessonDB.GetNextLessonForStudent(student.Id);

            if (nextLesson != null)
            {
                // הצגת התאריך בגדול
                NextLessonDate.Text = nextLesson.StartTime.ToString("dd/MM בשעה HH:mm");

                // שינוי צבע התאריך והוספת טקסט לפי הסטטוס (שימוש במאפיינים שיצרנו ב-Lesson.cs)
                try
                {
                    // משתמש ב-StatusColor שהגדרנו ב-Model
                    var color = (Color)ColorConverter.ConvertFromString(nextLesson.StatusColor);
                    NextLessonDate.Foreground = new SolidColorBrush(color);
                }
                catch
                {
                    NextLessonDate.Foreground = Brushes.Black;
                }

                // הצגת פרטים + הסטטוס (מאושר/ממתין) בתוך הסוגריים
                string statusText = nextLesson.DisplayStatus; // מגיע מ-Lesson.cs
                NextLessonDetails.Text = $"עם {nextLesson.TeacherName} ב-{nextLesson.Location}\nסטטוס: {statusText}";
            }
            else
            {
                NextLessonDate.Text = "אין שיעורים קרובים";
                NextLessonDate.Foreground = Brushes.Black;
                NextLessonDetails.Text = "לחץ על 'חפש מורה' כדי לקבוע!";
            }
        }

        private void LoadTeacherDashboard(Teacher teacher)
        {
            TeacherDashboard.Visibility = Visibility.Visible;
            StudentDashboard.Visibility = Visibility.Collapsed;

            TodaysDateText.Text = DateTime.Now.ToString("(dd/MM/yyyy)");

            // טעינת שיעורים להיום
            var allLessons = lessonDB.GetLessonsByTeacher(teacher.Id);
            var todaysLessons = allLessons.Where(l => l.StartTime.Date == DateTime.Today).OrderBy(l => l.StartTime).ToList();

            TodaysLessonsGrid.ItemsSource = todaysLessons;

            // חישוב הספק יומי (6 = מאושר/בוצע)
            int totalToday = todaysLessons.Count;
            int completedToday = todaysLessons.Count(l => l.Status == 6 || l.StartTime < DateTime.Now);

            DailyProgressText.Text = $"{completedToday}/{totalToday}";

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
    }
}
