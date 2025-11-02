using System;
using System.Data;
using System.Data.OleDb;
using System.Windows;

namespace ToTheLicense
{
    public partial class FindTeacher : Window
    {
        private OleDbConnection connection;

        public FindTeacher()
        {
            InitializeComponent();
            LoadTeachers();
        }

        private void LoadTeachers(string locationFilter = "", string vehicleFilter = "")
        {
            try
            {
                string connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=..\..\..\ViewModel\DataBase\Database11.accdb;";
                connection = new OleDbConnection(connectionString);
                connection.Open();

                string query = @"
                    SELECT 
                        t.id,
                        (u.FirstName & ' ' & u.LastName) AS FullName,
                        t.Location,
                        t.VehicleType,
                        t.LessonPrice
                    FROM tblTeacher AS t
                    INNER JOIN tblUsers AS u ON t.id = u.id
                    WHERE (t.Location LIKE @Location OR @Location = '')
                      AND (t.VehicleType LIKE @Vehicle OR @Vehicle = '');
                ";

                OleDbCommand command = new OleDbCommand(query, connection);
                command.Parameters.AddWithValue("@Location", "%" + locationFilter + "%");
                command.Parameters.AddWithValue("@Vehicle", "%" + vehicleFilter + "%");

                OleDbDataAdapter adapter = new OleDbDataAdapter(command);
                DataTable table = new DataTable();
                adapter.Fill(table);

                TeachersList.ItemsSource = table.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("שגיאה בטעינת המורים: " + ex.Message);
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string location = LocationTextBox.Text.Trim();
            string vehicle = VehicleTextBox.Text.Trim();
            LoadTeachers(location, vehicle);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            HomePage homePage = new HomePage();
            homePage.Show();
            this.Close();
        }
    }
}
