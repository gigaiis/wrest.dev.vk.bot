using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading;

namespace main
{
    public class Program
    {
        public static long user_id;
        public static string access_token;

        public static ThreadLongPollServer threadLongPollServer;

        private static void OnPoll(object[] updates)
        {
            foreach (var o in updates)
                Console.WriteLine(string.Format("[{0}]{1}\r\n", DateTime.Now, ((JArray)o).ToString()));
        }

        public static void Main(string[] args)
        {
            try
            {
                WebOAuth ResultWebOAuth = VKApi.Auth();

                user_id = ResultWebOAuth.user_id;
                access_token = ResultWebOAuth.access_token;

                (threadLongPollServer = new ThreadLongPollServer(OnPoll)).Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("EXCEPTION[{0}]: {1}\r\n >>> {2}", DateTime.Now, ex.Message, ex.StackTrace));
            }
        }
    }
}
