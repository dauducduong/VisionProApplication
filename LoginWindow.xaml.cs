using System;
using System.Windows;
using System.Windows.Controls;


namespace VisionProApplication
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public bool IsAuthenticated { get; private set; }
        public LoginWindow()
        {
            InitializeComponent();
            txtUsername.Focus();
        }


        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Password;

            if (username == "admin" && password == "admin")
            {
                IsAuthenticated = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Username or password is not correct", "Login In Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
