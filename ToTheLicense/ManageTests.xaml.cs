using System;
using System.Windows;
using System.Windows.Controls;
using Model;
using ViewModel;

namespace ToTheLicense
{
    public partial class ManageTests : Window
    {
        private Teacher currentTeacher;
        private TestDB testDB;
        private StudentDB studentDB;

        public ManageTests(Teacher teacher)
        {
            InitializeComponent();
            this.currentTeacher = teacher;
            this.testDB = new TestDB();
            this.studentDB = new StudentDB();

            LoadStudentsComboBox();
            LoadTests();
        }

        private void LoadStudentsComboBox()
        {
            try
            {
                // טעינת רשימת התלמידים של המורה כדי שיוכל לקבוע להם טסט יזום
                StudentList students = studentDB.GetStudentsByTeacher(currentTeacher.Id);
                StudentsComboBox.ItemsSource = students;
                TestDatePicker.SelectedDate = DateTime.Today; // ברירת מחדל להיום
            }
            catch (Exception ex)
            {
                MessageBox.Show("שגיאה בטעינת התלמידים: " + ex.Message);
            }
        }

        private void LoadTests()
        {
            if (currentTeacher != null)
            {
                TestList tests = testDB.GetTestsByTeacher(currentTeacher.Id);
                TestsList.ItemsSource = tests;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            HomePage home = new HomePage(currentTeacher);
            home.Show();
            this.Close();
        }

        // ==========================================
        // יצירת טסט פנימי ביוזמת המורה
        // ==========================================
        private void CreateInternalTest_Click(object sender, RoutedEventArgs e)
        {
            if (StudentsComboBox.SelectedItem is Student selectedStudent && TestDatePicker.SelectedDate.HasValue)
            {
                try
                {
                    TestEntity newTest = new TestEntity
                    {
                        StudentId = selectedStudent.Id,
                        TeacherId = currentTeacher.Id,
                        TestType = TestEntity.TYPE_INTERNAL,
                        Status = TestEntity.STATUS_APPROVED, // כי המורה קובע אותו מראש
                        TestDate = TestDatePicker.SelectedDate.Value,
                        Notes = "טסט פנימי שנקבע על ידי המורה"
                    };

                    testDB.Insert(newTest);
                    int rows = testDB.SaveChanges();

                    if (rows > 0)
                    {
                        MessageBox.Show($"טסט פנימי לתלמיד {selectedStudent.FullName} נקבע בהצלחה!", "הצלחה", MessageBoxButton.OK, MessageBoxImage.Information);

                        // ניקוי הטופס ורענון הרשימה
                        StudentsComboBox.SelectedItem = null;
                        TestDatePicker.SelectedDate = DateTime.Today;
                        LoadTests();
                    }
                    else
                    {
                        MessageBox.Show("אירעה שגיאה בקביעת הטסט.", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("שגיאה חמורה: " + ex.Message, "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("נא לבחור תלמיד ותאריך חוקי לטסט.", "שדה חסר", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // ==========================================
        // טיפול בבקשות מהתלמידים (אישור / דחייה)
        // ==========================================
        private void Approve_Click(object sender, RoutedEventArgs e)
        {
            UpdateTestStatus(sender, TestEntity.STATUS_APPROVED, "הטסט אושר ונקבע במערכת.");
        }

        private void Reject_Click(object sender, RoutedEventArgs e)
        {
            UpdateTestStatus(sender, TestEntity.STATUS_REJECTED, "הבקשה לטסט נדחתה.");
        }

        // ==========================================
        // עדכון תוצאת טסט שהתקיים (עבר / נכשל)
        // ==========================================
        private void Pass_Click(object sender, RoutedEventArgs e)
        {
            UpdateTestStatus(sender, TestEntity.STATUS_PASSED, "מעולה! התלמיד עבר את הטסט והסטטוס עודכן אצלו.");
        }

        private void Fail_Click(object sender, RoutedEventArgs e)
        {
            UpdateTestStatus(sender, TestEntity.STATUS_FAILED, "עודכן כי התלמיד נכשל בטסט.");
        }

        // פונקציית עזר לביצוע השמירה במסד הנתונים
        private void UpdateTestStatus(object sender, int newStatus, string successMessage)
        {
            try
            {
                Button btn = sender as Button;
                if (btn != null && btn.Tag is TestEntity test)
                {
                    test.Status = newStatus;

                    testDB.Update(test);
                    int rows = testDB.SaveChanges();

                    if (rows > 0)
                    {
                        MessageBox.Show(successMessage, "עדכון מערכת", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadTests(); // רענון המסך לאחר הצלחה כדי להעלים את הכפתורים הישנים
                    }
                    else
                    {
                        MessageBox.Show("אירעה שגיאה בשמירה למסד הנתונים.", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("שגיאה חמורה: " + ex.Message, "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}