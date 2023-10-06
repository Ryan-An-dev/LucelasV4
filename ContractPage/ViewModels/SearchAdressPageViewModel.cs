using AddressSearchManager;
using AddressSearchManager.Models;
using CommonModel.Model;
using DataAccess;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using PrsimCommonBase;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ContractPage.ViewModels
{
    public class SearchAdressPageViewModel : PrismCommonModelBase, IDialogAware
    {
        public AddressSearchManagerClass addrSearchManager { get; set; }
        public ReactiveCollection<AddressDetail> AddressDetails { get; set; }
        public IContainerProvider ContainerProvider { get; }
        public ReactiveProperty<ReceiptModel> args { get; set; }
        public AddressCommon Common => addrSearchManager.Common;
        public ReactiveProperty<string> Keyword { get; set; }

        private DelegateCommand _SearchDialogCommand;
        public DelegateCommand SearchDialogCommand =>
            _SearchDialogCommand ?? (_SearchDialogCommand = new DelegateCommand(SearchAddress));

        private DelegateCommand<string> _closeDialogCommand;
        public DelegateCommand<string> CloseDialogCommand =>
            _closeDialogCommand ?? (_closeDialogCommand = new DelegateCommand<string>(CloseDialog));

        public SearchAdressPageViewModel(IContainerProvider con) : base()
        {
            this.ContainerProvider = con;
            this.Keyword = new ReactiveProperty<string>().AddTo(disposable);
            this.addrSearchManager = new AddressSearchManagerClass();
            AddressDetails = new ReactiveCollection<AddressDetail>();
            this.args = new ReactiveProperty<ReceiptModel>().AddTo(disposable);
        }

        private void SearchBase(bool success)
        {
            if (success)
            {
                addrSearchManager.Details.ForEach(juso => AddressDetails.Add(juso));
            }

            if (!addrSearchManager.IsLoading && Common.ErrorCode != "0")
            {
                MessageBox.Show(Common.ErrorMessage);
            }
            
        }
        internal async void SearchAddress()
        {
            AddressDetails.Clear();
            bool success = await addrSearchManager.Search(Keyword.Value);
            SearchBase(success);
        }

        internal async Task OnScrollReachedBottom()
        {
            bool success = await addrSearchManager.SearchPage(Common.CurrentPage + 1);
            SearchBase(success);
        }

        protected virtual void CloseDialog(string parameter)
        {

            DialogResult temp = null;
            ButtonResult result = ButtonResult.None;
            if (parameter?.ToLower() == "true")
            {
                //if (this.SelectedPayment == null)
                //    return;
                //result = ButtonResult.OK;
                //temp = new DialogResult(result);
                //DialogParameters p = new DialogParameters();
                //p.Add("SelectedPaymentItem", this.SelectedPayment.Value);
                //temp.Parameters.Add("SelectedPaymentItem", p);
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
            
        }
    }
}
