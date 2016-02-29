using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Web;

namespace HttpTunnel.AspServer
{
    public class ClientInfo
    {
        public string Id;
        /// <summary>
        /// Обработка входящего трафика
        /// </summary>
        public Stream Input;
        public ManualResetEvent InputEnd;
        /// <summary>
        /// Обработка исходящего трафика
        /// </summary>
        public Stream Output;
        public ManualResetEvent OutputEnd;
        /// <summary>
        /// Подключение к локальному хоста
        /// </summary>
        public Stream Local;
    }
}