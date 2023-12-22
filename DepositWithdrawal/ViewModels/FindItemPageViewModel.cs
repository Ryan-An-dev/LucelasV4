using CommonModel;
using CommonModel.Model;
using DataAccess;
using DataAccess.NetWork;
using Newtonsoft.Json.Linq;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SettingPage.ViewModels;
using SettingPage.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DepositWithdrawal.ViewModels
{
    public class FindItemPageViewModel : PrsimListViewModelBase, INetReceiver,IDialogAware
    {
        public ReactiveCollection<PayCardType> PaymentCardList { get; set; }
        public ReactiveCollection<Contract> ContractItems { get; }
        public ReactiveProperty<Payment> SelectedPayment { get; set; }
        public BankModel SelectedBank { get; set; }
        public ReactiveProperty<ReceiptModel> args { get; set; }
        public IContainerProvider ContainerProvider { get; }

        private DelegateCommand<string> _closeDialogCommand;
        public DelegateCommand<string> CloseDialogCommand =>
            _closeDialogCommand ?? (_closeDialogCommand = new DelegateCommand<string>(CloseDialog));

        public FindItemPageViewModel(IContainerProvider containerprovider, IRegionManager regionManager, IDialogService dialogService) : base(regionManager, containerprovider, dialogService)
        {
            this.ContainerProvider= con;
            this.args= new ReactiveProperty<ReceiptModel>().AddTo(disposable);
            this.ContractItems= new ReactiveCollection<Contract>().AddTo(disposable);
            this.SelectedBank = new BankModel();
            PaymentCardList = new ReactiveCollection<PayCardType>().AddTo(disposable);
            SettingPageViewModel temp = ContainerProvider.Resolve<SettingPageViewModel>("GlobalData");
            this.PaymentCardList = temp.PayCardTypeInfos;
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
            parameters.TryGetValue("object", out item);

            //계약 찾기
            using (var network = this.ContainerProvider.Resolve<DataAgent.ContractDataAgent>()) {
                JObject jobj = new JObject();
                jobj["page_unit"] = 30;
                jobj["page_start_pos"] = 0;
                JObject jobj2 = new JObject();
                jobj2["payment_method"] = (int)item.ReceiptType.Value;
                jobj2["start_time"] = item.Month.Value.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss");
                jobj2["end_time"] = item.Month.Value.ToString("yyyy-MM-dd HH:mm:ss");
                //시간쪽 수정해야됨 starttime이랑 endtime 어떻게 정하기로 했는지 까먹음
                jobj2["complete"] = 2;

                if (item.ReceiptType.Value == ReceiptType.Cash) // 현금
                {
                    jobj2["payment_method"] = (int)item.ReceiptType.Value;
                    jobj2["cui_name"] = item.Contents.Value;
                }
                else if (item.ReceiptType.Value == ReceiptType.Cash) { //카드
                    jobj2["payment_method"] = (int)item.ReceiptType.Value;
                }
                else {  //계좌이체
                    jobj2["payment_method"] = (int)item.ReceiptType.Value;
                    jobj2["cui_name"] = item.Contents.Value;
                }


                jobj["search_option"] = jobj2;


                network.SetReceiver(this);
                network.GetContractList(jobj);
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
                            //Company item = MakeProductClass(CompanyId,ProductId);
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


        public Customer FindCustomer(int id)
        {
            SettingPageViewModel temp = this.ContainerProvider.Resolve<SettingPageViewModel>("GlobalData");
            Customer item = temp.CustomerInfos.First(c => c.Id.Value == id);
            return item;
        }
        public void OnSent()
        {
            
        }

        public override void UpdatePageItem(CommonModel.MovePageType param, int count)
        {
            
        }

        public override void AddButtonClick()
        {
            
        }

        public override void DeleteButtonClick(PrismCommonModelBase selecteditem)
        {
            
        }

        public override void RowDoubleClickEvent()
        {
            
        }

        public override void SearchTitle(string value)
        {
            
        }
    }
}
