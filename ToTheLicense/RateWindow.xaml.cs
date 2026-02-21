using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Model;
using ViewModel;

namespace ToTheLicense
{
    public partial class RateWindow : Window
    {
        private int _selectedRating = 0;
        private int _teacherId;
        private int _studentId;
        private RatingDB _ratingDB;

        // בנאי יחיד ומסודר שמקבל את כל הנתונים הנדרשים
        public RateWindow(string teacherName, DateTime lessonDate, int teacherId, int studentId)
        {
            InitializeComponent(); // חובה! טוען את העיצוב

            _teacherId = teacherId;
            _studentId = studentId;
            _ratingDB = new RatingDB();

            // עדכון הטקסטים בחלון
            TeacherNameText.Text = $"עם המורה: {teacherName}";

            // בדיקה למקרה שהאלמנט לא קיים ב-XAML הישן, למניעת קריסה
            if (LessonDateText != null)
            {
                LessonDateText.Text = $"השיעור מתאריך {lessonDate:dd/MM/yyyy}";
            }
        }

        private void Star_Click(object sender, RoutedEventArgs e)
        {
            Button clickedBtn = sender as Button;
            int rating = int.Parse(clickedBtn.Tag.ToString());
            _selectedRating = rating;

            UpdateStarsUI(rating);
            UpdateDescription(rating);
        }

        private void UpdateDescription(int rating)
        {
            // אם לא הוספת את הטקסט הזה ב-XAML, נדלג עליו כדי שלא יקרוס
            if (RatingDescription == null) return;

            switch (rating)
            {
                case 1: RatingDescription.Text = "מאכזב מאוד"; break;
                case 2: RatingDescription.Text = "לא משהו"; break;
                case 3: RatingDescription.Text = "בסדר גמור"; break;
                case 4: RatingDescription.Text = "טוב מאוד!"; break;
                case 5: RatingDescription.Text = "מצוין, נהניתי!"; break;
            }
        }

        private void UpdateStarsUI(int rating)
        {
            // שימוש בצבעים מוגדרים מראש
            var goldBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFC107")); // זהב
            var grayBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0")); // אפור

            Star1.Foreground = rating >= 1 ? goldBrush : grayBrush;
            Star2.Foreground = rating >= 2 ? goldBrush : grayBrush;
            Star3.Foreground = rating >= 3 ? goldBrush : grayBrush;
            Star4.Foreground = rating >= 4 ? goldBrush : grayBrush;
            Star5.Foreground = rating >= 5 ? goldBrush : grayBrush;
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedRating == 0)
            {
                MessageBox.Show("נא ללחוץ על הכוכבים כדי לדרג.");
                return;
            }

            try
            {
                TeacherRating newRating = new TeacherRating
                {
                    TeacherId = _teacherId,
                    StudentId = _studentId,
                    RatingValue = _selectedRating,
                    Comment = CommentBox.Text,
                    RatingDate = DateTime.Now
                };

                _ratingDB.AddRating(newRating);

                MessageBox.Show("תודה רבה! הדירוג שלך נקלט בהצלחה.");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("שגיאה בשמירת הדירוג: " + ex.Message);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}