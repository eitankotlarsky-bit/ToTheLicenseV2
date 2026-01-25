using Model;
using NavigationApp;
using System.Windows;
using System.Windows.Controls;
using WpfApp1;

namespace LeftMenuWPF
{
    public partial class MainWindow : Window
    {
        private static User loggedInUser;

        public static User LoggedInUser
        {
            get { return loggedInUser; }
            set { loggedInUser = value; }
        }

        public MainWindow(User user)
        {
            
            InitializeComponent();

            LoggedInUser = user;


            MainFrame.Navigate(new HomePage());
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new HomePage());
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new UserPage());
        }

        private void EditTeacher_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new TeacherEditPage());
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new StudentListPage());
        }
    }
}
