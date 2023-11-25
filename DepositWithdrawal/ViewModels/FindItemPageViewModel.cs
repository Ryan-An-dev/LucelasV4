using CommonModel;
using CommonModel.Model;
using DataAccess;
using DataAccess.NetWork;
using Newtonsoft.Json.Linq;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SettingPage.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DepositWithdrawal.ViewModels
{
    public class FindItemPageViewModel : PrismCommonViewModelBase, INetReceiver,IDialogAware
    {
        public ReactiveCollection<Contract> ContractItems { get; }
        public ReactiveProperty<Payment> SelectedPayment { get; set; }
        public BankModel SelectedBank { get; set; }
        public ReactiveProperty<ReceiptModel> args { get; set; }
        public IContainerProvider ContainerProvider { get; }

        private DelegateCommand<string> _closeDialogCommand;
        public DelegateCommand<string> CloseDialogCommand =>
            _closeDialogCommand ?? (_closeDialogCommand = new DelegateCommand<string>(CloseDialog));

        public FindItemPageViewModel(IContainerProvider con):base()
        {
            this.ContainerProvider= con;
            this.args= new ReactiveProperty<ReceiptModel>().AddTo(disposable);
            this.ContractItems= new ReactiveCollection<Contract>().AddTo(disposable);
            this.SelectedBank = new BankModel();
        }

        protected virtual void CloseDialog(string parameter)
        {

            DialogResult temp = null;
            ButtonResult result = ButtonResult.None;
            
            if (parameter?.ToLower() == "true")
            {
                if (this.SelectedPayment == null)
                    return;
                result = ButtonResult.OK;
                temp = new DialogResult(result);
                DialogParameters p = new DialogParameters();
                p.Add("SelectedPaymentItem", this.SelectedPayment.Value);
                temp.Parameters.Add("SelectedPaymentItem",p);
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

        public string Title => "";

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnConnected()
        {
            throw new NotImplementedException();
        }

        public void OnDialogClosed()
        {
            
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            ReceiptModel item = null;
            parameters.TryGetValue("AccountName", out item);
            this.args.Value = item;

            //요청보내고 (카드 or 현금에 대한 것)
            using (var network = this.ContainerProvider.Resolve<DataAgent.BankListDataAgent>()) {
                JObject jobj = new JObject();
                if (this.args.Value.ReceiptType.Value == ReceiptType.Cash)
                {
                    jobj["shi_key"] = "Cash";
                }
                else
                {
                    jobj["shi_key"] = this.args.Value.BankInfo.Value.AccountNum.Value;
                }
                jobj["shi_time"] = this.args.Value.Month.Value.ToString("yyyy-MM-dd HH:mm:ss");
                jobj["shi_id"] = this.args.Value.ReceiptNo.Value;
                jobj["shi_remain_price"] = this.args.Value.RemainPrice.Value;
                jobj["shi_use_content"] = this.args.Value.Contents.Value;
                network.SetReceiver(this);
                network.GetConnectedContract(jobj);
            }
        }

        public void OnRceivedData(ErpPacket packet)
        {
            string msg = Encoding.UTF8.GetString(packet.Body);
            JObject jobj = new JObject(JObject.Parse(msg));
            //ErpLogWriter.LogWriter.Trace(jobj.ToString());
            switch ((COMMAND)packet.Header.CMD)
            {
                case COMMAND.GetConnectedContract: //검색결과 받는곳 
                    SetSpareConnectedContractList(jobj);
                    break;
                case COMMAND.DeleteBankHistory: //데이터 삭제완료
                    
                    break;
            }
            
        }

        private void SetSpareConnectedContractList(JObject msg)
        {
            if (msg["contract_history"] != null)
            {
                foreach (JObject inner in msg["contract_history"] as JArray)
                {
                    int id = 0;
                    Customer Contractor = null;
                    
                    Contract temp = new Contract();
                   
                    if (inner["contractor"] != null)
                    {
                        if (inner["contractor"]["cui_id"] != null)
                        {
                            id = inner["cui_id"].ToObject<int>();
                            Contractor = FindCustomer(id);
                        }
                    }
                    if (inner["product"] != null) {
                        foreach (JObject Company in inner["product"] as JArray) {
                            int CompanyId = 0;
                            if (Company["aci_id"] != null)
                                CompanyId = Company["aci_id"].ToObject<int>();
                            int ProductId = 0;
                            if (Company["acpi_id"] != null)
                                ProductId = Company["acpi_id"].ToObject<int>();
                            Company item = MakeProductClass(CompanyId,ProductId);
                            //temp.Product.Add(item);
                        }
                    }


                    if (Contractor != null)
                        temp.Contractor.Value = Contractor;
                    if (inner["delivery_date"] != null)
                        temp.Delivery.Value = inner["delivery_date"].ToObject<DateTime>();
                    if (inner["total"] != null)
                        temp.Price.Value = inner["total"].ToObject<int>();
                    if (inner["con_id"] != null)
                        temp.Id.Value = inner["con_id"].ToObject<int>();
                    if (inner["create_time"] != null)
                        temp.Month.Value = inner["create_time"].ToObject<DateTime>();
                    if (inner["memo"] != null)
                        temp.Memo.Value = inner["memo"].ToString();
                    //if (inner["saler_id"] != null)
                    //    temp.Seller.Value = inner["saler_id"].ToObject<int>();
                    this.ContractItems.Add(temp);
                }
            }
        }


        private Company MakeProductClass(int companyId, int productId)
        {
            SettingPageViewModel temp = this.ContainerProvider.Resolve<SettingPageViewModel>("GlobalData");
            CompanyList item = temp.CompanyInfos.First(c => c.Id.Value == companyId);
            Product product = item.ProductList.First(c => c.ProductId.Value == productId);
            Company company = new Company(item.Id.Value,item.CompanyName.Value);
            //company.Product.Value = product.Clone();
            return company;
        }

        public Customer FindCustomer(int id)
        {
            SettingPageViewModel temp = this.ContainerProvider.Resolve<SettingPageViewModel>("GlobalData");
            Customer item = temp.CustomerInfos.First(c => c.Id.Value == id);
            return item;
        }
        public void OnSent()
        {
            throw new NotImplementedException();
        }

    }
}
