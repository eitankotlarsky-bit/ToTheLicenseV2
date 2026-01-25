using LeftMenuWPF;
using Model;
using System;
using System.Runtime.Remoting;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for HomePage.xaml
    /// </summary>
    public partial class HomePage : Page
    {
        public HomePage()
        {
            InitializeComponent();
            StartGradientAnimation();



            tbWelcomeMessage.Text = MainWindow.LoggedInUser.FirstName + " " + MainWindow.LoggedInUser.LastName;

            if (MainWindow.LoggedInUser is Teacher)
                tbWelcomeMessage.Text += Environment.NewLine + "is Teacher";

        }
       



        private void StartGradientAnimation()
        {
            // Animate the two GradientStop colors to shift over time
            var gs0 = (LinearGradientBrush)FindResource("AnimatedBackground");
            if (gs0 == null) return;


            var g0 = gs0.GradientStops[0];
            var g1 = gs0.GradientStops[1];


            ColorAnimation cAnim1 = new ColorAnimation()
            {
                From = (Color)ColorConverter.ConvertFromString("#FF0B486B"),
                To = (Color)ColorConverter.ConvertFromString("#FF2E8B57"),
                Duration = TimeSpan.FromSeconds(6),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };


            ColorAnimation cAnim2 = new ColorAnimation()
            {
                From = (Color)ColorConverter.ConvertFromString("#FF3B8D99"),
                To = (Color)ColorConverter.ConvertFromString("#FFFF6F61"),
                Duration = TimeSpan.FromSeconds(8),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };


            g0.BeginAnimation(GradientStop.ColorProperty, cAnim1);
            g1.BeginAnimation(GradientStop.ColorProperty, cAnim2);
        }


    }
}
