using Newtonsoft.Json;
using System;

namespace main
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var ResultWebOAuth = JsonConvert.DeserializeObject<WebResponse.WebOAuth>(
                    Web.Navigate(
                        string.Format(
                            "https://oauth.vk.com/token?grant_type=password&client_id={0}1&client_secret={1}&username={2}&password={3}",
                            Config.client_id,
                            Config.client_secret,
                            Config.username,
                            Config.password
                        )
                    ).Result
                );
                if (ResultWebOAuth.isHasError()) throw new Exception(string.Format("Error OAuth: {0}", ResultWebOAuth.error));
                else Console.WriteLine(ResultWebOAuth);
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("EXCEPTION[{0}]: {1}\r\n >>> {2}", DateTime.Now, ex.Message, ex.StackTrace));
            }
        }
    }
}
