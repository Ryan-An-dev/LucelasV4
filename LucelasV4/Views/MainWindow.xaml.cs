using LucelasV4.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace LucelasV4.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;
        public MainWindow()
        {
            InitializeComponent();
            initTimeLeft();
        }

        public void initTimeLeft()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(30);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            this.passwordBox.Password = "";
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
            if (e.ClickCount == 2)  // 더블 클릭인 경우
            {
                if (this.WindowState == WindowState.Normal)
                    this.WindowState = WindowState.Maximized;
                else
                {
                    this.WindowState = WindowState.Normal;
                }
                e.Handled = true;  // 이벤트 처리를 여기서 종료
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

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            MainWindowViewModel vm = (MainWindowViewModel)this.DataContext;
            vm.Password.Value = passwordBox.Password;
        }

    }
}
