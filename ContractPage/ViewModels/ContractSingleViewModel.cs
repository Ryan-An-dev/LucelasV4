using AddressSearchManager.Models;
using CommonModel;
using CommonModel.Model;
using DataAccess;
using DataAccess.NetWork;
using LogWriter;
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
using System.Collections.ObjectModel;
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
        public ReactiveProperty<PrismCommonModelBase> SelectedItem { get; set; }
        public DelegateCommand RowDoubleClick { get; }
        public DelegateCommand RowPayDoubleClick { get; }
        public DelegateCommand<object> CheckBoxAccountCommand { get; set; }
        public ContractSingleViewModel(IRegionManager regionManager, IContainerProvider containerProvider, IDialogService dialogService) : base(regionManager)
        {
            
            CheckBoxAccountCommand = new DelegateCommand<object>(execCheckBoxAccountCommand);
            this.SelectedItem = new ReactiveProperty<PrismCommonModelBase>().AddTo(this.disposable);
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
                    CustIsReadOnly.Value = false; //편집모드 진입
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
            this.RowDoubleClick = new DelegateCommand(RowDoubleClickEvent);
            RowPayDoubleClick = new DelegateCommand(RowPayDoubleClickExec);
            SettingPageViewModel temp = ContainerProvider.Resolve<SettingPageViewModel>();
            foreach (Employee emp in temp.EmployeeListViewModel.List) {
                this.EmployeeInfos.Add(emp);
            }
        }

        private void execCheckBoxAccountCommand(object args)
        {
            if(args is Employee)
            {
                Employee emp = args as Employee;
                this.Contract.Value.isChanged = true;
            }
        }
        public void RowPayDoubleClickExec() {
            if (SelectedPayment.Value == null)
                return;
            int total = this.Contract.Value.Price.Value;
            foreach (Payment inner in this.Contract.Value.Payment)
            {
                total -= inner.Price.Value;
            }
            DialogParameters p = new DialogParameters();
            p.Add("object", SelectedPayment.Value);
            p.Add("total", total);
            this.dialogService.ShowDialog("AddPaymentPage", p, r => SetPayment(r), "CommonDialogWindow");
        }

        public void RowDoubleClickEvent()
        {
            if (SelectedProduct.Value == null)
                return;
            DialogParameters dialogParameters = new DialogParameters();
            SelectedProduct.Value.ClearJson();
            dialogParameters.Add("object", SelectedProduct.Value as ContractedProduct);

            dialogService.ShowDialog("FindInventoryItem", dialogParameters, r =>
            {
                try
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        ContractedProduct item = r.Parameters.GetValue<ContractedProduct>("object");
                        this.Contract.Value.isChanged = true;
                    }
                }
                catch (Exception) { }

            }, "CommonDialogWindow");
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
                            jobj["cui_id"] = this.Contract.Value.Contractor.Value.Id.Value;
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
                    this.Contract.Value.isChanged = true;
                    SearchCompanySelectExcute();
                    this.Contract.Value.TotalPrice();
                    ProductMemoCombine();
                    break;
                case "DeleteProduct":
                    JObject jobj = new JObject();
                    int id = this.SelectedProduct.Value.FurnitureInventory.Value.Id.Value;
                    if (id == 0)
                    {
                        this.Contract.Value.Product.Remove(this.SelectedProduct.Value);
                        return;
                    }
                    jobj["product_id"] = id;
                    jobj["product_order_id"] = this.SelectedProduct.Value.Id.Value;
                    jobj["action"] = 2;
                    if (this.Contract.Value.ChangedItem["product_list"] == null)
                    {
                        JArray jarr = new JArray();
                        jarr.Add(jobj);
                        this.Contract.Value.ChangedItem["product_list"] = jarr;
                    }
                    else
                    {
                        (this.Contract.Value.ChangedItem["product_list"] as JArray).Add(jobj);
                    }
                    this.Contract.Value.Product.Remove(this.SelectedProduct.Value);
                    this.Contract.Value.isChanged = true;
                    this.Contract.Value.TotalPrice();
                    ProductMemoCombine();
                    break;
                case "AddPayment":
                    this.Contract.Value.isChanged = true;
                    AddPaymentExcute();
                    break;
                case "DeletePayment":
                    JObject jobject = new JObject();
                    int pay_id = this.SelectedPayment.Value.PaymentId.Value;
                    if (pay_id == 0) {
                        this.Contract.Value.Payment.Remove(this.SelectedPayment.Value);
                        return;
                    }
                    jobject["payment_id"] = pay_id;
                    JObject inner_pay = new JObject();
                    jobject["action"] = 2;

                    if (this.Contract.Value.ChangedItem["payment"] == null)
                    {
                        JArray jarr = new JArray();
                        jarr.Add(jobject);
                        this.Contract.Value.ChangedItem["payment"] =jarr;
                    }
                    else {
                        (this.Contract.Value.ChangedItem["payment"] as JArray).Add(jobject);
                    }
                    this.Contract.Value.Payment.Remove(this.SelectedPayment.Value);
                    this.Contract.Value.isChanged = true;
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
                    if (temp != null && temp.PaymentId.Value != 0)//수정
                    {
                        if (this.Contract.Value != null)
                        {
                            this.Contract.Value.isChanged = true;
                            this.Contract.Value.Payment.Remove(this.Contract.Value.Payment.FirstOrDefault(x=>x.PaymentId.Value == temp.PaymentId.Value));
                            this.Contract.Value.Payment.Add(temp);
                        }
                    }
                    else { //추가
                        if (this.Contract.Value != null)
                        {
                            this.Contract.Value.isChanged = true;
                            this.Contract.Value.Payment.Add(temp);
                        }
                    }
                    
                }
            }
            else
            {

            }
        }
        private void ProductMemoCombine() {
            this.Contract.Value.ProductMemoCombine.Value = "";
            foreach (ContractedProduct temp in this.Contract.Value.Product) {
                this.Contract.Value.ProductMemoCombine.Value +="["+temp.FurnitureInventory.Value.Name.Value +"]" +"\r\n" + temp.Memo.Value+"\r\n"+"\r\n";
            }
        }
        private void SearchCompanySelectExcute() {
            DialogParameters p = new DialogParameters();
            ContractedProduct temp =new ContractedProduct();
            p.Add("object", temp);
            this.dialogService.ShowDialog("FindInventoryItem", p, r => SetProduct(r), "CommonDialogWindow");
        }
        private void SetProduct(IDialogResult r) {
            if (r == null) return;
            if (r.Result == ButtonResult.OK)
            {
                if (!r.Parameters.ContainsKey("object")) return;
                else
                {
                    ContractedProduct temp = null;
                    r.Parameters.TryGetValue("object", out temp);
                    if (this.Contract.Value != null)
                    {
                        temp.Action.Value = AddDelete.Add;
                        this.Contract.Value.Product.Add(temp);
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
                        this.Contract.Value.Contractor.Value.ClearJson();
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
            
            
            Contract contract = navigationContext.Parameters["Contract"] as Contract;
            
            if (contract == null)
            {
                Title.Value = "신규계약 추가";
                IsNewContract.Value = Visibility.Collapsed;
                this.Contract.Value = new Contract();
                SettingPageViewModel employee = this.ContainerProvider.Resolve<SettingPageViewModel>();
                foreach (Employee emp in employee.EmployeeInfos)
                {
                    Employee newEmp = emp.Copy();
                    newEmp.Action.Value = AddDelete.Default;
                    newEmp.IsChecked.Value = false;
                    this.Contract.Value.DeliveryMan.Add(newEmp);
                }
            }
            else
            {
                Title.Value = "계약 내역 수정";
                IsNewContract.Value = Visibility.Visible;
                this.Contract.Value = contract;
                this.Contract.Value.ClearJson();
                this.Contract.Value.isChanged = false;
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
                            network.CreateContractHistory(this.Contract.Value.MakeJson());
                        }
                    }
                    else
                    { //내역수정일경우
                        if (this.Contract.Value.isChanged)
                        {
                            JObject inner = new JObject();
                            inner["con_id"] = this.Contract.Value.Id.Value;
                            inner["changed_item"] = this.Contract.Value.GetChangedItem();
                            this.Contract.Value.isChanged = false;
                            network.UpdateContract(inner);
                            ErpLogWriter.LogWriter.Debug(inner);
                            this.Contract.Value.ClearJson();
                        }
                    }
                }
                else
                {
                    jobj["con_id"] = this.Contract.Value.Id.Value;
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
                case COMMAND.CREATECONTRACT: //데이터 생성 완료
                case COMMAND.UPDATECONTRACT: //데이터 업데이트 완료
                case COMMAND.DELETECONTRACT: //데이터 삭제완료
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
