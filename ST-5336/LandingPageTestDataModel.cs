using Newtonsoft.Json;

namespace ST_5336
{

    public class LandingPageTestDataModel(string appName, string url, string locator)
    {
        public string AppName { get; } = appName;
        public string URL { get; } = url;
        public string Locator { get; } = locator;
    }
}