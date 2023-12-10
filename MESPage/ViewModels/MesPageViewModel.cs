using CommonModel;
using CommonModel.Model;
using DataAccess;
using DataAccess.NetWork;
using LogWriter;
using MaterialDesignThemes.Wpf;
using MESPage.Views;
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
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace MESPage.ViewModels
{

    public enum MovePageType { Next = 1, Prev }
    public class MesPageViewModel : PrismCommonViewModelBase, INavigationAware, INetReceiver
    {
        #region SearchCondition
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
        public ReactiveCollection<ContractedProduct> InventoryItems { get; }
        public ReactiveProperty<ContractedProduct> SelectedItem { get; set; }
        private IContainerProvider ContainerProvider { get; }
        public ReactiveCollection<FurnitureType> furnitureInfos { get; }

        public ReactiveProperty<bool> IsLoading { get; set; } //로딩
        public ReactiveProperty<Visibility> SearchVisibility { get; set; }

        public MesPageViewModel(IRegionManager regionManager, IContainerProvider containerProvider) : base(regionManager)
        {
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
            this.SearchPhone = new ReactiveProperty<string>().AddTo(this.disposable);
            NewButton = new DelegateCommand(NewButtonExecute);
            RowDoubleClick = new DelegateCommand(RowDoubleClickExecute);
            CmdGoPage = new DelegateCommand<object>(ExecCmdGoPage);
            InventoryItems = new ReactiveCollection<ContractedProduct>().AddTo(this.disposable);
            furnitureInfos = new ReactiveCollection<FurnitureType>().AddTo(this.disposable);
            SelectedItem = new ReactiveProperty<ContractedProduct>().AddTo(disposable);

            SettingPageViewModel temp = this.ContainerProvider.Resolve<SettingPageViewModel>("GlobalData");
            if (temp.FurnitureInfos.Count > 0)
            {
                this.furnitureInfos = temp.FurnitureInfos;
            }
            this.CountList.Add(30);
            this.CountList.Add(50);
            this.CountList.Add(70);
            this.CountList.Add(100);

            this.SearchVisibility = new ReactiveProperty<Visibility>(Visibility.Collapsed);
            this.IsLoading = new ReactiveProperty<bool>(false).AddTo(disposable);
            this.IsLoading.Subscribe(x => OnLoadingChanged(x));
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
                //accountList, CategoryList, ProductList 기본으로 요청하기
                JObject jobj = new JObject();
                jobj["page_unit"] = (ListCount.Value * CurrentPage.Value) > TotalItemCount.Value ? TotalItemCount.Value - (ListCount.Value * (CurrentPage.Value - 1)) : ListCount.Value;
                jobj["page_start_pos"] = (this.CurrentPage.Value - 1) * this.ListCount.Value;
                JObject search = new JObject();
                search["cui_name"] = this.SearchName.Value;
                search["start_time"] = this.StartDate.Value.ToString("yyyy-MM-dd HH:mm:ss");
                search["end_time"] = this.EndDate.Value.ToString("yyyy-MM-dd HH:mm:ss");
                search["complete"] = (int)this.SearchFullyCompleted.Value;
                search["cui_phone"] = this.SearchPhone.Value;
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
                jobj["page_unit"] = (ListCount.Value * CurrentPage.Value) > TotalItemCount.Value ? TotalItemCount.Value - (ListCount.Value * (CurrentPage.Value - 1)) : ListCount.Value;
                jobj["page_start_pos"] = (this.CurrentPage.Value - 1) * this.ListCount.Value;
                JObject search = new JObject();
                search["cui_name"] = this.SearchName.Value;
                search["start_time"] = this.StartDate.Value.ToString("yyyy-MM-dd HH:mm:ss");
                search["end_time"] = this.EndDate.Value.ToString("yyyy-MM-dd HH:mm:ss");
                search["complete"] = (int)this.SearchFullyCompleted.Value;
                search["cui_phone"] = this.SearchPhone.Value;
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
                    IsLoading.Value = false;
                    break;

            }
        }

        public void OnSent()
        {
            
        }
    }
}
