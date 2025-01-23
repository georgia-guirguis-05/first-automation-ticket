using Newtonsoft.Json;

namespace ST_5336
{

    public class LandingPageTestDataModel(string appName, string url, string locator)
    {
        [JsonProperty("AppName")]
        public string AppName { get; } = appName;
        [JsonProperty("Url")]
        public string Url { get; } = url;
        [JsonProperty("Locator")]
        public string Locator { get; set; } = locator;
    }
}