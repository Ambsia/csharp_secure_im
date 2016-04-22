using IMClient.model;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
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
using Color = System.Windows.Media.Color;

namespace IMClient
{
    /// <summary>
    /// Interaction logic for ChatDialogue.xaml
    /// </summary>
    /// 

    public partial class ChatDialogue : UserControl
    {
        public BindableStringBuilderWrapper RecieveText;
        private readonly MessagePad MessagePad;
        private BitmapImage fingerPrint;


        public ChatDialogue()
        {

            VerticalAlignment = VerticalAlignment.Top;
            HorizontalAlignment = HorizontalAlignment.Left;
            Height = 307;
            Width = 417;
            InitializeComponent();
            RecieveText = new BindableStringBuilderWrapper();
            MessagePad = new MessagePad();

            DataContext = new
            {
                receive = RecieveText,
                messagePad = MessagePad
            };
            txtOneTimePad.IsEnabled = false;

        }


        private void cmbEncMethod_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EncryptionStandard == null) return;

            EncryptionStandard.IsEnabled = (EncryptionMethod.SelectedIndex == 0);

            this.MessagePad.EncryptionMethodFlag = (EncryptionMethod.SelectedIndex == 0) ? 0 : 1;
            txtOneTimePad.IsEnabled = (EncryptionMethod.SelectedIndex != 0);

            txtOneTimePad.Text = txtOneTimePad.IsEnabled ? "Enter your key here..." : "Select one time pad to use me!";
        }

        private void txtOneTimePad_LostFocus(object sender, RoutedEventArgs e)
        {
            if (txtOneTimePad.Text.Length == 0)
                txtOneTimePad.Text = "Enter your key here...";
        }

        private void txtOneTimePad_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtOneTimePad.Text == "Enter your key here...")
                txtOneTimePad.Text = "";
        }

        private void txtRecieve_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        public void ChangeColourOfPrint()
        {
            try
            {
                Tuple<System.Drawing.Color, System.Drawing.Color> colourTuple = GenerateColourPairFromHash();

                if (System.IO.File.Exists(System.AppDomain.CurrentDomain.BaseDirectory + @"\Resources\" +
                    this.lblFingerPrint.Content.ToString().Replace(":", "").Substring(0, 5) + ".png"))
                    return;

                using (Bitmap imageWithNewColour = ChangeColor(colourTuple))
                {
                    if (imageWithNewColour == null) return;
                    imageWithNewColour.Save(System.AppDomain.CurrentDomain.BaseDirectory + @"\Resources\" +
                        this.lblFingerPrint.Content.ToString().Replace(":", "").Substring(0, 5) + ".png", ImageFormat.Png);
                    imageWithNewColour.Dispose();
                }
            }
            catch
            {
                //ignored
            }

        }

        public void LoadPrint()
        {
            try
            {
                fingerPrint = new BitmapImage();
                fingerPrint.BeginInit();
                fingerPrint.UriSource = new Uri(System.AppDomain.CurrentDomain.BaseDirectory + @"\Resources\" + this.lblFingerPrint.Content.ToString().Replace(":", "").Substring(0, 5) + ".png");

                fingerPrint.EndInit();
                this.FingerPrint.Source = fingerPrint;
            }
            catch
            {
                //ignored
            }
        }

        [SuppressMessage("ReSharper", "AccessToModifiedClosure")]
        public Tuple<System.Drawing.Color, System.Drawing.Color> GenerateColourPairFromHash()
        {
            //split the hash by the ':' character
            string[] strarray = lblFingerPrint.Content.ToString().Split(':'); 

            //create a byte array
            byte[] byteArray = new byte[8];
            int posInByteArray = 0;
            foreach (string str in strarray.TakeWhile(str => posInByteArray != 8))
            {
                //assign byte array with 8 byte values after converting them from base 16
                byteArray[posInByteArray++] = Convert.ToByte(str, 16);
            }
            //create 2 two colours with the byte array, the alpha is constant 255 as there were issues with very low alpha levels 
            System.Drawing.Color leftColour = System.Drawing.Color.FromArgb(255, byteArray[0], byteArray[1], byteArray[2]);
            System.Drawing.Color rightColour = System.Drawing.Color.FromArgb(255, byteArray[3], byteArray[4], byteArray[5]);

            return new Tuple<System.Drawing.Color, System.Drawing.Color>(leftColour, rightColour);
        }


        public static Bitmap ChangeColor(Tuple<System.Drawing.Color, System.Drawing.Color> colourPair)
        {
            try
            {
                Bitmap fingerPrintDefaultBmp = (Bitmap) System.Drawing.Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + @"\Resources\" + "fingerprint.png");
                Bitmap newFingerPrintBmp = new Bitmap(fingerPrintDefaultBmp.Width, fingerPrintDefaultBmp.Height);

                int pixlesBeforeChange = 131072;
                for (int widthPostion = 0; widthPostion < newFingerPrintBmp.Width; widthPostion++)
                {
                    for (int heightPosition = 0; heightPosition < newFingerPrintBmp.Height; heightPosition++)
                    {
                        var actualColour = fingerPrintDefaultBmp.GetPixel(widthPostion, heightPosition);

                        // if the pixel's alpha level isnt FF, then we must create a new pixel with the new colour
                        // combined with the actual alpha, otherwise we will have smooth black edges
                        if (actualColour.A != 255)
                        {
                            //decide the colour we're using left or right
                            var colour = pixlesBeforeChange <= 0 ? colourPair.Item2 : colourPair.Item1;
                            //set the colour with the correct alpha level
                            newFingerPrintBmp.SetPixel(widthPostion, heightPosition, System.Drawing.Color.FromArgb(actualColour.A, colour.R, colour.G, colour.B));
                            pixlesBeforeChange--;
                            continue;
                        }

                        if (pixlesBeforeChange <= 0)
                        {
                            //if we have changed colours, use the second colour (right)
                            newFingerPrintBmp.SetPixel(widthPostion, heightPosition, colourPair.Item2);
                            continue;
                        }
                        //if we are not changing colours, use the first colour (left)
                        newFingerPrintBmp.SetPixel(widthPostion, heightPosition, colourPair.Item1);

                        pixlesBeforeChange--;
                    }
                }
                return newFingerPrintBmp;

            }
            catch (Exception e)
            {
                return null;
            }
        }

        //public static Bitmap ChangeColor(System.Drawing.Color newColour)
        //{
        //    try
        //    {

        //        Bitmap fingerPrintDefaultBmp = (Bitmap) System.Drawing.Image.FromFile(System.AppDomain.CurrentDomain.BaseDirectory + @"\Resources\" + "fingerprint.png");

        //        Bitmap newFingerPrintBmp = new Bitmap(fingerPrintDefaultBmp.Width, fingerPrintDefaultBmp.Height);

        //        for (int widthPostion = 0; widthPostion < newFingerPrintBmp.Width; widthPostion++)
        //        {
        //            for (int heightPosition = 0; heightPosition < newFingerPrintBmp.Height; heightPosition++)
        //            {
        //                var actualColour = fingerPrintDefaultBmp.GetPixel(widthPostion, heightPosition);

        //                if (actualColour.A != 255) // if the pixel's alpha level isnt FF, then we must create a new pixel with the new colour, combined with the actual alpha, otherwise we will have smooth black edges
        //                {
        //                    System.Drawing.Color newColourWithAlpha = System.Drawing.Color.FromArgb(actualColour.A, newColour.R, newColour.G, newColour.B);
        //                    newFingerPrintBmp.SetPixel(widthPostion, heightPosition, newColourWithAlpha);
        //                    continue;
        //                }
        //                newFingerPrintBmp.SetPixel(widthPostion, heightPosition, newColour);
        //            }
        //        }
        //        return newFingerPrintBmp;

        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //}

        private void CopyToClipBoard(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(lblFingerPrint.Content.ToString());
        }


    }
}