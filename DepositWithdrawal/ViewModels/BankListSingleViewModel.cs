using CommonModel;
using CommonModel.Model;
using ControlzEx.Standard;
using DataAccess;
using DataAccess.NetWork;
using DepositWithdrawal.Views;
using LogWriter;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json.Linq;
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
using System.Reactive.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace DepositWithdrawal.ViewModels
{
    //여기 삭제랑 업데이트 할때 로딩아이콘 넣어야된다. 무조껀
    public class BankListSingleViewModel : PrismCommonViewModelBase, INavigationAware, IDisposable, INetReceiver
    {
        public IEnumerable<IncomeCostType> SearchIncomeCostTypeValues
        {
            get { return Enum.GetValues(typeof(IncomeCostType)).Cast<IncomeCostType>().Skip(1); }
        }
        public IEnumerable<ReceiptType> SearchReceiptTypeValues
        {
            get { return Enum.GetValues(typeof(ReceiptType)).Cast<ReceiptType>().Skip(1); }
        }
        public IEnumerable<FullyCompleted> SearchFullyCompletedValues
        {
            get { return Enum.GetValues(typeof(FullyCompleted)).Cast<FullyCompleted>().Skip(1); }
        }
        public ReactiveProperty<string> Title { get; } = new();
        public DelegateCommand SaveButton { get; }
        public ReactiveProperty<ReceiptModel> ReceiptModel { get; set; }

        private readonly CompositeDisposable _disposable = new();
        public DelegateCommand DeleteButton { get; }
        public ReactiveProperty<Visibility> VisibilityAddButton { get; } = new();
        public ReactiveProperty<Visibility> VisibilityContract { get; } = new();
        public ReactiveProperty<bool> IsEnableTab { get; } = new();
        public ReadOnlyReactiveProperty<bool> IsReverseEnableTab { get; }
        public ReactiveProperty<bool> IsCashOnly { get; set; }
        public ReactiveCollection<CategoryInfo> CategoryInfos { get; set; }
        public IContainerProvider ContainerProvider { get; }
        public DelegateCommand<string> AddContractItemButton { get; }
        public IDialogService dialogService { get; }
        public ReactiveCollection<FurnitureType> furnitureInfos { get; }
        public ReactiveProperty<Contract> SelectedContract { get; set; }

        public BankListSingleViewModel(IRegionManager regionManager,IContainerProvider containerProvider,IDialogService dialogService):base(regionManager)
        {
            this.SelectedContract = new ReactiveProperty<Contract>().AddTo(_disposable);
            this.dialogService = dialogService;
            this.ContainerProvider = containerProvider;
            Title.AddTo(_disposable);
            SaveButton = new DelegateCommand(SaveButtonExecute);
            DeleteButton = new DelegateCommand(DeleteButtonExecute);
            AddContractItemButton = new DelegateCommand<string>(ExcuteContractButton);
            ReceiptModel = new ReactiveProperty<ReceiptModel>().AddTo(_disposable);
            CategoryInfos = new ReactiveCollection<CategoryInfo>().AddTo(_disposable);
            IsEnableTab.AddTo(_disposable);
            IsReverseEnableTab = IsEnableTab.Select(x=>!x).ToReadOnlyReactiveProperty();
            IsCashOnly = new ReactiveProperty<bool>(false).AddTo(this.disposable);
            furnitureInfos = new ReactiveCollection<FurnitureType>().AddTo(_disposable);
            SettingPageViewModel temp = this.ContainerProvider.Resolve<SettingPageViewModel>("GlobalData");
            if (temp.FurnitureInfos.Count > 0)
            {
                this.furnitureInfos = temp.FurnitureInfos;
            }
        }

        private void ExcuteContractButton(string obj)
        {
            if (obj == "Add")
            {
                if (this.ReceiptModel.Value.RemainPrice.Value == 0)
                { // 남은 금액이 없으면 메시지박스 띄워줌 (-버튼만 활성화 됩니다. 등);
                    //return;
                }
                DialogParameters p = new DialogParameters();
                p.Add("object", this.ReceiptModel.Value);
                this.dialogService.ShowDialog("FindItemPage", p, r => FindContractItem(r), "CommonDialogWindow");
            }
            else {  //Delete
                if (this.SelectedContract.Value == null)
                    return;
                JArray jarr = new JArray();
                if (this.ReceiptModel.Value.ChangedItem["connected_contract"] != null)
                    jarr = this.ReceiptModel.Value.ChangedItem["connected_contract"] as JArray;

                foreach (Payment pay in SelectedContract.Value.Payment)
                {
                    if (!(pay.Action.Value == AddDelete.Default))
                    {
                        JObject inner = new JObject();
                        inner["con_id"] = SelectedContract.Value.Id.Value;
                        inner["payment_id"] = pay.PaymentId.Value;
                        inner["mode"] = (int)2;
                        jarr.Add(inner);
                        this.ReceiptModel.Value.AllocatedPrice.Value -= pay.Price.Value;
                    }
                }
                if (jarr.Count > 0)
                    this.ReceiptModel.Value.ChangedItem["connected_contract"] = jarr;
                
                this.ReceiptModel.Value.ConnectedContract.Remove(this.SelectedContract.Value);

            }
        }

        private void SetPayment(ReactiveCollection<Contract>args)
        {
            JArray jarr = new JArray();
            foreach (Contract temp in args)
            {
                foreach (Payment pay in temp.Payment)
                {
                    if (pay.IsSelected.Value)
                    {
                        pay.Action.Value = AddDelete.Add;
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            ReceiptModel.Value.isChanged = true;
                            this.ReceiptModel.Value.ConnectedContract.Add(temp);
                            this.ReceiptModel.Value.AllocatedPrice.Value += pay.Price.Value;
                        });
                    }
                }
            }
        }

        private void FindContractItem(IDialogResult r) {
            //Contract ID 받아야되는데 
            if(r == null) return;
            if (r.Result == ButtonResult.OK)
            {
                if (!r.Parameters.ContainsKey("object")) return;
                else {
                    ReactiveCollection<Contract> temp = null;
                    r.Parameters.TryGetValue("object", out temp);
                    if (temp == null) return;
                    SetPayment(temp);
                    
                }
            }
            else 
            { 
                
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
        public bool IsNavigationTarget(NavigationContext navigationContext) => false;

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            
        }
        
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var Receipt = navigationContext.Parameters["ReceiptModel"] as ReactiveProperty<ReceiptModel>;
            SettingPageViewModel temp = this.ContainerProvider.Resolve<SettingPageViewModel>("GlobalData");
            foreach (var item in temp.CategoryInfos) {
                this.CategoryInfos.Add(item);
            }
            this.VisibilityContract.Value = Visibility.Visible; //contract 추가 
            if (Receipt == null)
            {
                Title.Value = "현금내역 추가";
                VisibilityAddButton.Value = Visibility.Collapsed;
                IsEnableTab.Value = false; // 현금내역일때
                this.ReceiptModel.Value = new ReceiptModel();
                this.ReceiptModel.Value.Month.Value =DateTime.Now;
                this.ReceiptModel.Value.ReceiptType.Value = ReceiptType.Cash;
                this.ReceiptModel.Value.Tip.Value = "현금";
            }
            else
            {
                Title.Value = "내역 수정";
                IsEnableTab.Value = true; // 현금내역 아닐때
                this.ReceiptModel.Value = Receipt.Value;
                if (Receipt.Value.IncomeCostType.Value == IncomeCostType.Cost) {
                    this.VisibilityContract.Value = Visibility.Collapsed;
                }
                this.ReceiptModel.Value.isChanged = false;
                this.ReceiptModel.Value.ChangedItem.RemoveAll();
                using (var network = this.ContainerProvider.Resolve<DataAgent.ContractDataAgent>())
                {
                    JObject jobj = new JObject();
                    jobj["shi_id"] = (int)this.ReceiptModel.Value.ReceiptNo.Value;
                    network.SetReceiver(this);
                    network.GetConnectedContract(jobj);
                }
                //하나하나에 값 재할당 해줘야한다. 벨류 안바뀌게 
            }
            if (this.ReceiptModel.Value.ReceiptType.Value == ReceiptType.Cash) {
                IsCashOnly.Value = true;
            }
            
          
        }
        private void Save(int param) {
            using (var network = this.ContainerProvider.Resolve<DataAgent.BankListDataAgent>())
            {
                JObject jobj = new JObject();
                network.SetReceiver(this);
                if (param == 0) { // Update
                    if (this.Title.Value=="현금내역 추가") //신규등록일경우
                    { 
                        JObject inner = new JObject();
                        inner["shi_type"] = (int)this.ReceiptModel.Value.IncomeCostType.Value;
                        inner["shi_biz_type"] = this.ReceiptModel.Value.CategoryInfo.Value.CategoryId.Value;
                        inner["shi_cost"] = this.ReceiptModel.Value.Money.Value;
                        inner["shi_time"] = this.ReceiptModel.Value.Month.Value.ToString("yyyy-MM-dd HH:mm:ss");
                        inner["shi_use_content"] = this.ReceiptModel.Value.Contents.Value;
                        inner["shi_memo"] = this.ReceiptModel.Value.Memo.Value;
                        inner["shi_use_name"] = this.ReceiptModel.Value.Tip.Value;
                        jobj["create_history"] = inner;
                        network.CreateBankHistory(jobj);
                    }
                    else { //내역수정일경우
                        if (this.ReceiptModel.Value.isChanged) {
                            JObject inner = new JObject();
                            inner["shi_id"] = this.ReceiptModel.Value.ReceiptNo.Value;
                            inner["changed_item"] = this.ReceiptModel.Value.GetChangedItem();
                            jobj["update_history"] = inner;
                            network.UpdateBankHistory(jobj);
                        }
                    }
                }
                else
                {
                    jobj["delete_history"] = this.ReceiptModel.Value.ReceiptNo.Value;
                    network.DeleteBankHistory(jobj);
                }
                ReceiptModel.Value.isChanged = false;
                ReceiptModel.Value.ChangedItem.RemoveAll();
            }
        }

        public void OnRceivedData(ErpPacket packet)
        {
            string msg = Encoding.UTF8.GetString(packet.Body);
            try {
                JObject jobj = new JObject(JObject.Parse(msg));
                switch ((COMMAND)packet.Header.CMD)
                {
                    case COMMAND.CreateBankHistory: //데이터 생성 완료
                    case COMMAND.UpdateBankHistory: //데이터 업데이트 완료
                    case COMMAND.DeleteBankHistory: //데이터 삭제완료
                        Application.Current.Dispatcher.Invoke(() => {
                            DrawerHost.CloseDrawerCommand.Execute(Dock.Right, null);
                            Dispose(); //변경완료 후 변수 초기화
                            regionManager.RequestNavigate("ContentRegion", nameof(BankListPage));
                        });
                        break;
                    case COMMAND.GET_CONNECTED_CONTRACT:
                        SetContractHistory(jobj);
                        break;
                }
            }
            catch (Exception e) { }
        }
        private void SetContractHistory(JObject msg)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.ReceiptModel.Value.ConnectedContract.Clear();
            });
            
            if (msg.ToString().Trim() != string.Empty)
            {
                try
                {
                    if (msg["contract_history"] != null && !msg["contract_history"].ToString().Equals(""))
                    {
                        foreach (JObject inner in msg["contract_history"] as JArray)
                        {
                            Contract temp = new Contract();
                            int id = 0;
                            Customer Contractor = null;

                            if (inner["con_id"] != null)
                                temp.Id.Value = inner["con_id"].ToObject<int>();
                            //고객정보 파싱 부분 
                            if (inner["contractor"] != null)
                            {
                                Contractor = SetCustomer(inner["contractor"] as JObject);
                                Contractor.ClearJson();
                            }

                            //계약일자
                            if (inner["create_time"] != null)
                                temp.Month.Value = inner["create_time"].ToObject<DateTime>();

                            //계약ID
                            if (inner["seller_id"] != null)
                                temp.Seller.Value = FindEmployee(inner["seller_id"].ToObject<int>());

                            //계약 메모
                            if (inner["memo"] != null)
                                temp.Memo.Value = inner["memo"].ToString();

                            //계약 금액 완료 유무
                            if (inner["payment_complete"] != null)
                                temp.PaymentComplete.Value = (FullyCompleted)inner["payment_complete"].ToObject<int>();

                            //고객 정보
                            if (Contractor != null)
                                temp.Contractor.Value = Contractor;

                            //배송일자 
                            if (inner["delivery_date"] != null)
                                temp.Delivery.Value = inner["delivery_date"].ToObject<DateTime>();

                            //
                            SettingPageViewModel employee = this.ContainerProvider.Resolve<SettingPageViewModel>("GlobalData");
                            foreach (Employee emp in employee.EmployeeInfos)
                            {
                                emp.Action.Value = AddDelete.Default;
                                emp.isChanged = false;
                                temp.DeliveryMan.Add(emp);
                            }
                            if (inner["delivery_group"] != null && !inner["delivery_group"].ToString().Equals(""))
                            {
                                foreach (JObject jobj in inner["delivery_group"] as JArray)
                                {
                                    int emp_id = jobj["employee_id"].ToObject<int>();
                                    temp.DeliveryMan.FirstOrDefault(x => x.Id.Value == emp_id).IsChecked.Value = true;
                                }
                            }

                            //제품 
                            if (inner["product_list"] != null && !inner["product_list"].ToString().Equals(""))
                            {

                                foreach (JObject con in inner["product_list"] as JArray)
                                {
                                    ContractedProduct contractproduct = SetProduct(con);
                                    temp.Product.Add(contractproduct);
                                }
                                temp.TotalPrice();
                                string combine = "";
                                foreach (ContractedProduct item in temp.Product)
                                {
                                    if (combine != string.Empty)
                                    {
                                        combine += ", ";
                                    }
                                    combine +=
                                        item.FurnitureInventory.Value.ProductType.Value.Name.Value + " : "
                                        + item.FurnitureInventory.Value.Name.Value
                                        + "(" + item.SellCount.Value + "개)"
                                        ;
                                }
                                temp.ProductNameCombine.Value = combine;
                                ProductMemoCombine(temp);
                                
                            }
                            if (inner["total"] != null)
                                temp.Price.Value = inner["total"].ToObject<int>();
                            if (inner["payment_complete"] != null)
                                temp.PaymentComplete.Value = (FullyCompleted)inner["payment_complete"].ToObject<int>();


                            //계약금,잔금 부분
                            if (inner["payment"] != null && !inner["payment"].ToString().Equals(""))
                            {
                                JObject jobj = inner["payment"] as JObject;
                                Payment pay = new Payment();
                                if (jobj["payment_id"] != null)
                                    pay.PaymentId.Value = jobj["payment_id"].ToObject<int>();
                                if (jobj["payment_type"] != null)
                                    pay.PaymentType.Value = (PaymentType)jobj["payment_type"].ToObject<int>();
                                if (jobj["payment_completed"] != null)
                                    pay.PaymentCompleted.Value = (Complete)jobj["payment_completed"].ToObject<int>();
                                if (jobj["payment_method"] != null)
                                    pay.PaymentMethod.Value = (ReceiptType)jobj["payment_method"].ToObject<int>();
                                if (jobj["price"] != null)
                                    pay.Price.Value = jobj["price"].ToObject<int>();

                                if (jobj["payment_card"] != null)
                                {
                                    int paycard_id = jobj["payment_card"].ToObject<int>();
                                    if (paycard_id != 0)
                                    {
                                        SettingPageViewModel set = this.ContainerProvider.Resolve<SettingPageViewModel>("GlobalData");
                                        PayCardType item = set.PayCardTypeInfos.First(c => c.Id.Value == paycard_id);
                                        if (item != null)
                                            pay.SelectedPayCard.Value = item;
                                    }
                                }
                                temp.Payment.Add(pay);
                            }
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                this.ReceiptModel.Value.ConnectedContract.Add(temp);
                                this.ReceiptModel.Value.AllocatedPrice.Value += temp.Price.Value;
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErpLogWriter.LogWriter.Trace(ex.ToString());
                }
            }
        }
        private void ProductMemoCombine(Contract temper)
        {
            temper.ProductMemoCombine.Value = "";
            foreach (ContractedProduct temp in temper.Product)
            {
                temper.ProductMemoCombine.Value += "[" + temp.FurnitureInventory.Value.Name.Value + "]" + "\r\n" + temp.Memo.Value + "\r\n";
            }
        }
        private Employee FindEmployee(int id)
        {
            SettingPageViewModel temp = this.ContainerProvider.Resolve<SettingPageViewModel>("GlobalData");
            Employee item = temp.EmployeeInfos.First(c => c.Id.Value == id);
            return item;
        }

        private Customer SetCustomer(JObject jobj)
        {
            Customer customer = new Customer();
            if (jobj["cui_id"] != null)
                customer.Id.Value = jobj["cui_id"].ToObject<int>();
            if (jobj["cui_name"] != null)
                customer.Name.Value = jobj["cui_name"].ToObject<string>();
            if (jobj["cui_phone"] != null)
                customer.Phone.Value = jobj["cui_phone"].ToObject<string>();
            if (jobj["cui_address"] != null)
                customer.Address.Value = jobj["cui_address"].ToObject<string>();
            if (jobj["cui_address_detail"] != null)
                customer.Address1.Value = jobj["cui_address_detail"].ToObject<string>();
            if (jobj["cui_memo"] != null)
                customer.Memo.Value = jobj["cui_memo"].ToObject<string>();
            return customer;
        }

        private ContractedProduct SetProduct(JObject jobj)
        {
            ContractedProduct contractedProduct = new ContractedProduct();
            if (jobj["sell_price"] != null)
                contractedProduct.SellPrice.Value = jobj["sell_price"].ToObject<int>();
            if (jobj["order_count"] != null)
                contractedProduct.SellCount.Value = jobj["order_count"].ToObject<int>();
            if (jobj["product_info"] != null)
            {
                contractedProduct.FurnitureInventory.Value = SetProductInfo(jobj["product_info"] as JObject);
            }
            return contractedProduct;
        }
        private Product SetProductInfo(JObject jobj)
        {
            Product product = new Product();
            if (jobj["product_id"] != null)
                product.Id.Value = jobj["product_id"].ToObject<int>();
            if (jobj["product_type"] != null)
                product.ProductType.Value = GetProductType(jobj["product_type"].ToObject<int>());
            if (jobj["product_name"] != null)
                product.Name.Value = jobj["product_name"].ToObject<string>();
            if (jobj["product_price"] != null)
                product.Price.Value = jobj["product_price"].ToObject<int>();
            if (jobj["company"] != null)
                product.Company.Value = SetCompany(jobj["company"] as JObject);
            return product;
        }

        private Company SetCompany(JObject jobj)
        {

            Company temp = new Company();
            if (jobj["company_name"] != null)
                temp.CompanyName.Value = jobj["company_name"].ToString();
            if (jobj["company_phone"] != null)
                temp.CompanyPhone.Value = jobj["company_phone"].ToString();
            if (jobj["company_id"] != null)
                temp.Id.Value = jobj["company_id"].ToObject<int>();
            if (jobj["company_address"] != null)
                temp.CompanyAddress.Value = jobj["company_address"].ToString();
            if (jobj["company_address_detail"] != null)
                temp.CompanyAddressDetail.Value = jobj["company_address_detail"].ToString();
            return temp;

        }

        private FurnitureType GetProductType(int id)
        {
            return this.furnitureInfos.FirstOrDefault(x => x.Id.Value == id);
        }
        public void OnConnected()
        {
        }

        public void OnSent()
        {
        }
    }
}
