using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Web;

namespace HttpTunnel.AspServer
{
    public class ClientInfo
    {
        public string Id;
        public Stream Request;
        public Stream Responce;
        public TcpClient Socket;
    }
}