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

namespace LucelasV4.ViewModels
{
    public class MainWindowViewModel : PrismCommonViewModelBase, ICMDReceiver
    {
        public ReactiveCommand<string> MenuSelectCommand { get; }

        private CompositeDisposable disposables = new CompositeDisposable();

        private IRegionManager regionmanager;

        private SocketClientV2 NetManager;

        private IContainerProvider _Container;

        INetReceiver _Receiver = null;

        private DispatcherTimer m_timer  = null;


        private DispatcherTimer Loading_timer = null;


        public ReactiveProperty<Visibility> ConnectionCheck { get; set; }

        public ReactiveProperty<Visibility> SearchVisibility { get;set; }
        public MainWindowViewModel(IRegionManager regionmanager, IContainerProvider Container)
        {
            //Login module 생성하고, True일때 DataAccess 의 IMPCommandDistributor 를 전역에서 사용하도록 등록해준다.
            this._Container = Container;
            this.regionmanager = regionmanager;
            this.SearchVisibility = new ReactiveProperty<Visibility>().AddTo(this.disposables);
            this.ConnectionCheck = new ReactiveProperty<Visibility>(Visibility.Visible).AddTo(this.disposables);
            this.MenuSelectCommand = new ReactiveCommand<string>().WithSubscribe(i => this.ExecuteMenuSelectCommand(i)).AddTo(this.disposables);
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
            if (SelectedItem != string.Empty) {
                regionmanager.RequestNavigate("ContentRegion", SelectedItem);
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
    }
}
