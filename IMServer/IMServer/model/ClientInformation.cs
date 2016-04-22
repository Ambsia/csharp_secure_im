using IMServer.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IMServer
{
    [Serializable]
    public class ClientInformation
    {
        private readonly string _userName;
        private readonly string _userPassword;
        private string _salt;
        private RSAParameters _key;
        private string _fingerPrint;
       [NonSerialized] private bool _loggedIn;
       [NonSerialized] private ClientHandler _clientConnection;
        private List<string> _buddies; 
        public ClientInformation(string user, string unhashedPassword)
        {
            this._userName = user;
            this._salt = Cryptography.GenerateSalt();
            this._userPassword = Cryptography.HashPassword(unhashedPassword, _salt);
        }
        public ClientHandler ClientConnection
        {
            get { return this._clientConnection; }
            set { this._clientConnection = value; }
        }

        public bool LoggedIn
        {
            get { return this._loggedIn; }
            set { this._loggedIn = value; }
        }

        public string Username {
            get { return this._userName; }
        }

        public string Password {
            get { return this._userPassword; }
        }

        public string Salt
        {
            get { return this._salt; }
            set { this._salt = value; }
        }
        public string FingerPrint
        {
            get { return this._fingerPrint; }
            set { this._fingerPrint = value; }
        }

        public List<string> Buddies
        {
            get { return this._buddies; }
            set { this._buddies = value; }
        } 
        public RSAParameters PublicKey
        {
            get { return this._key; }
            set { this._key = value; }
        }

        public override string ToString()
        {
            return Environment.NewLine + "Username= " + this._userName + Environment.NewLine +
                   "Password= " + this.Password + Environment.NewLine +
                   "Salt= " + this._salt + Environment.NewLine +
                   "Logged in= " + this._loggedIn;
        }
    }
}
