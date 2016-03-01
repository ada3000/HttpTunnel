using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HttpTunnel;

namespace HttpTunnel.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            log4net.LogManager.GetLogger("1").Debug("1");

            using (TunnelExchange ex = new TunnelExchange())
            {
                ex.Bind("http://localhost:57244/tunnel", "yan", 8888);

                Console.ReadKey();
            }
        }
    }
}
