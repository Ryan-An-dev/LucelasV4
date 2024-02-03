using CommonModel;
using CommonModel.Model;
using DataAccess;
using DataAccess.NetWork;
using LogWriter;
using Newtonsoft.Json.Linq;
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
using System.Text;

namespace ContractPage.ViewModels
{
    public class CompletedPaymentPageViewModel : PrismCommonViewModelBase, IDialogAware, INetReceiver
    {
        public ReactiveProperty<Payment> Payment { get; set; }
        public ReactiveProperty<int> PaymentID { get; set; }
        public ReactiveProperty<ReceiptModel> ReceiptModel { get; set; }
        public ReactiveCollection<BankModel> BankList { get; set; } //할당된 은행계좌 리스트
        public ReactiveProperty<string>ContractorName { get; set; }
        public ReactiveCollection<CategoryInfo> CategoryList { get; set; }
        public IEnumerable<Complete> CompleteSelectValues //검색옵션
        {
            get { return Enum.GetValues(typeof(Complete)).Cast<Complete>(); }
        }
        private DelegateCommand<string> _closeDialogCommand;
        public DelegateCommand<string> CloseDialogCommand =>
            _closeDialogCommand ?? (_closeDialogCommand = new DelegateCommand<string>(CloseDialog));

        public IContainerProvider containerProvider { get; set; }

        public CompletedPaymentPageViewModel(IContainerProvider ContainerProvider) : base()
        {
            this.Payment = new ReactiveProperty<Payment>().AddTo(disposable);
            this.PaymentID = new ReactiveProperty<int>().AddTo(disposable);
            this.containerProvider = ContainerProvider;
            this.BankList = new ReactiveCollection<BankModel>().AddTo(disposable);
            this.CategoryList = new ReactiveCollection<CategoryInfo>().AddTo(disposable);
            this.ContractorName = new ReactiveProperty<string>().AddTo(disposable);
            SettingPageViewModel temp = this.containerProvider.Resolve<SettingPageViewModel>();
            this.BankList = temp.AccountInfos;
            this.CategoryList = temp.CategoryInfos;
            this.ReceiptModel = new ReactiveProperty<ReceiptModel>().AddTo(disposable);
        }

        public string Title => throw new NotImplementedException();

        protected virtual void CloseDialog(string parameter)
        {

            DialogResult temp = null;
            ButtonResult result = ButtonResult.None;

            if (parameter?.ToLower() == "true")
            {
                if (this.Payment.Value == null)
                    return;
                result = ButtonResult.OK;
                DialogParameters p = new DialogParameters();
                p.Add("object", this.Payment.Value);
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
                    this.PaymentID.Value = payment.PaymentId.Value;
                    this.Payment.Value = payment;
                }
            }
            if (parameters.ContainsKey("name"))
            {
                string name = "";
                parameters.TryGetValue("name", out name);
                if (name != "") { 
                    this.ContractorName.Value = name;
                }
            }
            SendData();
        }
        private void SendData()
        {
            using (var network = this.containerProvider.Resolve<DataAgent.ContractDataAgent>())
            {
                network.SetReceiver(this);
                JObject jobj = new JObject();
                jobj["payment_id"] = this.PaymentID.Value;
                network.GetConnectedPayment(jobj);
            }
        }
        public void OnRceivedData(ErpPacket packet)
        {
            string msg = Encoding.UTF8.GetString(packet.Body);
            JObject jobj = new JObject(JObject.Parse(msg));
            ErpLogWriter.LogWriter.Trace(jobj.ToString());
            switch ((COMMAND)packet.Header.CMD)
            {
                case COMMAND.GET_CONNECTED_PAYMENT: //데이터 조회
                    SetBankHistory(jobj);
                    break;

            }
        }
        private BankModel FindBankList(string accountNum)
        {
            BankModel model = this.BankList.FirstOrDefault(c => c.AccountNum.Value == accountNum);
            return model;
        }

        public CategoryInfo FindCategory(int id)
        {
            SettingPageViewModel temp = this.containerProvider.Resolve<SettingPageViewModel>();
            CategoryInfo item = temp.CategoryInfos.First(c => c.CategoryId.Value == id);
            return item;
        }
        public PayCardType FindCardType(int id)
        {
            SettingPageViewModel temp = this.containerProvider.Resolve<SettingPageViewModel>();
            PayCardType item = temp.PayCardTypeInfos.First(c => c.Id.Value == id);
            return item;
        }
        private void SetBankHistory(JObject msg)
        {
            if (msg.ToString().Trim() != string.Empty)
            {
                try
                {
                    if (msg["state_history"] == null || msg["state_history"].ToString().Equals(""))
                        return;
                    JObject inner = JObject.Parse(msg["state_history"].ToString());

                    ReceiptModel temp = new ReceiptModel();
                    if (inner["shi_use_name"] != null)
                        temp.Tip.Value = inner["shi_use_name"].ToString();
                    if (inner["shi_biz_type"] != null)
                        temp.CategoryInfo.Value = FindCategory(inner["shi_biz_type"].ToObject<int>());
                    if (inner["shi_memo"] != null)
                        temp.Memo.Value = inner["shi_memo"].ToString();
                    if (inner["shi_cost"] != null)
                        temp.Money.Value = inner["shi_cost"].ToObject<int>();
                    if (inner["shi_use_content"] != null)
                        temp.Contents.Value = inner["shi_use_content"].ToString();
                    if (inner["shi_type"] != null)
                        temp.IncomeCostType.Value = (IncomeCostType)inner["shi_type"].ToObject<int>();

                    if (inner["shi_id"] != null)
                        temp.ReceiptNo.Value = inner["shi_id"].ToObject<int>();
                    if (inner["shi_use_type"] != null)
                        temp.ReceiptType.Value = (ReceiptType)inner["shi_use_type"].ToObject<int>();
                    if (inner["shi_num"] != null)
                        temp.BankInfo.Value = FindBankList(inner["shi_num"].ToString());
                    //temp.CategoryInfo.Value = findCategoryItem(msg["shi_biz_type"].ToObject<int>()); //어떤 카테고리인지 찾는 로직 id로
                    if (inner["shi_time"] != null)
                        temp.Month.Value = inner["shi_time"].ToObject<DateTime>();
                    if (inner["shi_complete"] != null)
                        temp.FullyCompleted.Value = (AllocateType)inner["shi_complete"].ToObject<int>();
                    if (inner["shi_key"] != null)
                        temp.indexKey.Value = inner["shi_key"].ToString();
                    if (inner["shi_card_id"] != null)
                    {
                        if (inner["shi_card_id"].ToObject<int>() != 0)
                        {
                            temp.PayCardType.Value = FindCardType(inner["shi_card_id"].ToObject<int>());
                        }
                        else
                        {
                            PayCardType payCardType = new PayCardType();
                            payCardType.Id.Value = 0;
                            payCardType.Name.Value = "";
                            temp.PayCardType.Value = payCardType;
                        }
                    }
                    this.ReceiptModel.Value = temp;
                }
                catch (Exception ex)
                {

                }

            }
        }
        public void OnConnected()
        {
            
        }

        public void OnSent()
        {
            
        }
    }
}
