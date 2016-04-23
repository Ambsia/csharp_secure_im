using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMClient.model.events
{
    public class ImListReceivedFromServerArgs : EventArgs
    {
        private List<IMLibrary.Client> _clientList;

        public ImListReceivedFromServerArgs(List<IMLibrary.Client> clientList)
        {
            this._clientList = clientList;
        }

        public List<IMLibrary.Client> ClientList
        {
            get { return _clientList; }
        }

    }

    public delegate void ImListEventHandler(object sender, ImListReceivedFromServerArgs e);
}


