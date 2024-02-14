using LucelasV4.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace LucelasV4.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void Button_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = (this.WindowState == WindowState.Normal) ? WindowState.Maximized : WindowState.Normal;
        }

        private void Button_MouseLeftButtonDown_2(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void Button_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            MainWindowViewModel vm = (MainWindowViewModel)this.DataContext;
            vm.resetTimer();
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            MainWindowViewModel vm = (MainWindowViewModel)this.DataContext;
            vm.resetTimer();
        }
    }
}
