using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Text.RegularExpressions;
using IMClient;
using WpfApplication1.model;
using System.Windows.Media.Animation;
using IMLibrary;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using IMClient.controller;
using IMClient.utils;
using IMClient.view;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private ImClientHandler _clientHandler;
        private AuthWindow _authWindow;
        private Host _hostModel;
        public MainWindow()
        {
            InitializeComponent();
            _hostModel = new Host();
            DataContext = _hostModel;
        }
        private void Connect(object sender, RoutedEventArgs e)
        {

            button.IsEnabled = false;
            

            switch (_hostModel.CheckDetails())
            {
                case Host.DETAILS_MATCHED:
                    try
                    {
                        _clientHandler = new ImClientHandler(txtIP.Text, Int32.Parse(txtPort.Text));
                        if (_clientHandler.IsServerAvailable())
                        {
                            _authWindow = new AuthWindow(_clientHandler)
                            {
                                Left = this.Left,
                                Top = this.Top
                            };
                            _authWindow.Show();
                            this.Hide();
                        }
                    }
                    catch (Exception ex)
                    {
                        LabelErrorMessageAnimator(ex.Message, lblErrorMessage);
                    }
                    break;
                case Host.DETAILS_UNMATCHED:
                    LabelErrorMessageAnimator("Both details fail to meet the requirements", lblErrorMessage);
                    break;
                case Host.IP_UNMATCHED:
                    LabelErrorMessageAnimator("IP Address doesn't meet necessary requirements", lblErrorMessage);
                    break;
                case Host.PORT_UNMATCHED:
                    LabelErrorMessageAnimator("Port doesn't meet necessary requirements", lblErrorMessage);
                    break;
                default:
                    LabelErrorMessageAnimator("Unspecified error occurred...", lblErrorMessage);
                    break;
            }
            button.IsEnabled = true;
        }

        private void PortGotFocus(object sender, RoutedEventArgs e)
        {
            if (txtPort.Text == "Port...")
                txtPort.Text = "";
        }

        private void PortLostFocus(object sender, RoutedEventArgs e)
        {
            if (txtPort.Text.Length == 0)
                txtPort.Text = "Port...";
        }

        private void IpLostFocus(object sender, RoutedEventArgs e)
        {
            if (txtIP.Text.Length == 0)
                txtIP.Text = "IP Address...";
        }

        private void IpGotFocus(object sender, RoutedEventArgs e)
        {
            if (txtIP.Text == "IP Address...")
                txtIP.Text = "";
        }

        private void WindowBase_Closed(object sender, EventArgs e)
        {
            try
            {
                Application.Current.Shutdown();
            }
            catch
            {
                //ignored
            }
        }
    }
}
