using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopApp.HttpFolder
{
    public class HttpServiceCustom
    {
        static private string _baseUrlForProxy = "https://localhost:8000";
        static private string _baseUrlForApi = "https://localhost:7109";
        static public HttpClient GetProxyClient()
        {
            HttpClient client = new HttpClient { Timeout = TimeSpan.FromMinutes(30) };
            client.BaseAddress = new Uri(_baseUrlForProxy);
            return client;
        }

        static public HttpClient GetApiClient()
        {
            HttpClient client = new HttpClient { Timeout = TimeSpan.FromMinutes(30) };
            client.BaseAddress = new Uri(_baseUrlForApi);
            return client;
        }
    }
}
