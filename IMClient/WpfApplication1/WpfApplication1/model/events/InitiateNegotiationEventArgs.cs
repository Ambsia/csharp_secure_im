using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMClient.model.events
{

    public class InitiateNegotiationEventArgs : EventArgs
    {
        private List<IMLibrary.Client> _clientList;

        public InitiateNegotiationEventArgs(List<IMLibrary.Client> client)
        {
            this._clientList = client;
        }

        public List<IMLibrary.Client> ClientList
        {
            get { return _clientList; }
        }

    }

    public delegate void NegotiationEventHandler(object sender, InitiateNegotiationEventArgs e);
}



