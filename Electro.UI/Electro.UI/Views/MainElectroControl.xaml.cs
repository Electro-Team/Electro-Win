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
using Electro.UI.Windows;

namespace Electro.UI.Views
{
    /// <summary>
    /// Interaction logic for MainElectroControl.xaml
    /// </summary>
    public partial class MainElectroControl : UserControl
    {
        public MainElectroControl()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            ElectroMessageBox.Show("HELLO MA BOI!");
        }
    }
}
