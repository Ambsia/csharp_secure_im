using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMClient.model
{
    public class MessagePad : INotifyPropertyChanged
    {
        private string _message = "";
        private string _pad = "Select one time pad to use me!";
        private int _encryptionMethodFlag = 0;

        public int EncryptionMethodFlag
        {
            get
            {
                return _encryptionMethodFlag;
            }
            set { _encryptionMethodFlag = value; }
        }

        public string Message
        {
            get
            {
                return _message;
            }
            set
            {
                if (value.Length > Pad.Length && _encryptionMethodFlag != 0)
                {
                    
                    throw new Exception("The pad must be equal to the length of the message.");
                }
                if (_message != value)
                {
                    _message = value;
                    OnPropertyChanged("Message");
                }
            }
        }


        public string Pad
        {
            get
            {
                return _pad;
            }
            set
            {
                if (value.Length < Message.Length && _encryptionMethodFlag != 0)
                {
                    
                    throw new Exception("Incorrect port..");
                }
                if (_pad != value)
                {
                    _pad = value;
                    OnPropertyChanged("Pad");
                    OnPropertyChanged("Message");
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
