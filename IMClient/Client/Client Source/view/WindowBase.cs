using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfApplication1;

namespace IMClient.view
{
    public class WindowBase : Window
    {
        public void LabelErrorMessageAnimator(string errorMsg, Control control)
        {
            ((Label)control).Content = errorMsg;
            Storyboard sBoard = new Storyboard();
            TimeSpan timeLineDuration = TimeSpan.FromMilliseconds(500); //

            DoubleAnimation fadeIn = new DoubleAnimation()
            { From = 0.0, To = 1.0, Duration = new Duration(timeLineDuration) };

            DoubleAnimation fadeOut = new DoubleAnimation()
            { From = 1.0, To = 0.0, Duration = new Duration(timeLineDuration) };
            fadeOut.BeginTime = TimeSpan.FromSeconds(3);
            Storyboard.SetTargetName(fadeIn, control.Name);
            Storyboard.SetTargetProperty(fadeIn, new PropertyPath("Opacity", 1));
            sBoard.Children.Add(fadeIn);
            sBoard.Begin(control);

            Storyboard.SetTargetName(fadeOut, control.Name);
            Storyboard.SetTargetProperty(fadeOut, new PropertyPath("Opacity", 0));
            sBoard.Children.Add(fadeOut);
            sBoard.Begin(control);
        }

        protected void WindowBase_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}

