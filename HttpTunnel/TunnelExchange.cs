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
    /// Принимает входящие TCP подключения
    /// </summary>
    public class TunnelExchange
    {
        private TcpListener _listener;
        private string _tunnelUrl;
        private string _tunnelId;

        private List<TunnelClient> _clients = new List<TunnelClient>();

        public void Connect(string tunnelUrl, string tunnelId, int localPort)
        {
            _tunnelId = tunnelId;
            _tunnelUrl = tunnelUrl;

            OpenLocalPort(localPort);
        }


        private void OpenLocalPort(int localPort)
        {
            _listener = new TcpListener(IPAddress.Any, localPort);
            new Task(() =>
            {
                while (true)
                {
                    var client = _listener.AcceptTcpClient();
                    _clients.Add(new TunnelClient(_tunnelUrl, _tunnelId, client));
                }
            }).Start();          
        }
    }
}
