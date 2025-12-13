using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Model;
using ViewModel;

namespace ToTheLicense
{
    public partial class Register : Window
    {
        public Register()
        {
            InitializeComponent();
        }

        // שינוי נראות שדות בהתאם לבחירה (מורה/תלמיד)
        private void UserType_Changed(object sender, RoutedEventArgs e)
        {
            if (StudentFields == null || TeacherFields == null) return;

            if (rbStudent.IsChecked == true)
            {
                StudentFields.Visibility = Visibility.Visible;
                TeacherFields.Visibility = Visibility.Collapsed;
            }
            else
            {
                StudentFields.Visibility = Visibility.Collapsed;
                TeacherFields.Visibility = Visibility.Visible;
            }
        }

        // אימות קלט מספרי למחיר
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void BackToLogin_Click(object sender, RoutedEventArgs e)
        {
            Login login = new Login();
            login.Show();
            this.Close();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            // בדיקות תקינות (Validations) - כאן נשאיר הודעות כי המשתמש צריך לתקן
            if (string.IsNullOrWhiteSpace(txtUser.Text) || string.IsNullOrWhiteSpace(txtPass.Password) ||
                string.IsNullOrWhiteSpace(txtFirstName.Text) || string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                MessageBox.Show("נא למלא את כל שדות החובה.", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                if (rbStudent.IsChecked == true)
                {
                    // === הרשמת תלמיד ===
                    StudentDB sdb = new StudentDB();
                    Student student = new Student
                    {
                        UserName = txtUser.Text,
                        Password = txtPass.Password,
                        FirstName = txtFirstName.Text,
                        LastName = txtLastName.Text,
                        Email = txtEmail.Text,
                        LicenseType = cmbLicenseType.Text,
                        LessonsCount = 0
                    };

                    sdb.Insert(student);

                    if (sdb.SaveChanges() > 0)
                    {
                        // הצלחה! בלי הודעה קופצת, פשוט עוברים למסך התחברות
                        BackToLogin_Click(null, null);
                    }
                    else
                    {
                        // כישלון טכני (נדיר)
                        MessageBox.Show("ההרשמה נכשלה, נא לנסות שוב.", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    // === הרשמת מורה ===
                    if (string.IsNullOrWhiteSpace(txtPrice.Text))
                    {
                        MessageBox.Show("נא להזין מחיר לשיעור.", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    TeacherDB tdb = new TeacherDB();
                    Teacher teacher = new Teacher
                    {
                        UserName = txtUser.Text,
                        Password = txtPass.Password,
                        FirstName = txtFirstName.Text,
                        LastName = txtLastName.Text,
                        Email = txtEmail.Text,
                        Location = txtLocation.Text,
                        VehicleType = txtVehicle.Text,
                        LessonPrice = double.Parse(txtPrice.Text)
                    };

                    tdb.Insert(teacher);

                    if (tdb.SaveChanges() > 0)
                    {
                        // הצלחה! מעבר חלק למסך התחברות
                        BackToLogin_Click(null, null);
                    }
                    else
                    {
                        MessageBox.Show("ההרשמה נכשלה, נא לנסות שוב.", "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                // שגיאה קריטית (כמו דאטה בייס מנותק) - חייבים להודיע למשתמש
                MessageBox.Show("אירעה שגיאה טכנית:\n" + ex.Message, "תקלה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}