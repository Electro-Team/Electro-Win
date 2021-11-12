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

namespace Electro.UI.Controls
{
    /// <summary>
    /// Interaction logic for WindowControl.xaml
    /// </summary>
    public partial class WindowControl : UserControl
    {
        public bool CloseItem
        {
            get { return (bool)GetValue(CloseItemProperty); }
            set { SetValue(CloseItemProperty, value); }
        }

        public static readonly DependencyProperty CloseItemProperty =
            DependencyProperty.Register("CloseItem", typeof(bool), typeof(WindowControl),
                new PropertyMetadata(false, WindowControlChangedCallback));

        private static void WindowControlChangedCallback(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            WindowControl control = (WindowControl)d;
            control.CloseItem = (bool)e.NewValue;
            if (control.CloseItem)
            {
                control.CloseButton.Visibility = Visibility.Visible;
                control.MaximizeButton.Visibility = Visibility.Hidden;
                control.MinimizeButton.Visibility = Visibility.Hidden;
            }
        }

        public WindowControl()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            parentWindow?.Close();
        }
        private void MaximizeButton_Maximize(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow.WindowState == WindowState.Maximized)
            {
                parentWindow.WindowState = WindowState.Normal;
            }
            else if (parentWindow.WindowState == WindowState.Normal)
            {
                parentWindow.WindowState = WindowState.Maximized;
            }
        }
        private void MinimizeButton_Minimize(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            parentWindow.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_MouseEnter(object sender, MouseEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow.WindowState == WindowState.Maximized)
            {
                ToolTip restore = new ToolTip
                {
                    Content = String.Format("Restore")
                };
                (sender as Button).ToolTip = restore;
            }
            else if (parentWindow.WindowState == WindowState.Normal)
            {
                ToolTip maximize = new ToolTip
                {
                    Content = String.Format("Maximize")
                };
                (sender as Button).ToolTip = maximize;
            }
        }
    }
}
