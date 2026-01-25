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
using Model;
using LeftMenuWPF;

namespace NavigationApp
{
    /// <summary>
    /// Represents the login window for the application.
    /// Handles user authentication and navigation to the main window or registration window.
    /// </summary>
    public partial class Login : Window
    {
        /// <summary>
        /// Initializes a new instance of the Login window.
        /// </summary>
        public Login()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the click event for the login button.
        /// Validates user input and attempts to authenticate the user.
        /// </summary>
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Clear any previous error messages
            ErrorMessageTextBlock.Text = "";

            // Get values from the input fields
            string username = UsernameTextBox.Text;
            string password = PasswordInputBox.Password; // Use .Password for PasswordBox

            // Example validation:
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ErrorMessageTextBlock.Text = "Username and password are required.";
            }
            else if (password.Length > 15)
            {
                ErrorMessageTextBlock.Text = "Password must not be longer than 15 characters.";
                PasswordInputBox.Focus();
            }
            else
            {
                TeacherDB tdb = new TeacherDB();
                Teacher teacher = tdb.Login(username, password);

                if (teacher != null)
                {
                    MainWindow mainWindow = new MainWindow(teacher);
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    ErrorMessageTextBlock.Text = "Invalid username or password. try yaniv/123";
                }
            }
        }

        /// <summary>
        /// Allows the borderless window to be draggable by handling the mouse left button down event.
        /// </summary>
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // This allows the user to drag the window by clicking anywhere on it
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        /// <summary>
        /// Handles the click event for the custom close button.
        /// Closes the current window.
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close(); // Closes the current window
        }

        /// <summary>
        /// Handles the click event for the register button.
        /// Navigates to the registration window.
        /// </summary>
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterWindow();
            registerWindow.Show();
            this.Close();
        }
    }
}
