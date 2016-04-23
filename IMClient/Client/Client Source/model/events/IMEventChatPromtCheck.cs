
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMClient.model.events
{

    public class ImReceivedMessageEventArgs : EventArgs
    {
        private IMLibrary.Message _message;

        public ImReceivedMessageEventArgs(IMLibrary.Message message)
        {
            this._message = message;
        }
        public IMLibrary.Message Message
        {
            get { return _message; }
        }
    }
    public delegate void IMChatPromtEventHandler(object sender, ImReceivedMessageEventArgs e);
}
