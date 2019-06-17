using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace OnlineVault.Communication
{
    [Serializable]
    public class AccessToken
    {
        public string Username { get; set; }
        public string Address { get; set; }
        public string Token { get; set; }
        public DateTime CreationTime { get; }
        public DateTime LastUseTime { get; set; }
        public string UserAgent{ get; }

        public AccessToken(string username, string address, string token, string userAgent)
        {
            Username = username;
            Address = address;
            Token = token;
            UserAgent = userAgent;
            CreationTime = DateTime.Now;
            LastUseTime = DateTime.Now;
        }
    }
}
