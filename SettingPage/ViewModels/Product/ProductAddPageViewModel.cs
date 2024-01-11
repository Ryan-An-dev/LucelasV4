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
using DataAccess.NetWork;
using DataAccess;

namespace SettingPage.ViewModels
{
    public class ProductAddPageViewModel : PrismCommonViewModelBase, IDialogAware, INetReceiver
    {
        private DelegateCommand<string> _closeDialogCommand;
        public DelegateCommand<string> CloseDialogCommand =>
            _closeDialogCommand ?? (_closeDialogCommand = new DelegateCommand<string>(CloseDialog));
        public ReactiveProperty<Product> Product { get; set; }
        public ObservableCollection<FurnitureType> FurnitureType { get; set; }

        private DelegateCommand<string> _SearchCompany;
        public DelegateCommand<string> SearchCompany =>
            _SearchCompany ?? (_SearchCompany = new DelegateCommand<string>(ExecSearchCompany));

        private DelegateCommand _CreateCompanyDialogCommand;
        public DelegateCommand CreateCompanyDialogCommand =>
        _CreateCompanyDialogCommand ?? (_CreateCompanyDialogCommand = new DelegateCommand(ExecCreateCompany));

        private void ExecCreateCompany()
        {
            DialogService.ShowDialog("CompanyAddPage", null, r =>
            {
                try
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        Company item = r.Parameters.GetValue<Company>("object");
                        if (item != null)
                        {
                            using (var network = con.Resolve<DataAgent.CompanyDataAgent>())
                            {
                                network.SetReceiver(this);
                                JObject jobj = new JObject();
                                jobj["company_id"] = (int)0;
                                jobj["company_name"] = item.CompanyName.Value;
                                jobj["company_address_detail"] = item.CompanyAddressDetail.Value;
                                jobj["company_phone"] = item.CompanyPhone.Value;
                                jobj["company_address"] = item.CompanyAddress.Value;
                                network.Create(jobj);
                            }
                        }
                    }
                }
                catch (Exception) { }

            }, "CommonDialogWindow");
        }

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
            Product = new ReactiveProperty<Product>().AddTo(disposable);
            SettingPageViewModel temp = con.Resolve<SettingPageViewModel>();
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

                bool Check = false;
                this.Product.Value.Company.ForceValidate();
                Check |= this.Product.Value.Company.HasErrors;
                Check |= this.Product.Value.ValidateAllProperties();
                if (Check)
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
                Product Product = null;
                parameters.TryGetValue("object", out Product);
                if (Product != null)
                {
                    this.Product.Value = Product;
                }
            }
        }

        public void OnRceivedData(ErpPacket packet)
        {
         
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
