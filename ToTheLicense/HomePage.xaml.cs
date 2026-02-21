using System;
using System.Linq;
using System.Windows;
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

            // 1. עדכון בר התקדמות
            int lessonsDone = lessonDB.GetCompletedLessonsCount(student.Id);
            LicenseProgress.Value = lessonsDone;
            ProgressText.Text = $"{lessonsDone}/28 שיעורים בוצעו";

            // 2. עדכון ה-Roadmap (הפיצ'ר החדש!)
            UpdateRoadmap(student);

            // 3. בדיקה אם המשתמש זכאי לדרג את המורה שלו
            CheckForRateableTeacher(student.Id);
        }

        private void UpdateRoadmap(Student student)
        {
            TestDB testDB = new TestDB();

            // צבעים מוגדרים מראש
            var greenBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50")); // ירוק הצלחה
            var grayBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0"));  // אפור
            var orangeBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9800")); // כתום ממתין
            var lightGreenBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C8E6C9")); // רקע ירוק בהיר

            // === שלב 1: תיאוריה ===
            if (student.PassedTheory)
            {
                StepTheory.Background = lightGreenBrush;
                TxtTheoryStatus.Text = "עבר בהצלחה!";
                TxtTheoryStatus.Foreground = greenBrush;
                Line1.Fill = greenBrush; // צביעת הקו לשלב הבא
            }
            else
            {
                StepTheory.Background = grayBrush;
                TxtTheoryStatus.Text = "טרם בוצע";
                TxtTheoryStatus.Foreground = Brushes.Gray;
            }

            // === שלב 2: טסט פנימי ===
            TestEntity internalTest = testDB.GetLastTest(student.Id, TestEntity.TYPE_INTERNAL);
            bool internalPassed = false;

            if (internalTest != null && internalTest.Status == TestEntity.STATUS_PASSED)
            {
                StepInternal.Background = lightGreenBrush;
                TxtInternalStatus.Text = "עבר בהצלחה!";
                TxtInternalStatus.Foreground = greenBrush;
                Line2.Fill = greenBrush; // צביעת הקו לשלב הבא
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
                // אם עבר תיאוריה, השלב הזה נפתח (נצבע בלבן או אפור בהיר) אבל עדיין לא הושלם
                StepInternal.Background = student.PassedTheory ? Brushes.White : grayBrush;
                StepInternal.BorderBrush = student.PassedTheory ? Brushes.Gray : Brushes.Transparent;
                StepInternal.BorderThickness = student.PassedTheory ? new Thickness(1) : new Thickness(0);

                TxtInternalStatus.Text = student.PassedTheory ? "ממתין למורה" : "נעול";
            }

            // === שלב 3: טסט חיצוני (היעד!) ===
            TestEntity externalTest = testDB.GetLastTest(student.Id, TestEntity.TYPE_EXTERNAL);

            BtnRequestTest.Visibility = Visibility.Collapsed;
            TxtExternalStatus.Visibility = Visibility.Visible;

            if (externalTest != null)
            {
                if (externalTest.Status == TestEntity.STATUS_REQUESTED)
                {
                    BtnRequestTest.Visibility = Visibility.Visible;
                    BtnRequestTest.Content = "ממתין לאישור";
                    BtnRequestTest.IsEnabled = false; // אי אפשר לבקש שוב
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
                    StepExternal.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEBEE")); // אדום בהיר
                    TxtExternalStatus.Text = "נכשל, נסה שוב";
                    TxtExternalStatus.Foreground = Brushes.Red;

                    // אפשר לבקש שוב
                    BtnRequestTest.Visibility = Visibility.Visible;
                    BtnRequestTest.IsEnabled = true;
                    BtnRequestTest.Content = "בקש שוב";
                    TxtExternalStatus.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                // אם אין טסט חיצוני, נבדוק אם אפשר לבקש
                // התנאי: עבר תיאוריה + עבר טסט פנימי + השלים מינימום שיעורים (אופציונלי)
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

            // משיגים את המורה האחרון שלימד אותו כדי לשייך את הבקשה
            Lesson lastLesson = new LessonDB().GetLastCompletedLesson(s.Id);

            // אם אין שיעור שהושלם, ננסה למצוא את המורה המשויך בדרך אחרת (למשל שיעור עתידי)
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
                TestDate = DateTime.Now.AddDays(14), // תאריך ברירת מחדל (המורה ישנה את זה)
                Notes = "בקשת תלמיד מהאפליקציה"
            };

            testDB.Insert(newTest);
            testDB.SaveChanges();

            MessageBox.Show("בקשתך לטסט חיצוני נשלחה למורה בהצלחה!\nהמורה יעדכן את התאריך ויאשר את הבקשה.");
            UpdateRoadmap(s); // רענון מיידי של המסך
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

            // רענון הדירוג מהדאטה-בייס
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