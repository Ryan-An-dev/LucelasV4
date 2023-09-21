using DataAccess;
using DataAccess.Interface;
using DataAccess.NetWork;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Reactive.Disposables;
using System.Windows;

namespace LucelasV4.ViewModels
{
    public class MainWindowViewModel : BindableBase,ICMDReceiver
    {
        public ReactiveCommand<string> MenuSelectCommand { get; }

        private CompositeDisposable disposables = new CompositeDisposable();

        private IRegionManager regionmanager;

        private SocketClientV2 NetManager;

        private IContainerProvider _Container;

        INetReceiver _Receiver = null;

        public ReactiveProperty<Visibility> ConnectionCheck { get; set; }

        public MainWindowViewModel(IRegionManager regionmanager, IContainerProvider Container)
        {
            //Login module 생성하고, True일때 DataAccess 의 IMPCommandDistributor 를 전역에서 사용하도록 등록해준다.
            this._Container = Container;
            this.regionmanager = regionmanager;
            NetManager = this._Container.Resolve<SocketClientV2>();
            NetManager.SetMainConnectCheck(this);
            this.ConnectionCheck = new ReactiveProperty<Visibility>(Visibility.Collapsed).AddTo(this.disposables);
            this.MenuSelectCommand = new ReactiveCommand<string>().WithSubscribe(i => this.ExecuteMenuSelectCommand(i)).AddTo(this.disposables);
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
            this.ConnectionCheck.Value = Visibility.Collapsed;
        }

        public void OnConeectedFail(object sender, Exception ex)
        {
            throw new NotImplementedException();
        }

        public void OnSendFail(object sender, Exception ex)
        {
            this.ConnectionCheck.Value = Visibility.Visible;
        }

        public void OnReceiveFail(object sender, Exception ex)
        {
            this.ConnectionCheck.Value = Visibility.Visible;
        }
    }
}
