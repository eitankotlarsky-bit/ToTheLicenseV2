using System;
using System.Windows;
using System.Windows.Controls;
using Model;
using ViewModel;

namespace ToTheLicense
{
    public partial class ManageLessons : Window
    {
        private Teacher currentTeacher;
        private LessonDB lessonDB;

        // הגדרת הסטטוסים
        private const int STATUS_PENDING = 5;
        private const int STATUS_APPROVED = 6;
        private const int STATUS_CANCELLED = 7;
        private const int STATUS_COMPLETED = 8;

        public ManageLessons(Teacher teacher)
        {
            InitializeComponent();
            this.currentTeacher = teacher;
            this.lessonDB = new LessonDB();

            LoadLessons();
        }

        private void LoadLessons()
        {
            if (currentTeacher != null)
            {
                // שליפת הנתונים וחיבורם למסך
                LessonList lessons = lessonDB.GetLessonsByTeacher(currentTeacher.Id);
                LessonsList.ItemsSource = lessons;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            HomePage home = new HomePage(currentTeacher);
            home.Show();
            this.Close();
        }

        private void Approve_Click(object sender, RoutedEventArgs e)
        {
            UpdateLessonStatus(sender, STATUS_APPROVED);
        }

        private void Reject_Click(object sender, RoutedEventArgs e)
        {
            UpdateLessonStatus(sender, STATUS_CANCELLED);
        }

        // ... (אותו קוד) ...

        private void UpdateLessonStatus(object sender, int newStatus)
        {
            try
            {
                Button btn = sender as Button;
                if (btn != null && btn.Tag is Lesson lesson)
                {
                    lesson.Status = newStatus;

                    // עדכון טקסט לתצוגה מיידית (למרות שהדאטה בייס יעדכן גם כן)
                    if (newStatus == STATUS_APPROVED) lesson.StatusName = "מאושר";
                    if (newStatus == STATUS_CANCELLED) lesson.StatusName = "נדחה";
                    if (newStatus == STATUS_COMPLETED) lesson.StatusName = "בוצע"; // חדש

                    lessonDB.Update(lesson);
                    int rows = lessonDB.SaveChanges();

                    if (rows > 0)
                    {
                        LoadLessons();
                    }
                    else
                    {
                        MessageBox.Show("אירעה שגיאה בשמירה.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("שגיאה: " + ex.Message);
            }
        }
    }
}
        