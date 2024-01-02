
using CommonModel;
using CommonModel.Model;
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
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.ObjectExtensions;
using SettingPage.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace DepositWithdrawal.ViewModels
{
 
    public enum MovePageType { Next=1, Prev }
    public class BankListPageViewModel : PrismCommonViewModelBase, INavigationAware, INetReceiver
    {
        public ReactiveProperty<bool> Working { get; set; } //작업중인지 아닌지 표현
        public ReactiveProperty<bool> CompletedCheck { get; set; }
        public ReactiveProperty<bool> IsAcountChecked { get; set; } //전체선택
        public ReactiveProperty<DateTime> StartDate { get; set; }
        public ReactiveProperty<DateTime> EndDate { get; set; }
        public ReactiveCollection<ReceiptModel> ReceiptItems { get; set; } //Receipt List 표현
        public ReactiveCollection<BankModel> BankList { get; set; } //할당된 은행계좌 리스트
        public ReactiveProperty<ReceiptModel> SelectedItem { get; set; }
        public ReactiveCollection<CategoryInfo> CategoryList { get; set; }
        public ReactiveProperty<bool> IsCategoryChecked { get; set; }
        private IContainerProvider ContainerProvider { get; }
        public ReactiveProperty<ReceiptType> SearchReceiptType { get; set; }
        public ReactiveProperty<IncomeCostType> SearchIncomeCostType { get; set; }
        public ReactiveProperty<FullyCompleted> SearchFullyCompleted { get; set; }
        public IEnumerable<IncomeCostType> SearchIncomeCostTypeValues
        {
            get { return Enum.GetValues(typeof(IncomeCostType)).Cast<IncomeCostType>(); }
        }
        public IEnumerable<ReceiptType> SearchReceiptTypeValues
        {
            get { return Enum.GetValues(typeof(ReceiptType)).Cast<ReceiptType>(); }
        }
        public IEnumerable<FullyCompleted> SearchFullyCompletedValues
        {
            get { return Enum.GetValues(typeof(FullyCompleted)).Cast<FullyCompleted>(); }
        }
        public ReactiveProperty<int> TotalIncome { get; set; }
        public ReactiveProperty<int> TotalCost { get; set; }
        #region Paging
        public ObservableCollection<int> CountList { get; set; } = new ObservableCollection<int>();
        public ReactiveProperty<int> CurrentPage { get; set; }
        public ReactiveProperty<int> TotalPage { get; set; }
        public ReactiveProperty<int> TotalItemCount { get; set; }
        public ReactiveProperty<int> ListCount { get; set; }
        public ReactiveProperty<int> FirstItem { get; set; }
        #endregion

        #region Command
        public DelegateCommand NewButton { get; }
        public DelegateCommand SearchButton { get; }
        public DelegateCommand<object> IsAccountCheck { get; }
        public DelegateCommand<object> IsCategoryCheck { get; }
        public DelegateCommand RowDoubleClick { get; }
        public DelegateCommand<object> CmdGoPage { get; }
        public DelegateCommand<object> CheckBoxAccountCommand { get; }
        public DelegateCommand<object> CheckBoxCategoryCommand { get; }
        #endregion


        public BankListPageViewModel(IRegionManager regionManager, IContainerProvider containerProvider) : base(regionManager)
        {
            init();
            this.ContainerProvider = containerProvider;
            this.TotalIncome = new ReactiveProperty<int>(0).AddTo(this.disposable);
            this.TotalCost = new ReactiveProperty<int>(0).AddTo(this.disposable);
            this.IsAcountChecked = new ReactiveProperty<bool>(true).AddTo(this.disposable);
            this.IsCategoryChecked = new ReactiveProperty<bool>(true).AddTo(this.disposable);
            this.SearchReceiptType = new ReactiveProperty<ReceiptType>(0,mode:ReactivePropertyMode.DistinctUntilChanged).AddTo(this.disposable);
            this.BankList = new ReactiveCollection<BankModel>().AddTo(this.disposable);
            this.SearchFullyCompleted = new ReactiveProperty<FullyCompleted>((FullyCompleted)0).AddTo(this.disposable);
            this.SearchIncomeCostType = new ReactiveProperty<IncomeCostType>(0).AddTo(this.disposable);
            this.Working = new ReactiveProperty<bool>(false).AddTo(this.disposable);
            this.ReceiptItems = new ReactiveCollection<ReceiptModel>().AddTo(this.disposable);
            this.CategoryList = new ReactiveCollection<CategoryInfo>().AddTo(this.disposable);
            this.CompletedCheck = new ReactiveProperty<bool>(false).AddTo(this.disposable);
            this.SelectedItem = new ReactiveProperty<ReceiptModel>().AddTo(this.disposable);
            this.EndDate = new ReactiveProperty<DateTime>(DateTime.Now).AddTo(this.disposable);
            this.StartDate = new ReactiveProperty<DateTime>(DateTime.Today.AddMonths(-1)).AddTo(this.disposable);
            this.CurrentPage = new ReactiveProperty<int>(1).AddTo(this.disposable);
            this.ListCount = new ReactiveProperty<int>(30).AddTo(this.disposable);
            this.FirstItem = new ReactiveProperty<int>(0).AddTo(this.disposable);
            this.TotalPage = new ReactiveProperty<int>(0).AddTo(this.disposable);
            this.TotalItemCount = new ReactiveProperty<int>(0).AddTo(this.disposable);
            this.TotalItemCount.Subscribe(c => this.TotalPage.Value = (c / this.ListCount.Value) + 1);
            NewButton = new DelegateCommand(NewButtonExecute);
            RowDoubleClick = new DelegateCommand(RowDoubleClickExecute);
            SearchButton = new DelegateCommand(SerachButtonExecute);
            CmdGoPage = new DelegateCommand<object>(ExecCmdGoPage);
            IsAccountCheck = new DelegateCommand<object>(CheckBoxItemControlExecute);
            CheckBoxAccountCommand = new DelegateCommand<object>(InnerCheckboxExecute);
            IsCategoryCheck = new DelegateCommand<object>(CheckBoxItemControlCategoryExecute);
            CheckBoxCategoryCommand = new DelegateCommand<object>(InnerCheckboxCategoryExecute);
            SettingPageViewModel temp = this.ContainerProvider.Resolve<SettingPageViewModel>("GlobalData");
            this.BankList = temp.AccountInfos;
            this.CategoryList= temp.CategoryInfos;
        }
        private void InnerCheckboxCategoryExecute(object obj)
        {
            this.IsCategoryChecked.Value = false;
        }

        private void InnerCheckboxExecute(object obj)
        {
            this.IsAcountChecked.Value = false;
        }

        private void init()
        {
            CountList.Add(30);
            CountList.Add(40);
            CountList.Add(50);
            CountList.Add(100);
        }
        private void CheckBoxItemControlCategoryExecute(object x)
        {
            foreach (CategoryInfo item in this.CategoryList)
            {
                item.IsChecked.Value = (bool)x;
            }
        }
        private void CheckBoxItemControlExecute(object x)
        {
            foreach (BankModel item in this.BankList)
            {
                item.IsChecked.Value = (bool)x;
            }
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
        private JArray FindSelectedAccount() { 
            JArray jarr = new JArray();
            if (!this.IsAcountChecked.Value) {
                foreach (BankModel item in this.BankList) {
                    if (item.IsChecked.Value) { 
                        jarr.Add(item.AccountNum.Value);
                    }
                }
            }
            return jarr;
        }
        
        private void UpdatePageItem(MovePageType param, int count) {
            using (var network = this.ContainerProvider.Resolve<DataAgent.BankListDataAgent>())
            {
                network.SetReceiver(this);
                //accountList, CategoryList, ProductList 기본으로 요청하기
                JObject jobj = new JObject();
                if (!this.IsAcountChecked.Value)
                {
                    jobj["account_list"]=FindSelectedAccount();
                }
                if (!this.IsCategoryChecked.Value)
                {
                    jobj["shi_biz_type"] = FindSelectCategory();
                }

                //ListCount : 요청 유닛 수량 30
                //CurrentPage : 페이지 5

                jobj["next_preview"] = (int)param;
                jobj["page_unit"] = (this.ListCount.Value * CurrentPage.Value) > this.TotalItemCount.Value ?  this.TotalItemCount.Value - (this.ListCount.Value * (CurrentPage.Value - 1)) : this.ListCount.Value;
                jobj["page_start_pos"] = (this.CurrentPage.Value - 1) * this.ListCount.Value;
                jobj["start_time"] = this.StartDate.Value.ToString("yyyy-MM-dd HH:mm:ss");
                jobj["end_time"] = this.EndDate.Value.ToString("yyyy-MM-dd 23:59:59");
                jobj["complete"] = (int)this.SearchFullyCompleted.Value;
                jobj["shi_use_type"] = (int)this.SearchReceiptType.Value;
                jobj["shi_type"] = (int)this.SearchIncomeCostType.Value;
                network.GetBankHistory(jobj);
                this.Working.Value = true;
            }
        }
        private void SendData() {
            using (var network = this.ContainerProvider.Resolve<DataAgent.BankListDataAgent>())
            {
                network.SetReceiver(this);
                JObject jobj = new JObject();
                if (!this.IsAcountChecked.Value)
                {
                    jobj["account_list"] = FindSelectedAccount();
                }
                if (!this.IsCategoryChecked.Value) {
                    jobj["shi_biz_type"] = FindSelectCategory();
                }
                jobj["page_unit"] = this.ListCount.Value;
                jobj["page_start_pos"] = (this.CurrentPage.Value - 1) * this.ListCount.Value;
                jobj["start_time"] = this.StartDate.Value.ToString("yyyy-MM-dd HH:mm:ss");
                jobj["end_time"] = this.EndDate.Value.ToString("yyyy-MM-dd 23:59:59");
                jobj["complete"] = (int)this.SearchFullyCompleted.Value;
                jobj["shi_use_type"] = (int)this.SearchReceiptType.Value;
                jobj["shi_type"] = (int)this.SearchIncomeCostType.Value;
                network.GetBankHistory(jobj);
                this.Working.Value = true;
            }
        }

        private JArray FindSelectCategory()
        {
            JArray jarr = new JArray();
            if (!this.IsCategoryChecked.Value)
            {
                foreach (CategoryInfo item in this.CategoryList)
                {
                    if (item.IsChecked.Value)
                    {
                        jarr.Add(item.CategoryId.Value);
                    }
                }
            }
            return jarr;
        }

        private void SerachButtonExecute() {
            this.FirstItem.Value = 0;
            this.CurrentPage.Value = 1;
            this.TotalPage.Value = 0;
            this.TotalItemCount.Value = 0;
            SendData();
        }

        private void NewButtonExecute()
        {
            regionManager.RequestNavigate("BankListSingleRegion", nameof(BankListSingle));
            DrawerHost.OpenDrawerCommand.Execute(Dock.Right, null);
        }
        private void RowDoubleClickExecute()
        {
            var p = new NavigationParameters();
            if (SelectedItem.Value != null) {
                p.Add(nameof(CommonModel.Model.ReceiptModel), SelectedItem);
            }
            regionManager.RequestNavigate("BankListSingleRegion", nameof(BankListSingle), p);
            DrawerHost.OpenDrawerCommand.Execute(Dock.Right, null);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            SendData();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            
        }

        public void OnRceivedData(ErpPacket packet)
        {
            string msg = Encoding.UTF8.GetString(packet.Body);
            JObject jobj = new JObject(JObject.Parse(msg));
            ErpLogWriter.LogWriter.Trace(jobj.ToString());
            switch ((COMMAND)packet.Header.CMD) {
                case COMMAND.GetBankHistory: //데이터 조회
                    SetBankHistory(jobj);
                break;
                
            }
        }

        private void SetBankHistory(JObject msg)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.ReceiptItems.Clear();
                
            });
            if (msg.ToString().Trim() != string.Empty) {
                try {
                    if (msg["state_history"] == null)
                        return;
                    if (msg["state_history"].ToString().Equals(""))
                        return;
                    int i = 1 +((CurrentPage.Value - 1) * ListCount.Value);
                    if (msg["history_count"] != null)
                        this.TotalItemCount.Value = msg["history_count"].ToObject<int>();
                    if (msg["total_income"] != null)
                        this.TotalIncome.Value = msg["total_income"].ToObject<int>();
                    else
                        this.TotalIncome.Value = 0;
                    if (msg["total_cost"] != null)
                        this.TotalCost.Value = msg["total_cost"].ToObject<int>();
                    else
                        this.TotalCost.Value = 0;
                    foreach (JObject inner in msg["state_history"] as JArray)
                    {
                        ReceiptModel temp = new ReceiptModel(
                            inner["shi_use_name"].ToString(), 
                            FindCategory(inner["shi_biz_type"].ToObject<int>()), 
                            inner["shi_memo"].ToString(), 
                            inner["shi_cost"].ToObject<int>(), 
                            inner["shi_use_content"].ToString(), 
                            inner["shi_type"].ToObject<int>()
                            );
                        if(inner["shi_id"] !=null)
                            temp.ReceiptNo.Value = inner["shi_id"].ToObject<int>();
                        if (inner["shi_use_type"]!= null)
                            temp.ReceiptType.Value = (ReceiptType)inner["shi_use_type"].ToObject<int>();
                        if (inner["shi_num"]!=null)
                            temp.BankInfo.Value = FindBankList(inner["shi_num"].ToString());
                        //temp.CategoryInfo.Value = findCategoryItem(msg["shi_biz_type"].ToObject<int>()); //어떤 카테고리인지 찾는 로직 id로
                        if (inner["shi_time"]!= null)
                            temp.Month.Value = inner["shi_time"].ToObject<DateTime>();
                        if (inner["shi_complete"] != null)
                            temp.FullyCompleted.Value = (AllocateType)inner["shi_complete"].ToObject<int>();
                        if (inner["shi_key"] != null)
                            temp.indexKey.Value = inner["shi_key"].ToString();
                        if (inner["connected_contract"] != null)
                        {
                            foreach (JObject jobj in inner["connected_contract"] as JArray)
                            {
                                Contract contract = new Contract();
                                if (jobj["con_id"] != null) { 
                                    contract.Id.Value = jobj["con_id"].ToObject<int>();
                                }
                                if (jobj["pay_id"] != null) { 
                                    Payment payment = new Payment();
                                    payment.PaymentId.Value = jobj["pay_id"].ToObject<int>();
                                    contract.Payment.Add(payment);
                                }
                                temp.ConnectedContract.Add(contract);
                            }
                        }
                        if (inner["remain_price"] != null)
                            temp.RemainPrice.Value = inner["remain_price"].ToObject<int>();
                        temp.ListNo.Value = i;
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            this.ReceiptItems.Add(temp);
                        });
                        
                        i++;
                    }
                } catch (Exception ex) 
                {
                
                }
                
            }
        }

        private BankModel FindBankList(string accountNum)
        {
            BankModel model=this.BankList.FirstOrDefault(c => c.AccountNum.Value == accountNum);
            return model;
        }

        public CategoryInfo FindCategory(int id) {
            SettingPageViewModel temp = this.ContainerProvider.Resolve<SettingPageViewModel>("GlobalData");
            CategoryInfo item = temp.CategoryInfos.First(c => c.CategoryId.Value == id);
            return item;
        }

        private void SetProductCategoryList(JObject msg)
        {
            if (msg.ToString().Equals(""))
            {
                return;
            }
            try
            {
                //foreach (JObject inner in msg["company_product"] as JArray)
                //{
                //    ReactiveProperty<string> CompanyName = new ReactiveProperty<string>(inner["company_name"].ToString());
                //    ReactiveProperty<int> CompanyID = new ReactiveProperty<int>(inner["company_id"].ToObject<int>());
                //    ReactiveCollection<Product> ProductList = new ReactiveCollection<Product>();
                //    foreach(JObject dept in inner["product"] as JArray) 
                //    {
                //        ReactiveProperty<string> ProductName = new ReactiveProperty<string>(dept["acpi_product_name"].ToString());
                //        ReactiveProperty<int> ProductPrice = new ReactiveProperty<int>(inner["acpi_product_price"].ToObject<int>());
                //        Product product = new Product(ProductPrice, ProductName);
                //        ProductList.Add(product);
                //    }
                //    Company company = new Company(CompanyID, CompanyName, ProductList);
                //    this.CompanyList.Add(company);
                //}
            }
            catch (Exception e) { }
        }

  
       

        public void OnConnected()
        {
            
        }

        public void OnSent()
        {
            
        }

    }
}
