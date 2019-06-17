using Ceen;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace OnlineVault.Communication
{
    public class AuthenticationHandler : IHttpModule
    {
        #region Fields
        private VaultServer vaultServer;
        private enum ErrorCodes
        {
            InvalidPassword,
            PasswordsNotMatching,
            UserNotExistent,
            InvalidUsername,
            InvalidCredentials,
            Success
        }
        /// <summary>
        /// Dictionary of tokens. Index is the string token and value is the token with information related to accounts.
        /// The goal is to be able to monitor the use of tokens and potentially restrict the use of tokens in case that
        /// the ip address of the client doesn't match the address that got granted the previous access token.
        /// </summary>
        volatile Dictionary<string, AccessToken> tokens = new Dictionary<string, AccessToken>();
        /// <summary>
        /// Dictionary of accounts. Index is username, value is account.
        /// </summary>
        volatile Dictionary<string, Account> accounts = new Dictionary<string, Account>();
        #endregion
        #region Properties
        #endregion
        #region Constructors
        public AuthenticationHandler(VaultServer vaultServer)
        {
            this.vaultServer = vaultServer;
        }
        #endregion
        #region Public Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<bool> HandleAsync(IHttpContext context)
        {
            string address = ((IPEndPoint)context.Request.RemoteEndPoint).Address.ToString();
            string sessiontoken = context.Request.Cookies[vaultServer.SessionHandler.CookieName];
            if (context.Request.Method.ToLower() == "post")
            {
                if (context.Request.Path.ToLower() == "/register")
                {
                    string username = context.Request.Form["Username"];
                    string password = context.Request.Form["Password"];
                    string confirmpassword = context.Request.Form["ConfirmPassword"];
                    ErrorCodes errorCodes = ValidateInput(username, password);
                    #region Validate input
                    if (errorCodes == ErrorCodes.InvalidPassword)
                    {
                        SendXMLResponse(GetXMLResponse(errorCodes.ToString(), "Invalid password. Please change it and try again!"), context);
                        return true;
                    }
                    else if(errorCodes == ErrorCodes.InvalidUsername)
                    {
                        SendXMLResponse(GetXMLResponse(errorCodes.ToString(), "Invalid username. Please change it and try again!"), context);
                        return true;
                    }
                    else if(confirmpassword != password)
                    {
                        SendXMLResponse(GetXMLResponse(ErrorCodes.PasswordsNotMatching.ToString(), "Passwords do not match. Please change them and try again!"), context);
                        return true;
                    }
                    #endregion

                    if (this.accounts.ContainsKey(username))
                    {
                        //Username exists already
                        SendXMLResponse(GetXMLResponse(ErrorCodes.InvalidUsername.ToString(), "Username exists already! Please try another username!"), context);
                        return true;
                    }

                    Account account = new Account(username, Cryptography.Crypto.GenerateSHA512String(password));
                    account.Vault = new Models.Vault();
                    this.accounts[username] = account;
                    tokens[sessiontoken] = new AccessToken(username, address, sessiontoken, context.Request.Headers["User-Agent"]);
                    SendXMLResponse(GetXMLResponse(ErrorCodes.Success.ToString(), "/dashboard.html"), context);
                    return true;
                }
                else if(context.Request.Path.ToLower() == "/login")
                {
                    string username = context.Request.Form["Username"];
                    string password = context.Request.Form["Password"];
                    ErrorCodes errorCodes = ValidateInput(username, password);
                    #region Validate input
                    if (errorCodes == ErrorCodes.InvalidPassword)
                    {
                        SendXMLResponse(GetXMLResponse(errorCodes.ToString(), "Invalid password. Please change it and try again!"), context);
                        return true;
                    }
                    else if (errorCodes == ErrorCodes.InvalidUsername)
                    {
                        SendXMLResponse(GetXMLResponse(errorCodes.ToString(), "Invalid username. Please change it and try again!"), context);
                        return true;
                    }
                    #endregion
                    if (!this.accounts.ContainsKey(username))
                    {
                        //Username doesn't exist
                        SendXMLResponse(GetXMLResponse(ErrorCodes.UserNotExistent.ToString(), "User is not existent! Please check the username and try again!"), context);
                        return true;
                    }
                    Account account = accounts[username];
                    if (!account.Authenticate(password))
                    {
                        //Password doesn't match with provided username
                        SendXMLResponse(GetXMLResponse(ErrorCodes.InvalidCredentials.ToString(), "Invalid credentials! Please modify them and try again!"), context);
                        return true;
                    }
                    //Credentials are correct, associate username with token.
                    tokens[sessiontoken] = new AccessToken(username, address, sessiontoken, context.Request.Headers["User-Agent"]);
                    SendXMLResponse(GetXMLResponse(ErrorCodes.Success.ToString(), "/dashboard.html"), context);
                    return true;
                }
                else if(context.Request.Path.ToLower() == "/vault")
                {
                    Account account = GetAccount(context);
                    if (account != null)
                    {
                        string xml = await context.Request.Body.ReadAllAsStringAsync();
                        account.Vault = new Models.Vault(xml);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else if(context.Request.Method.ToLower() == "get")
            {
                if(context.Request.Path.ToLower() == "/logout")
                {
                    tokens.Remove(sessiontoken);
                    return true;
                }
                else if(context.Request.Path.ToLower() == "/accesstokens")
                {
                    //Return all the authentication tokens provided for currently authenticated user.
                    if (IsAuthenticated(context))
                    {
                        //Get the currently used token then iterate over the list of currently active tokens.
                        //If Username matches, add the token information to the response.
                        AccessToken token = tokens[sessiontoken];
                        string xml = "<Tokens>";
                        foreach(var kp in tokens)
                        {
                            if(kp.Value.Username == token.Username)
                            {
                                xml += "<Token>";
                                xml += "<CreationDate>" + kp.Value.CreationTime.ToUniversalTime() + "</CreationDate>";
                                xml += "<LastUsedTime>" + kp.Value.LastUseTime.ToUniversalTime() + "</LastUsedTime>";
                                xml += "<UserAgent>" + kp.Value.UserAgent + "</UserAgent>";
                                xml += "<Address>" + kp.Value.Address + "</Address>";
                                xml += "</Token>";
                            }
                        }
                        xml += "</Tokens>";
                        context.Response.AddHeader("Content-type", "text/xml");
                        await context.Response.WriteAllAsync(xml);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if(context.Request.Path.ToLower().Contains("/revokeaccess"))
                {
                    //Revokes access to provided session token if it is associated with the current account.
                    string token = context.Request.Path.ToLower().Split("?")[1];
                    if (IsAuthenticated(context))
                    {
                        if(tokens[sessiontoken].Username == tokens[token].Username)
                        {
                            tokens.Remove(token);
                            return true;
                        }
                    }
                }
                else if(context.Request.Path.ToLower() == "/revokeallaccess")
                {
                    //Revokes access to all session tokens associated with the current account.
                    if (IsAuthenticated(context))
                    {
                        //Get all access tokens that match in username with the current token.
                        //Remove all of them from the tokens list.
                        AccessToken token = tokens[sessiontoken];
                        List<string> toRemoveTokens = new List<string>();
                        foreach (var kp in tokens)
                        {
                            if (kp.Value.Username == token.Username)
                            {
                                toRemoveTokens.Add(kp.Value.Token);
                            }
                        }
                        foreach(String t in toRemoveTokens)
                        {
                            tokens.Remove(t);
                        }
                        return true;
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// Checks if user is authenticated.
        /// </summary>
        /// <param name="context">Http context.</param>
        /// <returns>True if user is authenticated.</returns>
        public bool IsAuthenticated(IHttpContext context)
        {
            var sessiontoken = context.Request.Cookies[vaultServer.SessionHandler.CookieName];
            //Check if the session token exists.
            if (tokens.ContainsKey(sessiontoken))
            {
                //Check if the session token is valid.
                if (tokens[sessiontoken].Address == ((IPEndPoint)context.Request.RemoteEndPoint).Address.ToString())
                {
                    //The token is still valid and client didn't change. Grant Access.
                    tokens[sessiontoken].LastUseTime = DateTime.Now;
                    return true;
                }
                else
                {
                    //Token exists but the client is different than the initial requester. Deny access.
                    return false;
                }                
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Returns the account of the user.
        /// </summary>
        /// <param name="context">HttpContext.</param>
        /// <returns>Account of user associated with session.</returns>
        public Account GetAccount(IHttpContext context)
        {
            if (IsAuthenticated(context))
            {
                var sessiontoken = context.Request.Cookies[vaultServer.SessionHandler.CookieName];
                return accounts[tokens[sessiontoken].Username];
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// Serialize account and token information to disk.
        /// </summary>
        public void SaveToDisk()
        {
            try
            {
                using (Stream stream = File.Open("accounts.bin", FileMode.Create))
                {
                    BinaryFormatter bin = new BinaryFormatter();
                    bin.Serialize(stream, accounts);
                }
                using (Stream stream = File.Open("tokens.bin", FileMode.Create))
                {
                    BinaryFormatter bin = new BinaryFormatter();
                    bin.Serialize(stream, tokens);
                }
                foreach(Account account in accounts.Values)
                {
                    account.SaveVaultToDisk();
                }
            }
            catch(Exception e)
            {
                
            }
        }
        /// <summary>
        /// Deserialize account and token information from disk.
        /// </summary>
        public void LoadFromDisk()
        {
            try
            {
                using (Stream stream = File.Open("accounts.bin", FileMode.Open))
                {
                    BinaryFormatter bin = new BinaryFormatter();
                    accounts = (Dictionary<string, Account>)bin.Deserialize(stream);
                }
                using (Stream stream = File.Open("tokens.bin", FileMode.Open))
                {
                    BinaryFormatter bin = new BinaryFormatter();
                    tokens = (Dictionary<string, AccessToken>)bin.Deserialize(stream);
                }
            }
            catch (Exception)
            {
            }
        }
        #endregion
        #region Private Methods
        /// <summary>
        /// Generate an xml response
        /// </summary>
        /// <param name="errorCode">Code of the error.</param>
        /// <param name="errorText">Description of the error.</param>
        /// <returns></returns>
        private string GetXMLResponse(string errorCode, string errorText)
        {
            string response = "<Response>";
            response += "<code>" + errorCode + "</code>";
            response += "<text>" + errorText + "</text>";
            response += "</Response>";
            return response;
        }
        /// <summary>
        /// Send an XML response.
        /// </summary>
        /// <param name="xml">XML string to be sent.</param>
        /// <param name="context">Http context.</param>
        private async void SendXMLResponse(string xml, IHttpContext context)
        {
            context.Response.AddHeader("Content-type", "text/xml");
            await context.Response.WriteAllAsync(xml);
        }
        /// <summary>
        /// Validates the authentication input.
        /// </summary>
        /// <param name="username">Provided username.</param>
        /// <param name="password">Provided password.</param>
        /// <returns></returns>
        private ErrorCodes ValidateInput(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrWhiteSpace(username))
            {
                return ErrorCodes.InvalidUsername;
            }
            else if (string.IsNullOrWhiteSpace(password) || string.IsNullOrEmpty(password))
            {
                return ErrorCodes.InvalidPassword;
            }
            else
            {
                return ErrorCodes.Success;
            }
        }
        #endregion
    }
}
