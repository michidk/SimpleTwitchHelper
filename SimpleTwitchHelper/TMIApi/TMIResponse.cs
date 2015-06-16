using Newtonsoft.Json;
using TwitchCSharp.Models;

namespace SimpleTwitchHelper.TMIApi
{
    public class TMIResponse : TwitchResponse
    {
        [JsonProperty("chatter_count")]
        public long ChatterCount { get; set; }

        [JsonProperty("chatters")]
        public Chatters Chatters { get; set; }
    }
}