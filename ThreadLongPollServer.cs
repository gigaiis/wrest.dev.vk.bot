using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace main
{
    public delegate void PollCallBack(object[] updates);
    public class ThreadLongPollServer
    {
        public struct FlagAbout
        {
            public string Short;
            public string Full;

            public FlagAbout(string s, string f)
            {
                Short = s;
                Full = f;
            }
        }
        public static Dictionary<long, FlagAbout> Flags = new Dictionary<long, FlagAbout>() {
            { 1, new FlagAbout("UNREAD","Message is unread") },
            { 2, new FlagAbout("OUTBOX","Message is outgoing") },
            { 4, new FlagAbout("REPLIED","Message was answered") },
            { 8, new FlagAbout("IMPORTANT","Message is marked as important") },
            { 16, new FlagAbout("CHAT","Message sent via chat") },
            { 32, new FlagAbout("FRIENDS","Message sent by a friend") },
            { 64, new FlagAbout("SPAM","Message marked as \"Spam\"") },
            { 128, new FlagAbout("DELЕTЕD","Message was deleted") },
            { 256, new FlagAbout("FIXED","Message was user-checked for spam") },
            { 512, new FlagAbout("MEDIA","Message has media content") },
            { 65536, new FlagAbout("HIDDEN","Greeting message from a community. A dialog with such message should not be raisen in the list (show it only when a dialog has been opened directly). Flag is unavailable for versions < 2") }
        };

        private getLongPollServer _poll;
        private PollCallBack _callback;
        private Thread _threadLongPollServer;   
        public ThreadLongPollServer(PollCallBack callback)
        {
            _threadLongPollServer = new Thread(ThreadMethod);
            _callback = callback;
        }

        private void ThreadMethod()
        {
            _poll = VKApi.GetLongPollServer();
            while (_threadLongPollServer.ThreadState == ThreadState.Running)
            {
                var poll = JsonConvert.DeserializeObject<PollResult>(Web.Navigate(string.Format(
                    "https://{0}?act=a_check&key={1}&ts={2}&wait=25&access_token={3}",
                    _poll.server,
                    _poll.key,
                    _poll.ts,
                    Program.access_token
                )).Result);
                if (poll.isHasError())
                {
                    if (poll.failed == 1) _poll.ts = poll.ts;
                    else if (poll.failed == 2)
                    {
                        //"failed":2 — the key’s active period expired.
                        //It's necessary to receive a key using the messages.getLongPollServer method.
                        _poll = VKApi.GetLongPollServer();
                    }
                    else if (poll.failed == 3)
                    {
                        //"failed":3 — user information was lost.
                        //It's necessary to request a new key and ts with the help of the messages.getLongPollServer method.
                        _poll = VKApi.GetLongPollServer();
                    }
                    else throw new Exception((poll.failed == 4) ?
                      "An invalid version number was passed in the version parameter" :
                      "Unknows error");
                }
                else if (poll.updates.Length > 0)
                {
                    _poll.ts = poll.ts;
                    _callback(poll.updates);
                }
            }
        }
        public void Run() => _threadLongPollServer.Start();
        public void Resume() => _threadLongPollServer.Resume();
        public void Suspend() => _threadLongPollServer.Suspend();
    }
}
