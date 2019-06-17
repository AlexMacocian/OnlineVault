using Ceen.Httpd.Handler;
using Ceen.Httpd.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OnlineVault.Communication
{
    /// <summary>
    /// Class that implements Server class for Vault
    /// </summary>
    public class VaultServer
    {
        #region Fields
        Server server;
        #endregion
        #region Properties
        /// <summary>
        /// Server object handling connections.
        /// </summary>
        public Server Server { get => server; }
        /// <summary>
        /// Handler for session persistence.
        /// </summary>
        public SessionHandler SessionHandler { get; }
        /// <summary>
        /// Handler for vaultserver.
        /// </summary>
        public VaultServerHandler VaultServerHandler { get; }
        /// <summary>
        /// Handler for serving files.
        /// </summary>
        public FileHandler FileHandler { get; }
        /// <summary>
        /// Handler for authentication and registration.
        /// </summary>
        public AuthenticationHandler AuthenticationHandler { get; }

        #endregion
        #region Constructors
        public VaultServer()
        {
            server = new Server();
            SessionHandler = new SessionHandler();
            SessionHandler.ExpirationSeconds = TimeSpan.FromDays(365);
            SessionHandler.CookieName = "session-token";
            SessionHandler.SessionCookieSecure = true;
            VaultServerHandler = new VaultServerHandler(this);
            FileHandler = new FileHandler("src");
            AuthenticationHandler = new AuthenticationHandler(this);
            server
                .AddEncryption(new X509Certificate2("localhost.pfx", "psdsd"))
                .AddHttpModule(SessionHandler)
                .AddHttpModule(AuthenticationHandler)
                .AddHttpModule(VaultServerHandler)
                .AddHttpModule(FileHandler)
                .AddLogger(new CLFStdOut())
                .AddLogger(new CLFStdErr());
        }
        #endregion
        #region Public Methods
        public void Listen()
        {
            Server.Listen();
        }
        public Task ListenAsync()
        {
            return Server.ListenAsync();
        }
        public void SaveToDisk()
        {
            AuthenticationHandler.SaveToDisk();
        }
        public void LoadToDisk()
        {
            AuthenticationHandler.LoadFromDisk();
        }
        public void Stop()
        {
            server.Stop();
        }
        #endregion
        #region Private Methods
        #endregion
    }
}
