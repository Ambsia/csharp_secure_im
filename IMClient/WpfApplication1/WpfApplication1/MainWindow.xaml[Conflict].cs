using System;
using System.Collections.Generic;
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
namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IMClientHandler _clientHandler;
        private AuthWindow _authWindow;

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        public void move_window(object sender, MouseButtonEventArgs e)
        {
            ReleaseCapture();
            SendMessage(new WindowInteropHelper(this).Handle,
                WM_NCLBUTTONDOWN, HT_CAPTION, 0);
        }

        private void EXIT(object sender, MouseButtonEventArgs e)
        {
            Environment.Exit(0);
        }

        private void Activate_Title_Icons(object sender, MouseEventArgs e)
        {
            //hover effect, make sure your grid is named "Main" or replace "Main" with the name of your grid
            Close_btn.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFD10D53"));
        }

        private void Deactivate_Title_Icons(object sender, MouseEventArgs e)
        {
            Close_btn.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF970338"));
        }

        private void Close_pressing(object sender, MouseButtonEventArgs e)
        {
            Close_btn.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF850835"));
        }

        private void PortGotFocus(object sender, RoutedEventArgs e)
        {

            if (txtPort.Text == "Port...")
            {
                txtPort.Text = "";
            }
        }

        private void PortLostFocus(object sender, RoutedEventArgs e)
        {
            if (txtPort.Text.Length == 0)
            {
                txtPort.Text = "Port...";
            }
        }

        private void IPLostFocus(object sender, RoutedEventArgs e)
        {
            if (txtIP.Text.Length == 0)
            {
                txtIP.Text = "IP Address...";
            }
        }

        private void IPGotFocus(object sender, RoutedEventArgs e)
        {
            if (txtIP.Text == "IP Address...")
            {
                txtIP.Text = "";
            }
        }



        private Host _hostModel;

        public MainWindow()
        {
            InitializeComponent();
            _hostModel = new Host();
            DataContext = _hostModel;

  
        }

        public void LabelErrorMessageAnimator(string errorMsg)
        {
            lblErrorMessage.Content = errorMsg;
            Storyboard sBoard = new Storyboard();
            TimeSpan timeLineDuration = TimeSpan.FromMilliseconds(500); //

            DoubleAnimation fadeIn = new DoubleAnimation()
            { From = 0.0, To = 1.0, Duration = new Duration(timeLineDuration) };

            DoubleAnimation fadeOut = new DoubleAnimation()
            { From = 1.0, To = 0.0, Duration = new Duration(timeLineDuration) };
            fadeOut.BeginTime = TimeSpan.FromSeconds(3);
            Storyboard.SetTargetName(fadeIn, lblErrorMessage.Name);
            Storyboard.SetTargetProperty(fadeIn, new PropertyPath("Opacity", 1));
            sBoard.Children.Add(fadeIn);
            sBoard.Begin(lblErrorMessage);

            Storyboard.SetTargetName(fadeOut, lblErrorMessage.Name);
            Storyboard.SetTargetProperty(fadeOut, new PropertyPath("Opacity", 0));
            sBoard.Children.Add(fadeOut);
            sBoard.Begin(lblErrorMessage);

        }

        private void Connect(object sender, RoutedEventArgs e)
        {
              txtIP.Text = "192.168.0.18";
        //   txtIP.Text = "10.34.99.94";
            //txtPort.Text = "443";
        
            try
            {
                _clientHandler = new IMClientHandler(txtIP.Text, Int32.Parse(txtPort.Text));
                if (_clientHandler.IsServerAvailable())
                {
                    _authWindow = new AuthWindow(_clientHandler);
                    _authWindow.Left = this.Left;
                    _authWindow.Top = this.Top;
                    _authWindow.Show();
                    this.Hide();
                }
                else
                {
                    LabelErrorMessageAnimator("Incorrect server details provided.");
                }
            }
            catch (Exception ex)
            {
                LabelErrorMessageAnimator("Unspecified error occured whilst connecting...");
            }

        }

    }
}
