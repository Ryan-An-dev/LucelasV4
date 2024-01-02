using Newtonsoft.Json.Linq;
using Prism;
using System.Text.Json.Serialization;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;

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
        public ReactiveProperty<System.DateTime> Delivery { get; set; } //배송일자
        [JsonPropertyName("contractor")]
        public ReactiveProperty<Customer> Contractor { get; set; } //주문자

        [JsonPropertyName("total")]
        public ReactiveProperty<int> Price { get; set; } //토탈 가격

        public ReactiveProperty<bool> DepositComplete { get; set; } 
        public ReactiveProperty<FullyCompleted> PaymentComplete { get; set; }
        [JsonPropertyName("payment")]
        public ReactiveCollection<Payment> Payment { get; set; } //지불 클래스
        [JsonPropertyName("product")]
        public ReactiveCollection<ContractedProduct> Product { get; set; } //주문 상품 클래스
        [JsonPropertyName("memo")]
        public ReactiveProperty<string> Memo { get; set; } //메모
        [JsonPropertyName("saler_id")]
        public ReactiveProperty<Employee> Seller { get; set; } //판매자

        public ReactiveProperty<string> ProductNameCombine { get; set; }

        public ReactiveProperty<string> ProductMemoCombine { get; set; }

        public Contract()
        {
            this.Memo = new ReactiveProperty<string>().AddTo(disposable);
            this.Id = new ReactiveProperty<int>().AddTo(disposable);
            this.ListNo = new ReactiveProperty<int>().AddTo(disposable);
            this.Month = CreateDateTimeProperty("delivery_date");
            this.Contractor = new ReactiveProperty<Customer>().AddTo(disposable);
            this.Delivery = CreateDateTimeProperty("delivery_date");
            this.Seller = new ReactiveProperty<Employee>(new Employee()).AddTo(disposable);
            this.Price = new ReactiveProperty<int>(0).AddTo(disposable);
            this.DepositComplete= new ReactiveProperty<bool>().AddTo(disposable);
            this.PaymentComplete = new ReactiveProperty<FullyCompleted>(FullyCompleted.NotYet).AddTo(disposable);
            this.Payment = new ReactiveCollection<Payment>().AddTo(disposable);
            this.Product = new ReactiveCollection<ContractedProduct>().AddTo(disposable);
            ProductNameCombine = new ReactiveProperty<string>().AddTo(disposable);
            ProductMemoCombine = new ReactiveProperty<string>().AddTo(disposable);
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
            this.Seller.Subscribe(x => ChangedJson("seller_id", x.Id.Value));
            this.Memo.Subscribe(x => ChangedJson("memo", x));
            this.Delivery.Subscribe(x => ChangedDelevery("delivery_date", x));
            this.Contractor.Subscribe(x => ChangedJson("cui_id", x.Id.Value));
            this.Payment.ToObservable().Subscribe(updatedItems => { isChanged = true; });
        }
        public void ChangedDelevery(string name, object value)
        {
            if (value != null)
            {
                if (value is DateTime)
                {
                    DateTime time = (DateTime)value;
                    ChangedItem[name] = time.ToString("yyyy-MM-dd-HH-mm");
                }
                isChanged = true;
            }
        }
        public void TotalPrice() {
            int temper = 0; 
            foreach (ContractedProduct item in this.Product) {
                temper+=item.total.Value;
            }
            this.Price.Value = temper;
        }

        public void ClearJson() { 
            this.ChangedItem.RemoveAll();
            foreach (Payment item in Payment) { 
                item.ClearJson();
                item.isChanged = false;
            }
            foreach (ContractedProduct item in Product) {
                item.ClearJson();
                item.isChanged = false;
            }
        }
        /// <summary>
        /// 현재 정보 모두 파싱해서 내리는 곳
        /// 
        /// </summary>
        /// <returns></returns>
        public JObject MakeJson()
        {
            JObject NewObject = new JObject();

            NewObject["create_time"] = this.Month.Value.ToString("yyyy-MM-dd");
            NewObject["total"] = this.Price.Value;
            NewObject["seller_id"] = this.Seller.Value.Id.Value;
            NewObject["memo"] = this.Memo.Value;
            NewObject["delivery_date"] = this.Delivery.Value.ToString("yyyy-MM-dd-HH-mm");

            //고객
            JObject contractor = new JObject();
            contractor["cui_id"] = this.Contractor.Value.Id.Value;
            contractor["cui_name"] = this.Contractor.Value.Name.Value;
            contractor["cui_phone"] = this.Contractor.Value.Phone.Value;
            contractor["cui_address"] = this.Contractor.Value.Address.Value;
            contractor["cui_address_detail"] = this.Contractor.Value.Address1.Value;
            contractor["cui_memo"] = this.Contractor.Value.Memo.Value;
            NewObject["contractor"] = contractor;


            //페이먼트
            JArray jarrPayment = new JArray();
            foreach (Payment item in Payment)
            {
                jarrPayment.Add(item.MakeJson());
            }
            if (jarrPayment.Count > 0)
                NewObject["payment"] = jarrPayment;


            //제품
            JArray jarrProduct = new JArray();
            foreach (ContractedProduct item in Product)
            {
                jarrProduct.Add(item.MakeJson());
            }
            if (jarrProduct != null)
                NewObject["product"] = jarrProduct;

            return NewObject;
        }
        /// <summary>
        /// 고객정보는 따로 Update한다.
        /// 회사, 제품 역시 따로 Update한다.
        /// </summary>
        /// <returns>고객,회사,제품 제외한 변경된 Json Return</returns>
        public JObject GetChangedItem()
        {
            //계약자 자체가 바뀐경우 
            //이때는 바뀐 id 를 줘야한다.

            //기존계약자의 내용만 수정한경우 
            //--이거는 따로 처리되어서 상관없다.

            JArray jarrPayment = new JArray();
            foreach (Payment item in Payment) {
                JObject jobj = new JObject();
                if (item.isChanged && item.PaymentId.Value == 0) //신규 생성
                {
                    jobj["payment_id"] = item.PaymentId.Value;
                    jobj["changed_item"] = item.MakeJson();
                }
                if (item.isChanged && item.PaymentId.Value != 0) //기존 수정
                {
                    jobj["payment_id"] = item.PaymentId.Value;
                    jobj["changed_item"] = item.ChangedItem;
                }
                if (ChangedItem["payment"] == null) {
                    jarrPayment.Add(jobj);
                    ChangedItem["payment"] = jarrPayment;
                }
                else{
                    (ChangedItem["payment"] as JArray).Add(jobj);
                }
                
            }

            if (jarrPayment.Count > 0) {
                if (ChangedItem["payment"] == null)
                {
                    ChangedItem["payment"] = jarrPayment;
                }
                else{
                    
                }
            }
                

            JArray jarrProduct = new JArray();
            foreach (ContractedProduct item in Product)
            {
                if (item.isChanged)
                {
                    jarrProduct.Add(item.MakeJson());
                }
            }
            if (jarrProduct != null)
                ChangedItem["product"] = jarrProduct;
            

            return ChangedItem;
        }
    }
}
