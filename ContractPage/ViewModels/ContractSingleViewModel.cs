using AddressSearchManager.Models;
using CommonModel.Model;
using DataAccess;
using DataAccess.NetWork;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog.Targets;
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
        public DelegateCommand SearchAddress { get; } 
        public DelegateCommand<string> AddContractItemButton { get; }
        private IRegionManager RegionManager { get; }
        public IContainerProvider ContainerProvider { get; }
        public ReactiveProperty<Payment>SelectedPayment { get; set; }
        public ReactiveProperty<Furniture> SelectedProduct { get; set; }
        public IDialogService dialogService { get; }
        public ContractSingleViewModel(IRegionManager regionManager, IContainerProvider containerProvider, IDialogService dialogService):base(regionManager)
        {
            ContainerProvider = containerProvider;
            this.dialogService = dialogService;
            SelectedPayment = new ReactiveProperty<Payment>().AddTo(disposable);
            SelectedProduct = new ReactiveProperty<Furniture>().AddTo(disposable);
            AddContractItemButton = new DelegateCommand<string>(ExecAddContractItemButton);
            RegionManager = regionManager;
            SaveButton = new DelegateCommand(SaveButtonExecute);
            DeleteButton = new DelegateCommand(DeleteButtonExecute);
            SearchAddress = new DelegateCommand(SearchAdressExcute);
            Contract = new ReactiveProperty<Contract>().AddTo(disposable);
            Title.Value = "신규등록";
        }

        private void ExecAddContractItemButton(string obj)
        {
            switch (obj) {
                case "AddProduct":

                    break;
                case "DeleteProduct":
                    this.Contract.Value.Product.Remove(this.SelectedProduct.Value);
                    break;
                case "AddPayment":
                    this.Contract.Value.Payment.Add(new Payment());
                    break;
                case "DeletePayment":
                    this.Contract.Value.Payment.Remove(this.SelectedPayment.Value);
                    break;
            }
        }

        private void SearchAdressExcute()
        {
            DialogParameters p = new DialogParameters();
            p.Add("Contractor", this.Contract.Value.Contractor.Value);
            this.dialogService.ShowDialog("SearchAdressPage", p, r => FindAddressItem(r), "CommonDialogWindow");
        }
        private void FindAddressItem(IDialogResult r)
        {
            //Contract ID 받아야되는데 
            if (r == null) return;
            if (r.Result == ButtonResult.OK)
            {
                if (!r.Parameters.ContainsKey("SelectedAddress")) return;
                else
                {
                    AddressDetail temp = null;
                    r.Parameters.TryGetValue("SelectedAddress", out temp);
                    if (this.Contract.Value != null) {
                        this.Contract.Value.Contractor.Value.Address.Value = temp.도로명주소1;
                    }
                    
                }
            }
            else
            {

            }
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

                        if (this.Contract.Value.isChanged) {
                            network.CreateContractHistory(this.Contract.Value.GetChangedItem());
                        }
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

        public override JObject GetChangedItem()
        {
            throw new NotImplementedException();
        }
    }
}
