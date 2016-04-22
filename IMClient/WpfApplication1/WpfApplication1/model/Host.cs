using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WpfApplication1.model
{
    public class Host : INotifyPropertyChanged
    {
        private string _ip = "IP Address...";
        private string _port = "Port...";

        Regex ipRegex = new Regex(@"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$");
        Regex portRegex = new Regex(@"^\d{1,6}$");

        public const int DETAILS_UNMATCHED = 0;
        public const int DETAILS_MATCHED = 1;
        public const int IP_UNMATCHED = 2;
        public const int PORT_UNMATCHED = 3;
        public Host() { }

        public int CheckDetails()
        {
            if (ipRegex.IsMatch(_ip) && portRegex.IsMatch(_port))
            {
                return DETAILS_MATCHED;
            }
            else if (!ipRegex.IsMatch(_ip) && !portRegex.IsMatch(_port))
            {
                return DETAILS_UNMATCHED;
            }
            else if (!ipRegex.IsMatch(_ip))
            {
                return IP_UNMATCHED;
            }
            else if (!portRegex.IsMatch(_port))
            {
                return PORT_UNMATCHED;
            }
            return -1;
        }


        public string IP
        {
            get
            {
                return _ip;
            }
            set
            {
                if (!ipRegex.IsMatch(value) && value.ToString() != "IP Address...") {
                    throw new Exception("Incorrect address..");
                }
                if (_ip != value)
                {
                    _ip = value;
                    OnPropertyChanged("IP");
                }
            }
        }


        public string Port
        {
            get
            {
                return _port;
            }
            set
            {
               if (!portRegex.IsMatch(value) && value.ToString() != "Port...")
                {
                    throw new Exception("Incorrect port..");
                }
                if (_port != value)
                {
                    _port = value;
                    OnPropertyChanged("Port");
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
