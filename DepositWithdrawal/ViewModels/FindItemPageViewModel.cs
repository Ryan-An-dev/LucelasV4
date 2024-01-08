using CommonModel;
using CommonModel.Model;
using CommonModule.Views;
using DataAccess;
using DataAccess.NetWork;
using LogWriter;
using Newtonsoft.Json.Linq;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SettingPage.ViewModels;
using SettingPage.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace DepositWithdrawal.ViewModels
{
    public class FindItemPageViewModel : PrsimListViewModelBase, INetReceiver,IDialogAware
    {
        public ReactiveProperty<ReceiptType> SearchReceiptType { get; set; }
        public IEnumerable<ReceiptType> SearchReceiptTypeValues
        {
            get { return Enum.GetValues(typeof(ReceiptType)).Cast<ReceiptType>(); }
        }

        public ReactiveProperty<DateTime> StartDate { get; set; }
        public ReactiveProperty<DateTime> EndDate { get; set; }

        public int OriginAllocatedPrice { get; set; }

        public ReactiveCollection<FurnitureType> furnitureInfos { get; }
        public ReactiveCollection<PayCardType> PaymentCardList { get; set; }
        public ReactiveProperty<PayCardType>SelectedPaymentCard { get; set; }
        public ReactiveCollection<Contract> ContractItems { get; }
        public ReactiveProperty<Payment> SelectedPayment { get; set; }
        public BankModel SelectedBank { get; set; }
        public ReactiveProperty<ReceiptModel> args { get; set; }
        public IContainerProvider ContainerProvider { get; }
        private DelegateCommand<string> _SearchExecute;
        public DelegateCommand<string> SearchExecute =>
            _SearchExecute ?? (_SearchExecute = new DelegateCommand<string>(SearchExecuteCommand));
        public ReactiveProperty<Visibility> isCard { get; set; }
        private DelegateCommand<string> _closeDialogCommand;
        public DelegateCommand<string> CloseDialogCommand =>
            _closeDialogCommand ?? (_closeDialogCommand = new DelegateCommand<string>(CloseDialog));

        public FindItemPageViewModel(IContainerProvider containerprovider, IRegionManager regionManager, IDialogService dialogService) : base(regionManager, containerprovider, dialogService)
        {
            this.isCard = new ReactiveProperty<Visibility>(Visibility.Collapsed).AddTo(this.disposable);
            this.SelectedPaymentCard = new ReactiveProperty<PayCardType>().AddTo(this.disposable);
            this.EndDate = new ReactiveProperty<DateTime>(DateTime.Now).AddTo(this.disposable);
            this.StartDate = new ReactiveProperty<DateTime>(DateTime.Today.AddMonths(-1)).AddTo(this.disposable);
            SearchReceiptType = new ReactiveProperty<ReceiptType>().AddTo(this.disposable);
            furnitureInfos = new ReactiveCollection<FurnitureType>().AddTo(this.disposable);
            this.ContainerProvider= containerprovider;
            this.args= new ReactiveProperty<ReceiptModel>().AddTo(disposable);
            this.ContractItems= new ReactiveCollection<Contract>().AddTo(disposable);
            this.SelectedBank = new BankModel();
            PaymentCardList = new ReactiveCollection<PayCardType>().AddTo(disposable);
            SettingPageViewModel temp = ContainerProvider.Resolve<SettingPageViewModel>("GlobalData");
            this.PaymentCardList = temp.PayCardTypeInfos;
            PayCardType temper = new PayCardType();
            temper.Id.Value = 0;
            temper.Name.Value = "전체";
            this.PaymentCardList.Insert(0, temper);
            if (temp.FurnitureInfos.Count > 0)
            {
                this.furnitureInfos = temp.FurnitureInfos;
            }
        }
        private void SearchExecuteCommand(string obj)
        {
            using (var network = this.ContainerProvider.Resolve<DataAgent.ContractDataAgent>())
            {
                JObject jobj = new JObject();
                jobj["page_unit"] = 30;
                jobj["page_start_pos"] = 0;
                JObject jobj2 = new JObject();
                jobj2["payment_method"] = (int)SearchReceiptType.Value;
                jobj2["start_time"] = this.StartDate.Value.ToString("yyyy-MM-dd HH:mm:ss");
                jobj2["end_time"] = this.EndDate.Value.ToString("yyyy-MM-dd HH:mm:ss");
                jobj2["complete"] = 2;

                if (args.Value.ReceiptType.Value == ReceiptType.Cash) // 현금
                {
                    jobj2["cui_name"] = args.Value.Contents.Value;
                }
                else
                {  //계좌이체
                    if (args.Value.CategoryInfo.Value.Name.Value.Contains("대금"))
                    {
                        jobj2["payment_card_type"] = this.SelectedPaymentCard.Value.Id.Value;
                    }
                    else
                    {
                        jobj2["cui_name"] = Keyword.Value;
                    }
                }
                jobj["search_option"] = jobj2;


                network.SetReceiver(this);
                network.GetContractForReceipt(jobj);
            }
        }
        
        protected virtual void CloseDialog(string parameter)
        {
            DialogResult temp = null;
            ButtonResult result = ButtonResult.None;
            if (parameter?.ToLower() == "true")
            {
                result = ButtonResult.OK;
                DialogParameters p = new DialogParameters();
                p.Add("object", this.ContractItems);
                temp = new DialogResult(result, p);
            }
            else if (parameter?.ToLower() == "false")
            {   
                this.args.Value.AllocatedPrice.Value = OriginAllocatedPrice;
                result = ButtonResult.Cancel;
                temp = new DialogResult(result);
            }
            RaiseRequestClose(temp);
        }
        public virtual void RaiseRequestClose(IDialogResult dialogResult)
        {
            RequestClose?.Invoke(dialogResult);
        }

        public string Title => "";

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnConnected()
        {
            
        }

        public void OnDialogClosed()
        {
            
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            ReceiptModel item = null;
            parameters.TryGetValue("object", out item);
            args.Value = item;
            OriginAllocatedPrice = item.AllocatedPrice.Value;
            SearchReceiptType.Value = item.ReceiptType.Value;
            if (item.CategoryInfo.Value.Name.Value == "기타")
            {
                Keyword.Value = item.Contents.Value;
            }
            this.StartDate.Value = item.Month.Value.AddDays(-7);
            this.EndDate.Value = item.Month.Value;
            if (args.Value.CategoryInfo.Value.Name.Value.Contains("대금"))
            {
                isCard.Value = Visibility.Visible;
                string bank = args.Value.CategoryInfo.Value.Name.Value.Split(" ")[0].Trim();
                this.SelectedPaymentCard.Value = PaymentCardList.FirstOrDefault(x => x.Name.Value == bank);
                SearchReceiptType.Value = ReceiptType.Card;
            }
            //계약 찾기
            using (var network = this.ContainerProvider.Resolve<DataAgent.ContractDataAgent>()) {
                JObject jobj = new JObject();
                jobj["page_unit"] = 30;
                jobj["page_start_pos"] = 0;
                JObject jobj2 = new JObject();
                jobj2["payment_method"] = (int)item.ReceiptType.Value;
                jobj2["start_time"] = item.Month.Value.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss");
                jobj2["end_time"] = item.Month.Value.ToString("yyyy-MM-dd HH:mm:ss");
                //시간쪽 수정해야됨 starttime이랑 endtime 어떻게 정하기로 했는지 까먹음
                jobj2["complete"] = 2;

                if (item.ReceiptType.Value == ReceiptType.Cash) // 현금
                {
                    jobj2["payment_method"] = (int)item.ReceiptType.Value;
                    jobj2["cui_name"] = item.Contents.Value;
                }
                else if (item.ReceiptType.Value == ReceiptType.Cash) { //카드
                    jobj2["payment_method"] = (int)item.ReceiptType.Value;
                }
                else {  //계좌이체
                    if (args.Value.CategoryInfo.Value.Name.Value.Contains("대금"))
                    {
                        string bank = args.Value.CategoryInfo.Value.Name.Value.Split(" ")[0].Trim();
                        jobj2["payment_card_type"] = PaymentCardList.FirstOrDefault(x => x.Name.Value == bank).Id.Value;
                        jobj2["payment_method"] = (int)ReceiptType.Card;
                    }
                    else {
                        jobj2["cui_name"] = item.Contents.Value;
                    }
                    jobj2["payment_method"] = (int)item.ReceiptType.Value;
                }
                jobj["search_option"] = jobj2;


                network.SetReceiver(this);
                network.GetContractForReceipt(jobj);
            }
        }

        public void OnRceivedData(ErpPacket packet)
        {
            string msg = Encoding.UTF8.GetString(packet.Body);
            JObject jobj = new JObject(JObject.Parse(msg));
            ErpLogWriter.LogWriter.Trace(jobj.ToString());
            switch ((COMMAND)packet.Header.CMD)
            {
                case COMMAND.GET_CONTRACT_FOR_RECEIPT: //검색결과 받는곳 
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        this.ContractItems.Clear();
                    });
                    SetContractList(jobj);
                    break;
            }
            
        }

        private void SetContractList(JObject msg)
        {
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

                            //최종금액


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
                                Payment pay = new Payment();
                                if (inner["payment"]["payment_id"] != null)
                                    pay.PaymentId.Value = inner["payment"]["payment_id"].ToObject<int>();
                                if (inner["payment"]["payment_type"] != null)
                                    pay.PaymentType.Value = (PaymentType)inner["payment"]["payment_type"].ToObject<int>();
                                if (inner["payment"]["payment_completed"] != null)
                                    pay.PaymentCompleted.Value = (Complete)inner["payment"]["payment_completed"].ToObject<int>();
                                if (inner["payment"]["payment_method"] != null)
                                    pay.PaymentMethod.Value = (ReceiptType)inner["payment"]["payment_method"].ToObject<int>();
                                if (inner["payment"]["price"] != null)
                                    pay.Price.Value = inner["payment"]["price"].ToObject<int>();
                                if (inner["payment"]["payment_card"] != null)
                                {
                                    int paycard_id = inner["payment"]["payment_card"].ToObject<int>();
                                    if (paycard_id != 0)
                                    {
                                        SettingPageViewModel set = this.ContainerProvider.Resolve<SettingPageViewModel>("GlobalData");
                                        PayCardType item = set.PayCardTypeInfos.First(c => c.Id.Value == paycard_id);
                                        if (item != null)
                                            pay.SelectedPayCard.Value = item;
                                    }
                                }
                                pay.MyPayCheckEvent += PaymentEvent;
                                temp.Payment.Add(pay);
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
        private void PaymentEvent(bool temp,int price) {
            if (temp)
            {
                args.Value.AllocatedPrice.Value += price;
            }
            else {
                args.Value.AllocatedPrice.Value -= price;
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
        private void ProductMemoCombine(Contract temper)
        {
            temper.ProductMemoCombine.Value = "";
            foreach (ContractedProduct temp in temper.Product)
            {
                temper.ProductMemoCombine.Value += "[" + temp.FurnitureInventory.Value.Name.Value + "]" + "\r\n" + temp.Memo.Value + "\r\n";
            }
        }
        public Customer FindCustomer(int id)
        {
            SettingPageViewModel temp = this.ContainerProvider.Resolve<SettingPageViewModel>("GlobalData");
            Customer item = temp.CustomerInfos.First(c => c.Id.Value == id);
            return item;
        }
        public void OnSent()
        {
            
        }

        public override void UpdatePageItem(CommonModel.MovePageType param, int count)
        {
            
        }

        public override void AddButtonClick()
        {
            
        }

        public override void DeleteButtonClick(PrismCommonModelBase selecteditem)
        {
            
        }

        public override void RowDoubleClickEvent()
        {
            
        }

        public override void SearchTitle(string value)
        {
            
        }
    }
}
