using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Electro.UI.ViewModels;
using Electro.UI.Windows;

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
                control.MinimizeToTrayButton.Visibility = Visibility.Hidden;
                control.MinimizeButton.Visibility = Visibility.Hidden;
            }
        }

        public WindowControl()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            var result = ElectroMessageBox.Show("Electro Service will be turned off.\nAre you willing to exit?",
                "Warning", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                Window parentWindow = Window.GetWindow(this);
                parentWindow?.Close();
            }
        }
        private void MinimizeToTrayButton_MinimizeToTray(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            if (((parentWindow.Content as Grid).DataContext as MainViewModel).DnsViewModel.IsTurnedOn)
            {
                parentWindow.Hide();
            }
            else
            {
                parentWindow?.Close();
            }
        }
        private void MinimizeButton_Minimize(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            parentWindow.WindowState = WindowState.Minimized;
        }

        private void MinimizeToTrayButton_MouseEnter(object sender, MouseEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow.WindowState == WindowState.Normal)
            {
                ToolTip maximize = new ToolTip
                {
                    Content = String.Format("Close")
                };
                (sender as Button).ToolTip = maximize;
            }
        }
    }
}
