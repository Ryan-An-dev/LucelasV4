using AddressSearchManager.Models;
using CommonModel;
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
    public class ContractSingleViewModel : PrismCommonViewModelBase, INavigationAware, IDisposable, INetReceiver
    {
        /// <summary>
        /// 새로운 계약 : Collapse  // 기존계약 : Visible
        /// </summary>
        public ReactiveProperty<Visibility> IsNewContract { get; set; }
        /// <summary>
        /// 새로운 계약 : Visible  // 기존계약 : Collapse
        /// </summary>
        public ReactiveProperty<Visibility> IsNewContractReverse { get; set; }
        /// <summary>
        /// 편집,완료 버튼
        /// </summary>
        public ReactiveProperty<string> ButtonName { get; set; }
        /// <summary>
        /// 인적사항 편집모드 버튼 커멘드
        /// </summary>
        public DelegateCommand<string> SetEditMode { get; set; }
        /// <summary>
        ///  편집모드 = false // 읽기전용 = true;
        /// </summary>
        public ReactiveProperty<bool> CustIsReadOnly { get; set; }

        public DelegateCommand SaveButton { get; }
        public ReactiveProperty<string> Title { get; } = new();
        private readonly CompositeDisposable _disposable = new();
        public ReactiveProperty<Contract> Contract { get; set; }
        public DelegateCommand DeleteButton { get; }
        public DelegateCommand SearchAddress { get; }
        public DelegateCommand<string> SearchName { get; }
        public DelegateCommand<string> AddContractItemButton { get; }
        private IRegionManager RegionManager { get; }
        public IContainerProvider ContainerProvider { get; }
        public ReactiveProperty<Payment> SelectedPayment { get; set; }
        public ReactiveProperty<ContractedProduct> SelectedProduct { get; set; }
        public ReactiveCollection<Employee> EmployeeInfos { get; set; }
        public ReactiveProperty<Employee> SelectedEmployee { get; set; }
        public IDialogService dialogService { get; }
        public ContractSingleViewModel(IRegionManager regionManager, IContainerProvider containerProvider, IDialogService dialogService) : base(regionManager)
        {
            CustIsReadOnly = new ReactiveProperty<bool>(true).AddTo(disposable);
            SetEditMode = new DelegateCommand<string>(ExecSetEditMode);
            ButtonName = new ReactiveProperty<string>().AddTo(disposable);
            IsNewContract = new ReactiveProperty<Visibility>().AddTo(disposable);
            IsNewContractReverse = new ReactiveProperty<Visibility>().AddTo(disposable);
            IsNewContract.Subscribe(x =>
            {
                if (x == Visibility.Collapsed) //신규 계약
                {
                    IsNewContractReverse.Value = Visibility.Visible;
                }
                else { //기존계약
                    IsNewContractReverse.Value = Visibility.Collapsed;
                    ButtonName.Value = "수정";
                }
            });
            ContainerProvider = containerProvider;
            this.dialogService = dialogService;
            SelectedPayment = new ReactiveProperty<Payment>().AddTo(disposable);
            SelectedProduct = new ReactiveProperty<ContractedProduct>().AddTo(disposable);
            AddContractItemButton = new DelegateCommand<string>(ExecAddContractItemButton);
            RegionManager = regionManager;
            SaveButton = new DelegateCommand(SaveButtonExecute);
            DeleteButton = new DelegateCommand(DeleteButtonExecute);
            SearchAddress = new DelegateCommand(SearchAdressExcute);
            SearchName = new DelegateCommand<string>(SearchNameExcute);
            Contract = new ReactiveProperty<Contract>().AddTo(disposable);
            Title.Value = "신규등록";
            EmployeeInfos = new ReactiveCollection<Employee>().AddTo(disposable);

            SelectedEmployee = new ReactiveProperty<Employee>().AddTo(disposable);
        }

        
        private void ExecSetEditMode(string obj)
        {
            switch (obj) {
                case "수정":
                    CustIsReadOnly.Value = false; //편집모드 진입
                    this.Contract.Value.Contractor.Value.ClearJson(); //변경내역 초기화
                    ButtonName.Value = "완료";
                    break;

                case "완료":
                    CustIsReadOnly.Value = true; //변경내역 저장 로직
                    if (this.Contract.Value.Contractor.Value.isChanged) {
                        using (var network = this.ContainerProvider.Resolve<DataAgent.CustomerDataAgent>())
                        {
                            JObject jobj = new JObject();
                            network.SetReceiver(this);
                            jobj["customer_id"] = this.Contract.Value.Contractor.Value.Id.Value;
                            jobj["changed_item"] = this.Contract.Value.Contractor.Value.ChangedItem;
                            network.UpdateCustomerList(jobj);
                        }
                    }
                    ButtonName.Value = "수정";
                    break;
            }
        }

        private void ExecAddContractItemButton(string obj)
        {
            switch (obj) {
                case "AddProduct":
                    SearchCompanySelectExcute();
                    break;
                case "DeleteProduct":
                    this.Contract.Value.Product.Remove(this.SelectedProduct.Value);
                    break;
                case "AddPayment":
                    AddPaymentExcute();
                    break;
                case "DeletePayment":
                    this.Contract.Value.Payment.Remove(this.SelectedPayment.Value);
                    break;
            }
        }
        private void AddPaymentExcute()
        {
            int total = this.Contract.Value.Price.Value;
            foreach (Payment inner in this.Contract.Value.Payment) {
                total-=inner.Price.Value;
            }
            DialogParameters p = new DialogParameters();
            p.Add("object", new Payment());
            p.Add("total", total);
            this.dialogService.ShowDialog("AddPaymentPage", p, r => SetPayment(r), "CommonDialogWindow");
        }
        private void SetPayment(IDialogResult r)
        {
            if (r == null) return;
            if (r.Result == ButtonResult.OK)
            {
                if (!r.Parameters.ContainsKey("object")) return;
                else
                {
                    Payment temp = null;
                    r.Parameters.TryGetValue("object", out temp);
                    if (this.Contract.Value != null)
                    {
                        this.Contract.Value.Payment.Add(temp);
                    }
                }
            }
            else
            {

            }
        }
        private void SearchCompanySelectExcute() {
            DialogParameters p = new DialogParameters();
            p.Add("object", null);
            this.dialogService.ShowDialog("SearchCompanyPage", p, r => SetProduct(r), "CommonDialogWindow");
        }
        private void SetProduct(IDialogResult r) {
            if (r == null) return;
            if (r.Result == ButtonResult.OK)
            {
                if (!r.Parameters.ContainsKey("object")) return;
                else
                {
                    Product temp = null;
                    r.Parameters.TryGetValue("object", out temp);
                    if (this.Contract.Value != null)
                    {
                        ContractedProduct pro = new ContractedProduct();
                        pro.ForTotal += this.Contract.Value.TotalPrice;
                        pro.SetSub();
                        pro.FurnitureInventory.Value = temp;
                        this.Contract.Value.Product.Add(pro);
                    }
                }
            }
            else
            {

            }
        }

        private void SearchAdressExcute()
        {
            DialogParameters p = new DialogParameters();
            p.Add("Contractor", this.Contract.Value.Contractor.Value);
            this.dialogService.ShowDialog("SearchAdressPage", p, r => FindAddressItem(r), "CommonDialogWindow");
        }
        private void SearchNameExcute(string name)
        {
            DialogParameters p = new DialogParameters();
            p.Add("object", name);
            this.dialogService.ShowDialog("SearchNamePage", p, r => FindNameItem(r), "CommonDialogWindow");
        }
        private void FindAddressItem(IDialogResult r)
        {
            //Contract ID 받아야되는데 
            if (r == null) return;
            if (r.Result == ButtonResult.OK)
            {
                if (!r.Parameters.ContainsKey("object")) return;
                else
                {
                    AddressDetail temp = null;
                    r.Parameters.TryGetValue("object", out temp);
                    if (this.Contract.Value != null) {
                        this.Contract.Value.Contractor.Value.Address.Value = temp.도로명주소1;
                    }
                }
            }
            else
            {

            }
        }
        private void FindNameItem(IDialogResult r)
        {
            //Contract ID 받아야되는데 
            if (r == null) return;
            if (r.Result == ButtonResult.OK)
            {
                if (!r.Parameters.ContainsKey("object")) return;
                else
                {
                    Customer temp = null;
                    r.Parameters.TryGetValue("object", out temp);
                    if (this.Contract.Value != null)
                    {
                        this.Contract.Value.Contractor.Value = temp;
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
        public bool IsNavigationTarget(NavigationContext navigationContext) => false;

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            this.Dispose();
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            SettingPageViewModel temp = this.ContainerProvider.Resolve<SettingPageViewModel>("GlobalData");
            foreach (Employee emp in temp.EmployeeInfos)
            {
                this.EmployeeInfos.Add(emp);
            }
            Contract contract = navigationContext.Parameters["Contract"] as Contract;
            
            if (Contract == null)
            {
                Title.Value = "신규계약 추가";
                IsNewContract.Value = Visibility.Collapsed;
                this.Contract.Value = new Contract();
            }
            else
            {
                Title.Value = "계약 내역 수정";
                IsNewContract.Value = Visibility.Visible;
                this.Contract.Value = contract;
                //하나하나에 값 재할당 해줘야한다. 벨류 안바뀌게 
            }
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

    }
}
