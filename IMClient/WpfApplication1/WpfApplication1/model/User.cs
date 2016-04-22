using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IMClient.model
{
    [Serializable]
    public class User : IMLibrary.Client
    {
        private int _receivedMessages;
        private bool _newMessages;

        public User(string userName, bool available, string fingerPrint, RSAParameters publicKey, List<string> buddyList, int receivedMessages) 
            : base(userName, available, fingerPrint, publicKey, buddyList)
        {
            this._receivedMessages = receivedMessages;
            this._newMessages = false;
        }

        public bool NewMessages
        {
            get { return _newMessages; }
            set
            {
                if (_newMessages != value)
                {
                    this._newMessages = value;
                    NotifyPropertyChanged("NewMessages");
                }
            }
        }

        public int Received
        {
            get { return _receivedMessages; }
            set
            {
                if (_receivedMessages != value)
                {
                    this._receivedMessages = value;
                    NotifyPropertyChanged("Received");
                }
            }

        }

    }
}
