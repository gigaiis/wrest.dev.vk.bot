using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace main
{
    public class Program
    {
        public const string filenamemsg = "msgs.json";

        public static TimeSpan TimeAVG = TimeSpan.Zero;
        public static TimeSpan TimeOutMessages = new TimeSpan(0, 00, 45);
        public static TimeSpan TimeCheckMessages = new TimeSpan(0, 0, 30);
        public static List<Msg> Msgs = new List<Msg>() { };

        public static long user_id;
        public static string access_token;

        public static ThreadLongPollServer threadLongPollServer;

        private static void OnPoll(object[] updates)
        {
            foreach (var o in updates)
            {
                JArray arr = (JArray)o;
                long code = (long)arr[0];
                if (code == 3)
                {
                    long message_id = (long)arr[1];
                    long undefinedFlag = (long)arr[2];
                    long peer_id = (long)arr[3];

                    if (undefinedFlag == 1)
                    {
                        // The message with message_id in the chat with peer_id has been read
                        var status = "OK";

                        var lE = Msgs.Where(e => e.message_id == message_id);
                        if (lE.Count() > 0) Msgs.Remove(lE.First());
                        else status = "NOT FOUND";

                        Console.WriteLine("[{0}][Mark as read] > message_id: {1}, peer_id: {2} <-- STATUS: {3}",
                            DateTime.Now,
                            message_id,
                            peer_id,
                            status);
                    }
                    else
                    {
                        Console.WriteLine("[CODE = 3][undefinedFlag = " + Convert.ToString(undefinedFlag) + "]");
                    }
                }
                else if (code == 4)
                {
                    long Flag = (long)arr[2];
                    long message_id = (long)arr[1];
                    long peer_id = (long)arr[3];
                    long timestamp = (long)arr[4];
                    //  Add a new message.
                    TimeAVG = Dev.UnixTimeToDateTime(Dev.GetCurrentUnixTime - timestamp).TimeOfDay;
                    Console.WriteLine("[{0}][Add a new message] > message_id: {1}, peer_id: {2}, timestamp: {3}, msg: {4}, obj: \r\n{5}",
                        DateTime.Now,
                        message_id,
                        peer_id,
                        timestamp,
                        (string)arr[5],
                        arr[6].ToString());
                    if ((Flag & 16) == 16)
                    {
                        if ((Flag & 2) == 2)
                        {
                            // Message is outgoing
                            Console.WriteLine(string.Format("Message is outgoing[CUR TIMESTAMP]: {0}", Dev.GetCurrentUnixTime));

                            Msgs.Add(new Msg(message_id, peer_id, timestamp));
                            //[9/29/2018 9:40:05 AM] [Add a new message] >

                            //        message_id: 581174
                            //        peer_id: 234269291
                            //        timestamp?: 1538203206
                            //        msg: asd
                            //        obj?: {
                            //  "title": " ... "
                            //}


                        }
                        else
                        {
                            if (peer_id != user_id)
                            {
                                Console.WriteLine("GET MESSAGE");
                            }
                            else
                            {
                                // Get message from self
                                Console.WriteLine("LS MESSAGE");
                            }
                        }
                    }
                }
                else if (code == 6)
                {
                    // In the chat with chat_id=202 (peer_id=2000000202) all incoming message before local_id=1619361 have been read:
                    // [6, 2000000202, 1619361]

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
                else Console.WriteLine(string.Format("[{0}]{1}", DateTime.Now, ((JArray)o).ToString().Replace("\r", string.Empty).Replace("\n", string.Empty).Replace("\t", string.Empty).Replace(" ", string.Empty)));
                if (code == 1 || code == 4 || code == 11 || code == 61)
                {
                    long Flag = (long)arr[2];
                    foreach (var _f in ThreadLongPollServer.Flags)
                    {
                        if ((Flag & _f.Key) == _f.Key)
                            Console.WriteLine(string.Format("{0,6}\t {1,10}\t {2,50}", _f.Key, _f.Value.Short, _f.Value.Full));
                    }
                    Console.WriteLine(string.Format("{0,6}", Flag));
                }
            }
        }

        public static async void Init()
        {
            if (!File.Exists(filenamemsg))
            {
                StreamWriter writer = new StreamWriter(filenamemsg);
                await writer.WriteLineAsync(JsonConvert.SerializeObject(Msgs));
                writer.Close();
            }
            else
            {
                StreamReader reader = new StreamReader(filenamemsg);
                Msgs = JsonConvert.DeserializeObject<List<Msg>>(await reader.ReadToEndAsync());
                reader.Close();
            }

            Thread threadWorker = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        var a = Dev.GetCurrentUnixTime;
                        var r = Msgs.Where(b => Dev.UnixTimeToDateTime(a - b.timestamp).TimeOfDay - TimeAVG > TimeOutMessages).ToList();
                        string sr = "";
                        foreach (var i in r) { sr += i.message_id + ","; }
                        // if (Msgs.Count != 0 || r.Count != 0) Console.WriteLine(string.Format("Msgs have {0} rows and {1} rows have is timeout state", Msgs.Count, r.Count));
                        if (sr.Length > 0)
                        {
                            Console.WriteLine("DELETE MESSAGES: {0}", sr = sr.Remove(sr.Length - 1));
                            Dictionary<string, long> pairs = VKApi.Run<Dictionary<string, long>>("messages.delete", new Dictionary<string, object>()
                            {
                                {"message_ids", sr },
                                {"spam", 0 },
                                {"delete_for_all", 1 }
                            });

                            // List<string> success = new List<string>();
                            // List<string> unsuccess = new List<string>();
                            foreach (var i in pairs)
                            {
                                // if (i.Value == 1) success.Add(i.Key);
                                // else unsuccess.Add(i.Key);

                                var lE = Msgs.Where(e => e.message_id == Convert.ToInt32(i.Key));
                                if (lE.Count() > 0) Msgs.Remove(lE.First());
                            }

                        }


                        StreamWriter writer = new StreamWriter(filenamemsg);
                        writer.WriteLineAsync(JsonConvert.SerializeObject(Msgs));
                        writer.Close();
                    }
                    catch (Exception ex) { Console.WriteLine(string.Format("EXCEPTION[{0}][THREAD WORKER]: {1}", DateTime.Now, ex.Message)); }
                    Thread.Sleep(TimeCheckMessages);
                }
            });
            threadWorker.Start();
        }
        public static void Main(string[] args)
        {
            Thread.GetDomain().UnhandledException += (sender, eventArgs) => OnExit((Exception)eventArgs.ExceptionObject);
            AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) => OnExit(null);

            try
            {
                Init();
                WebOAuth ResultWebOAuth = VKApi.Auth();
                user_id = ResultWebOAuth.user_id;
                access_token = ResultWebOAuth.access_token;
                (threadLongPollServer = new ThreadLongPollServer(OnPoll)).Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("EXCEPTION[{0}]: {1}\r\n >>> {2}", DateTime.Now, ex.Message, ex.StackTrace));
            }

            Console.Read();
            Environment.Exit(0);
        }

        private static void OnExit(Exception exception)
        {
            using (StreamWriter writer = new StreamWriter(filenamemsg))
            {
                writer.WriteLineAsync(JsonConvert.SerializeObject(Msgs));
                writer.Close();
            }
        }
    }
}
