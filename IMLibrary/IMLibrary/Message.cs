using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace IMLibrary
{
    [Serializable]
    public sealed class Message
    {
        private string _messageSent;
        private string _to;
        private string _from;
        private bool _syncFlag;
        private int _encryptionMethodFlag;
        private int _encryptionStandardFlag;

        public Message(string messageSent, string to, string from, int standardFlag, int methodFlag, bool syncFlag)
        {
            this._messageSent = messageSent;
            this._to = to;
            this._from = from;
            this._encryptionStandardFlag = standardFlag;
            this._encryptionMethodFlag = methodFlag;
            this._syncFlag = syncFlag;
        }

        public bool SyncFlag
        {
            get { return _syncFlag; }
            set { _syncFlag = value; }
        }

        public int EncryptionStandardFlag
        {
            get { return _encryptionStandardFlag; }
            set { _encryptionStandardFlag = value; }
        }

        public int EncryptionMethodFlag
        {
            get { return _encryptionMethodFlag; }
            set { _encryptionMethodFlag = value; }
        }

        public string To
        {
            get { return _to; }
            set { _to = value; }
        }

        public string From
        {
            get { return _from; }
            set { _from = value; }
        }

        public string MessageSent
        {
            get { return _messageSent; }
            set { _messageSent = value; }
        }
    }
}
