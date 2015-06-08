using System.Collections.Generic;

namespace SimpleTwitchHelper
{
    public class TwitchStatus
    {
        public string Username { get; set; }
        public string Displayname { get; set; }
        public string StreamTitle { get; set; }
        public string Game { get; set; }
        public bool Live { get; set; }
        public long Viewers { get; set; }
        public long Chatters { get; set; }
        public long Views { get; set; }
        public long Followers { get; set; }
        public long Subscribers { get; set; }
        public double AverageFps { get; set; }
        public List<string> ViewersList { get; set; }
    }
}