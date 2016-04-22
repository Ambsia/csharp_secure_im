using IMClient;
using IMClient.controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using IMClient.model.events;
using IMClient.view;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for AuthWindow.xaml
    /// </summary>
    public partial class AuthWindow : WindowBase
    {

        private void UsernameGotFocus(object sender, RoutedEventArgs e)
        {

            if (txtUsername.Text == "Username...")
            {
                txtUsername.Text = "";
            }
        }

        private void UsernameLostFocus(object sender, RoutedEventArgs e)
        {
            if (txtUsername.Text.Length == 0)
            {
                txtUsername.Text = "Username...";
            }
        }

        private void PasswordLostFocus(object sender, RoutedEventArgs e)
        {
            if (txtPassword.Password.Length == 0)
            {
                txtPassword.Password = "Password...";
            }
        }

        private void PasswordGotFocus(object sender, RoutedEventArgs e)
        {
            if (txtPassword.Password == "Password...")
            {
                txtPassword.Password = "";
            }
        }


        private ImClientHandler _clientHandler;
        private Client _clientModel;


        private ImServerErrorEventHandler _serverErrorEventHandler;
        private ImMainWindowNew _mainWindow;

        public AuthWindow(ImClientHandler clientHandler)
        {
            InitializeComponent();
            
            this._clientHandler = clientHandler;
            this._clientModel = new Client();
            DataContext = _clientModel;
        
            _clientHandler.LoginAuthorised += LoginAuthorised;
            _serverErrorEventHandler = ServerError;
            _clientHandler.ServerError += _serverErrorEventHandler;

            txtPassword.ToolTip = "Password must be atleast 10-20 characters long, contain a\n uppercase character, a number and a special character.";
            txtUsername.ToolTip = "Username must be atleast 4-12 characters long,\n and only contain characters.";

        }

        private void ServerError(object sender, ImServerErrorArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                
                lblErrorMessage.Content = e.ErrorMessage;
                LabelErrorMessageAnimator(e.ErrorMessage, lblErrorMessage);
            }));
         
        }


        private void LoginAuthorised(object sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => {
                Hide();
                _mainWindow = new ImMainWindowNew(this._clientHandler);
                _mainWindow.Show();
            }));
        }

        private void Register(object sender, RoutedEventArgs e)
        {
            switch (_clientModel.CheckDetails())
            {
                case Client.DetailsMatched:
                    _clientModel.UserPassword = txtPassword.Password;
                    _clientHandler.Register(_clientModel); break;
                case Client.DetailsUnmatched:
                    LabelErrorMessageAnimator("Both details fail to meet the requirements.", lblErrorMessage);
                    break;
                case Client.UsernameUnmatched:
                    LabelErrorMessageAnimator("Username doesn't meet necessary requirements.", lblErrorMessage);
                    break;
                case Client.PasswordUnmatched:
                    LabelErrorMessageAnimator("Password doesn't meet necessary requirements.", lblErrorMessage);
                    break;
            }
           
        }

        private void Login(object sender, RoutedEventArgs e)
        {
            _clientModel.UserPassword = txtPassword.Password;
            _clientHandler.Login(_clientModel);
        }

 
    }
}
