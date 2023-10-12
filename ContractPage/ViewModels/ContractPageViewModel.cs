using CommonModel.Model;
using ContractPage.Views;
using DataAccess;
using DataAccess.NetWork;
using LogWriter;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json.Linq;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using PrsimCommonBase;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SettingPage.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace ContractPage.ViewModels
{
    public enum MovePageType { Next = 1, Prev }
    public class ContractPageViewModel : PrismCommonModelBase, INavigationAware, INetReceiver
    {
        #region SearchCondition
        
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


        #region Paging
        public ReactiveProperty<int> CurrentPage { get; set; }
        public ReactiveProperty<int> TotalPage { get; set; }
        public ReactiveProperty<int> TotalItemCount { get; set; }
        public ReactiveProperty<int> ListCount { get; set; }
        public ReactiveProperty<int> FirstItem { get; set; }
        #endregion

        public DelegateCommand NewButton { get; }
        public DelegateCommand<object> CmdGoPage { get; }
        public DelegateCommand RowDoubleClick { get; }
        public ReactiveCollection<Contract> ContractItems { get; }
        public ReactiveProperty<Contract> SelectedItem { get; set; }
        private IContainerProvider ContainerProvider { get; }

        public ContractPageViewModel(IRegionManager regionManager, IContainerProvider containerProvider):base(regionManager)
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
            NewButton = new DelegateCommand(NewButtonExecute);
            RowDoubleClick = new DelegateCommand(RowDoubleClickExecute);
            CmdGoPage = new DelegateCommand<object>(ExecCmdGoPage);
            ContractItems = new ReactiveCollection<Contract>().AddTo(this.disposable);
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
            using (var network = this.ContainerProvider.Resolve<DataAgent.ContractDataAgent>())
            {
                network.SetReceiver(this);
                //accountList, CategoryList, ProductList 기본으로 요청하기
                JObject jobj = new JObject();
                jobj["next_preview"] = (int)param;
                jobj["con_name"] = this.SearchName.Value;
                jobj["page_unit"] = (this.ListCount.Value * CurrentPage.Value) > this.TotalItemCount.Value ? (this.ListCount.Value * CurrentPage.Value) - this.TotalItemCount.Value : this.ListCount.Value;
                jobj["page_start_pos"] = (this.CurrentPage.Value - 1) * this.ListCount.Value;
                jobj["start_time"] = this.StartDate.Value.ToString("yyyy-MM-dd HH:mm:ss");
                jobj["end_time"] = this.EndDate.Value.ToString("yyyy-MM-dd HH:mm:ss");
                jobj["complete"] = (int)this.SearchFullyCompleted.Value;
                network.GetContractList(jobj);
            }
        }
        private void SendData()
        {
            using (var network = this.ContainerProvider.Resolve<DataAgent.ContractDataAgent>())
            {
                network.SetReceiver(this);
                JObject jobj = new JObject();
                jobj["con_name"] = this.SearchName.Value;
                jobj["page_unit"] = (this.ListCount.Value * CurrentPage.Value) > this.TotalItemCount.Value ? this.TotalItemCount.Value - (this.ListCount.Value * (CurrentPage.Value - 1)) : this.ListCount.Value;
                jobj["page_start_pos"] = (this.CurrentPage.Value - 1) * this.ListCount.Value;
                jobj["start_time"] = this.StartDate.Value.ToString("yyyy-MM-dd HH:mm:ss");
                jobj["end_time"] = this.EndDate.Value.ToString("yyyy-MM-dd HH:mm:ss");
                jobj["complete"] = (int)this.SearchFullyCompleted.Value;
                network.GetContractList(jobj);
            }
        }
        private void NewButtonExecute()
        {
            regionManager.RequestNavigate("ContractSingleRegion", nameof(ContractSingle));
            DrawerHost.OpenDrawerCommand.Execute(Dock.Right, null);
        }
        private void RowDoubleClickExecute()
        {
            var p = new NavigationParameters();
            if (SelectedItem != null)
            {
                p.Add(nameof(Contract), SelectedItem);
            }

            regionManager.RequestNavigate("ContractSingleRegion", nameof(ContractSingle), p);
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
            this.Dispose();
        }

        public void OnRceivedData(ErpPacket packet)
        {
            string msg = Encoding.UTF8.GetString(packet.Body);
            JObject jobj = null;
            try { jobj = new JObject(JObject.Parse(msg)); } 
            catch (Exception) {
                return;
            }
            
            ErpLogWriter.LogWriter.Trace(jobj.ToString());
            switch ((COMMAND)packet.Header.CMD)
            {
                case COMMAND.GetContractList: //데이터 조회
                    SetContractHistory(jobj);
                    break;

            }
        }
        private void SetContractHistory(JObject msg)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.ContractItems.Clear();
            });
            if (msg.ToString().Trim() != string.Empty)
            {
                try {
                    int i = 1 + ((CurrentPage.Value - 1) * ListCount.Value);
                    if (msg["history_count"] != null)
                        this.TotalItemCount.Value = msg["history_count"].ToObject<int>();
                    if (msg["contract_history"] != null) {
                        foreach (JObject inner in msg["contract_history"] as JArray)
                        {
                            int id = 0;
                            //Customer Contractor = JsonSerializer.Deserialize<Customer>(inner["contractor"].ToString());
                            
                            //if (inner["contractor"] != null)
                            //{
                            //    if (inner["contractor"]["cui_id"] != null)
                            //    {
                            //        id = inner["cui_id"].ToObject<int>();
                            //        Contractor = FindCustomer(id);
                            //    }
                            //}
                            Contract temp = JsonSerializer.Deserialize<Contract>(inner.ToString());

                            //if (inner["create_time"] != null)
                            //    temp.Month.Value = inner["create_time"].ToObject<DateTime>();
                            //if (inner["con_id"] != null)
                            //    temp.Id.Value = inner["con_id"].ToObject<int>();
                            //if (inner["memo"] != null)
                            //    temp.Memo.Value = inner["memo"].ToString();
                            //if (inner["payment_complete"] != null)
                            //    temp.PaymentComplete.Value = (FullyCompleted)inner["payment_complete"].ToObject<int>();
                            //if (Contractor != null)
                            //    temp.Contractor.Value = Contractor;
                            //if (inner["delivery_date"] != null)
                            //    temp.Delivery.Value = inner["delivery_date"].ToObject<DateTime>();
                            //if (inner["total"] != null)
                            //    temp.Price.Value = inner["total"].ToObject<int>();
                            this.ContractItems.Add(temp);
                        }
                    }
                }
                catch (Exception ex) { }
            }
        }
        public Customer FindCustomer(int id)
        {
            SettingPageViewModel temp = this.ContainerProvider.Resolve<SettingPageViewModel>("GlobalData");
            Customer item = temp.CustomerInfos.First(c => c.Id.Value == id);
            return item;
        }
        //public Product FindProduct(int cpid,int pdid)
        //{
        //    SettingPageViewModel temp = this.ContainerProvider.Resolve<SettingPageViewModel>("GlobalData");
        //    Company item = temp.CompanyInfos.First(c => c.Id.Value == cpid);
        //    return item;
        //}
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
