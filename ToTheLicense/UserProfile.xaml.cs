using System;
using System.Windows;
using Model;
using ViewModel;

namespace ToTheLicense
{
    public partial class Profile : Window
    {
        private User currentUser;

        public Profile(User user)
        {
            InitializeComponent();
            this.currentUser = user;
            LoadUserData();
        }

        private void LoadUserData()
        {
            // טעינת נתונים בסיסיים
            txtFirstName.Text = currentUser.FirstName;
            txtLastName.Text = currentUser.LastName;
            txtEmail.Text = currentUser.Email;
            txtPassword.Text = currentUser.Password;

            // יצירת ראשי תיבות לעיגול
            if (!string.IsNullOrEmpty(currentUser.FirstName) && !string.IsNullOrEmpty(currentUser.LastName))
                InitialsText.Text = $"{currentUser.FirstName[0]}{currentUser.LastName[0]}";

            // טעינת נתונים לפי סוג
            if (currentUser is Teacher teacher)
            {
                TeacherSection.Visibility = Visibility.Visible;
                txtLocation.Text = teacher.Location;
                txtVehicle.Text = teacher.VehicleType;
                txtPrice.Text = teacher.LessonPrice.ToString();
            }
            else if (currentUser is Student student)
            {
                StudentSection.Visibility = Visibility.Visible;
                cmbLicenseType.Text = student.LicenseType;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // עדכון האובייקט בזיכרון
                currentUser.FirstName = txtFirstName.Text;
                currentUser.LastName = txtLastName.Text;
                currentUser.Email = txtEmail.Text;
                currentUser.Password = txtPassword.Text;

                if (currentUser is Teacher teacher)
                {
                    teacher.Location = txtLocation.Text;
                    teacher.VehicleType = txtVehicle.Text;
                    if (double.TryParse(txtPrice.Text, out double price))
                        teacher.LessonPrice = price;

                    // שמירה למסד הנתונים
                    TeacherDB tdb = new TeacherDB();
                    tdb.Update(teacher);
                    tdb.SaveChanges();
                }
                else if (currentUser is Student student)
                {
                    student.LicenseType = cmbLicenseType.Text;

                    // שמירה למסד הנתונים
                    StudentDB sdb = new StudentDB();
                    sdb.Update(student);
                    sdb.SaveChanges();
                }

                MessageBox.Show("הפרופיל עודכן בהצלחה!", "שמירה", MessageBoxButton.OK, MessageBoxImage.Information);

                // חזרה לדף הבית (מרענן את הנתונים שם)
                HomePage home = new HomePage(currentUser);
                home.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("שגיאה בשמירת הנתונים: " + ex.Message, "תקלה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            // חזרה לדף הבית ללא שמירה
            HomePage home = new HomePage(currentUser);
            home.Show();
            this.Close();
        }
    }
}