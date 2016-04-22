using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IMLibrary
{
    [Serializable]
    public  class Client : INotifyPropertyChanged
    {

        public string[] VisibilityToString = new[] {"Online", "Offline", "Busy"};
        private string _userName;
        private bool _available;
        private Visibility _visibility;
        private string _visiblityAsString;
        private RSAParameters _publicKey;
        private string _fingerPrint;
        private List<string> _buddies; 

        public enum Visibility
        {
            Visible = 0,
            InVisible,
            Busy
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public Client(string userName, bool available, string fingerPrint, RSAParameters publicKey, List<string> buddyList)
        {
            this._userName = userName;
            this._available = available;
            this._fingerPrint = fingerPrint;
            this._publicKey = publicKey;
            this._buddies = buddyList;
        }

        public RSAParameters PublicKey
        {
            get { return _publicKey; }
            set
            {
                this._publicKey = value;
                NotifyPropertyChanged("PublicKey");

            }
        }

        public string FingerPrint
        {
            get { return _fingerPrint; }
            set
            {
                if (_fingerPrint != value)
                {
                    this._fingerPrint = value;
                    NotifyPropertyChanged("FingerPrint");
                }
            }
        }

        public List<string> Buddies
        {
           get { return _buddies; }
            set
            {
                if (_buddies != value)
                {
                    this._buddies = value;
                    NotifyPropertyChanged("Buddies");
                }
            }
        } 

        public string UserName
        {
            get { return this._userName; }
            set
            {
                if (_userName != value)
                {
                    this._userName = value;
                    NotifyPropertyChanged("UserName");
                }
            }
        }

        public Visibility PublicVisibility
        {
            get { return this._visibility; }
            set
            {
                if (_visibility != value)
                {
                    this._visibility = value;
                    this._visiblityAsString = VisibilityToString[((int)value)];
                    NotifyPropertyChanged("PublicVisibility");
                }
            }
        }


        public string VisibilityAsString
        {
            get { return this._visiblityAsString; }
        }



        public bool Available
        {
            get { return this._available; }
            set
            {
                if (_available != value)
                {
                    this._available = value;
                    NotifyPropertyChanged("Available");
                }
            }
        }

    }
}
