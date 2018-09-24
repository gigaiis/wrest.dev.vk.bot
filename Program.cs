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
            {
                JArray arr = (JArray)o;
                long code = (long)arr[0];
                if (code == 4)
                {
                    //  Add a new message.
                    Console.WriteLine("[{0}][Add a new message] > \r\n\r\n\tmessage_id: {1}\r\n\tpeer_id: {2}\r\n\ttimestamp?: {3}\r\n\tmsg: {4}\r\n\tobj?: {5}\r\n",
                        DateTime.Now,
                        (long)arr[1],
                        (long)arr[3],
                        (long)arr[4],
                        (string)arr[5],
                        arr[6].ToString());
                }
                else if (code == 8)
                {
                    //  A friend $user_id is online.$extra is not0, if flag64was passed in mode.The 
                    //    low byte(remaining from the division into 256) of anextra number 
                    //    contains the platform ID(ref. 7.Platforms). $timestamp is a time 
                    //    of the last action of$user_iduser.

                }
                else if (code == 9)
                {
                    //  A friend $user_id is offline ($flags is 0, if the user left the website(for example,
                    //    by pressing Log Out) and1, if offline is due to timing out (for example,
                    //    the away status)). $timestamp is a time of the last action of$user_id user.

                    //Console.WriteLine("[{0}][A friend is offline] > \r\n\tuser_ud: {1} from [{2}]\r\n\ttimestamp: {3}\r\n",
                    //    DateTime.Now,
                    //    -(long)arr[1],
                    //    (long)arr[2] == 0 ? "Log Out button" : "Timing out",
                    //    (long)arr[3]
                    //);
                }
                else if (code == 61)
                {
                    //User with user_id = 123456 has started to type message in the dialog:
                    //[61,123456,1]
                }
                else Console.WriteLine(string.Format("[{0}]{1}\r\n", DateTime.Now, ((JArray)o).ToString()));
                if (code == 1 || code == 4 || code == 11 || code == 61)
                {
                    long Flag = (long)arr[2];
                    foreach (var _f in ThreadLongPollServer.Flags)
                    {
                        if ((Flag & _f.Key) == _f.Key)
                            Console.WriteLine(string.Format("{0,6}\t {1,10}\t {2,50}", _f.Key, _f.Value.Short, _f.Value.Full));
                    }
                    Console.WriteLine(string.Format("{0,6}\r\n", Flag));
                }
            }
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
