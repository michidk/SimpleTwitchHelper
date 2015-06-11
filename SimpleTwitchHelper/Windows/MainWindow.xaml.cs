using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using SimpleLoggingSystem;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using MessageBox = System.Windows.MessageBox;
using Timer = System.Timers.Timer;

namespace SimpleTwitchHelper.Windows
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool CountdownRunning;
        public int SecondsToCount;

        private bool hidden;
        private string timeString;
        private Timer timer;

        public MainWindow()
        {
            Closed += WindowClosed;

            InitializeComponent();
            InitWindow();
        }

        private void InitWindow()
        {
            //setup webbrowser
            Helper.SetSilent(TwitchChatBrowser, true);
            TwitchChatBrowser.Visibility = Visibility.Hidden;
            TwitchChatBrowser.Navigated += Navigated;
            TwitchChatBrowser.Navigate(String.Format(Globals.ChatPopupUrl, Globals.Status.Username));
  
            timer = new Timer(1000);
            timer.Elapsed += TimerTick;

            new CustomLog(LogBox);

            var channel = Globals.Client.GetMyChannel();
            StreamTitleBox.Text = channel.Status;
            GameNameBox.Text = channel.Game;
            WelcomeLabel.Content = String.Format(WelcomeLabel.Content.ToString(), Globals.Status.Displayname);

            UpdateHotbarButtons();
            UpdateAdControlVisibility();

            CreateCountdownFile();
        }

        private void Navigated(object sender, NavigationEventArgs navigationEventArgs)
        {
            TwitchChatBrowser.Visibility = Visibility.Visible;
        }

        private static void CreateCountdownFile()
        {
            File.Create(Globals.CountdownFile).Close();
        }

        private void CountdownValidation(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[0-9,:]");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void StartCountdownClick(object sender, RoutedEventArgs e)
        {
            if (CountdownRunning)
            {
                StopCountdown();
            }
            else
            {
                InitCountdown();
            }
        }

        public void InitCountdown()
        {
            var text = CountdownBox.Text;
            var fullTime = new Regex("^[0-9]+:[0-5][0-9]$").IsMatch(text);
            var onlyMinutes = new Regex("^[0-9]+$").IsMatch(text);

            if (!fullTime && !onlyMinutes)
            {
                MessageBox.Show("Time format invalid. Has to be: XX:XX");
            }
            else if (onlyMinutes)
            {
                var min = int.Parse(text);
                StartCountdown(min * 60);
            }
            else
            {
                try
                {
                    var splitted = text.Split(':');
                    var minS = splitted[0];
                    var secS = splitted[1];
                    var min = int.Parse(minS);
                    var sec = int.Parse(secS);

                    StartCountdown(min * 60 + sec);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Time format invalid. Has to be: XX:XX [Minutes:Seconds]\nError: " + ex.Message);
                }
            }
        }

        public void StartCountdown(int sec)
        {
            if (sec == 0)
            {
                MessageBox.Show("Time is 0");
                return;
            }

            CountdownRunning = true;

            SecondsToCount = sec;
            timer.Start();

            StartCountdownButton.Content = "Stop";
            Globals.Logger.Log("Started Countdown");
            timeString = GetTimeString(SecondsToCount);
            SaveToFile();
        }

        public void StopCountdown()
        {
            CountdownRunning = false;
            timer.Stop();

            StartCountdownButton.Content = "Start";
            Globals.Logger.Log("Stopped Countdown");
            SaveToFile();
        }

        private void TimerTick(object sender, ElapsedEventArgs e)
        {
            SecondsToCount--;

            Application.Current.Dispatcher.Invoke(() =>
            {
                bool finished = SecondsToCount <= 0;
                timeString = GetTimeString(SecondsToCount);
                CountdownBox.Text = timeString;

                if (finished)
                {
                    StopCountdown();
                }

                SaveToFile();

                if (finished)
                {
                    Globals.Logger.Log("Countdown Finished!");
                    if (Globals.Config.ShowAlert)
                    {
                        var image = MessageBoxImage.None;
                        if (Globals.Config.PlaySound)
                            image = MessageBoxImage.Warning;

                        MessageBox.Show("Countdown is over!", "Alert", MessageBoxButton.OK, image);
                    }
                }
            });
        }

        public void SaveToFile()
        {
            var text = CountdownRunning ? Globals.Config.CountingMessage.Replace("{time}", timeString) : Globals.Config.FinishedMessage;
            File.WriteAllText(Globals.CountdownFile, text);
        }

        public string GetTimeString(int secondsToCount)
        {
            var time = TimeSpan.FromSeconds(secondsToCount);
            var min = (int)Math.Floor(time.TotalMinutes);
            var sec = time.Seconds;
            return String.Format("{0:00}:{1:00}", min, sec);
        }

        private void GetDataButtonClick(object sender, RoutedEventArgs e)
        {
            var channel = Globals.Client.GetMyChannel();
            StreamTitleBox.Text = channel.Status;
            GameNameBox.Text = channel.Game;
            Globals.Logger.Log("Refreshed stream title & game");
        }

        private void SetDataButtonClick(object sender, RoutedEventArgs e)
        {
            Globals.Status.StreamTitle = StreamTitleBox.Text;
            Globals.Status.Game = GameNameBox.Text;
            Globals.Client.Update(StreamTitleBox.Text, GameNameBox.Text);
            Globals.Logger.Log("Updated title and game.");
        }

        private void SettingsButtonClick(object sender, RoutedEventArgs e)
        {
            var win = new SettingsWindow();
            win.ShowDialog();

            UpdateHotbarButtons();
            UpdateAdControlVisibility();
        }

        private void LogoutButtonClick(object sender, RoutedEventArgs e)
        {
            Helper.Logout();
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            Helper.Shutdown();
        }

        private void ClearTitleButtonClick(object sender, RoutedEventArgs e)
        {
            StreamTitleBox.Text = "";
        }

        private void ClearGameButtonClick(object sender, RoutedEventArgs e)
        {
            GameNameBox.Text = "";
        }

        private void LeftHotbarButtonClick(object sender, RoutedEventArgs e)
        {
            Execute(Globals.Config.LeftHotbarButton);
        }

        private void MiddleHotbarButtonClick(object sender, RoutedEventArgs e)
        {
            Execute(Globals.Config.MiddleHotbarButton);
        }

        private void RightHotbarButtonClick(object sender, RoutedEventArgs e)
        {
            Execute(Globals.Config.RightHotbarButton);
        }

        private void StatusBarDoubleClick(object sender, RoutedEventArgs e)
        {
            hidden = !hidden;
            if (hidden)
            {
                StatsBox.Visibility = Visibility.Hidden;
                if (!Globals.Config.ShowAdControl) ViewerList.Visibility = Visibility.Hidden;
            }
            else
            {
                StatsBox.Visibility = Visibility.Visible;
                if (!Globals.Config.ShowAdControl) ViewerList.Visibility = Visibility.Visible;
            }
        }

        private void Execute(HotbarButton button)
        {
            var path = button.Executable;
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }
            try
            {
                Process.Start(path);
            }
            catch (Exception e)
            {
                MessageBox.Show("Error executing " + path + ":\n" + e.Message);
                Globals.Logger.Log("Error executing " + path + ": " + e.Message, LogType.Error);
            }
        }

        private void UpdateHotbarButtons()
        {
            UpdateHotbarButton(ref LeftHotbarButton, Globals.Config.LeftHotbarButton);
            UpdateHotbarButton(ref MiddleHotbarButton, Globals.Config.MiddleHotbarButton);
            UpdateHotbarButton(ref RightHotbarButton, Globals.Config.RightHotbarButton);
        }

        private void UpdateHotbarButton(ref Button button, HotbarButton cfg)
        {
            button.Content = cfg.Text;
            if (String.IsNullOrWhiteSpace(cfg.Text))
            {
                button.Visibility = Visibility.Hidden;
            }
            else
            {
                button.Visibility = Visibility.Visible;
            }
        }

        private void UpdateAdControlVisibility()
        {
            if (Globals.Config.ShowAdControl)
            {
                ViewerList.Visibility = Visibility.Hidden;
                AdControl.Visibility = Visibility.Visible;
            }
            else
            {
                ViewerList.Visibility = Visibility.Visible;
                AdControl.Visibility = Visibility.Hidden;
            }
        }

        private void TriggerComercial30ButtonClick(object sender, RoutedEventArgs e)
        {
            TriggerComercial(30);
        }

        private void TriggerComercial60ButtonClick(object sender, RoutedEventArgs e)
        {
            TriggerComercial(60);
        }

        private void TriggerComercial90ButtonClick(object sender, RoutedEventArgs e)
        {
            TriggerComercial(90);
        }

        private void TriggerComercial120ButtonClick(object sender, RoutedEventArgs e)
        {
            TriggerComercial(120);
        }

        private void TriggerComercial150ButtonClick(object sender, RoutedEventArgs e)
        {
            TriggerComercial(150);
        }

        private void TriggerComercial180ButtonClick(object sender, RoutedEventArgs e)
        {
            TriggerComercial(180);
        }

        private void TriggerComercial(int length)
        {
            var resp = Globals.Client.TriggerCommercial(length);
            Globals.Logger.Log("Triggered Commercial! Length: " + length);
            if (!resp.WasSuccesfull())
            {
                Globals.Logger.Log(resp.Message, LogType.Warning);
            }
        }

        private void TweetButtonClick(object sender, RoutedEventArgs e)
        {
            Process.Start(Globals.TweetLink + ReplaceTemplate(Globals.Config.TwitterTemplate));
        }

        private string ReplaceTemplate(string template)
        {
            template = template.Replace("{channel}", Globals.Status.Displayname);
            template = template.Replace("{title}", Globals.Status.StreamTitle);
            template = template.Replace("{game}", Globals.Status.Game);
            template = template.Replace("{viewers}", Globals.Status.Viewers.ToString());
            if (Globals.Status != null) template = template.Replace("{link}", Globals.TwitchLink + Globals.Status.Username);

            return template;
        }
    }
}