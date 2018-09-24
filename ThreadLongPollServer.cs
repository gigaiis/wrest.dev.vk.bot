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
