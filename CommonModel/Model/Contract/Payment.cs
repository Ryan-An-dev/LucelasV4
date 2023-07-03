using PrsimCommonBase;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModel.Model
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum PaymentType {
        [Description("계약금")]
        ContractFee = 1,
        [Description("잔금")]
        ContractBalance = 2,
    }

    public class Payment : PrismCommonModelBase
    {
        public ReactiveProperty<ReceiptType> PaymentMethod { get; set; } //계좌, 카드 , 계좌이체 , 현금
        public ReactiveProperty<PaymentType> PaymentType  { get; set; } // 계약금, 잔금
        public ReactiveProperty<bool> PaymentCompleted { get; set; }
        public ReactiveProperty<int> Price { get; set; }

        public Payment() : base()
        {
            this.PaymentType = new ReactiveProperty<PaymentType>().AddTo(disposable);
            this.PaymentMethod = new ReactiveProperty<ReceiptType>().AddTo(disposable);
            this.PaymentCompleted = new ReactiveProperty<bool>().AddTo(disposable);
            this.Price= new ReactiveProperty<int>().AddTo(disposable);
        }
    }
}
