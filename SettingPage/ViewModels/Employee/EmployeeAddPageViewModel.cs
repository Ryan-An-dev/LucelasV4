using AddressSearchManager.Models;
using CommonModel.Model;
using Newtonsoft.Json.Linq;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using PrsimCommonBase;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SettingPage.ViewModels
{
    public class EmployeeAddPageViewModel : PrismCommonViewModelBase, IDialogAware
    {
        private DelegateCommand<string> _closeDialogCommand;
        public DelegateCommand<string> CloseDialogCommand =>
            _closeDialogCommand ?? (_closeDialogCommand = new DelegateCommand<string>(CloseDialog));
        public ReactiveProperty<Employee> Employee { get; set; }
        public DelegateCommand SearchAddress { get; }
        IDialogService DialogService;

        public EmployeeAddPageViewModel(IDialogService dialogService)
        {
            DialogService = dialogService;
            Employee = new ReactiveProperty<Employee>().AddTo(disposable);
            SearchAddress = new DelegateCommand(SearchAdressExcute);
        }

        public string Title => throw new NotImplementedException();

        public event Action<IDialogResult> RequestClose;

        protected virtual void CloseDialog(string parameter)
        {

            DialogResult temp = null;
            ButtonResult result = ButtonResult.None;
            if (parameter?.ToLower() == "true")
            {
                if (this.Employee.Value == null)
                    return;
                result = ButtonResult.OK;
                DialogParameters p = new DialogParameters();
                p.Add("object", this.Employee.Value);
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
                Employee Employee = null;
                parameters.TryGetValue("object", out Employee);
                if (Employee != null)
                {
                    this.Employee.Value = Employee;
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
                        Employee.Value.Address.Value = temp.도로명주소1;
                    }
                }
            }
            else
            {

            }
        }
    }
}
