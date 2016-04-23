using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMClient.model.events
{
    public class ImServerErrorArgs : EventArgs
    {
        string _errorMsg;
    
        public ImServerErrorArgs(string errorMsg)
        {
            this._errorMsg = errorMsg;
        }

        public string ErrorMessage
        {
            get { return _errorMsg; }
        }
    }

    public delegate void ImServerErrorEventHandler(object sender, ImServerErrorArgs e);
}
