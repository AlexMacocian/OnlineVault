using OnlineVault.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace OnlineVault.Communication
{
    /// <summary>
    /// Client class for connections
    /// </summary>
    [Serializable]
    public class Account
    {
        #region Fields
        [NonSerialized]
        Vault vault;
        string username, password;
        #endregion
        #region Properties
        /// <summary>
        /// Username of the account
        /// </summary>
        public string Username { get => username; set => username = value; }
        /// <summary>
        /// Password of the account
        /// </summary>
        public string Password { get => password; set => password = value; }
        /// <summary>
        /// Vault associated with the account.
        /// </summary>
        public Vault Vault
        {
            get
            {
                if (vault == null)
                {
                    string path = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "vaults");
                    string encryptedVault = Encoding.ASCII.GetString(File.ReadAllBytes(path + "/" + username + ".vlt"));
                    string serializedVault = Cryptography.Crypto.Decrypt(encryptedVault, password);
                    vault = new Vault(serializedVault);
                }
                return vault;
            }
            set
            {
                vault = value;
            }
        }
        #endregion
        #region Constructors
        public Account(string username, string password)
        {
            this.username = username;
            this.password = password;
        }
        #endregion
        #region Public Methods
        /// <summary>
        /// Function checks if provided password matches the stored hashed password.
        /// </summary>
        /// <param name="password">Clear-text password.</param>
        /// <returns>True if password matches.</returns>
        public bool Authenticate(string password)
        {
            return this.password == Cryptography.Crypto.GenerateSHA512String(password);
        }
        /// <summary>
        /// Saves vault to disk.
        /// </summary>
        /// <param name="path">Folder to save vault in.</param>
        public void SaveVaultToDisk()
        {
            string serializedVault = vault.Serialize();
            string encryptedVault = Cryptography.Crypto.Encrypt(serializedVault, password);
            byte[] vaultBytes = Encoding.ASCII.GetBytes(encryptedVault);
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "vaults");
            FileStream fs = File.Create(path + "/" + username + ".vlt");
            fs.Write(vaultBytes, 0, vaultBytes.Length);
            fs.Close();
        }
        #endregion
        #region Private Methods
        #endregion
    }
}
