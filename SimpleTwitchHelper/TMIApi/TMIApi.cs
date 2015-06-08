using RestSharp;
using TwitchCSharp.Helpers;

namespace SimpleTwitchHelper.TMIApi
{
    public class TMIApi
    {
        private const string url = "https://tmi.twitch.tv/";
        private readonly RestClient client;

        public TMIApi()
        {
            client = new RestClient(url);
            client.AddHandler("application/json", new DynamicJsonDeserializer());
        }

        public TMIResponse GetResponse(string name)
        {
            var request = new RestRequest("group/user/{name}/chatters", Method.GET);
            request.AddUrlSegment("name", name);
            var response = client.Execute<TMIResponse>(request);
            return response.Data;
        }
    }
}