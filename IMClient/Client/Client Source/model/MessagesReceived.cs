using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMClient.model
{
    public class Messages : INotifyPropertyChanged
    {
        private string _received = "hey";
        public string Received
        {
            get { return _received; }
            set
            {
                if (this._received != value)
                {
                    _received = value;
                    OnPropertyChanged("Received");
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }

        }


    }
}
