using Newtonsoft.Json;
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

    public struct messages_search_obj
    {
        public long date;
        public long from_id;
        public long id;
        [JsonProperty("out")]
        public long _out;
        public long peer_id;
        public string text;
        // public long "conversation_message_id": 214,
        // public object[] "fwd_messages": [],
        // "important": false,
        // "random_id": 0,
        // "attachments": [],
        public bool is_hidden;
        public override string ToString() => text;
    }

    public struct getMessagesSearch
    {
        public long count;
        public messages_search_obj[] items;
    }

    public struct PollResult 
    {
        public long ts;
        public object[] updates;
        public long failed;
        public bool isHasError() => failed != 0;
    }

}
