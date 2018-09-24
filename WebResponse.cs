using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace main
{
    public class WebOAuth
    {
        public string access_token;
        public long expires_in;
        public long user_id;
        public Object error;
        public bool isHasError() => error != null;
        public override string ToString() => string.Format("access_token = {0}, expires_in = {1}, user_id = {2}", access_token, expires_in, user_id);
    }

    public class ApiResult
    {
        public Object response;
        public Object error;
        public bool isHasError() => error != null;
    }

    public struct getLongPollServer
    {
        public string key;
        public string server;
        public long ts;
        public override string ToString() => string.Format("key = {0}, server = {1}, ts = {2}", key, server, ts);

    }

    public struct PollResult 
    {
        public long ts;
        public object[] updates;
        public long failed;
        public bool isHasError() => failed != 0;
    }

}
