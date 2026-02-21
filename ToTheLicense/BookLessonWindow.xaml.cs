using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Model;
using ViewModel;

namespace ToTheLicense
{
    public partial class BookLessonWindow : Window
    {
        private Teacher selectedTeacher;
        private Student currentStudent;
        private LessonDB lessonDB;

        public BookLessonWindow(Teacher teacher, Student student)
        {
            InitializeComponent();
            this.selectedTeacher = teacher;
            this.currentStudent = student;
            this.lessonDB = new LessonDB();

            // Populate UI
            TeacherNameText.Text = $"עם: {teacher.FullName}";

            // עדכון הדירוג בתצוגה (F1 מציג ספרה אחת אחרי הנקודה)
            TeacherRatingText.Text = $"דירוג: ⭐ {teacher.Rating:F1}";

            VehicleBox.Text = teacher.VehicleType;
            LocationBox.Text = teacher.Location;
            PriceText.Text = $"₪{teacher.LessonPrice}";

            // Prevent booking in the past
            LessonDatePicker.DisplayDateStart = DateTime.Today;
        }

        private void LessonDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LessonDatePicker.SelectedDate == null) return;

            TimeComboBox.Items.Clear();
            TimeComboBox.IsEnabled = true;
            DateTime date = LessonDatePicker.SelectedDate.Value;

            // Get existing lessons to calculate free slots
            List<Lesson> existing = lessonDB.GetLessonsByDate(selectedTeacher.Id, date);

            // Generate slots from 08:00 to 18:00
            for (int h = 8; h <= 18; h++)
            {
                DateTime start = date.Date.AddHours(h);
                DateTime end = start.AddMinutes(45);

                // Don't show past hours if today is selected
                if (date.Date == DateTime.Today && start < DateTime.Now) continue;

                // Check overlap
                bool busy = existing.Any(l => l.StartTime < end && l.EndTime > start);

                if (!busy)
                {
                    TimeComboBox.Items.Add(start.ToString("HH:mm"));
                }
            }

            if (TimeComboBox.Items.Count == 0)
            {
                TimeComboBox.Items.Add("אין פנוי");
                TimeComboBox.IsEnabled = false;
            }
            else
            {
                TimeComboBox.SelectedIndex = 0;
            }
        }

        private void Book_Click(object sender, RoutedEventArgs e)
        {
            // Validation
            if (LessonDatePicker.SelectedDate == null || !TimeComboBox.IsEnabled || TimeComboBox.SelectedItem == null)
            {
                MessageBox.Show("נא לבחור תאריך ושעה תקינים");
                return;
            }

            try
            {
                // 1. Calculate Times
                string timeStr = TimeComboBox.SelectedItem.ToString();
                if (timeStr == "אין פנוי") return;

                DateTime date = LessonDatePicker.SelectedDate.Value.Date;
                TimeSpan time = TimeSpan.Parse(timeStr);

                DateTime startTime = date.Add(time);
                DateTime endTime = startTime.AddMinutes(45);

                // 2. Create Lesson Object
                Lesson newLesson = new Lesson
                {
                    TeacherId = selectedTeacher.Id,
                    Location = LocationBox.Text, // לוקח את המיקום שהמשתמש יכול לערוך
                    Price = selectedTeacher.LessonPrice,
                    Status = 0,
                    Notes = "",
                    StartTime = startTime,
                    EndTime = endTime,
                    VehicleType = selectedTeacher.VehicleType
                };

                // 3. Save to DB
                lessonDB.BookLesson(newLesson, currentStudent.Id);
                int rows = lessonDB.SaveChanges();

                if (rows > 0)
                {
                    this.Close();
                }
                else
                {
                    MessageBox.Show("אירעה שגיאה בקביעת השיעור.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("שגיאה קריטית: " + ex.Message);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}