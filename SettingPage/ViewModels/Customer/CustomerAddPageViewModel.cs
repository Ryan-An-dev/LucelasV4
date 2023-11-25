using AddressSearchManager.Models;
using CommonModel.Model;
using CommonModule.Views;
using Newtonsoft.Json.Linq;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using CommonModel;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows;

namespace SettingPage.ViewModels
{
    public class CustomerAddPageViewModel : PrismCommonViewModelBase, IDialogAware
    {
        public DelegateCommand SearchAddress { get; }

        private DelegateCommand<string> _closeDialogCommand;
        public DelegateCommand<string> CloseDialogCommand =>
            _closeDialogCommand ?? (_closeDialogCommand = new DelegateCommand<string>(CloseDialog));

        public ReactiveProperty<Customer> Customer { get; set; }
        public CustomerAddPageViewModel(IDialogService _DialogService,IContainerProvider con):base(_DialogService, con)
        {
            SearchAddress = new DelegateCommand(SearchAdressExcute);
            Customer = new ReactiveProperty<Customer>().AddTo(disposable);
        }

        public string Title => throw new NotImplementedException();

        public event Action<IDialogResult> RequestClose;

        protected virtual void CloseDialog(string parameter)
        {
            
            //유효성검사
            DialogResult temp = null;
            ButtonResult result = ButtonResult.None;
            if (parameter?.ToLower() == "true")
            {
                
                if (this.Customer.Value == null)
                    return;
                if (this.Customer.Value.ValidateAllProperties())
                {
                    con.Resolve<AlertWindow1>().Show();
                    return;
                }
                result = ButtonResult.OK;
                DialogParameters p = new DialogParameters();
                p.Add("object", this.Customer.Value);
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
        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {

        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.ContainsKey("object"))
            {
                Customer Customer = null;
                parameters.TryGetValue("object", out Customer);
                if (Customer != null)
                {
                    this.Customer.Value = Customer;
                }
            }
        }

        private void SearchAdressExcute()
        {
            this.DialogService.ShowDialog("SearchAdressPage", null, r => FindAddressItem(r), "CommonDialogWindow");
        }
        private void FindAddressItem(IDialogResult r)
        {
            //Contract ID 받아야되는데 
            if (r == null) return;
            if (r.Result == ButtonResult.OK)
            {
                if (!r.Parameters.ContainsKey("object")) return;
                else
                {
                    AddressDetail temp = null;
                    r.Parameters.TryGetValue("object", out temp);
                    if (temp != null)
                    {
                        Customer.Value.Address.Value = temp.도로명주소1;
                    }
                }
            }
            else
            {

            }
        }
    }
}
