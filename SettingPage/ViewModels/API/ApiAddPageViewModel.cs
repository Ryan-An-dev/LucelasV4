using AddressSearchManager.Models;
using CommonModel;
using CommonModel.Model;
using CommonModule.Views;
using Newtonsoft.Json.Linq;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
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
        public ReactiveProperty<API> API { get; set; }
        public ApiAddPageViewModel(IDialogService _DialogService, IContainerProvider con) : base(_DialogService, con)
        {
            DialogService = _DialogService;
            API = new ReactiveProperty<API>().AddTo(disposable);
        }

        public string Title => throw new NotImplementedException();

        public event Action<IDialogResult> RequestClose;

        protected virtual void CloseDialog(string parameter)
        {

            DialogResult temp = null;
            ButtonResult result = ButtonResult.None;
            if (parameter?.ToLower() == "true")
            {
                if (this.API.Value == null)
                    return;
                if (this.API.Value.ValidateAllProperties())
                {
                    con.Resolve<AlertWindow1>().Show();
                    return;
                }
                result = ButtonResult.OK;
                DialogParameters p = new DialogParameters();
                p.Add("object", this.API.Value);
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
                API API = null;
                parameters.TryGetValue("object", out API);
                if (API != null)
                {
                    this.API.Value = API;
                }
            }
        }
    }
}
