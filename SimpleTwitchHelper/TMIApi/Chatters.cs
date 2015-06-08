using System.Collections.Generic;
using Newtonsoft.Json;

namespace SimpleTwitchHelper.TMIApi
{
    [JsonObject("chatters")]
    public class Chatters
    {
        [JsonProperty("moderators")]
        public List<string> ModeratorList { get; set; }

        [JsonProperty("staff")]
        public List<string> StaffList { get; set; }

        [JsonProperty("admins")]
        public List<string> AdminList { get; set; }

        [JsonProperty("global_mods")]
        public List<string> GlobalModList { get; set; }

        [JsonProperty("viewers")]
        public List<string> ViewerList { get; set; }
    }
}