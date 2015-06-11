using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using SimpleConfigurationSystem;
using SimpleLoggingSystem;
using SimpleTwitchHelper.Windows;
using TwitchCSharp.Clients;
using static System.String;
using MSG = System.Windows.MessageBox;

namespace SimpleTwitchHelper
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Dispatcher MainDispatcher;

        private TrackedThread fetcherThread;
        private MainWindow mainWindow;

        private long startFollowers = -1;
        private long topViews;

        private void ApplicationStartup(object sender, StartupEventArgs e)
        {
            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            MainDispatcher = Application.Current.Dispatcher;



            Globals.Logger = new Logger(Globals.LogFile);
            CreateFolder();
            LoadedConfigurationResult<Config> result = Configuration<Config>.LoadConfig(CustomLog.LogWrapper);
            Globals.Config = result.Configuration;

            ProcessLogin();
            Globals.Logger.Log("Simple Twitch Helper Initialized");
        }

        private void CreateFolder()
        {
            DirectoryInfo dir  = Directory.CreateDirectory(Globals.MyFolderPath);
        }

        private void ProcessLogin()
        {
            if (!IsNullOrWhiteSpace(Globals.Config.AuthKey))
            {
                CheckAuthKey();
            }
            else
            {
                if (CheckForValidAuthKey())
                {
                    StartMain();
                }
                else
                {
                    Helper.Shutdown();
                }
            }
        }

        private void CheckAuthKey()
        {
            if (!Login(Globals.Config.AuthKey))
            {
                Globals.Config.AuthKey = "";
                ProcessLogin();
            }
            else
            {
                StartMain();

                ExecStartupPrograms();
            }
        }

        private bool CheckForValidAuthKey()
        {
            var validLogin = false;
            while (!validLogin)
            {
                bool? result = false;

                var win = new LoginWindow();
                result = win.ShowDialog();

                if ((result == null) || (result != true))
                {
                    return false;
                }

                if (Login(win.Authkey))
                {
                    validLogin = true;
                }
                else
                {
                    MSG.Show("You auth key is invalid", "Try again!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
            return true;
        }

        public bool Login(string authkey)
        {
            var tempClient = new TwitchAuthenticatedClient(authkey, Globals.ClientId);
            var user = tempClient.GetMyUser();

            if (user == null || IsNullOrWhiteSpace(user.Name))
            {
                return false;
            }
            Globals.Status.Username = user.Name;
            Globals.Status.Displayname = user.DisplayName;

            Globals.Client = new TwitchNamedClient(Globals.Status.Username, authkey, Globals.ClientId);

            Globals.Config.AuthKey = authkey;
            Globals.Config.Save();
            return true;
        }

        private void StartMain()
        {
            fetcherThread = new TrackedThread(FetchData);
            fetcherThread.Thread.Start();
            mainWindow = new MainWindow();
            mainWindow.Show();
        }

        public void ExecStartupPrograms()
        {
            var execs = Globals.Config.StartupExecutables;
            if (IsNullOrWhiteSpace(execs))
            {
                return;
            }

            var prgms = execs.Split(';');
            foreach (var s in prgms)
            {
                if (IsNullOrWhiteSpace(s))
                {
                    return;
                }

                try
                {
                    Process.Start(s);
                }
                catch (Exception e)
                {
                    MSG.Show("Error executing " + s + ":\n" + e.Message);
                    Globals.Logger.Log("Error executing " + s + ": " + e.Message, LogType.Error);
                }
            }
        }

        public void FetchData()
        {
            while (true)
            {
                var streamResult = Globals.Client.GetMyStream();
                if (streamResult == null)
                {
                    MSG.Show("Couldn't connect to the twitch-servers!");
                    return;
                }

                var stream = streamResult.Stream;   // streamResult.stream == null if not live
                if (stream != null)
                {
                    Globals.Status.Live = true;
                    Globals.Status.Viewers = stream.Viewers;
                    Globals.Status.AverageFps = stream.AverageFps;
                }
                else
                {
                    Globals.Status.Live = false;
                }

                var channel = Globals.Client.GetMyChannel();
                Globals.Status.StreamTitle = channel.Status;
                Globals.Status.Game = channel.Game;
                Globals.Status.Views = channel.Views;
                Globals.Status.Followers = channel.Followers;

                var subs = Globals.Client.GetSubscribers();
                if (subs != null)
                    Globals.Status.Subscribers = subs.Total;

                var response = Globals.TmiApi.GetResponse(Globals.Status.Username);
                Globals.Status.Chatters = response.ChatterCount;
                var cht = response.Chatters;
                var chatterList = new List<string>();
                chatterList.AddRange(cht.AdminList);
                chatterList.AddRange(cht.GlobalModList);
                chatterList.AddRange(cht.ModeratorList);
                chatterList.AddRange(cht.StaffList);
                chatterList.AddRange(cht.ViewerList);
                Globals.Status.ViewersList = chatterList;

                Dispatcher.Invoke(DispatcherPriority.Normal, (Action) (UpdateData));

                Thread.Sleep(Globals.PullDataRate * 1000);
            }
        }

        public void UpdateData()
        {
            if (Globals.Status.Live)
            {
                mainWindow.StatusLabel.Content = "Live";
                mainWindow.StatusLabel.Foreground = new SolidColorBrush(Color.FromRgb(239, 69, 57));
            }
            else
            {
                mainWindow.StatusLabel.Content = "Offline";
                mainWindow.StatusLabel.Foreground = new SolidColorBrush(Colors.Black);
            }

            mainWindow.ViewersLabel.Content = Globals.Status.Viewers;
            mainWindow.ChattersLabel.Content = Globals.Status.Chatters;
            mainWindow.FollowersLabel.Content = Globals.Status.Followers;
            mainWindow.ViewsLabel.Content = Globals.Status.Views;
            mainWindow.ViewerList.ItemsSource = Globals.Status.ViewersList;
            mainWindow.FPSLabel.Content = Format("{0:0.00}", Globals.Status.AverageFps);

            if (startFollowers == -1)
            {
                startFollowers = Globals.Status.Followers;
            }
            mainWindow.NewFollowersLabel.Content = Globals.Status.Followers - startFollowers;

            if (Globals.Status.Viewers > topViews)
            {
                topViews = Globals.Status.Viewers;
                mainWindow.TopViewsLabel.Content = topViews;
            }
        }
    }
}