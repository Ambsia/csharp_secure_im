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
using System.Windows.Shapes;

namespace IMClient
{
    /// <summary>
    /// Interaction logic for demo.xaml
    /// </summary>
    public partial class demo : WindowTest
    {
        public demo() : base()
        {
            InitializeComponent();
           
        }


        public override void EXIT(object sender, MouseButtonEventArgs e)
        {
            base.EXIT(sender,e);
        }

        public override void move_window(object sender, MouseButtonEventArgs e)
        {
            base.move_window(sender, e);
        }


    }
}
