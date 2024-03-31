using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace FileStorageApp.Client
{
    public class HttpServiceCustom
    {
        static private string _baseUrl = "https://localhost:8000";
        static public HttpClient GetProxyClient()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(_baseUrl);
            return client;
        }
    }
}
