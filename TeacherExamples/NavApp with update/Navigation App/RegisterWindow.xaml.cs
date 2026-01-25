using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ViewModel;

namespace NavigationApp
{
    /// <summary>
    /// Interaction logic for RegisterWindow.xaml
    /// </summary>
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private bool ValidateFields(out string errorMessage, out Control focusControl)
        {
            errorMessage = "";
            focusControl = null;

            string username = UsernameTextBox.Text.Trim();
            string password = PasswordBox.Password;
            string email = EmailTextBox.Text.Trim();
            string firstName = FirstNameTextBox.Text.Trim();
            string lastName = LastNameTextBox.Text.Trim();
            DateTime? startWorkDate = StartWorkDatePicker.SelectedDate;

            if (string.IsNullOrWhiteSpace(username))
            {
                errorMessage = "Username is required.";
                focusControl = UsernameTextBox;
                return false;
            }
            if (string.IsNullOrWhiteSpace(password))
            {
                errorMessage = "Password is required.";
                focusControl = PasswordBox;
                return false;
            }
            if (string.IsNullOrWhiteSpace(email))
            {
                errorMessage = "Email is required.";
                focusControl = EmailTextBox;
                return false;
            }
            if (string.IsNullOrWhiteSpace(firstName))
            {
                errorMessage = "First name is required.";
                focusControl = FirstNameTextBox;
                return false;
            }
            if (string.IsNullOrWhiteSpace(lastName))
            {
                errorMessage = "Last name is required.";
                focusControl = LastNameTextBox;
                return false;
            }
            if (!startWorkDate.HasValue)
            {
                errorMessage = "Start work date is required.";
                focusControl = StartWorkDatePicker;
                return false;
            }

            return true;
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorMessageTextBlock.Text = "";

            string errorMessage;
            Control focusControl;
            if (!ValidateFields(out errorMessage, out focusControl))
            {
                ErrorMessageTextBlock.Text = errorMessage;
                focusControl?.Focus();
                return;
            }
            else
            {
                // All fields are valid, continue with registration logic here
                string username = UsernameTextBox.Text.Trim();
                string password = PasswordBox.Password;
                string email = EmailTextBox.Text.Trim();
                string firstName = FirstNameTextBox.Text.Trim();
                string lastName = LastNameTextBox.Text.Trim();
                DateTime startWorkDate = StartWorkDatePicker.SelectedDate.Value;

                // Example: Save the new teacher to a data store (pseudo code)
                // Replace with actual repository and constructor as needed
                var teacher = new Teacher
                {
                    UserName = username,
                    Password = password,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    StartWorkDate = startWorkDate
                };

                TeacherDB db = new TeacherDB();
                db.Insert(teacher);
                db.SaveChanges();

                // Optionally, navigate to login or main window
                var loginWindow = new Login();
                loginWindow.Show();
                this.Close();
                
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new Login();
            loginWindow.Show();
            this.Close();
        }
    }
}
