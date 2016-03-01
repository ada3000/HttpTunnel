using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Web;

using Lib.Base;
using System.Threading.Tasks;
using System.Threading;
using System.Web.Hosting;

using HttpTunnel;
using log4net;

namespace HttpTunnel.AspServer
{
    public static class State
    {
        private static Dictionary<string, ClientInfo> Clients = new Dictionary<string, ClientInfo>();

        public static readonly ILog Logger = LogManager.GetLogger("Common");

        private static ClientInfo GetClientInfo(string clientId)
        {
            lock (Clients)
                if (!Clients.ContainsKey(clientId))
                {
                    Clients.Add(clientId, new ClientInfo());
                    OpenLocalSocket(clientId);
                }

            return Clients[clientId];
        }
        public static void SetClientInput(string clientId, Stream stream, ManualResetEvent ev = null)
        {
            Logger.DebugFormat("SetClientInput for:" + clientId);
            var info = GetClientInfo(clientId);
            if (info.InputEnd != null) info.InputEnd.Set();

            info.Input = stream;
            info.InputEnd = ev;

            info.Input.BindIn2Out(() => info.Local);
        }

        public static void SetClientOutput(string clientId, Stream stream, ManualResetEvent ev = null)
        {
            Logger.DebugFormat("SetClientOutput for:" + clientId);

            var info = GetClientInfo(clientId);
            if (info.OutputEnd != null) info.OutputEnd.Set();
            info.Output = stream;
            info.OutputEnd = ev;
        }        

        private static void OpenLocalSocket(string clientId)
        {
            Logger.DebugFormat("OpenLocalSocket for:" + clientId);

            var proxy = Config.Proxies.Where(p => p.Id == clientId).FirstOrDefault();
            if (proxy == null)
                throw new Exception("Proxy config not found for clientId='" + clientId + "'");

            var info = Clients[clientId];

            var client = new System.Net.Sockets.TcpClient();
            client.Connect(proxy.Server, proxy.Port);
            info.Local = client.GetStream();

            info.Local.BindIn2Out(() => info.Output);
        }

        

        private static Config _config;
        public static Config Config
        {
            get
            {
                string file = HostingEnvironment.MapPath("~/bin/Config.xml");
                return _config ?? (_config = File.ReadAllText(file).ToXmlReader().Deserialize<Config>());
            }
        }
    }
}