using Newtonsoft.Json.Linq;
using PrsimCommonBase;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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

        [JsonPropertyName("payment_method")]
        public ReactiveProperty<ReceiptType> PaymentMethod { get; set; } //계좌, 카드 , 계좌이체 , 현금
        [JsonPropertyName("payment_type")]
        public ReactiveProperty<PaymentType> PaymentType  { get; set; } // 계약금, 잔금

        [JsonPropertyName("payment_completed")]
        public ReactiveProperty<Complete> PaymentCompleted { get; set; }
        [JsonPropertyName("price")]
        public ReactiveProperty<int> Price { get; set; }

        public Payment() : base()
        {
            this.PaymentType = new ReactiveProperty<PaymentType>().AddTo(disposable);
            this.PaymentMethod = new ReactiveProperty<ReceiptType>().AddTo(disposable);
            this.PaymentCompleted = new ReactiveProperty<Complete>().AddTo(disposable);
            this.Price= new ReactiveProperty<int>(0).AddTo(disposable);
            SetObserver();
        }

        public override void SetObserver()
        {
            this.PaymentType.Subscribe(x => ChangedJson("payment_type", x));
            this.PaymentMethod.Subscribe(x => ChangedJson("payment_method", x));
            this.PaymentCompleted.Subscribe(x => ChangedJson("payment_completed", x));
            this.Price.Subscribe(x => ChangedJson("price", x));
        }
    }
}
