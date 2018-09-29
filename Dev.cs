using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace main
{
    public static class Dev
    {
        public static Int32 GetCurrentUnixTime => (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        public static DateTime UnixTimeToDateTime(double unixTimeStamp) => (new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).AddSeconds(unixTimeStamp).ToLocalTime();
    }
}
