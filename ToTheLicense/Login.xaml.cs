using System;
using System.Data.OleDb;
using System.Windows;

namespace ToTheLicense
{
    public partial class Login : Window
    {
        private OleDbConnection connection;

        public Login()
        {
            InitializeComponent();
            string connectionString =
                @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=DataBase\Database11.accdb;Persist Security Info=True;";
            connection = new OleDbConnection(connectionString);
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordInputBox.Password; // לא חובה Trim בסיסמאות

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ErrorMessageTextBlock.Text = "Please enter both username and password.";
                return;
            }

            try
            {
                if (connection.State != System.Data.ConnectionState.Open)
                    connection.Open();

                string query =
                    "SELECT ID, Username, [Password], FirstName " +
                    "FROM tblUsers WHERE Username = ? AND [Password] = ?";

                using (OleDbCommand cmd = new OleDbCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("?", username);
                    cmd.Parameters.AddWithValue("?", password);

                    using (OleDbDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader != null && reader.Read())
                        {
                            ErrorMessageTextBlock.Text = string.Empty;

                            string firstName = reader["FirstName"]?.ToString();
                            string displayName = string.IsNullOrWhiteSpace(firstName)
                                ? (reader["Username"]?.ToString() ?? "User")
                                : firstName;

                            // בלי MessageBox — נכנסים ישר לבית
                            HomePage homePage = new HomePage(displayName);
                            homePage.Show();
                            this.Close();
                            return;
                        }
                        else
                        {
                            ErrorMessageTextBlock.Text = "Invalid username or password.";
                            // אופציונלי: ניקוי סיסמה לאחר כישלון
                            PasswordInputBox.Clear();
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessageTextBlock.Text = "Database connection error: " + ex.Message;
                return;
            }
            finally
            {
                if (connection.State == System.Data.ConnectionState.Open)
                    connection.Close();
            }
        }


        private void CloseButton_Click(object sender, RoutedEventArgs e) => this.Close();

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) => this.DragMove();
    }
}
