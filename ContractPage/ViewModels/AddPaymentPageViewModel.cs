using CommonModel;
using CommonModel.Model;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SettingPage.ViewModels;
using SettingPage.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ContractPage.ViewModels
{
    public class AddPaymentPageViewModel : PrismCommonViewModelBase, IDialogAware
    {
        public ReactiveCollection<PayCardType> PaymentCardList { get; set; }

        public ReactiveProperty<Payment> SelectedItem { get; set; }

        public IEnumerable<PaymentType> PaymentTypeSelectValues //검색옵션
        {
            get { return Enum.GetValues(typeof(PaymentType)).Cast<PaymentType>(); }
        }
        public IEnumerable<ReceiptType> PaymentMethodSelectValues //검색옵션
        {
            get { return Enum.GetValues(typeof(ReceiptType)).Cast<ReceiptType>(); }
        }
        public IEnumerable<Complete> CompleteSelectValues //검색옵션
        {
            get { return Enum.GetValues(typeof(Complete)).Cast<Complete>(); }
        }
        private DelegateCommand<string> _closeDialogCommand;
        public DelegateCommand<string> CloseDialogCommand =>
            _closeDialogCommand ?? (_closeDialogCommand = new DelegateCommand<string>(CloseDialog));

        public IContainerProvider containerProvider { get; set; }

        public AddPaymentPageViewModel(IContainerProvider ContainerProvider) :base()
        {
            PaymentCardList = new ReactiveCollection<PayCardType>().AddTo(disposable);
            SelectedItem = new ReactiveProperty<Payment>().AddTo(disposable);
            SettingPageViewModel temp = ContainerProvider.Resolve<SettingPageViewModel>("GlobalData");
            this.PaymentCardList = temp.PayCardTypeInfos;
        }

        public string Title => throw new NotImplementedException();

        protected virtual void CloseDialog(string parameter)
        {

            DialogResult temp = null;
            ButtonResult result = ButtonResult.None;

            if (parameter?.ToLower() == "true")
            {
                if (this.SelectedItem.Value == null)
                    return;
                result = ButtonResult.OK;
                DialogParameters p = new DialogParameters();
                p.Add("object", this.SelectedItem.Value);
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
            if (parameters.ContainsKey("object"))
            {
                Payment payment = null;
                parameters.TryGetValue("object", out payment);
                if (payment != null)
                {
                    this.SelectedItem.Value = payment;
                }
            }
        }
    }
}
