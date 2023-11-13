using AddressSearchManager.Models;
using AddressSearchManager;
using CommonModel.Model;
using Newtonsoft.Json.Linq;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using PrsimCommonBase;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Reactive.Bindings.Extensions;
using DataAccess.NetWork;
using DataAccess;
using MaterialDesignThemes.Wpf;
using System.Text;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using LogWriter;

namespace ContractPage.ViewModels
{
    public class SearchNamePageViewModel : PrismCommonViewModelBase, IDialogAware, INetReceiver
    {
        public AddressSearchManagerClass addrSearchManager { get; set; }
        public ObservableCollection<Customer> CustomerList { get; set; }
        public IContainerProvider ContainerProvider { get; }
        public ReactiveProperty<ReceiptModel> args { get; set; }
        public Customer SelectedItem { get; set; }
        public ReactiveProperty<string> Keyword { get; set; }

        private DelegateCommand<string> _SearchDialogCommand;
        public DelegateCommand<string> SearchDialogCommand =>
            _SearchDialogCommand ?? (_SearchDialogCommand = new DelegateCommand<string>(SearchBase));

        private DelegateCommand<string> _closeDialogCommand;
        public DelegateCommand<string> CloseDialogCommand =>
            _closeDialogCommand ?? (_closeDialogCommand = new DelegateCommand<string>(CloseDialog));
        public DelegateCommand RowDoubleClick { get; }
        
        
        #region Paging Name
        public ReactiveProperty<int> CurrentPage { get; set; }
        public ReactiveProperty<int> TotalPage { get; set; }
        public ReactiveProperty<int> TotalItemCount { get; set; }
        public ReactiveProperty<int> ListCount { get; set; }
        public ReactiveProperty<int> FirstItem { get; set; }
        public DelegateCommand<object> CmdGoPage { get; }
        public ObservableCollection<int> CountList { get; set; } = new ObservableCollection<int>();
        #endregion

        public IDialogService dialogService { get; set; }

        public SearchNamePageViewModel(IContainerProvider con,IDialogService DialogService) : base()
        {
            this.dialogService = DialogService;
            this.ContainerProvider = con;
            this.Keyword = new ReactiveProperty<string>().AddTo(disposable);
            CustomerList = new ObservableCollection<Customer>();
            this.args = new ReactiveProperty<ReceiptModel>().AddTo(disposable);
            this.ListCount = new ReactiveProperty<int>(30).AddTo(this.disposable);
            this.FirstItem = new ReactiveProperty<int>(0).AddTo(this.disposable);
            this.TotalPage = new ReactiveProperty<int>(0).AddTo(this.disposable);
            this.TotalItemCount = new ReactiveProperty<int>(0).AddTo(this.disposable);
            this.TotalItemCount.Subscribe(c => this.TotalPage.Value = (c / this.ListCount.Value) + 1);
            this.CurrentPage = new ReactiveProperty<int>(1).AddTo(this.disposable);
            CmdGoPage = new DelegateCommand<object>(ExecCmdGoPage);
            CountList.Add(30);
            CountList.Add(50);
            CountList.Add(100);
            this.RowDoubleClick = new DelegateCommand(RowDoubleClickEvent);
        }

        private void RowDoubleClickEvent()
        {
            CloseDialog("true");
        }

        public SearchNamePageViewModel()
        {

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
            using (var network = this.ContainerProvider.Resolve<DataAgent.CustomerDataAgent>())
            {
                network.SetReceiver(this);
                JObject jobj = new JObject();
                jobj["next_preview"] = (int)param;
                jobj["page_unit"] = (this.ListCount.Value * CurrentPage.Value) > this.TotalItemCount.Value ? (this.ListCount.Value * CurrentPage.Value) - this.TotalItemCount.Value : this.ListCount.Value;
                jobj["page_start_pos"] = (this.CurrentPage.Value - 1) * this.ListCount.Value;
                JObject inner = new JObject();
                inner["employee_name"] = this.Keyword.Value;
                jobj["search_option"] = inner;
                network.GetCustomerList(jobj);
            }
        }
        private void SearchBase(string value)
        {
            if (string.IsNullOrEmpty(value))
                return;
            using (var network = this.ContainerProvider.Resolve<DataAgent.CustomerDataAgent>())
            {
                network.SetReceiver(this);
                //accountList, CategoryList, ProductList 기본으로 요청하기
                JObject jobj = new JObject();
                jobj["page_unit"] = (this.ListCount.Value * CurrentPage.Value) > this.TotalItemCount.Value ? (this.ListCount.Value * CurrentPage.Value) - this.TotalItemCount.Value : this.ListCount.Value;
                jobj["page_start_pos"] = (this.CurrentPage.Value - 1) * this.ListCount.Value;
                JObject inner = new JObject();
                inner["customer_name"] = value;
                jobj["search_option"] = inner;
                network.GetCustomerList(jobj);
            }
        }
        internal async void SearchName(string value)
        {
            if (!string.IsNullOrEmpty(value)) {
                CustomerList.Clear();
                SearchBase(value);
            }
        }

        protected virtual void CloseDialog(string parameter)
        {

            DialogResult temp = null;
            ButtonResult result = ButtonResult.None;

            if (parameter?.ToLower() == "true")
            {
                if (this.SelectedItem == null)
                    return;
                result = ButtonResult.OK;
                DialogParameters p = new DialogParameters();
                p.Add("object", this.SelectedItem);
                temp = new DialogResult(result, p);
            }
            else if (parameter?.ToLower() == "false")
            {
                result = ButtonResult.Cancel;
                temp = new DialogResult(result);
            }
            RaiseRequestClose(temp);
        }
        public virtual void RaiseRequestClose(IDialogResult dialogResult)
        {
            RequestClose?.Invoke(dialogResult);
        }

        public string Title => throw new NotImplementedException();

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {

        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            Customer temp = null;
            parameters.TryGetValue("Contractor", out temp);
            if (temp.Name.Value.Equals("")) {
                return;
            }
            this.Keyword.Value= temp.Name.Value;
            SearchBase(this.Keyword.Value);
        }

        public void OnRceivedData(ErpPacket packet)
        {
            JObject jobj = null;
            string msg = Encoding.UTF8.GetString(packet.Body);

            try
            {
                jobj = new JObject(JObject.Parse(msg));
            }
            catch (Exception e) { return; }
            ErpLogWriter.LogWriter.Trace(jobj.ToString());
            switch ((COMMAND)packet.Header.CMD)
            {
                case COMMAND.GETCUSTOMERINFO: //데이터 조회 완료
                    if (jobj != null)
                    {
                        JArray jarr = new JArray();
                        try { jarr = jobj["customer_list"] as JArray; } catch (Exception e) { break; }
                        if (jarr == null)
                            return;

                        if (jarr.Count > 0)
                        {
                            foreach (JObject inner in jarr)
                            {
                                Customer customer = new Customer();
                                if (inner["cui_id"] != null)
                                    customer.Id.Value = inner["cui_id"].ToObject<int>();
                                if (inner["cui_name"] != null)
                                    customer.Name.Value = inner["cui_name"].ToString();
                                if (inner["cui_phone"] != null)
                                    customer.Phone.Value = inner["cui_phone"].ToObject<string>();
                                if (inner["cui_address"] != null)
                                    customer.Address.Value = inner["cui_address"].ToObject<string>();
                                if (inner["cui_address_detail"] != null)
                                    customer.Address1.Value = inner["cui_address_detail"].ToObject<string>();
                                if (inner["cui_memo"] != null)
                                    customer.Memo.Value = inner["cui_memo"].ToObject<string>();
                                Application.Current.Dispatcher.BeginInvoke(() =>
                                {
                                    this.CustomerList.Add(customer);
                                });
                            }
                        }
                        else {
                            DialogParameters dialogParameters = new DialogParameters();
                            dialogParameters.Add("object", new Customer());
                            dialogService.ShowDialog("CustomerAddPage", dialogParameters, r =>
                            {
                                try
                                {
                                    if (r.Result == ButtonResult.OK)
                                    {
                                        Customer item = r.Parameters.GetValue<Customer>("object");
                                        if (item != null)
                                        {
                                            using (var network = ContainerProvider.Resolve<DataAgent.CustomerDataAgent>())
                                            {
                                                network.SetReceiver(this);
                                                JObject jobj = new JObject();
                                                jobj["cui_id"] = (int)0;
                                                jobj["cui_name"] = item.Name.Value;
                                                jobj["cui_phone_num"] = item.Phone.Value;
                                                jobj["cui_address"] = item.Address.Value;
                                                jobj["cui_address_detail"] = item.Address1.Value;
                                                jobj["cui_memo"] = item.Memo.Value;
                                                network.CreateCustomerList(jobj);
                                            }
                                        }
                                    }
                                }
                                catch (Exception) { }

                            }, "CommonDialogWindow");
                        }
                    }
                    break;
            }
        }

        public void OnConnected()
        {
            throw new NotImplementedException();
        }

        public void OnSent()
        {
            throw new NotImplementedException();
        }
    }
}
