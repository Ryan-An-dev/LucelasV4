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

namespace ContractPage.ViewModels
{
    public class SearchNamePageViewModel : PrismCommonModelBase, IDialogAware, INetReceiver
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

        public SearchNamePageViewModel(IContainerProvider con) : base()
        {
            this.ContainerProvider = con;
            this.Keyword = new ReactiveProperty<string>().AddTo(disposable);
            CustomerList = new ObservableCollection<Customer>();
            this.args = new ReactiveProperty<ReceiptModel>().AddTo(disposable);
        }

        public SearchNamePageViewModel()
        {

        }

        private void SearchBase(string value)
        {
            using (var network = this.ContainerProvider.Resolve<DataAgent.CustomerDataAgent>())
            {
                network.SetReceiver(this);
                //accountList, CategoryList, ProductList 기본으로 요청하기
                JObject jobj = new JObject();
                jobj["contractor_name"] = value;
                network.GetCustomerList(jobj);
            }
        }
        internal async void SearchName(string value)
        {
            CustomerList.Clear();
            SearchBase(value);
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
                p.Add("SelectedCustomer", this.SelectedItem);
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
            this.Keyword.Value=temp.Name.Value;
            SearchBase(this.Keyword.Value);
        }

        public override JObject GetChangedItem()
        {
            throw new NotImplementedException();
        }

        public void OnRceivedData(ErpPacket packet)
        {
            JObject jobj = null;
            string msg = Encoding.UTF8.GetString(packet.Body);
            try
            {
                jobj = new JObject(JObject.Parse(msg));
            }
            catch (Exception e) { }
            switch ((COMMAND)packet.Header.CMD)
            {
                case COMMAND.GETCUSTOMERINFO: //데이터 조회 완료
                    if (jobj != null)
                    {
                        JArray jarr = new JArray();
                        jarr=jobj["contractor"] as JArray;
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

                            this.CustomerList.Add(customer);
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
