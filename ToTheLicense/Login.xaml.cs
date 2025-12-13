using System; // הוספנו using System
using System.Windows;
using System.Windows.Input;
using Model;
using ViewModel;

namespace ToTheLicense
{
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed) DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void RegisterLink_Click(object sender, RoutedEventArgs e)
        {
            Register registerWindow = new Register();
            registerWindow.Show();
            this.Close();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordInputBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ErrorMessageTextBlock.Text = "נא להזין שם משתמש וסיסמה";
                return;
            }

            ErrorMessageTextBlock.Text = "";

            try
            {
                // 1. בדיקת מורה
                TeacherDB teacherDB = new TeacherDB();
                Teacher teacher = teacherDB.Login(username, password);

                if (teacher != null)
                {
                    HomePage homePage = new HomePage(teacher);
                    homePage.Show();
                    this.Close();
                    return;
                }

                // 2. בדיקת תלמיד
                StudentDB studentDB = new StudentDB();
                Student student = studentDB.Login(username, password);

                if (student != null)
                {
                    HomePage homePage = new HomePage(student);
                    homePage.Show();
                    this.Close();
                    return;
                }

                ErrorMessageTextBlock.Text = "שם משתמש או סיסמה שגויים";
            }
            catch (Exception ex)
            {
                // זה יראה לך אם הבעיה היא בנתיב לדאטה בייס
                MessageBox.Show("שגיאת התחברות לדאטה בייס:\n" + ex.Message, "שגיאה", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}