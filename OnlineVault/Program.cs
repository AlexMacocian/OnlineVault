using System;
using Ceen.Httpd;
using Ceen.Httpd.Handler;
using Ceen.Httpd.Logging;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using Ceen;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Collections.Generic;
using System.IO;
using Ceen.Security.Login;
using OnlineVault.Communication;
using OnlineVault.Models;

namespace OnlineVault
{
    class Program
    {
        private static VaultServer vaultServer;

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            vaultServer = new VaultServer();
            vaultServer.LoadToDisk();
            vaultServer.ListenAsync();
            Console.ReadLine();
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            vaultServer.SaveToDisk();
        }
    }
}
