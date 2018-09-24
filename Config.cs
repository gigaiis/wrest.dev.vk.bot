using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace main
{
    public static class Config
    {
        internal static readonly int client_id = 3140623;
        internal static readonly string client_secret = "VeWdmVclDCtn6ihuP1nt";
        internal static readonly string username = Environment.ExpandEnvironmentVariables("%VK_LOGIN%");
        internal static readonly object password = Environment.ExpandEnvironmentVariables("%VK_PASSWORD%");
    }
}
