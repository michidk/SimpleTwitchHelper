using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace SimpleTwitchHelper.Windows
{
    /// <summary>
    ///     Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public string Authkey;

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void QuitButtonClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void LoginButtonClick(object sender, RoutedEventArgs e)
        {
            Authkey = AuthKeyBox.Password;
            this.DialogResult = true;
            this.Close();
        }

        private void OpenLinkLabelClick(object sender, MouseButtonEventArgs e)
        {
            Process.Start(Globals.CreateAuthKeyLink);
        }
    }
}