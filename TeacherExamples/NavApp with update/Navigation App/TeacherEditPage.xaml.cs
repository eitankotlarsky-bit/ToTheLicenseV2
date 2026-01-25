using LeftMenuWPF;
using Model;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace NavigationApp
{
    /// <summary>
    /// Interaction logic for TeacherEditPage.xaml
    /// </summary>
    public partial class TeacherEditPage : Page
    {
        private Teacher currentTeacher;

        public TeacherEditPage()
        {
            InitializeComponent();
            Loaded += TeacherEditPage_Loaded;

            Teacher teacher = (Teacher)MainWindow.LoggedInUser;

            DataContext = teacher;
        }

        private void TeacherEditPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadTeacher();
        }

        /// <summary>
        /// Loads the currently logged-in teacher from <see cref="MainWindow.LoggedInUser"/>, 
        /// populates the page controls with the teacher's data, and sets UI state
        /// when no teacher is available.
        /// </summary>
        /// <remarks>
        /// Pseudocode / detailed plan:
        /// 1. Attempt to obtain the logged-in user as a Teacher using pattern matching:
        ///    - if MainWindow.LoggedInUser is a Teacher assign to local variable 'teacher'
        ///    - else treat as no teacher available
        /// 2. If a teacher is available:
        ///    - set 'currentTeacher' field to the teacher reference
        ///    - populate each UI control from the teacher model:
        ///        - UsernameTextBox.Text <- teacher.UserName (use empty string if null)
        ///        - PasswordBox.Password <- teacher.Password (use empty string if null)
        ///        - EmailTextBox.Text <- teacher.Email (use empty string if null)
        ///        - FirstNameTextBox.Text <- teacher.FirstName (use empty string if null)
        ///        - LastNameTextBox.Text <- teacher.LastName (use empty string if null)
        ///        - StartWorkDatePicker.SelectedDate <- teacher.StartWorkDate or null when default(DateTime)
        ///    - clear any previous error message and enable Save button
        /// 3. If no teacher available:
        ///    - set currentTeacher to null
        ///    - set an error message in ErrorMessageTextBlock
        ///    - disable the Save button
        ///    - clear input controls to avoid showing stale data
        /// 4. Use pattern matching to avoid InvalidCastException and redundant checks.
        /// 5. Keep UI state consistent (enabled/disabled/message) for downstream logic.
        /// </remarks>
        private void LoadTeacher()
        {
            // Use safe pattern matching instead of an explicit cast + redundant type check.
            if (MainWindow.LoggedInUser is Teacher teacher)
            {
                currentTeacher = teacher;

                // Populate UI with current values, guarding against null model properties.
                UsernameTextBox.Text = teacher.UserName ?? string.Empty;
                PasswordBox.Password = teacher.Password ?? string.Empty;
                EmailTextBox.Text = teacher.Email ?? string.Empty;
                FirstNameTextBox.Text = teacher.FirstName ?? string.Empty;
                LastNameTextBox.Text = teacher.LastName ?? string.Empty;

                // Convert default(DateTime) (0001-01-01) to null so DatePicker appears empty.
                StartWorkDatePicker.SelectedDate =
                    teacher.StartWorkDate == default(DateTime) ? (DateTime?)null : teacher.StartWorkDate;

                // Clear previous error state and enable saving.
                ErrorMessageTextBlock.Text = string.Empty;
                SaveButton.IsEnabled = true;
            }
            else
            {
                // No teacher available — clear model reference, reset UI and disable save.
                currentTeacher = null;

                ErrorMessageTextBlock.Text = "No teacher loaded for editing.";
                SaveButton.IsEnabled = false;

                // Clear controls to avoid displaying stale values.
                UsernameTextBox.Text = string.Empty;
                PasswordBox.Password = string.Empty;
                EmailTextBox.Text = string.Empty;
                FirstNameTextBox.Text = string.Empty;
                LastNameTextBox.Text = string.Empty;
                StartWorkDatePicker.SelectedDate = null;
            }
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
            if (password.Length > 15)
            {
                errorMessage = "Password must not be longer than 15 characters.";
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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
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

            if (currentTeacher == null)
            {
                ErrorMessageTextBlock.Text = "No teacher loaded.";
                return;
            }

            // Update model with UI values
            currentTeacher.UserName = UsernameTextBox.Text.Trim();
            currentTeacher.Password = PasswordBox.Password;
            currentTeacher.Email = EmailTextBox.Text.Trim();
            currentTeacher.FirstName = FirstNameTextBox.Text.Trim();
            currentTeacher.LastName = LastNameTextBox.Text.Trim();
            currentTeacher.StartWorkDate = StartWorkDatePicker.SelectedDate.Value;

            // Persist changes:
            // The project has a ViewModel/TeacherDB. If you want to save to DB,
            // add the appropriate save call here (e.g. TeacherDB.Update or TeacherDB.Insert + SaveChanges).
            // Example placeholder (uncomment and adjust if methods exist):
            var db = new ViewModel.TeacherDB();
            db.Update(currentTeacher); 
            db.SaveChanges();

            MessageBox.Show("Teacher details saved.");

            // Navigate back to the user page
            NavigationService?.Navigate(new UserPage());
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Discard changes and return to previous page
            NavigationService?.Navigate(new UserPage());
        }
    }
}