using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace main
{
    public static class VKApi
    {
        public static readonly string v = "5.85";
        public static WebOAuth Auth()
        {
            var Response = JsonConvert.DeserializeObject<WebOAuth>(
                Web.Navigate(
                    string.Format(
                        "https://oauth.vk.com/token?grant_type=password&client_id={0}&client_secret={1}&username={2}&password={3}",
                        Config.client_id,
                        Config.client_secret,
                        Config.username,
                        Config.password
                    )
                ).Result
            );
            if (Response.isHasError()) throw new Exception(string.Format("Error OAuth: {0}", Response.error));
            return Response;
        }
        public static T Run<T>(string APIName, Dictionary<string, object> args)
        {
            var Request = string.Format("https://api.vk.com/method/{0}?", APIName);
            foreach (var i in args) Request += string.Format("{0}={1}&", i.Key, i.Value.ToString());
            Request += string.Format("access_token={0}&v={1}", Program.access_token, v);
            ApiResult result = JsonConvert.DeserializeObject<ApiResult>(Web.Navigate(Request).Result);
            if (result.isHasError()) throw new Exception(result.error.ToString());
            return JsonConvert.DeserializeObject<T>(result.response.ToString());
        }
        public static getLongPollServer GetLongPollServer() => Run<getLongPollServer>("messages.getLongPollServer",
            new Dictionary<string, object>() {
            });
    }
}
