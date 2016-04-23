using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IMClient
{
   public class Client : INotifyPropertyChanged
    {
        private string _userName = "Username...";
        private string _userPassword = "Password...";
        private string _cryptKey;
        private bool _loggedIn;
        public Regex RegCheckUsername = new Regex(@"^[A-Za-z]{4,12}$");
    //  public Regex RegCheckPassword = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{10,20}$");
        public Regex RegCheckPassword = new Regex(@"^(?!.{31})(?=.{8})(?=.*[^A-Za-z])(?=.*[A-Z])(?=.*[a-z]).*$");
        public const int DetailsMatched = 0;
        public const int DetailsUnmatched = 1;
        public const int PasswordUnmatched = 2;
        public const int UsernameUnmatched = 3;

        
        public Client()
        {

        }

        public Client(string username, string userpassword)
        {
            this._userName = username;
            this._userPassword = userpassword;
        }

        public int CheckDetails()
        {
            if (RegCheckUsername.IsMatch(_userName) && RegCheckPassword.IsMatch(_userPassword)) {
                return DetailsMatched;
            } else if (!RegCheckUsername.IsMatch(_userName) && !RegCheckPassword.IsMatch(_userPassword)) 
            {
                return DetailsUnmatched;
            } else if (!RegCheckUsername.IsMatch(_userName))
            {
                return UsernameUnmatched;
            } else if (!RegCheckPassword.IsMatch(_userPassword))
            {
                return PasswordUnmatched;
            }
            return -1;
        }


        public string UserName
        {
            get { return this._userName; }
            set
            {
                if (!RegCheckUsername.IsMatch(value))
                {
                    _userName = value;
                    throw new Exception("Username incorrect; must be atleast 4 characters, but no more than 12.");
                }
                if (_userName != value)
                {
                    _userName = value;
                    OnPropertyChanged("UserName");
                }
            }
        }

        public string UserPassword
        {
            get { return this._userPassword; }
            set
            {
                if (_userPassword != value)
                {
                    _userPassword = value;
                    OnPropertyChanged("UserPassword");
                }
            }
        }

        public bool LoggedIn
        {
            get
            {
                return _loggedIn;
            }

            set
            {
               _loggedIn = value;
                OnPropertyChanged("LoggedIn");
            }
        }

       public string CryptKey
       {
           get
           {
               return _cryptKey;
           }
           set
           {
               _cryptKey = value;
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

