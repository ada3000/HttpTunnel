using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HttpTunnel
{
    public static class StreamHelper
    {
        /// <summary>
        /// Передача входящих данных из sourceIn в destanationOut
        /// </summary>
        /// <param name="sourceIn"></param>
        /// <param name="destanationOut"></param>
        /// <param name="buffSize"></param>
        public static void BindIn2Out(this Stream sourceIn, Func<Stream> destanationOut, int buffSize = 0x1000)
        {
            byte[] buff = new byte[buffSize];

            new Task(() =>
            {
                while (true)
                {
                    Stream dest = destanationOut();

                    if (sourceIn.CanRead)
                    {
                        int cnt = sourceIn.Read(buff, 0, buff.Length);
                        if (cnt > 0 && dest != null)
                            dest.Write(buff, 0, cnt);

                        continue;
                    }
                    Thread.Sleep(100);
                }
            }).Start();
        }
    }
}
