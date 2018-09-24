using System;

namespace main
{
    public static class Config
    {
        internal static readonly int client_id = 3140623;
        internal static readonly string client_secret = "VeWdmVclDCtn6ihuP1nt";
        internal static readonly string username = Environment.GetEnvironmentVariable("VK_LOGIN", EnvironmentVariableTarget.User);
        internal static readonly object password = Environment.GetEnvironmentVariable("VK_PASSWORD", EnvironmentVariableTarget.User);
    }
}
