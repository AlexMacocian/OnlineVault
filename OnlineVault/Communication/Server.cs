using Ceen;
using Ceen.Httpd;
using Ceen.Httpd.Handler;
using Ceen.Httpd.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OnlineVault.Communication
{
    /// <summary>
    /// Server class to encapsulate the httpd server object
    /// </summary>
    public class Server
    {
        #region Fields
        private ServerConfig serverConfig;
        private CancellationTokenSource cancellationToken;
        private bool usingSSL, running;
        private int port;
        private SecureString sslPassword;
        #endregion
        #region Properties
        public bool UsingSSL { get => usingSSL; }
        public bool Running {
            get
            {
                return running;
            }
        }
        public int Port { get => port; set => port = value; }
        #endregion
        #region Constructors
        public Server()
        {
            serverConfig = new ServerConfig();
        }
        #endregion
        #region Public Methods
        /// <summary>
        /// Adds encryption to server.
        /// </summary>
        /// <param name="sslCertificate">Certificate to be used for encryption.</param>
        /// <returns>This server object.</returns>
        public Server AddEncryption(X509Certificate2 sslCertificate)
        {
            usingSSL = true;
            serverConfig.SSLCertificate = sslCertificate;
            return this;
        }
        /// <summary>
        /// Adds a module to the server.
        /// </summary>
        /// <param name="module">Module to be added to the server.</param>
        /// <returns>This server.</returns>
        public Server AddModule(IModule module)
        {
            serverConfig.AddModule(module);
            return this;
        }
        /// <summary>
        /// Adds a http module to the server.
        /// </summary>
        /// <param name="httpModule">Http module to be added.</param>
        /// <returns>This server.</returns>
        public Server AddHttpModule(IHttpModule httpModule)
        {
            serverConfig.AddRoute(httpModule);
            return this;
        }
        /// <summary>
        /// Adds a logger module to the server.
        /// </summary>
        /// <param name="logger">Logger to be added to the server.</param>
        /// <returns>This server.</returns>
        public Server AddLogger(ILogger logger)
        {
            serverConfig.AddLogger(logger);
            return this;
        }
        /// <summary>
        /// Launches the server.
        /// </summary>
        public void Listen()
        {
            if (!running)
            {
                cancellationToken = new CancellationTokenSource();
                var task = HttpServer.ListenAsync(
                    new IPEndPoint(IPAddress.Any, 443),
                    usingSSL,
                    serverConfig,
                    cancellationToken.Token
                );
                running = true;
                while (running)
                {

                }
            }
        }
        /// <summary>
        /// Launches the server asynchronously.
        /// </summary>
        /// <returns>Task object that runs the server. If server is currently running, returns null.</returns>
        public Task ListenAsync()
        {
            if (!running)
            {
                cancellationToken = new CancellationTokenSource();
                running = true;
                return HttpServer.ListenAsync(
                    new IPEndPoint(IPAddress.Any, 443),
                    usingSSL,
                    serverConfig,
                    cancellationToken.Token
                );
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// Stops the server from running.
        /// </summary>
        public void Stop()
        {
            cancellationToken.Cancel();
            running = false;
        }
        #endregion
        #region Private Methods

        #endregion
    }
}
