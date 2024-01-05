using Newtonsoft.Json.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CommonModel.Model
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum PaymentType {
        [Description("계약금")]
        ContractFee = 0,
        [Description("잔금")]
        ContractBalance = 1,
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum Complete
    {
        [Description("완료")]
        Complete = 1,
        [Description("미완료")]
        InComplete= 0,
    }

    public class Payment : PrismCommonModelBase
    {
        public ReactiveProperty<bool> IsSelected { get; set; }
        public ReactiveProperty<AddDelete> Action { get; set; }
        public IEnumerable<PaymentType> PaymentTypeSelectValues //검색옵션
        {
            get { return Enum.GetValues(typeof(PaymentType)).Cast<PaymentType>(); }
        }
        public IEnumerable<ReceiptType> PaymentMethodSelectValues //검색옵션
        {
            get { return Enum.GetValues(typeof(ReceiptType)).Cast<ReceiptType>().Skip(1); }
        }
        public IEnumerable<Complete> CompleteSelectValues //검색옵션
        {
            get { return Enum.GetValues(typeof(Complete)).Cast<Complete>(); }
        }
        public ReactiveProperty<int> PaymentId { get; set; }
        [JsonPropertyName("payment_method")]
        public ReactiveProperty<ReceiptType> PaymentMethod { get; set; } //계좌, 카드 , 계좌이체 , 현금
        [JsonPropertyName("payment_type")]
        public ReactiveProperty<PaymentType> PaymentType  { get; set; } // 계약금, 잔금

        [JsonPropertyName("payment_completed")]
        public ReactiveProperty<Complete> PaymentCompleted { get; set; }

        public ReactiveProperty<PayCardType>SelectedPayCard { get; set; }
        [JsonPropertyName("price")]
        public ReactiveProperty<int> Price { get; set; }

        public ReactiveProperty<Visibility> cardVisibility { get; set; }

        public Payment() : base()
        {
            this.IsSelected = new ReactiveProperty<bool>(false).AddTo(disposable);
            this.Action = new ReactiveProperty<AddDelete>(AddDelete.Add).AddTo(disposable);
            this.PaymentId = new ReactiveProperty<int>().AddTo(disposable);
            this.PaymentType = new ReactiveProperty<PaymentType>().AddTo(disposable);
            this.PaymentMethod = new ReactiveProperty<ReceiptType>(0).AddTo(disposable);
            this.PaymentCompleted = new ReactiveProperty<Complete>().AddTo(disposable);
            this.SelectedPayCard = new ReactiveProperty<PayCardType>(new PayCardType()).AddTo(disposable);
            this.Price= new ReactiveProperty<int>(0).AddTo(disposable);
            this.cardVisibility = new ReactiveProperty<Visibility>(System.Windows.Visibility.Collapsed).AddTo(disposable);
            SetObserver();
        }
        private void multiObserver(string name , ReceiptType x) {
            ChangedJsonADD(name, x);
            if (x == (ReceiptType)2)
            {
                cardVisibility.Value = System.Windows.Visibility.Visible;
            }
            else {
                cardVisibility.Value = System.Windows.Visibility.Collapsed;
            }
        }

        public void ChangedJsonADD(string name, object value)
        {
            if (value != null)
            {
                if (value is int)
                {
                    ChangedItem[name] = (int)value;
                }
                else if (value is string)
                {
                    ChangedItem[name] = value.ToString();
                }
                else if (value is JToken)
                {
                    ChangedItem[name] = value.ToString();
                }
                else if (value is Enum)
                {
                    ChangedItem[name] = (int)value;
                }
                else if (value is DateTime)
                {
                    DateTime time = (DateTime)value;
                    ChangedItem[name] = time.ToString("yyyy-MM-dd");
                }
                if(this.PaymentId.Value != 0)
                    this.Action.Value = AddDelete.Update;
                isChanged = true;
            }
        }

        public JObject MakeJson() {
            JObject jobj = new JObject();
            jobj["action"] = (int)this.Action.Value;
            jobj["payment_type"] = (int)this.PaymentType.Value;
            jobj["payment_completed"] = (int)this.PaymentCompleted.Value;
            jobj["payment_method"] = (int)this.PaymentMethod.Value;
            jobj["price"] = (int)this.Price.Value;
            jobj["payment_card"] = (int)this.SelectedPayCard.Value.Id.Value;
            return jobj;
        }

        public override void SetObserver()
        {
            this.PaymentType.Subscribe(x => ChangedJsonADD("payment_type", x));
            this.PaymentMethod.Subscribe(x => multiObserver("payment_method", x));
            this.PaymentCompleted.Subscribe(x => ChangedJsonADD("payment_completed", x));
            this.Price.Subscribe(x => ChangedJsonADD("price", x));
            this.Action.Subscribe(x => ChangedJson("action", x));
            this.SelectedPayCard.Subscribe(x => ChangedJsonADD("payment_card", x.Id.Value));
            this.IsSelected.Subscribe(x=>OnMyEvent(x,this.Price.Value));
        }

        public delegate void PayCheckEvent(bool isChecked,int price);
        public event PayCheckEvent MyPayCheckEvent;
        public virtual void OnMyEvent(bool message,int price) { 
            MyPayCheckEvent?.Invoke(message,price);
        }
    }
}
