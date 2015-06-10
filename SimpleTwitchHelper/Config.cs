using SimpleConfigurationSystem;

namespace SimpleTwitchHelper
{
    public class Config : Configuration<Config>
    {
        public string AuthKey { get; set; }
        public bool ShowAdControl { get; set; }
        public bool ShowAlert { get; set; }
        public bool PlaySound { get; set; }
        public string CountingMessage { get; set; }
        public string FinishedMessage { get; set; }
        public string StartupExecutables { get; set; }
        public HotbarButton LeftHotbarButton { get; set; }
        public HotbarButton MiddleHotbarButton { get; set; }
        public HotbarButton RightHotbarButton { get; set; }
        public string TwitterTemplate { get; set; }

        public Config() : base(Globals.ConfigFile) {}

        public override void LoadDefaults()
        {
            //AuthKey = "";
            ShowAdControl = false;
            ShowAlert = false;
            PlaySound = false;
            CountingMessage = "Starting in {time}!";
            FinishedMessage = "Starting soon!";
            StartupExecutables = "";
            LeftHotbarButton = new HotbarButton();
            MiddleHotbarButton = new HotbarButton("Twitch", "http://www.twitch.tv/");
            RightHotbarButton = new HotbarButton();
            TwitterTemplate = "{title}: {link}";
        }
    }
}