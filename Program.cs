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
        public static readonly string[] notfoundanswers = { "???", "Не понял", "Всмысле?", "🤔" };

        public static Dictionary<long, long> LastSendMessageIDtoUserID = new Dictionary<long, long>();
        public static string Editor_save_message = "";

        public static TimeSpan TimeAVG = TimeSpan.Zero;
        public static TimeSpan ExpiredTime = new TimeSpan(24, 0, 0);
        public static TimeSpan MinTimeNewDialog = new TimeSpan(3, 0, 0);
#if DEBUG
        public static TimeSpan TimeOutMessages = new TimeSpan(30, 00, 45);
#else
        public static TimeSpan TimeOutMessages = new TimeSpan(12, 0, 0);
#endif
        public static TimeSpan TimeCheckMessages = new TimeSpan(0, 0, 30);
        public static List<Msg> Msgs = new List<Msg>() { };

        public static long user_id;
        public static string access_token;

        public static List<long> IgnorePeer_id = new List<long>();

        public static ThreadLongPollServer threadLongPollServer;

        public static Dictionary<string /* msg_in */, List<List<messages_search_obj>> /* result */> hash_results =
            new Dictionary<string, List<List<messages_search_obj>>>();

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
#if DEBUG
                        Console.WriteLine("[{0}][Mark as read] > message_id: {1}, peer_id: {2} <-- STATUS: {3}",
                            DateTime.Now,
                            message_id,
                            peer_id,
                            status);
#endif
                    }
                    else
                    {
#if DEBUG
                        Console.WriteLine("[CODE = 3][undefinedFlag = " + Convert.ToString(undefinedFlag) + "]");
#endif
                    }
                }
                else if (code == 4)
                {
                    long Flag = (long)arr[2];
                    long message_id = (long)arr[1];
                    long peer_id = (long)arr[3];
                    long timestamp = (long)arr[4];
                    //  Add a new message.
                    TimeAVG = Dev.UnixTimeToDateTime(Math.Abs(Dev.GetCurrentUnixTime - timestamp)).TimeOfDay;
#if DEBUG
                    Console.WriteLine("[{0}][Add a new message] > message_id: {1}, peer_id: {2}, timestamp: {3}, msg: {4}, obj: \r\n{5}",
                        DateTime.Now,
                        message_id,
                        peer_id,
                        timestamp,
                        (string)arr[5],
                        arr[6].ToString());
#endif
                    if ((Flag & 2) == 2)
                    {
                        // Message is outgoing
#if DEBUG
                        Console.WriteLine(string.Format("Message is outgoing[CUR TIMESTAMP]: {0}", Dev.GetCurrentUnixTime));
#endif
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
#if DEBUG
                            Console.WriteLine("GET MESSAGE");
#endif
                            if (peer_id == 234269291 || peer_id == 189576135 || peer_id == 138025269)
                            {
                                new Task(() =>
                                {
                                    var input_message = ((string)arr[5]).ToLower();
                                    var input_message_split = input_message.Split(' ').ToList();

                                    var isGFind = false;

                                    var offset = 0;
                                    var count = 100;

                                    long Editor_message_id = 0;
                                    bool isEditor = LastSendMessageIDtoUserID.ContainsKey(peer_id);

                                    if (isEditor)
                                    {
                                        Editor_message_id = LastSendMessageIDtoUserID[peer_id];
                                        try
                                        {
                                            VKApi.Run<object>("messages.edit", new Dictionary<string, object>()
                                            {
                                                { "peer_id", peer_id},
                                                { "message", "Search for message: " + input_message},
                                                { "message_id", Editor_message_id }
                                            });
                                        }
                                        catch (Exception ex) { Console.WriteLine("Editor error: {0}", ex.Message); }
                                    }

                                    List<string> result_messages = new List<string>();

                                    List<List<messages_search_obj>> hash_list = new List<List<messages_search_obj>>();

                                    if (hash_results.ContainsKey(input_message))
                                    {
                                        foreach (var items_history in hash_results[input_message])
                                            foreach (var q in items_history)
                                                if (q._out == 1)
                                                {
                                                    result_messages.Add(q.text);
                                                    break;
                                                }
                                    }   
                                    else
                                    {
                                        while (true)
                                        {
                                            var res_getMessagesSearch = VKApi.Run<getMessagesSearch>("messages.search", new Dictionary<string, object>()
                                            {
                                                { "q", input_message },
                                                { "offset", offset },
                                                { "count", 100 }
                                            });
                                            Console.WriteLine("messages.search...[{0}...{1} of {2}]", offset, offset + count, res_getMessagesSearch.count);
                                            var SearchItems = res_getMessagesSearch.items.Where((a) => a.text.Length > 0).ToList();
                                            foreach (var i in SearchItems)
                                            {
                                                var _peer_id = i.peer_id;
                                                /*(!IgnorePeer_id.Contains(_peer_id)) && (i._out == 0)*/
                                                if ((_peer_id > 0) && (_peer_id < 2000000000))    // GET MESSAGE
                                                {
                                                    var message = i.text.ToLower();
                                                    var message_split = message.Split(' ').ToList();
                                                    if (input_message_split.Count == message_split.Count)
                                                    {
                                                        Console.WriteLine("messages.search result: msg_id: {0}, msg: {1}", i.id, i.text);
                                                        var res_getMessagesHistory = VKApi.Run<getMessagesSearch>("messages.getHistory", new Dictionary<string, object>()
                                                        {
                                                            { "offset", -10 + 1 },
                                                            { "count", 10 },
                                                            { "peer_id", _peer_id },
                                                            { "start_message_id", i.id }
                                                        });
                                                        var items_history = res_getMessagesHistory.items.Reverse().ToList();
                                                        var _count = 0;

                                                        hash_list.Add(items_history);

                                                        foreach (var q in items_history)
                                                        {
                                                            if (Dev.UnixTimeToDateTime(Math.Abs(q.date - i.date)).TimeOfDay > MinTimeNewDialog + TimeAVG) break;
                                                            else if (i._out != q._out  /*q._out == 1*/)
                                                            {
                                                                if (q.text.Length > 0)
                                                                {
                                                                    result_messages.Add(q.text);
                                                                    _count++;
                                                                    // isGFind = true;
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                        if (_count == 0) IgnorePeer_id.Add(_peer_id);
                                                    }
                                                }
                                            }
                                            if ((res_getMessagesSearch.count <= offset + count) || (isGFind) ||
                                                (offset > 5000) || (result_messages.Count > 10)) break;
                                            else
                                            {
                                                offset += count;
                                                //Thread.Sleep(1000);
                                            }
                                        }

                                        hash_results.Add(input_message, hash_list);

                                    }

                                    if (isEditor)
                                    {
                                        try
                                        {
                                            VKApi.Run<object>("messages.edit", new Dictionary<string, object>()
                                            {
                                                { "peer_id", peer_id},
                                                { "message", Editor_save_message },
                                                { "message_id", Editor_message_id }
                                            });
                                        }
                                        catch (Exception ex) { Console.WriteLine("Editor error: {0}", ex.Message); }
                                        Editor_message_id = 0;
                                    }

                                    string send_message = notfoundanswers[new Random().Next(notfoundanswers.Length)];

                                    //result_messages = result_messages.Where((a) => a.Key.ToString().Length > 0);
                                    if (result_messages.Count > 0)
                                    {
                                        int counter = result_messages.Count;
                                        int rand = new Random().Next(counter);
                                        send_message = result_messages[rand];
                                        Console.WriteLine("result message have {0} items: ", counter);
                                        for (int i = 0; i < counter; i++)
                                            Console.WriteLine(">> {0} {1}", result_messages[i], (i == rand) ? "<----" : "");
                                        try
                                        {
                                            var send_message_id = VKApi.Run<long>("messages.send", new Dictionary<string, object>()
                                            {
                                                { "user_id", peer_id },
                                                { "message",  Editor_save_message = send_message}
                                            });
                                            if (LastSendMessageIDtoUserID.ContainsKey(peer_id))
                                                LastSendMessageIDtoUserID[peer_id] = send_message_id;
                                            else LastSendMessageIDtoUserID.Add(peer_id, send_message_id);
                                        }
                                        catch (Exception ex) { Console.WriteLine("[EX]: {0}", ex.Message); }
                                    }
                                }).Start();
                            }
                        }
                        else
                        {
                            // Get message from self
#if DEBUG
                            Console.WriteLine("LS MESSAGE");
#endif
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
#if DEBUG
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
#endif
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
                try
                {
                    Msgs = JsonConvert.DeserializeObject<List<Msg>>(await reader.ReadToEndAsync());
                }
                catch { }
                reader.Close();
            }

            if (Msgs == null) Msgs = new List<Msg>();

            Thread threadWorker = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        var a = Dev.GetCurrentUnixTime;
                        var r = Msgs.Where(b => Dev.UnixTimeToDateTime(Math.Abs(a - b.timestamp)).TimeOfDay - TimeAVG > TimeOutMessages).ToList();
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

                                var lE = Msgs.Where(e => e.message_id == Convert.ToInt32(i.Key)).ToList();
                                if (lE.Count() > 0)
                                {
                                    foreach (var itm in lE)
                                    {
                                        Msgs.Remove(itm);
                                    }
                                }
                            }

                        }


                        StreamWriter writer = new StreamWriter(filenamemsg);
                        writer.WriteLineAsync(JsonConvert.SerializeObject(Msgs));
                        writer.Close();
                    }
                    catch (Exception ex) { Console.WriteLine(string.Format("EXCEPTION[{0}][THREAD WORKER]: {1}\r\n{2}", DateTime.Now, ex.Message, ex.StackTrace)); }
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
