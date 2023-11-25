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
using System.Collections.ObjectModel;
using System.Linq;

namespace SettingPage.ViewModels
{
    public class ProductAddPageViewModel : PrismCommonViewModelBase, IDialogAware
    {
        private DelegateCommand<string> _closeDialogCommand;
        public DelegateCommand<string> CloseDialogCommand =>
            _closeDialogCommand ?? (_closeDialogCommand = new DelegateCommand<string>(CloseDialog));
        public ReactiveProperty<FurnitureInventory> Product { get; set; }
        public ObservableCollection<FurnitureType> FurnitureType { get; set; }

        private DelegateCommand<string> _SearchCompany;
        public DelegateCommand<string> SearchCompany =>
            _SearchCompany ?? (_SearchCompany = new DelegateCommand<string>(ExecSearchCompany));

        private void ExecSearchCompany(string obj)
        {
            DialogService.ShowDialog("CompanySearchList", null, r =>
            {
                try
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        Company item = r.Parameters.GetValue<Company>("object");
                        if (item != null)
                        {
                            this.Product.Value.Company.Value = item;
                        }
                    }
                }
                catch (Exception) { }

            }, "CommonDialogWindow");
        }
        public ProductAddPageViewModel(IDialogService _DialogService, IContainerProvider con) : base(_DialogService, con)
        {
            Product = new ReactiveProperty<FurnitureInventory>().AddTo(disposable);
            SettingPageViewModel temp = con.Resolve<SettingPageViewModel>("GlobalData");
            this.FurnitureType = temp.FurnitureInfos;
            
        }

        public string Title => throw new NotImplementedException();

        public event Action<IDialogResult> RequestClose;

        protected virtual void CloseDialog(string parameter)
        {

            DialogResult temp = null;
            ButtonResult result = ButtonResult.None;
            if (parameter?.ToLower() == "true")
            {
                if (this.Product.Value == null)
                    return;
                if (this.Product.Value.ValidateAllProperties())
                {
                    con.Resolve<AlertWindow1>().Show();
                    return;
                }
                result = ButtonResult.OK;
                DialogParameters p = new DialogParameters();
                p.Add("object", this.Product.Value);
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
                FurnitureInventory Product = null;
                parameters.TryGetValue("object", out Product);
                if (Product != null)
                {
                    this.Product.Value = Product;
                }
            }
        }


    }
}
