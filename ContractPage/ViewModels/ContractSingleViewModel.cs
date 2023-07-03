using CommonModel.Model;
using DataAccess;
using DataAccess.NetWork;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json.Linq;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using PrsimCommonBase;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SettingPage.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace ContractPage.ViewModels
{
    public class ContractSingleViewModel : PrismCommonModelBase, INavigationAware, IDisposable, INetReceiver
    {
        public DelegateCommand SaveButton { get; }
        public ReactiveProperty<string> Title { get; } = new();
        private readonly CompositeDisposable _disposable = new();
        public ReactiveProperty<Contract> Contract { get; set; }
        public DelegateCommand DeleteButton { get; }
        private IRegionManager RegionManager { get; }
        public IContainerProvider ContainerProvider { get; }
        public DelegateCommand<string> AddProductItemButton { get; }
        public IDialogService dialogService { get; }
        public ContractSingleViewModel(IRegionManager regionManager, IContainerProvider containerProvider, IDialogService dialogService):base(regionManager)
        {
            ContainerProvider = containerProvider;
            dialogService = dialogService;

            //AddProductItemButton = new DelegateCommand<string>(ExeAddProductItemButton);
            RegionManager = regionManager;
            SaveButton = new DelegateCommand(SaveButtonExecute);
            DeleteButton = new DelegateCommand(DeleteButtonExecute);
            Contract = new ReactiveProperty<Contract>().AddTo(disposable);
            Title.Value = "신규등록";
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            this.Dispose();
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var Contract = navigationContext.Parameters["Contract"] as ReactiveProperty<Contract>;
            SettingPageViewModel temp = this.ContainerProvider.Resolve<SettingPageViewModel>("GlobalData");
            //foreach (var item in temp.CategoryInfos)
            //{
            //    this.CategoryInfos.Add(item);
            //}
            //this.VisibilityContract.Value = Visibility.Visible; //contract 추가 
            if (Contract == null)
            {
                Title.Value = "신규계약 추가";
                this.Contract.Value = new Contract();
                this.Contract.Value.Month.Value = DateTime.Now;
            }
            else
            {
                Title.Value = "계약 내역 수정";
                
                //하나하나에 값 재할당 해줘야한다. 벨류 안바뀌게 
            }
            //if (this.ReceiptModel.Value.ReceiptType.Value == ReceiptType.Cash)
            //{
            //    IsCashOnly.Value = true;
            //}
        }
        private void SaveButtonExecute()
        {
            Save(0);
        }

        private void DeleteButtonExecute()
        {
            Save(1);
        }

        private void Save(int param)
        {
            using (var network = this.ContainerProvider.Resolve<DataAgent.ContractDataAgent>())
            {
                JObject jobj = new JObject();
                network.SetReceiver(this);
                if (param == 0)
                { // Update
                    if (this.Title.Value == "신규계약 추가") //신규등록일경우
                    {
                        JObject inner = new JObject();
                        //inner["shi_type"] = (int)this.Contract.Value.IncomeCostType.Value;
                        //inner["shi_biz_type"] = this.Contract.Value.CategoryInfo.Value.CategoryId.Value;
                        //inner["shi_cost"] = this.Contract.Value.Money.Value;
                        //inner["shi_time"] = this.Contract.Value.Month.Value.ToString("yyyy-MM-dd HH:mm:ss");
                        //inner["shi_use_content"] = this.Contract.Value.Contents.Value;
                        //inner["shi_memo"] = this.Contract.Value.Memo.Value;
                        //inner["shi_use_name"] = this.Contract.Value.Tip.Value;
                        //jobj["create_history"] = inner;
                        network.CreateContractHistory(jobj);
                    }
                    else
                    { //내역수정일경우
                        //if (this.Contract.Value.IsChanged.Value)
                        //{
                        //    JObject inner = new JObject();
                        //    inner["shi_id"] = this.Contract.Value.ReceiptNo.Value;
                        //    inner["changed_property"] = this.Contract.Value.ChangedItem;
                        //    jobj["update_history"] = inner;
                        //    network.UpdateContract(jobj);
                        //}
                    }
                }
                else
                {
                    jobj["delete_history"] = this.Contract.Value.Id.Value;
                    network.DeleteContract(jobj);
                }
            }
        }

        public void OnRceivedData(ErpPacket packet) //완료 커맨드가 들어와야 첫화면으로 넘어간다.
        {
            string msg = Encoding.UTF8.GetString(packet.Body);
            try
            {
                JObject jobj = new JObject(JObject.Parse(msg));
                //ErpLogWriter.LogWriter.Trace(jobj.ToString());
            }
            catch (Exception e) { }
            switch ((COMMAND)packet.Header.CMD)
            {
                case COMMAND.UpdateBankHistory: //데이터 업데이트 완료
                case COMMAND.DeleteBankHistory: //데이터 삭제완료
                    Application.Current.Dispatcher.Invoke(() => {
                        DrawerHost.CloseDrawerCommand.Execute(Dock.Right, null);
                        this.Contract.Value.CompleteChangedData(); //변경완료 후 변수 초기화
                        Dispose();
                        regionManager.RequestNavigate("ContentRegion", nameof(ContractPage));
                    });
                    break;
            }
        }

        public void OnConnected()
        {
        }

        public void OnSent()
        {
        }
    }
}
