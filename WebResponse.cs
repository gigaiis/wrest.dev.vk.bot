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
            public string access_token;
            public long expires_in;
            public long user_id;
            public Object error;
            public bool isHasError() => error != null;
            public override string ToString() => string.Format("access_token = {0}, expires_in = {1}, user_id = {2}", access_token, expires_in, user_id);
        }
    }
}
