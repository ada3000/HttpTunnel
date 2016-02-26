using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Web;

using Lib.Base;

namespace HttpTunnel.AspServer
{
    public static class State
    {
        public static Dictionary<string, ClientInfo> Clients = new Dictionary<string, ClientInfo>();

        private static Config _config;
        public static Config Config
        {
            get
            {
                return _config ?? (_config = File.ReadAllText("Config.xml").ToXmlReader().Deserialize<Config>());
            }
        }
    }
}