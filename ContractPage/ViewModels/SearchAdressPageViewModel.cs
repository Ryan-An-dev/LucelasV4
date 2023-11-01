using AddressSearchManager;
using AddressSearchManager.Models;
using CommonModel.Model;
using DataAccess;
using Newtonsoft.Json.Linq;
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
    public class SearchAdressPageViewModel : PrismCommonViewModelBase, IDialogAware
    {
        public AddressSearchManagerClass addrSearchManager { get; set; }
        public ReactiveCollection<AddressDetail> AddressDetails { get; set; }
        public IContainerProvider ContainerProvider { get; }
        public ReactiveProperty<ReceiptModel> args { get; set; }
        public AddressDetail SelectedItem { get; set; }
        public AddressCommon Common => addrSearchManager.Common;
        public ReactiveProperty<string> Keyword { get; set; }
        
        private DelegateCommand _SearchDialogCommand;
        public DelegateCommand SearchDialogCommand =>
            _SearchDialogCommand ?? (_SearchDialogCommand = new DelegateCommand(SearchAddress));

        private DelegateCommand<string> _SearchExecute;
        public DelegateCommand<string> SearchExecute =>
            _SearchExecute ?? (_SearchExecute = new DelegateCommand<string>(SearchAddress));

        private DelegateCommand<string> _closeDialogCommand;
        public DelegateCommand<string> CloseDialogCommand =>
            _closeDialogCommand ?? (_closeDialogCommand = new DelegateCommand<string>(CloseDialog));

        public SearchAdressPageViewModel(IContainerProvider con) : base()
        {
            this.ContainerProvider = con;
            this.Keyword = new ReactiveProperty<string>().AddTo(disposable);
            this.addrSearchManager = new AddressSearchManagerClass();
            AddressDetails = new ReactiveCollection<AddressDetail>().AddTo(disposable);
            this.args = new ReactiveProperty<ReceiptModel>().AddTo(disposable);
        }

        public SearchAdressPageViewModel()
        {
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
        internal async void SearchAddress(string value)
        {
            AddressDetails.Clear();
            bool success = await addrSearchManager.Search(value);
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
                if (this.SelectedItem == null)
                    return;
                result = ButtonResult.OK;
                DialogParameters p = new DialogParameters();
                p.Add("SelectedAddress", this.SelectedItem);
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
            
        }
    }
}
