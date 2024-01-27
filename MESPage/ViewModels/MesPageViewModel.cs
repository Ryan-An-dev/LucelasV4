using CommonModel;
using CommonModel.Model;
using DataAccess;
using DataAccess.NetWork;
using LogWriter;
using MaterialDesignThemes.Wpf;
using MESPage.Views;
using Newtonsoft.Json;
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
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Markup;

namespace MESPage.ViewModels
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum InventoryType
    {
        [Description("계약재고")]
        Contract = 1,
        [Description("창고재고")]
        Stock = 2,
    }
    public enum MovePageType { Next = 1, Prev }
    public class MesPageViewModel : PrismCommonViewModelBase, INavigationAware, INetReceiver
    {
        #region SearchCondition
        public ReactiveProperty<InventoryType> SelectInventoryType { get; set; }
        public ReactiveProperty<Purpose> SearchPurpose { get; set; }
        public ReactiveProperty<FurnitureType>SelectedType { get; set; }
        public ReactiveProperty<string> SearchPhone { get; set; }
        public ReactiveProperty<string> SearchName { get; set; }
        public ReactiveProperty<DateTime> EndDate { get; set; }
        public ReactiveProperty<FullyCompleted> SearchFullyCompleted { get; set; }
        public IEnumerable<FullyCompleted> SearchFullyCompletedValues
        {
            get { return Enum.GetValues(typeof(FullyCompleted)).Cast<FullyCompleted>(); }
        }
        public IEnumerable<InventoryType> SearchFullInventoryType
        {
            get { return Enum.GetValues(typeof(InventoryType)).Cast<InventoryType>(); }
        }
        public IEnumerable<Purpose> SearchPurposeValues
        {
            get { return Enum.GetValues(typeof(Purpose)).Cast<Purpose>(); }
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
        public ReactiveCollection<FurnitureInventory> InventoryItems { get; }
        public ReactiveProperty<FurnitureInventory> SelectedItem { get; set; }
        private IContainerProvider ContainerProvider { get; }
        public ReactiveCollection<FurnitureType> furnitureInfos { get; }

        public ReactiveProperty<bool> IsLoading { get; set; } //로딩
        public ReactiveProperty<Visibility> SearchVisibility { get; set; }

        public MesPageViewModel(IRegionManager regionManager, IContainerProvider containerProvider) : base(regionManager)
        {
            this.SelectInventoryType = new ReactiveProperty<InventoryType>(InventoryType.Contract).AddTo(this.disposable);
            this.SearchPurpose = new ReactiveProperty<Purpose>().AddTo(this.disposable);
            this.ContainerProvider = containerProvider;
            this.SearchFullyCompleted = new ReactiveProperty<FullyCompleted>((FullyCompleted)0).AddTo(this.disposable);
            this.EndDate = new ReactiveProperty<DateTime>(DateTime.Now).AddTo(this.disposable);
            this.CurrentPage = new ReactiveProperty<int>(1).AddTo(this.disposable);
            this.ListCount = new ReactiveProperty<int>(30).AddTo(this.disposable);
            this.FirstItem = new ReactiveProperty<int>(0).AddTo(this.disposable);
            this.TotalPage = new ReactiveProperty<int>(0).AddTo(this.disposable);
            this.SearchName = new ReactiveProperty<string>("").AddTo(this.disposable);
            this.TotalItemCount = new ReactiveProperty<int>(0).AddTo(this.disposable);
            this.TotalItemCount.Subscribe(c => this.TotalPage.Value = (c / this.ListCount.Value) + 1);
            this.SearchButton = new DelegateCommand(SearchContractExecute);
            this.SearchPhone = new ReactiveProperty<string>().AddTo(this.disposable);
            NewButton = new DelegateCommand(NewButtonExecute);
            RowDoubleClick = new DelegateCommand(RowDoubleClickExecute);
            CmdGoPage = new DelegateCommand<object>(ExecCmdGoPage);
            InventoryItems = new ReactiveCollection<FurnitureInventory>().AddTo(this.disposable);
            furnitureInfos = new ReactiveCollection<FurnitureType>().AddTo(this.disposable);
            SelectedItem = new ReactiveProperty<FurnitureInventory>().AddTo(disposable);

            SettingPageViewModel temp = this.ContainerProvider.Resolve<SettingPageViewModel>();
            if (temp.FurnitureInfos.Count > 0)
            {
                this.furnitureInfos = temp.FurnitureInfos;
            }
            FurnitureType all = new FurnitureType();
            all.Id.Value = 0;
            all.Name.Value = "모두";
            this.furnitureInfos.Insert(0, all);
            this.SelectedType = new ReactiveProperty<FurnitureType>(furnitureInfos[0]).AddTo(this.disposable);

            this.CountList.Add(30);
            this.CountList.Add(50);
            this.CountList.Add(70);
            this.CountList.Add(100);

            this.SearchVisibility = new ReactiveProperty<Visibility>(Visibility.Collapsed);
            this.IsLoading = new ReactiveProperty<bool>(false).AddTo(disposable);
            this.IsLoading.Subscribe(x => OnLoadingChanged(x));
            this.SelectInventoryType.Subscribe(x => OnChangedInventoryType(x));
        }
        private void OnChangedInventoryType(InventoryType type)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.InventoryItems.Clear();
            });
        }


        private void OnLoadingChanged(bool isLoading)
        {
            SearchVisibility.Value = isLoading ? Visibility.Visible : Visibility.Collapsed;

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
            using (var network = this.ContainerProvider.Resolve<DataAgent.InventoryDataAgent>())
            {
                network.SetReceiver(this);
                JObject jobj = new JObject();
                jobj["page_unit"] = (ListCount.Value * CurrentPage.Value) > TotalItemCount.Value ? TotalItemCount.Value - (ListCount.Value * (CurrentPage.Value - 1)) : ListCount.Value;
                jobj["page_start_pos"] = (this.CurrentPage.Value - 1) * this.ListCount.Value;
                JObject search = new JObject();
                int[] temp = new int[1];
                temp[0] = (int)this.SearchPurpose.Value;
                search["receiving_type"] = new JArray(temp);
                search["product_type"] = (int)this.SelectedType.Value.Id.Value;
                search["inventory_type"] = (int)this.SelectInventoryType.Value;
                search["end_time"] = this.EndDate.Value.ToString("yyyy-MM-dd 23:59:59");
                jobj["search_option"] = search;
                network.Get(jobj);
                IsLoading.Value = true;
            }
        }
        private void SendData()
        {
            using (var network = this.ContainerProvider.Resolve<DataAgent.InventoryDataAgent>())
            {
                network.SetReceiver(this);
                JObject jobj = new JObject();
                jobj["page_unit"] = this.ListCount.Value;
                jobj["page_start_pos"] = (this.CurrentPage.Value - 1) * this.ListCount.Value;
                JObject search = new JObject();
                int[] temp = new int[1];
                temp[0] = (int)this.SearchPurpose.Value;
                search["receiving_type"] = new JArray(temp);
                search["inventory_type"] = (int)this.SelectInventoryType.Value;
                search["product_type"] = (int)this.SelectedType.Value.Id.Value;
                search["end_time"] = this.EndDate.Value.ToString("yyyy-MM-dd 23:59:59");
                jobj["search_option"] = search;
                network.Get(jobj);
                IsLoading.Value = true;
            }
        }
        private void NewButtonExecute()
        {
            regionManager.RequestNavigate("MesSingleRegion", nameof(MesSingle));
            DrawerHost.OpenDrawerCommand.Execute(Dock.Right, null);
        }
        private void RowDoubleClickExecute()
        {
            if (SelectedItem.Value == null)
                return;
            SelectedItem.Value.ClearJson();
            SelectedItem.Value.isChanged = false;
            var p = new NavigationParameters();
            if (SelectedItem != null)
            {
                p.Add("object", SelectedItem.Value);
            }

            regionManager.RequestNavigate("MesSingleRegion", nameof(MesSingle), p);
            DrawerHost.OpenDrawerCommand.Execute(Dock.Right, null);
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnConnected()
        {
            
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            this.Dispose();
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            this.FirstItem.Value = 0;
            this.CurrentPage.Value = 1;
            this.TotalPage.Value = 0;
            this.TotalItemCount.Value = 0;
            string msg;
            navigationContext.Parameters.TryGetValue("object", out msg);
            if (msg == null)
            {
               
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    this.SelectInventoryType.Value = InventoryType.Contract;
                    this.EndDate.Value = DateTime.Now.AddDays(3);
                    this.SearchPurpose.Value = Purpose.PreOrder;
                });
            }
            SendData();
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
                case COMMAND.GET_INVENTORY_LIST: //데이터 조회
                    if (jobj["inventory_history"] != null && !jobj["inventory_history"].ToString().Equals(""))
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            this.InventoryItems.Clear();
                        });
                        
                        JArray jarr = JArray.Parse(jobj["inventory_history"].ToString());
                        int i = 0;
                        foreach (var item in jarr)
                        {
                            FurnitureInventory temp = new FurnitureInventory();
                            if (item["count"] != null)
                                temp.Count.Value = item["count"].Value<int>();
                            if (temp.Count.Value == 0) { 
                                continue;
                            }
                            if (item["inventory_id"]!=null)
                                temp.Id.Value = item["inventory_id"].Value<int>();
                            if (item["memo"] != null)
                                temp.Memo.Value = item["memo"].Value<string>();
                            if (item["receiving_date"] != null) {
                                try {
                                    temp.StoreReachDate.Value = item["receiving_date"].ToObject<DateTime>();
                                } catch (Exception) {
                                    temp.StoreReachDate.Value = null;
                                }
                            }
                            if (item["product_info"] != null) { 
                                temp.Product.Value = SetProductInfo(item["product_info"] as JObject);
                                if (temp.Product.Value.Price.Value != 0)
                                    temp.RealPrice.Value = temp.Product.Value.Price.Value;
                            }
                            if (item["receiving_type"] != null)
                            {
                                try {
                                    temp.ReceivingType.Value = (Purpose)item["receiving_type"].ToObject<int>();
                                } catch (Exception) { 
                                
                                }
                            }
                            if (item["connected_contract"] != null)
                            {
                                if (!(item["connected_contract"].ToString().Equals("")))
                                {
                                    temp.ContractedContract.Value = SetContract(item["connected_contract"] as JObject);
                                    temp.CountEnable.Value = false;
                                }
                                else {
                                    temp.CountEnable.Value = true;
                                }
                                
                            }
                            temp.No.Value = ++i;
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                
                                    this.InventoryItems.Add(temp);
                            });
                        }
                        this.TotalItemCount.Value = jobj["history_count"].Value<int>();
                        this.TotalPage.Value = (this.TotalItemCount.Value / this.ListCount.Value) + 1;
                        this.FirstItem.Value = (this.CurrentPage.Value - 1) * this.ListCount.Value;
                    }
                    else
                    {
                        
                    }
                    IsLoading.Value = false;
                    break;

            }
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
        private Contract SetContract(JObject jobj)
        {
            Contract temp = new Contract();
            if (jobj["con_id"] != null)
                temp.Id.Value = jobj["con_id"].Value<int>();
            if (jobj["cui_name"] !=null)
                temp.Contractor.Value.Name.Value = jobj["cui_name"].Value<string>();
            if (jobj["cui_address"] !=null)
                temp.Contractor.Value.Address.Value = jobj["cui_address"].Value<string>();
            if (jobj["cui_address_detail"]!=null)
                temp.Contractor.Value.Address1.Value = jobj["cui_address_detail"].Value<string>();
            if (jobj["delivery_date"]!=null)
                temp.Delivery.Value = jobj["delivery_date"].Value<DateTime>();
            if (jobj["create_time"] != null)
                temp.Month.Value = jobj["create_time"].Value<DateTime>();
            return temp;
        }

        public void OnSent()
        {
            
        }
    }
}
