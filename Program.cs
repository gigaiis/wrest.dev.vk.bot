using Newtonsoft.Json;

namespace main
{
    class Program
    {
        static async void Main(string[] args)
        {
            var res = JsonConvert.DeserializeObject<WebResponse.WebOAuth>(
                await Web.Navigate(
                    string.Format(
                        "https://oauth.vk.com/token?grant_type=password&client_id={0}&client_secret={1}&username={2}&password={3}",
                        Config.client_id,
                        Config.client_secret,
                        Config.username,
                        Config.password
                    )
                )
            );
        }
    }
}
