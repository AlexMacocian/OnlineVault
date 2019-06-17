using Ceen;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OnlineVault.Communication
{
    public class SessionHandler : IHttpModule
    {
        /// <summary>
		/// The name of the storage module
		/// </summary>
		public const string STORAGE_MODULE_NAME = "session-storage";
        /// <summary>
        /// Gets or sets the name of the cookie with the token.
        /// </summary>
        public string CookieName { get; set; } = "ceen-session-token";
        /// <summary>
        /// Gets or sets the number of seconds a session is valid.
        /// </summary>
        public TimeSpan ExpirationSeconds { get; set; } = TimeSpan.FromMinutes(30);
        /// <summary>
        /// Gets or sets a value indicating if the session cookie gets the &quot;secure&quot; option set,
        /// meaning that it will only be sent over HTTPS
        /// </summary>
        public bool SessionCookieSecure { get; set; } = true;
        /// <summary>
        /// Handles the request
        /// </summary>
        /// <returns>The awaitable task.</returns>
        /// <param name="context">The requests context.</param>
        public async Task<bool> HandleAsync(IHttpContext context)
        {
            if (!context.Request.Cookies.ContainsKey(CookieName))
            {
                context.Response.AddCookie(CookieName, Guid.NewGuid().ToString());
            }
            return false;
        }
    }
}
