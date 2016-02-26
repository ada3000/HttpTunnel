using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace HttpTunnel.AspServer
{
    [XmlRoot("config"), XmlType("config")]
    public class Config
    {
        [XmlAttribute("responceContentType")]
        public string ResponceContentType;

        [XmlElement("proxy")]
        public ProxyConfig[] Proxies;
    }
    public class ProxyConfig
    {
        [XmlAttribute("id")]
        public string Id;

        [XmlAttribute("server")]
        public string Server;

        [XmlAttribute("port")]
        public int Port;
    }
}