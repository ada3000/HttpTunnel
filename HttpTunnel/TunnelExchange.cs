using log4net;
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
    public class TunnelExchange: IDisposable
    {
        private readonly ILog Logger = LogManager.GetLogger(typeof(TunnelExchange));

        private TcpListener _listener;
        private string _tunnelUrl;
        private string _tunnelId;
        private int _port;
        private Task _acceptConnections;

        private List<TunnelClient> _clients = new List<TunnelClient>();

        public void Bind(string tunnelUrl, string tunnelId, int listenPort)
        {
            _tunnelId = tunnelId;
            _tunnelUrl = tunnelUrl;
            _port = listenPort;

            CreateListener();
            StartAcceptConnections();
        }

        private void CreateListener()
        {
            try
            {
                Logger.Debug("CreateListener ...");

                _listener = new TcpListener(IPAddress.Any, _port);
                _listener.Start();

                Logger.Debug("CreateListener success");
            }
            catch(Exception ex)
            {
                Logger.Error("CreateListener " + ex);
            }
        }



        private void StartAcceptConnections()
        {
            _acceptConnections = new Task(AcceptConnections);
            _acceptConnections.Start();
        }

        private void AcceptConnections()
        {
            while (true)
            {
                try
                {                
                    Logger.Debug("AcceptConnections ...");

                    var client = _listener.AcceptTcpClient();
                    _clients.Add(new TunnelClient(_tunnelUrl, _tunnelId, client));

                    Logger.Debug("AcceptConnection RemoteEndPoint=" + client.Client.RemoteEndPoint.ToString());
                }
                catch (Exception ex)
                {
                    Logger.Debug("AcceptConnections "+ex);
                }
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    _listener.Stop();
                    _acceptConnections.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
