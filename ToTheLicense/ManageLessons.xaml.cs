using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Windows;

namespace ToTheLicense
{
    public partial class ManageLessons : Window
    {
        private OleDbConnection connection;

        public ManageLessons()
        {
            InitializeComponent();
            LoadLessons();
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            HomePage homePage = new HomePage();
            homePage.Show();
            this.Close();
        }

        private void LoadLessons()
        {
            try
            {
                string connectionString =
                    @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=..\..\..\ViewModel\DataBase\Database11.accdb;";
                connection = new OleDbConnection(connectionString);
                connection.Open();

                // שליפת שיעורים כולל שם התלמיד
                string query = @"SELECT L.*, U.FirstName, U.LastName 
                                 FROM tblLessons AS L 
                                 INNER JOIN tblUsers AS U 
                                 ON L.StudentID = U.id 
                                 WHERE L.TeacherID = 1";

                OleDbCommand command = new OleDbCommand(query, connection);
                OleDbDataReader reader = command.ExecuteReader();

                List<dynamic> lessons = new List<dynamic>();

                while (reader.Read())
                {
                    var l = new
                    {
                        Id = Convert.ToInt32(reader["id"]),
                        StudentID = Convert.ToInt32(reader["StudentID"]),
                        StudentName = $"{reader["FirstName"]} {reader["LastName"]}",
                        TeacherID = Convert.ToInt32(reader["TeacherID"]),
                        Location = reader["Location"].ToString(),
                        Price = Convert.ToDouble(reader["Price"]),
                        Status = reader["Status"].ToString(),
                        Notes = reader["Notes"].ToString(),
                        StartTime = Convert.ToDateTime(reader["StartTime"]),
                        EndTime = Convert.ToDateTime(reader["EndTime"]),
                        VehicleType = reader["VehicleType"].ToString()
                    };
                    lessons.Add(l);
                }

                LessonsList.ItemsSource = lessons;
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("שגיאה בטעינת שיעורים: " + ex.Message);
            }
            finally
            {
                if (connection != null)
                    connection.Close();
            }
        }
    }
}
