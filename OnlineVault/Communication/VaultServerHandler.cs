using Ceen;
using Ceen.Httpd.Handler;
using Newtonsoft.Json;
using OnlineVault.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace OnlineVault.Communication
{
    /// <summary>
    /// Handler for connections to server
    /// </summary>
    public class VaultServerHandler : IHttpModule
    {
        private VaultServer vaultServer;
        private RedirectHandler skipAuthRedirectHandler = new RedirectHandler("dashboard.html");
        private RedirectHandler indexRedirectHandler = new RedirectHandler("index.html");

        public VaultServerHandler(VaultServer vaultServer)
        {
            this.vaultServer = vaultServer;
        }
        /// <summary>
        /// Handling procedure.
        /// </summary>
        /// <param name="context">Http context.</param>
        public async Task<bool> HandleAsync(IHttpContext context)
        {
            if (vaultServer.AuthenticationHandler.IsAuthenticated(context))
            {
                if (context.Request.Path.ToLower().Contains("vault"))
                {
                    if(context.Request.Method.ToLower() == "get")
                    {
                        SendVault(context);
                        return true;
                    }
                    else if(context.Request.Method.ToLower() == "post")
                    {

                    }
                }
                else if(context.Request.Path.ToLower() == "/index.html" || context.Request.Path.ToLower() == "")
                {
                    await skipAuthRedirectHandler.HandleAsync(context);
                    return true;
                }
                return false;
            }
            else
            {
                if (context.Request.Path.ToLower().Contains("dashboard"))
                {
                    await indexRedirectHandler.HandleAsync(context);
                    return true;
                }
                return false;
            }
        }
        /// <summary>
        /// Send vault object for current user.
        /// </summary>
        /// <param name="context">Http context.</param>
        private async void SendVault(IHttpContext context)
        {
            context.Response.AddHeader("Content-type", "text/xml");
            await context.Response.WriteAllAsync(vaultServer.AuthenticationHandler.GetAccount(context).Vault.Serialize());
        }
    }
}
