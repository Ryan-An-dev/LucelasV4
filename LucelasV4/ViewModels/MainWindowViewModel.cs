using CommonModel;
using DataAccess;
using DataAccess.Interface;
using DataAccess.NetWork;
using DataAccess.Repository;
using HomePage.ViewModels;
using LogWriter;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SettingPage.ViewModels;
using System;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using MaterialDesignThemes.Wpf;
using LoginPage.ViewModels;
using CommonModule.Views;
using LucelasV4.Views;

namespace LucelasV4.ViewModels
{
    

    public class SampleItem : PrismCommonModelBase
    {
        public string? Title { get; set; }
        public PackIconKind SelectedIcon { get; set; }
        public PackIconKind UnselectedIcon { get; set; }
        private object? _notification = null;

        public object? Notification
        {
            get { return _notification; }
            set { SetProperty(ref _notification, value); }
        }

        public override void SetObserver()
        {
            throw new NotImplementedException();
        }
    }
    public class MainWindowViewModel : PrismCommonViewModelBase, ICMDReceiver
    {
        public ReactiveCollection<SampleItem> ListItems { get; set; }
        public ReactiveCommand<string> MenuSelectCommand { get; }
        public DelegateCommand PasswordCheckCommand { get; }
        public DelegateCommand AddTimerCommand { get; }
        private CompositeDisposable disposables = new CompositeDisposable();

        private IRegionManager regionmanager;

        private SocketClientV2 NetManager;

        private IContainerProvider _Container;

        INetReceiver _Receiver = null;

        private DispatcherTimer m_timer  = null;
        private DispatcherTimer timer;
        public ReactiveProperty<TimeSpan> TimeLeft { get; set; }

        public ReactiveProperty<string> TimeString { get; set; }

        private DispatcherTimer Loading_timer = null;


        public ReactiveProperty<Visibility> ConnectionCheck { get; set; }

        public ReactiveProperty<Visibility> SearchVisibility { get;set; }
        public ReactiveProperty<Visibility> Lock { get; set; }
        public ReactiveProperty<string> ID { get; set; }
        public ReactiveProperty<string> Password { get; set; }
        public ReactiveProperty<string> originPassword { get; set; }
        public MainWindowViewModel(IRegionManager regionmanager, IContainerProvider Container)
        {
            //Login module 생성하고, True일때 DataAccess 의 IMPCommandDistributor 를 전역에서 사용하도록 등록해준다.
            this.TimeLeft = new ReactiveProperty<TimeSpan>().AddTo(this.disposables);
            this.AddTimerCommand = new DelegateCommand(ExecAddTimerCommand);
            this.Lock = new ReactiveProperty<Visibility>(Visibility.Collapsed).AddTo(this.disposables);
            this.ID = new ReactiveProperty<string>().AddTo(this.disposables);
            this.originPassword = new ReactiveProperty<string>().AddTo(this.disposables);
            this.Password = new ReactiveProperty<string>().AddTo(this.disposables);
            this.TimeString = new ReactiveProperty<string>().AddTo(this.disposables);  
            this._Container = Container;
            this.regionmanager = regionmanager;
            this.SearchVisibility = new ReactiveProperty<Visibility>().AddTo(this.disposables);
            this.ConnectionCheck = new ReactiveProperty<Visibility>(Visibility.Visible).AddTo(this.disposables);
            this.PasswordCheckCommand = new DelegateCommand(ExecutePasswordCheckCommand);
            this.MenuSelectCommand = new ReactiveCommand<string>().WithSubscribe(i => this.ExecuteMenuSelectCommand(i)).AddTo(this.disposables);
            this.ListItems = new ReactiveCollection<SampleItem>().AddTo(this.disposables);
            init();
            
        }

        private void ExecAddTimerCommand()
        {
            TimeLeft.Value = TimeSpan.FromMinutes(10);
        }

        public void loginInfo() {
            LoginViewModel instance = _Container.Resolve<LoginViewModel>();
            this.ID.Value = instance.ID.Value;
            this.originPassword.Value = instance.Password.Value;
        }
        private void ExecutePasswordCheckCommand()
        {
            if (this.originPassword.Value == this.Password.Value)
            {
                Lock.Value = Visibility.Collapsed;
                this.Password.Value = string.Empty;
            }
            else { 
                MessageBox.Show("비밀번호가 틀렸습니다.");
                return;
            }
        }

        public void initTimeLeft()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            TimeLeft.Value = TimeSpan.FromMinutes(10);
            timer.Start();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (TimeLeft.Value.TotalSeconds <= 0)
            {
                timer.Stop();
                Lock.Value = Visibility.Visible;
            }
            else
            {
                TimeLeft.Value = TimeLeft.Value.Add(TimeSpan.FromSeconds(-1));
                TimeString.Value = TimeLeft.Value.ToString(@"mm\:ss");
            }
        }
        private void init()
        {
            _Container.Resolve<HomePageViewModel>().MenuSelectCommand = this.MenuSelectCommand;
            ListItems = new()
            {
                new SampleItem
                {
                    Title = "Payment",
                    SelectedIcon = PackIconKind.CreditCard,
                    UnselectedIcon = PackIconKind.CreditCardOutline,
                },
                new SampleItem
                {
                    Title = "Home",
                    SelectedIcon = PackIconKind.Home,
                    UnselectedIcon = PackIconKind.HomeOutline,
                },
                new SampleItem
                {
                    Title = "Special",
                    SelectedIcon = PackIconKind.Star,
                    UnselectedIcon = PackIconKind.StarOutline,
                },
                new SampleItem
                {
                    Title = "Shared",
                    SelectedIcon = PackIconKind.Users,
                    UnselectedIcon = PackIconKind.UsersOutline,
                },
                new SampleItem
                {
                    Title = "Files",
                    SelectedIcon = PackIconKind.Folder,
                    UnselectedIcon = PackIconKind.FolderOutline,
                },
                new SampleItem
                {
                    Title = "Library",
                    SelectedIcon = PackIconKind.Bookshelf,
                    UnselectedIcon = PackIconKind.Bookshelf,
                },
            };
        }

        public void initLoadingTimer()
        {
            this.Loading_timer = new DispatcherTimer();
            this.Loading_timer.Interval = TimeSpan.FromMilliseconds(300);
            this.Loading_timer.Tick += Loading_timer_Elapsed;
            this.Loading_timer.Start();
        }
        public void initSettingTimer()
        {
            this.m_timer = new DispatcherTimer();
            this.m_timer.Interval = TimeSpan.FromMilliseconds(300);
            this.m_timer.Tick += initSettingTimer_Elapsed;
            this.m_timer.Start();
        }

        private void initSettingTimer_Elapsed(object sender, EventArgs e)
        {
            SettingPageViewModel instance = _Container.Resolve<SettingPageViewModel>();
            if (instance.IsLoading.Value)
            {
                this.SearchVisibility.Value = Visibility.Visible;
            }
            else
            {
                this.SearchVisibility.Value = Visibility.Collapsed;
                this.m_timer.Tick -= initSettingTimer_Elapsed;
                this.m_timer.Stop();
                this.m_timer = null;
            }
        }

        private void Loading_timer_Elapsed(object sender, EventArgs e)
        {
            HomePageViewModel home = _Container.Resolve<HomePageViewModel>();
            if (home.IsLoading.Value)
            {

            }
            else {
                this.Loading_timer.Tick -= Loading_timer_Elapsed;
                this.Loading_timer.Stop();
                this.Loading_timer = null;
                initSettingTimer();
                SettingPageViewModel instance = _Container.Resolve<SettingPageViewModel>();
                instance.initData();
            }
        }

        private void ExecuteMenuSelectCommand(string SelectedItem)
        {
            if (SelectedItem.Contains("_"))
            {
                string item = SelectedItem.Split('_')[0];
                string option = SelectedItem.Split('_')[1];
                NavigationParameters temp = new NavigationParameters();
                temp.Add("object", option);
                if (SelectedItem != string.Empty)
                {
                    regionmanager.RequestNavigate("ContentRegion", item, temp);
                }
            }
            else {
                if (SelectedItem != string.Empty)
                {
                    regionmanager.RequestNavigate("ContentRegion", SelectedItem);
                }
            }
            
        }

        public void OnRceivedData(ErpPacket packet)
        {
            throw new NotImplementedException();
        }

        public void OnConnected()
        {
            ErpLogWriter.LogWriter.Trace(string.Format("연결됨"));
            this.ConnectionCheck.Value = Visibility.Collapsed;
            if (this.m_timer != null) { 
                this.m_timer.Stop();
                this.m_timer = null;
            }
        }

        public void Reconnect() {
            ErpLogWriter.LogWriter.Trace(string.Format("재접속 시도 돌기 시작"));
            if (m_timer != null) {
                return;
            }
           
        }

        private void M_timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            ErpLogWriter.LogWriter.Trace(string.Format("재접속 시도 타이머 함수 시작"));
            SocketClientV2 socket = this._Container.Resolve<SocketClientV2>();
            if (socket.state == ConnectState.Disconnected) { 
                LoginRepository temp = this._Container.Resolve<LoginRepository>();
                socket.Receiver = temp;
                temp.Reconnect();
            }
        }

        public void OnConeectedFail(object sender, Exception ex)
        {
            this.ConnectionCheck.Value = Visibility.Visible;
            //Reconnect();
        }

        public void OnSendFail(object sender, Exception ex)
        {
            this.ConnectionCheck.Value = Visibility.Visible;
            //Reconnect();
        }

        public void OnReceiveFail(object sender, Exception ex)
        {
            this.ConnectionCheck.Value = Visibility.Visible;
            //Reconnect();
        }

        internal void resetTimer()
        {
            if(timer!=null)
                timer.Stop();
            initTimeLeft();
        }
    }
}
