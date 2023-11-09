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
    public class ApiAddPageViewModel : PrismCommonViewModelBase, IDialogAware
    {
        IDialogService DialogService;
        private DelegateCommand<string> _closeDialogCommand;
        public DelegateCommand<string> CloseDialogCommand =>
            _closeDialogCommand ?? (_closeDialogCommand = new DelegateCommand<string>(CloseDialog));
        public ReactiveProperty<BankModel> BankModel { get; set; }
        public ApiAddPageViewModel(IDialogService _DialogService)
        {
            DialogService = _DialogService;
            BankModel = new ReactiveProperty<BankModel>().AddTo(disposable);
        }

        public string Title => throw new NotImplementedException();

        public event Action<IDialogResult> RequestClose;

        protected virtual void CloseDialog(string parameter)
        {

            DialogResult temp = null;
            ButtonResult result = ButtonResult.None;
            if (parameter?.ToLower() == "true")
            {
                if (this.BankModel.Value == null)
                    return;
                result = ButtonResult.OK;
                DialogParameters p = new DialogParameters();
                p.Add("object", this.BankModel.Value);
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
            this.disposable.Dispose();
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            if (parameters.ContainsKey("object"))
            {
                BankModel BankModel = null;
                parameters.TryGetValue("object", out BankModel);
                if (BankModel != null)
                {
                    this.BankModel.Value = BankModel;
                }
            }
        }
    }
}
