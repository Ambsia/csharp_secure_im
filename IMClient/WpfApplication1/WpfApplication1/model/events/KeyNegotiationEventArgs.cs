using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMClient.model.events
{
    public class KeyNegotiationEventArgs : EventArgs
    {
        private Tuple<byte[], byte[]> _keyVectorCombo;
        private string _senderUsername;

        public KeyNegotiationEventArgs(Tuple<byte[], byte[]> keyVectorCombo, string senderUsername)
        {
            this._keyVectorCombo = keyVectorCombo;
            this._senderUsername = senderUsername;
        }

        public Tuple<byte[], byte[]> KeyVectorCombo
        {
            get { return _keyVectorCombo; }
        }

        public string SenderUsername
        {
            get { return _senderUsername; }
        }
    }

    public delegate void KeyNegotiationEventHandler(object sender, KeyNegotiationEventArgs e);
}

