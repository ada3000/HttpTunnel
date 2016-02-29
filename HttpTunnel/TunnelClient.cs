using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HttpTunnel
{
    /// <summary>
    /// Обрабатывает входящие TCP подключения
    /// </summary>
    internal class TunnelClient
    {
        private TcpClient _client;
        private Stream _out;
        private Stream _in;

        private string _tunnelUrl;
        private string _tunnelId;

        public TunnelClient(string tunnelUrl, string tunnelId, TcpClient client)
        {
            _tunnelId = tunnelId;
            _tunnelUrl = tunnelUrl;
            _client = client;

            BindClient();
            ConnectOutTunnel();
            ConnectInTunnel();
        }

        private void BindClient()
        {
            _client.GetStream().BindIn2Out(() => _out);
        }

        private void ConnectInTunnel()
        {
            throw new NotImplementedException();
        }

        private void ConnectOutTunnel()
        {
            throw new NotImplementedException();
        }

        private HttpWebRequest CreateRequest(string url, string method="POST")
        {
            HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;

            request.Method = method;
            request.Accept = @"text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8";
            request.Headers.Add("Accept-Encoding", "gzip, deflate, sdch");
            request.Headers.Add("Accept-Language", "ru -RU,ru;q=0.8,en-US;q=0.6,en;q=0.4");
            request.Connection = @"keep-alive";
            request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            request.UserAgent = @"Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/48.0.2564.116 Safari/537.36";

            return request;
        }
    }
}
