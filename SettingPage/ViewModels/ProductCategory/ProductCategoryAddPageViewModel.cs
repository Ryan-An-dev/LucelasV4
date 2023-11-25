using CommonModel.Model;
using CommonModule.Views;
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
using System.Linq;

namespace SettingPage.ViewModels
{
    public class ProductCategoryAddPageViewModel : PrismCommonViewModelBase, IDialogAware
    {
        private DelegateCommand<string> _closeDialogCommand;
        public DelegateCommand<string> CloseDialogCommand =>
            _closeDialogCommand ?? (_closeDialogCommand = new DelegateCommand<string>(CloseDialog));

        public ReactiveProperty<FurnitureType> Category { get; set; }

        public ProductCategoryAddPageViewModel(IDialogService _DialogService, IContainerProvider con) : base(_DialogService, con)
        {
            Category = new ReactiveProperty<FurnitureType>().AddTo(disposable);
        }

        public string Title => throw new NotImplementedException();

        public event Action<IDialogResult> RequestClose;

        protected virtual void CloseDialog(string parameter)
        {

            DialogResult temp = null;
            ButtonResult result = ButtonResult.None;
            if (parameter?.ToLower() == "true")
            {
                if (this.Category.Value == null)
                    return;
                if (this.Category.Value.ValidateAllProperties())
                {
                    con.Resolve<AlertWindow1>().Show();
                    return;
                }
                result = ButtonResult.OK;
                DialogParameters p = new DialogParameters();
                p.Add("object", this.Category.Value);
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
            if (parameters.ContainsKey("object")) {
                FurnitureType category = null;
                parameters.TryGetValue("object", out category);
                if (category != null)
                {
                    this.Category.Value = category;
                }
            }
        }
    }
}
