using Newtonsoft.Json.Linq;
using Prism;
using System.Text.Json.Serialization;
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
        [JsonPropertyName("con_id")]
        public ReactiveProperty<int> Id { get; set; }
        public ReactiveProperty<int> ListNo { get; set; }
        [JsonPropertyName("create_time")]
        public ReactiveProperty<DateTime> Month { get; set; } //계약 생성 날짜
        [JsonPropertyName("delivery_date")]
        public ReactiveProperty<DateTime> Delivery { get; set; } //배송일자
        [JsonPropertyName("contractor")]
        public ReactiveProperty<Customer> Contractor { get; set; } //주문자

        [JsonPropertyName("total")]
        public ReactiveProperty<int> Price { get; set; } //토탈 가격

        public ReactiveProperty<bool> DepositComplete { get; set; } 
        public ReactiveProperty<FullyCompleted> PaymentComplete { get; set; }
        [JsonPropertyName("payment")]
        public ReactiveCollection<Payment> Payment { get; set; } //지불 클래스
        [JsonPropertyName("product")]
        public ReactiveCollection<FurnitureInventory> Product { get; set; } //주문 상품 클래스
        [JsonPropertyName("memo")]
        public ReactiveProperty<string> Memo { get; set; } //메모
        [JsonPropertyName("saler_id")]
        public ReactiveProperty<int> SalerId { get; set; } //판매자

        
        public Contract()
        {
            this.Memo = new ReactiveProperty<string>().AddTo(disposable);
            this.Id = new ReactiveProperty<int>().AddTo(disposable);
            this.ListNo = new ReactiveProperty<int>().AddTo(disposable);
            this.Month = new ReactiveProperty<DateTime>(DateTime.Now).AddTo(disposable);
            this.Contractor = new ReactiveProperty<Customer>().AddTo(disposable);
            this.Delivery = new ReactiveProperty<DateTime>(DateTime.Now).AddTo(disposable);
            this.SalerId = new ReactiveProperty<int>().AddTo(disposable);
            this.Price = new ReactiveProperty<int>(0).AddTo(disposable);
            this.DepositComplete= new ReactiveProperty<bool>().AddTo(disposable);
            this.PaymentComplete = new ReactiveProperty<FullyCompleted>(FullyCompleted.NotYet).AddTo(disposable);
            this.Payment = new ReactiveCollection<Payment>().AddTo(disposable);
            this.Product = new ReactiveCollection<FurnitureInventory>().AddTo(disposable);
            this.Contractor.Value = new Customer();
            SetObserver();
        }
        public void CompleteChangedData()
        {
            ChangedItem.RemoveAll();
            isChanged = false;

        }
        public override void SetObserver() {
            this.Month.Subscribe(x => ChangedJson("create_time", x));
            this.Price.Subscribe(x => ChangedJson("total", x));
            this.SalerId.Subscribe(x => ChangedJson("saler_id", x));
            this.Memo.Subscribe(x => ChangedJson("memo", x));
            this.Delivery.Subscribe(x => ChangedJson("delivery_date", x));
            this.Contractor.Value.SetObserver();
            
        }


        /// <summary>
        /// 고객정보는 따로 Update한다.
        /// 회사, 제품 역시 따로 Update한다.
        /// </summary>
        /// <returns>고객,회사,제품 제외한 변경된 Json Return</returns>
        public JObject GetChangedItem()
        {
            JArray jarrPayment = new JArray();
            foreach (Payment item in Payment) {
                if (item.isChanged) {
                    jarrPayment.Add(item.GetChangedItem());
                }
            }
            if(jarrPayment.Count>0)
                ChangedItem["payment"] = jarrPayment;

            JArray jarrProduct = null;
            foreach (FurnitureInventory item in Product)
            {
                if (item.isChanged)
                {
                    jarrProduct = new JArray();
                    jarrProduct.Add(item.GetChangedItem());
                }
            }
            if (jarrProduct != null)
                ChangedItem["product"] = jarrProduct;

            return ChangedItem;
        }
    }
}
