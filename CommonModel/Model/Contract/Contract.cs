using Newtonsoft.Json.Linq;
using Prism;
using PrsimCommonBase;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModel.Model
{
    public class Contract : PrismCommonModelBase
    {
        public ReactiveProperty<int> Id { get; set; }
        public ReactiveProperty<int> ListNo { get; set; }
        public ReactiveProperty<DateTime> Month { get; set; } //계약 생성 날짜
        public ReactiveProperty<DateTime> Delivery { get; set; } //배송일자
        public ReactiveProperty<Customer> Contractor { get; set; } //주문자
        public ReactiveProperty<int> Price { get; set; } //토탈 가격
        public ReactiveProperty<bool> DepositComplete { get; set; } 
        public ReactiveProperty<FullyCompleted> PaymentComplete { get; set; }
        public ReactiveCollection<Payment> Payment { get; set; } //지불 클래스
        public ReactiveCollection<Company> Product { get; set; } //상품 클래스
        public ReactiveProperty<string> Memo { get; set; } //메모
        public ReactiveProperty<int> SalerId { get; set; } //판매자
        public JObject ChangedItem { get; set; }
        public ReactiveProperty<bool> IsChanged { get; set; }
        public Contract()
        {
            this.IsChanged = new ReactiveProperty<bool>(false).AddTo(disposable);
            this.Memo = new ReactiveProperty<string>().AddTo(disposable);
            this.Id = new ReactiveProperty<int>().AddTo(disposable);
            this.ListNo = new ReactiveProperty<int>().AddTo(disposable);
            this.Month = new ReactiveProperty<DateTime>().AddTo(disposable);
            this.Contractor = new ReactiveProperty<Customer>().AddTo(disposable);
            this.Delivery = new ReactiveProperty<DateTime>().AddTo(disposable);
            this.SalerId = new ReactiveProperty<int>().AddTo(disposable);
            this.Price = new ReactiveProperty<int>().AddTo(disposable);
            this.DepositComplete= new ReactiveProperty<bool>().AddTo(disposable);
            this.PaymentComplete = new ReactiveProperty<FullyCompleted>(FullyCompleted.NotYet).AddTo(disposable);
            this.Payment = new ReactiveCollection<Payment>().AddTo(disposable);
            this.Product = new ReactiveCollection<Company>().AddTo(disposable);
            this.ChangedItem = new JObject();
        }
        private void SetObserver() {
            this.Price.Subscribe(x => ChangedJson("total", x));
            this.SalerId.Subscribe(x => ChangedJson("saler_id", x));
            this.Memo.Subscribe(x => ChangedJson("memo", x));
            this.Delivery.Subscribe(x => ChangedJson("delivery_date", x));
            
        }
        private void ChangedJson(string name, object value)
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
                else if (value is JToken) {
                    ChangedItem[name] = value.ToString();
                }
                this.IsChanged.Value = true;
            }
        }
    }
}
