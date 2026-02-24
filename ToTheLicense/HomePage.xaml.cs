using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Model;
using ViewModel;

namespace ToTheLicense
{
    public partial class HomePage : Window
    {
        private User currentUser;
        private LessonDB lessonDB;
        private Lesson _lessonToRate;

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

            UpdateRoadmap(student);
            CheckForRateableTeacher(student.Id);
        }

        private void UpdateRoadmap(Student student)
        {
            TestDB testDB = new TestDB();
            var greenBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50"));
            var grayBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0"));
            var orangeBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9800"));
            var lightGreenBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C8E6C9"));

            if (student.PassedTheory)
            {
                StepTheory.Background = lightGreenBrush;
                TxtTheoryStatus.Text = "עבר בהצלחה!";
                TxtTheoryStatus.Foreground = greenBrush;
                Line1.Fill = greenBrush;
            }
            else
            {
                StepTheory.Background = grayBrush;
                TxtTheoryStatus.Text = "טרם בוצע";
                TxtTheoryStatus.Foreground = Brushes.Gray;
            }

            TestEntity internalTest = testDB.GetLastTest(student.Id, TestEntity.TYPE_INTERNAL);
            bool internalPassed = false;

            if (internalTest != null && internalTest.Status == TestEntity.STATUS_PASSED)
            {
                StepInternal.Background = lightGreenBrush;
                TxtInternalStatus.Text = "עבר בהצלחה!";
                TxtInternalStatus.Foreground = greenBrush;
                Line2.Fill = greenBrush;
                internalPassed = true;
            }
            else if (internalTest != null && internalTest.Status == TestEntity.STATUS_APPROVED)
            {
                StepInternal.Background = orangeBrush;
                TxtInternalStatus.Text = $"נקבע ל-{internalTest.TestDate:dd/MM}";
                TxtInternalStatus.Foreground = Brushes.Black;
            }
            else
            {
                StepInternal.Background = student.PassedTheory ? Brushes.White : grayBrush;
                StepInternal.BorderBrush = student.PassedTheory ? Brushes.Gray : Brushes.Transparent;
                StepInternal.BorderThickness = student.PassedTheory ? new Thickness(1) : new Thickness(0);
                TxtInternalStatus.Text = student.PassedTheory ? "ממתין למורה" : "נעול";
            }

            TestEntity externalTest = testDB.GetLastTest(student.Id, TestEntity.TYPE_EXTERNAL);

            BtnRequestTest.Visibility = Visibility.Collapsed;
            TxtExternalStatus.Visibility = Visibility.Visible;

            if (externalTest != null)
            {
                if (externalTest.Status == TestEntity.STATUS_REQUESTED)
                {
                    BtnRequestTest.Visibility = Visibility.Visible;
                    BtnRequestTest.Content = "ממתין לאישור";
                    BtnRequestTest.IsEnabled = false;
                    BtnRequestTest.Background = Brushes.LightGray;
                    TxtExternalStatus.Visibility = Visibility.Collapsed;
                }
                else if (externalTest.Status == TestEntity.STATUS_APPROVED)
                {
                    StepExternal.Background = orangeBrush;
                    TxtExternalStatus.Text = $"נקבע: {externalTest.TestDate:dd/MM}";
                    TxtExternalStatus.Foreground = Brushes.Black;
                }
                else if (externalTest.Status == TestEntity.STATUS_PASSED)
                {
                    StepExternal.Background = lightGreenBrush;
                    TxtExternalStatus.Text = "יש רישיון! 🎉";
                    TxtExternalStatus.Foreground = greenBrush;
                    TxtExternalStatus.FontWeight = FontWeights.Bold;
                }
                else if (externalTest.Status == TestEntity.STATUS_FAILED)
                {
                    StepExternal.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEBEE"));
                    TxtExternalStatus.Text = "נכשל, נסה שוב";
                    TxtExternalStatus.Foreground = Brushes.Red;

                    BtnRequestTest.Visibility = Visibility.Visible;
                    BtnRequestTest.IsEnabled = true;
                    BtnRequestTest.Content = "בקש שוב";
                    TxtExternalStatus.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (student.PassedTheory && internalPassed)
                {
                    BtnRequestTest.Visibility = Visibility.Visible;
                    BtnRequestTest.IsEnabled = true;
                    TxtExternalStatus.Visibility = Visibility.Collapsed;
                }
                else
                {
                    TxtExternalStatus.Text = "נעול";
                }
            }
        }

        private void RequestExternalTest_Click(object sender, RoutedEventArgs e)
        {
            TestDB testDB = new TestDB();
            Student s = currentUser as Student;

            Lesson lastLesson = new LessonDB().GetLastCompletedLesson(s.Id);
            if (lastLesson == null)
                lastLesson = new LessonDB().GetNextLessonForStudent(s.Id);

            int teacherId = lastLesson != null ? lastLesson.TeacherId : 0;

            if (teacherId == 0)
            {
                MessageBox.Show("עליך לבצע לפחות שיעור אחד או לקבוע שיעור עם מורה לפני בקשת טסט.");
                return;
            }

            TestEntity newTest = new TestEntity
            {
                StudentId = s.Id,
                TeacherId = teacherId,
                TestType = TestEntity.TYPE_EXTERNAL,
                Status = TestEntity.STATUS_REQUESTED,
                TestDate = DateTime.Now.AddDays(14),
                Notes = "בקשת תלמיד מהאפליקציה"
            };

            testDB.Insert(newTest);
            testDB.SaveChanges();

            MessageBox.Show("בקשתך לטסט חיצוני נשלחה למורה בהצלחה!\nהמורה יעדכן את התאריך ויאשר את הבקשה.");
            UpdateRoadmap(s);
        }

        private void CheckForRateableTeacher(int studentId)
        {
            _lessonToRate = lessonDB.GetLastCompletedLesson(studentId);

            if (_lessonToRate != null)
            {
                RatingDB rateDB = new RatingDB();
                bool alreadyRated = rateDB.HasRated(_lessonToRate.TeacherId, studentId);

                if (!alreadyRated)
                {
                    RateButton.Visibility = Visibility.Visible;
                }
                else
                {
                    RateButton.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                RateButton.Visibility = Visibility.Collapsed;
            }
        }

        private void RateLesson_Click(object sender, RoutedEventArgs e)
        {
            if (_lessonToRate != null && currentUser is Student student)
            {
                RateWindow win = new RateWindow(_lessonToRate.TeacherName, _lessonToRate.StartTime, _lessonToRate.TeacherId, student.Id);
                win.ShowDialog();
                CheckForRateableTeacher(student.Id);
            }
        }

        private void LoadTeacherDashboard(Teacher teacher)
        {
            TeacherDashboard.Visibility = Visibility.Visible;
            StudentDashboard.Visibility = Visibility.Collapsed;

            TeacherDB tdb = new TeacherDB();
            double freshRating = tdb.GetRatingDirectly(teacher.Id);
            teacher.Rating = freshRating;

            if (teacher.Rating > 0)
                TeacherRatingScore.Text = teacher.Rating.ToString("F1");
            else
                TeacherRatingScore.Text = "חדש";

            TodaysDateText.Text = DateTime.Now.ToString("(dd/MM/yyyy)");

            var allLessons = lessonDB.GetLessonsByTeacher(teacher.Id);
            var todaysLessons = allLessons.Where(l => l.StartTime.Date == DateTime.Today).OrderBy(l => l.StartTime).ToList();

            TodaysLessonsGrid.ItemsSource = todaysLessons;

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

        // הפונקציה החדשה של הטסטים
        private void ManageTests_Click(object sender, RoutedEventArgs e)
        {
            if (currentUser is Teacher teacher)
            {
                ManageTests page = new ManageTests(teacher);
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