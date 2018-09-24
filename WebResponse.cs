using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace main
{
    public static class WebResponse
    {
        public struct WebOAuth
        {
            public string access_token { get; }
            public long expires_in { get; }
            public long user_id { get; }
        }
    }
}
