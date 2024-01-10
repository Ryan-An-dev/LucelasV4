using CommonModel;
using CommonModel.Model;
using DataAccess;
using DataAccess.NetWork;
using DeliveryPage.Views;
using LogWriter;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json.Linq;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SettingPage.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace DeliveryPage.ViewModels
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum SearchPurpose
    {
        [Description("모두")]
        All = 0,
        [Description("배달예정")]
        BookingDelivery = 2,
        [Description("배달완료")]
        Completed = 3,
    }
    public enum MovePageType { Next = 1, Prev }
    public class DeliveryPageViewModel : PrismCommonViewModelBase, INavigationAware, INetReceiver
    {
        #region SearchCondition
        public IEnumerable<SearchPurpose> SearchPurpose
        {
            get { return Enum.GetValues(typeof(SearchPurpose)).Cast<SearchPurpose>(); }
        }
        public ReactiveProperty<SearchPurpose> SelectedPurpose { get; set; }
        public ReactiveProperty<string> SearchPhone { get; set; }
        public ReactiveProperty<string> SearchName { get; set; }
        public ReactiveProperty<DateTime> StartDate { get; set; }
        public ReactiveProperty<DateTime> EndDate { get; set; }
        public ReactiveProperty<FullyCompleted> SearchFullyCompleted { get; set; }
        public IEnumerable<FullyCompleted> SearchFullyCompletedValues
        {
            get { return Enum.GetValues(typeof(FullyCompleted)).Cast<FullyCompleted>(); }
        }

        #endregion
        public ObservableCollection<int> CountList { get; set; } = new ObservableCollection<int>();
        public DelegateCommand SearchButton { get; }

        #region Paging
        public ReactiveProperty<int> CurrentPage { get; set; }
        public ReactiveProperty<int> TotalPage { get; set; }
        public ReactiveProperty<int> TotalItemCount { get; set; }
        public ReactiveProperty<int> ListCount { get; set; }
        public ReactiveProperty<int> FirstItem { get; set; }
        public DelegateCommand<object> CmdGoPage { get; }
        #endregion

        public DelegateCommand NewButton { get; }
        public DelegateCommand RowDoubleClick { get; }
        public ReactiveCollection<Contract> ContractItems { get; }
        public ReactiveProperty<Contract> SelectedItem { get; set; }
        private IContainerProvider ContainerProvider { get; }
        public ReactiveCollection<FurnitureType> furnitureInfos { get; }
        public ReactiveCollection<Employee> Employees { get; }
        public ReactiveProperty<Employee> SearchEmployee { get; set; }
        public DeliveryPageViewModel(IRegionManager regionManager, IContainerProvider containerProvider) : base(regionManager)
        {
            this.SelectedPurpose = new ReactiveProperty<SearchPurpose>(0).AddTo(this.disposable);
            this.ContainerProvider = containerProvider;
            this.SearchFullyCompleted = new ReactiveProperty<FullyCompleted>((FullyCompleted)0).AddTo(this.disposable);
            this.EndDate = new ReactiveProperty<DateTime>(DateTime.Today).AddTo(this.disposable);
            this.StartDate = new ReactiveProperty<DateTime>(DateTime.Today.AddMonths(-1)).AddTo(this.disposable);
            this.CurrentPage = new ReactiveProperty<int>(1).AddTo(this.disposable);
            this.ListCount = new ReactiveProperty<int>(30).AddTo(this.disposable);
            this.FirstItem = new ReactiveProperty<int>(0).AddTo(this.disposable);
            this.TotalPage = new ReactiveProperty<int>(0).AddTo(this.disposable);
            this.SearchName = new ReactiveProperty<string>("").AddTo(this.disposable);
            this.TotalItemCount = new ReactiveProperty<int>(0).AddTo(this.disposable);
            this.TotalItemCount.Subscribe(c => this.TotalPage.Value = (c / this.ListCount.Value) + 1);
            this.SearchButton = new DelegateCommand(SearchContractExecute);
            this.SearchPhone = new ReactiveProperty<string>("").AddTo(this.disposable);
            this.SearchEmployee = new ReactiveProperty<Employee>().AddTo(this.disposable);
            NewButton = new DelegateCommand(NewButtonExecute);
            RowDoubleClick = new DelegateCommand(RowDoubleClickExecute);
            CmdGoPage = new DelegateCommand<object>(ExecCmdGoPage);
            ContractItems = new ReactiveCollection<Contract>().AddTo(this.disposable);
            furnitureInfos = new ReactiveCollection<FurnitureType>().AddTo(this.disposable);
            SelectedItem = new ReactiveProperty<Contract>().AddTo(disposable);
            this.Employees = new ReactiveCollection<Employee>().AddTo(this.disposable);
            SettingPageViewModel temp = this.ContainerProvider.Resolve<SettingPageViewModel>();
            if (temp.FurnitureInfos.Count > 0)
            {
                this.furnitureInfos = temp.FurnitureInfos;
            }
            if (temp.EmployeeInfos.Count > 0) { 
                this.Employees = temp.EmployeeInfos;
                Employee tempEmp = new Employee();
                tempEmp.Id.Value = 0;
                tempEmp.Name.Value = "전체";
                this.Employees.Insert(0, tempEmp);
            }
            this.CountList.Add(30);
            this.CountList.Add(50);
            this.CountList.Add(70);
            this.CountList.Add(100);
        }

        private void SearchContractExecute()
        {
            SendData();
        }

        private void ExecCmdGoPage(object param)
        {
            MovePageType moveType = (MovePageType)param;
            if (this.CurrentPage.Value == this.TotalPage.Value && moveType == MovePageType.Next)
            {
                return;
            }
            if (this.CurrentPage.Value == 1 && moveType == MovePageType.Prev)
            {
                return;
            }
            switch (moveType)
            {
                case MovePageType.Next:
                    this.CurrentPage.Value = this.CurrentPage.Value == this.TotalPage.Value ? this.CurrentPage.Value : this.CurrentPage.Value + 1;
                    break;
                case MovePageType.Prev:
                    this.CurrentPage.Value = this.CurrentPage.Value == 1 ? 1 : this.CurrentPage.Value - 1;
                    break;
                default:
                    break;
            }
            UpdatePageItem(moveType, this.ListCount.Value);
        }
        private void UpdatePageItem(MovePageType param, int count)
        {
            using (var network = this.ContainerProvider.Resolve<DataAgent.DeliveryDataAgent>())
            {
                network.SetReceiver(this);
                //accountList, CategoryList, ProductList 기본으로 요청하기
                JObject jobj = new JObject();
                jobj["page_unit"] = (ListCount.Value * CurrentPage.Value) > TotalItemCount.Value ? TotalItemCount.Value - (ListCount.Value * (CurrentPage.Value - 1)) : ListCount.Value;
                jobj["page_start_pos"] = (this.CurrentPage.Value - 1) * this.ListCount.Value;
                JObject search = new JObject();
                search["employee_id"] = this.SearchEmployee.Value == null ? 0 : SearchEmployee.Value.Id.Value;
                search["cui_name"] = this.SearchName.Value;
                search["start_time"] = this.StartDate.Value.ToString("yyyy-MM-dd HH:mm:ss");
                search["end_time"] = this.EndDate.Value.ToString("yyyy-MM-dd 23:59:59");
                search["cui_phone"] = this.SearchPhone.Value;
                search["receiving_type"] = (int)this.SelectedPurpose.Value;
                jobj["search_option"] = search;
                network.GetDeliveryList(jobj);
            }
        }
        private void SendData()
        {
            using (var network = this.ContainerProvider.Resolve<DataAgent.DeliveryDataAgent>())
            {
                network.SetReceiver(this);
                JObject jobj = new JObject();
                jobj["page_unit"] = this.ListCount.Value;
                jobj["page_start_pos"] = (this.CurrentPage.Value - 1) * this.ListCount.Value;
                JObject search = new JObject();
                search["employee_id"] = this.SearchEmployee.Value == null ? 0 : SearchEmployee.Value.Id.Value;
                search["cui_name"] = this.SearchName.Value;
                search["start_time"] = this.StartDate.Value.ToString("yyyy-MM-dd HH:mm:ss");
                search["end_time"] = this.EndDate.Value.ToString("yyyy-MM-dd 23:59:59");
                search["cui_phone"] = this.SearchPhone.Value;
                search["receiving_type"] = (int)this.SelectedPurpose.Value;
                jobj["search_option"] = search;
                network.GetDeliveryList(jobj);
            }
        }
        private void NewButtonExecute()
        {
            regionManager.RequestNavigate("DeliverySinglePageRegion", nameof(DeliverySinglePage));
            DrawerHost.OpenDrawerCommand.Execute(Dock.Right, null);
        }
        private void RowDoubleClickExecute()
        {
            if (SelectedItem.Value == null)
                return;
            SelectedItem.Value.ClearJson();
            var p = new NavigationParameters();
            if (SelectedItem != null)
            {
                p.Add(nameof(Contract), SelectedItem.Value);
            }

            regionManager.RequestNavigate("DeliverySinglePageRegion", nameof(DeliverySinglePage), p);
            DrawerHost.OpenDrawerCommand.Execute(Dock.Right, null);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            this.FirstItem.Value = 0;
            this.CurrentPage.Value = 1;
            this.TotalPage.Value = 0;
            this.TotalItemCount.Value = 0;
            SendData();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            this.Dispose();
        }

        public void OnRceivedData(ErpPacket packet)
        {
            string msg = Encoding.UTF8.GetString(packet.Body);
            JObject jobj = null;
            try { jobj = new JObject(JObject.Parse(msg)); }
            catch (Exception)
            {
                return;
            }
             ErpLogWriter.LogWriter.Trace(jobj.ToString());
            switch ((COMMAND)packet.Header.CMD)
            {
                case COMMAND.GET_DELIVERYLIST: //데이터 조회
                    SetContractHistory(jobj);
                    break;

            }
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
            if (jobj["product_order_id"] != null)
                contractedProduct.Id.Value = jobj["product_order_id"].ToObject<int>();

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
        private void ProductMemoCombine(Contract temper)
        {
            temper.ProductMemoCombine.Value = "";
            foreach (ContractedProduct temp in temper.Product)
            {
                temper.ProductMemoCombine.Value += "[" + temp.FurnitureInventory.Value.Name.Value + "]" + "\r\n" + temp.Memo.Value + "\r\n";
            }
        }
        private void SetContractHistory(JObject msg)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.ContractItems.Clear();
            });
            //ErpLogWriter.LogWriter.Trace(msg.ToString());
            if (msg.ToString().Trim() != string.Empty)
            {
                try
                {
                    int i = 1 + ((CurrentPage.Value - 1) * ListCount.Value);
                    if (msg["history_count"] != null)
                        this.TotalItemCount.Value = msg["history_count"].ToObject<int>();
                    if (msg["contract_history"] != null)
                    {
                        foreach (JObject inner in msg["contract_history"] as JArray)
                        {
                            Contract temp = new Contract();
                            int id = 0;
                            temp.ListNo.Value = i++;
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

                            //배송 완료 
                            if (inner["delivery_complete"]!=null)
                                temp.DeliveryComplete.Value = (DeliveryComplete)inner["delivery_complete"].ToObject<int>();


                            SettingPageViewModel employee = this.ContainerProvider.Resolve<SettingPageViewModel>();
                            foreach (Employee emp in employee.EmployeeInfos)
                            {
                                Employee newEmp = emp.Copy();
                                newEmp.Action.Value = AddDelete.Default;
                                newEmp.IsChecked.Value = false;
                                temp.DeliveryMan.Add(newEmp);
                            }

                            if (inner["delivery_group"] != null && !inner["delivery_group"].ToString().Equals(""))
                            {
                                string combine = "";
                                foreach (JObject jobj in inner["delivery_group"] as JArray)
                                {
                                    int emp_id = jobj["employee_id"].ToObject<int>();
                                    temp.DeliveryMan.FirstOrDefault(x => x.Id.Value == emp_id).IsChecked.Value = true;
                                    if (combine != string.Empty) {
                                        combine += ", ";
                                    }
                                    combine += temp.DeliveryMan.FirstOrDefault(x => x.Id.Value == emp_id).Name.Value;
                                }
                                temp.DeliveryManCombine.Value = combine;
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

                                foreach (JObject jobj in inner["payment"] as JArray)
                                {
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
                                            SettingPageViewModel set = this.ContainerProvider.Resolve<SettingPageViewModel>();
                                            PayCardType item = set.PayCardTypeInfos.First(c => c.Id.Value == paycard_id);
                                            if (item != null)
                                                pay.SelectedPayCard.Value = item;
                                        }
                                    }
                                    temp.Payment.Add(pay);
                                }

                            }
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                this.ContractItems.Add(temp);
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

        private Employee FindEmployee(int id)
        {
            SettingPageViewModel temp = this.ContainerProvider.Resolve<SettingPageViewModel>();
            Employee item = temp.EmployeeInfos.First(c => c.Id.Value == id);
            return item;
        }

        public void OnConnected()
        {

        }

        public void OnSent()
        {

        }
    }
}
