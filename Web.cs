using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;

namespace main
{
    public class Web
    {
        public static CookieContainer Cookies = new CookieContainer();
        public static async Task<string> Navigate(string Url,
            string Method = "GET",
            string Data = "")
        {
            HttpWebResponse response = await Task.Run(() =>
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
                request.CookieContainer = Cookies;
                request.Method = Method;
                request.ContentType = "application/x-www-form-urlencoded";
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/67.0.3396.87 Safari/537.36";
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8";
                request.Headers.Add("accept-language", "en-US,en;q=0.9");
                request.Timeout = 30000;
                if (Method.Equals("POST"))
                {
                    byte[] byteArr = System.Text.Encoding.UTF8.GetBytes(Data);
                    request.ContentLength = byteArr.Length;
                    using (Stream s = request.GetRequestStream())
                        s.Write(byteArr, 0, byteArr.Length);
                }
                return (HttpWebResponse)request.GetResponse();
            });
            StreamReader str = new StreamReader(response.GetResponseStream());
            var sR = str.ReadToEnd();
            return sR;
        }
    }
}
