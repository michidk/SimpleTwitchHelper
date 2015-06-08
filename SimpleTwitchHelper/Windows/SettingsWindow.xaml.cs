using System.Windows;

namespace SimpleTwitchHelper.Windows
{
    /// <summary>
    ///     Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private bool showAdControl;

        public SettingsWindow()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            var cfg = Globals.Config;

            showAdControl = cfg.ShowAdControl;

            AlertCheckBox.IsChecked = cfg.ShowAlert;
            SoundCheckBox.IsChecked = cfg.PlaySound;
            CountingMessageBox.Text = cfg.CountingMessage;
            FinishedMessageBox.Text = cfg.FinishedMessage;
            CountdownPathBox.Text = Globals.CountdownFile;

            StartUpExecutablesBox.Text = cfg.StartupExecutables;

            var left = cfg.LeftHotbarButton;
            LeftButtonTextBox.Text = left.Text;
            LeftButtonPathBox.Text = left.Executable;
            var middle = cfg.MiddleHotbarButton;
            MiddleButtonTextBox.Text = middle.Text;
            MiddleButtonPathBox.Text = middle.Executable;
            var right = cfg.RightHotbarButton;
            RightButtonTextBox.Text = right.Text;
            RightButtonPathBox.Text = right.Executable;

            UpdateContent();
        }

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            var cfg = Globals.Config;

            cfg.ShowAdControl = showAdControl;

            if (AlertCheckBox.IsChecked != null)
            {
                cfg.ShowAlert = (bool) AlertCheckBox.IsChecked;
            }
            if (SoundCheckBox.IsChecked != null)
            {
                cfg.PlaySound = (bool)SoundCheckBox.IsChecked;
            }
            cfg.CountingMessage = CountingMessageBox.Text;
            cfg.FinishedMessage = FinishedMessageBox.Text;

            cfg.StartupExecutables = StartUpExecutablesBox.Text;

            cfg.LeftHotbarButton.Text = LeftButtonTextBox.Text;
            cfg.LeftHotbarButton.Executable = LeftButtonPathBox.Text;
            cfg.MiddleHotbarButton.Text = MiddleButtonTextBox.Text;
            cfg.MiddleHotbarButton.Executable = MiddleButtonPathBox.Text;
            cfg.RightHotbarButton.Text = RightButtonTextBox.Text;
            cfg.RightHotbarButton.Executable = RightButtonPathBox.Text;

            cfg.Save();
            Globals.Logger.Log("Settings succesfully saved!");
            Close();
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void LogoutButtonClick(object sender, RoutedEventArgs e)
        {
            Helper.Logout();
        }

        private void ResetSettingsButtonClick(object sender, RoutedEventArgs e)
        {
            Helper.ResetSettings();
        }

        private void CopyPathButtonClick(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Globals.CountdownFile);
        }

        private void AdControlToggleButtonClick(object sender, RoutedEventArgs e)
        {
            showAdControl = !showAdControl;
            UpdateContent();
        }

        private void UpdateContent()
        {
            AdControlToggleButton.Content = showAdControl ? "Show Viewer-List" : "Show Ad-Control";
        }
    }
}