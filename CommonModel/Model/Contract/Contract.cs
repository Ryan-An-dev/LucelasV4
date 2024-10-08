﻿using Newtonsoft.Json.Linq;
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
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.ComponentModel;

namespace CommonModel.Model
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum DeliveryComplete {
        [Description("배달예정")]
        NotYet = 0,
        [Description("배달완료")]
        Completed = 1
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum DeliveryFinal
    {

        [Description("모두")]
        All = 0,
        [Description("배송일자 확정")]
        Checked = 1,
        [Description("배송일자 미확정")]
        UnChecked = 2
    }

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
        public ReactiveCollection<ContractedProduct> Product { get; set; } //주문 상품 클래스
        [JsonPropertyName("memo")]
        public ReactiveProperty<string> Memo { get; set; } //메모
        [JsonPropertyName("saler_id")]
        public ReactiveProperty<Employee> Seller { get; set; } //판매자

        public ReactiveProperty<string> ProductNameCombine { get; set; }

        public ReactiveProperty<string> ProductMemoCombine { get; set; }

        public ReactiveProperty<string> DeliveryManCombine { get; set; }
        public ReactiveCollection<Employee> DeliveryMan { get; set; }
        public ReactiveProperty<DeliveryComplete> DeliveryComplete { get; set; }
        public ReactiveProperty<AllocateType> Complete { get; set; }
        public ReactiveProperty<DateTime> DeliveryTime { get; set; }
        public ReactiveProperty<DeliveryFinal> DeliveryFinalize { get; set; }
        public Contract()
        {
            DeliveryTime = new ReactiveProperty<DateTime>().AddTo(disposable);
            DeliveryFinalize = new ReactiveProperty<DeliveryFinal>(DeliveryFinal.UnChecked);
            Complete = new ReactiveProperty<AllocateType>(AllocateType.NotYet).AddTo(disposable);
            DeliveryComplete = new ReactiveProperty<DeliveryComplete>(0).AddTo(disposable);
            DeliveryManCombine = new ReactiveProperty<string>().AddTo(disposable);
            DeliveryMan = new ReactiveCollection<Employee>().AddTo(disposable);
            this.Memo = new ReactiveProperty<string>().AddTo(disposable);
            this.Id = new ReactiveProperty<int>().AddTo(disposable);
            this.ListNo = new ReactiveProperty<int>().AddTo(disposable);
            this.Month = CreateDateTimeProperty("계약일자");
            this.Contractor = new ReactiveProperty<Customer>().AddTo(disposable);
            this.Delivery = CreateDateTimeProperty("배송일자");
            this.Seller = new ReactiveProperty<Employee>(new Employee()).AddTo(disposable);
            this.Price = CreateProperty<int>("총금액");
            this.DepositComplete= new ReactiveProperty<bool>().AddTo(disposable);
            this.PaymentComplete = new ReactiveProperty<FullyCompleted>(FullyCompleted.NotYet).AddTo(disposable);
            this.Payment = new ReactiveCollection<Payment>().AddTo(disposable);
            this.Product = new ReactiveCollection<ContractedProduct>().AddTo(disposable);
            ProductNameCombine = new ReactiveProperty<string>().AddTo(disposable);
            ProductMemoCombine = new ReactiveProperty<string>().AddTo(disposable);
            this.Contractor.Value = new Customer();
            SetObserver();
        }
        public string Validate()
        {
            
            if (this.Contractor.Value.ValidateAllProperties())
            {
                return "고객정보가 입력되지 않았습니다.";
            }
           
            if (this.Payment.Count == 0)
            {
                return "계약금 및 잔금이 등록되지 않았습니다.";
            }
            if (this.Product.Count == 0)
            {
                return "판매 상품이 등록되지 않았습니다.";
            }

            if (this.DeliveryMan.Count == 0) {
                return "배송인원이 선택되지 않았습니다.";
            }
            if (this.Seller.Value.Id.Value == 0)
            {
                return "판매자 입력이 되지 않았습니다.";
            }
            return "";
        }

        public void CompleteChangedData()
        {
            ChangedItem.RemoveAll();
            isChanged = false;
        }
        public override void SetObserver() {
            this.Complete.Subscribe(x => ChangedJson("complete", (int)x));
            this.DeliveryComplete.Subscribe(x => ChangedJson("delivery_complete", (int)x));
            this.DeliveryMan.ToObservable().Subscribe(updatedItems => { isChanged = true; });
            this.Month.Subscribe(x => ChangedJson("create_time", x));
            this.Price.Subscribe(x => ChangedJson("total", x));
            this.Seller.Subscribe(x => ChangedJson("seller_id", x.Id.Value));
            this.Memo.Subscribe(x => ChangedJson("memo", x));
            this.Delivery.Subscribe(x => ChangedDelivery("delivery_date", x));
            this.Contractor.Subscribe(x => ChangedJson("cui_id", x.Id.Value));
            this.DeliveryFinalize.Subscribe(x => ChangedJson("delivery_finalize", x));
            this.Payment.ToObservable().Subscribe(updatedItems => { isChanged = true; });
            this.DeliveryTime.Subscribe(x => ChangedDeleveryTime("delivery_date", x));
        }
        private void ChangedDelivery(string name, DateTime value) {
            DateTime temp = new DateTime(value.Year,value.Month,value.Day,this.DeliveryTime.Value.Hour,this.DeliveryTime.Value.Minute,0);
            ChangedItem[name] = temp.ToString("yyyy-MM-dd HH:mm:ss");
            isChanged = true;
        }

        public void ChangedDeleveryTime(string name, DateTime value)
        {
            DateTime temp = new DateTime(this.Delivery.Value.Year, this.Delivery.Value.Month, this.Delivery.Value.Day, value.Hour, value.Minute, 0);
            ChangedItem[name] = temp.ToString("yyyy-MM-dd HH:mm:ss");
            isChanged = true;
        }
        //public void TotalPrice() {
        //    int temper = 0; 
        //    foreach (ContractedProduct item in this.Product) {
        //        temper+=item.total.Value;
        //    }
        //    this.Price.Value = temper;
        //}

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
            foreach (Employee item in DeliveryMan) { 
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
            NewObject["delivery_finalize"] = (int)this.DeliveryFinalize.Value;
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
                if (item.Action.Value != AddDelete.Default) {
                    jarrPayment.Add(item.MakeJson());
                }
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
                NewObject["product_list"] = jarrProduct;

            JArray jarrDelivery = new JArray();
            foreach (Employee item in DeliveryMan)
            {
                if (!item.IsChecked.Value) {
                    continue;
                }
                JObject jobj = new JObject();
                jobj["employee_id"] = item.Id.Value;
                jobj["action"] = 1;
                jarrDelivery.Add(jobj);
            }
            if (jarrDelivery.Count > 0) {
                NewObject["delivery_group"] = jarrDelivery;
            }

            return NewObject;
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
                JObject jobj = new JObject();
                if (item.isChanged && item.PaymentId.Value == 0) //신규 생성
                {
                    jobj["payment_id"] = item.PaymentId.Value;
                    jobj["changed_item"] = item.MakeJson();
                    jobj["action"]= (int)item.Action.Value;
                    if (ChangedItem["payment"] == null)
                    {
                        jarrPayment.Add(jobj);
                        if (!item.ChangedItem.ToString().Equals(""))
                        {
                            ChangedItem["payment"] = jarrPayment;
                        }
                    }
                    else
                    {
                        if (!item.ChangedItem.ToString().Equals(""))
                        {
                            (ChangedItem["payment"] as JArray).Add(jobj);
                        }
                    }
                }
                if (item.isChanged && item.PaymentId.Value != 0) //기존 수정
                {
                    jobj["payment_id"] = item.PaymentId.Value;
                    jobj["changed_item"] = item.ChangedItem;
                    jobj["action"] = (int)item.Action.Value;
                    if (ChangedItem["payment"] == null)
                    {
                        jarrPayment.Add(jobj);
                        if (!item.ChangedItem.ToString().Equals(""))
                        {
                            ChangedItem["payment"] = jarrPayment;
                        }
                    }
                    else
                    {
                        if (!item.ChangedItem.ToString().Equals(""))
                        {
                            (ChangedItem["payment"] as JArray).Add(jobj);
                        }
                    }
                }
                item.isChanged = false;
            }

            if (jarrPayment.Count > 0) {
                ChangedItem["payment"] = jarrPayment;
            }

            JArray jarrProduct = new JArray();
            foreach (ContractedProduct item in Product)
            {
                if (item.isChanged)
                {
                    jarrProduct.Add(item.MakeJson());
                }
            }
            if (jarrProduct != null && jarrProduct.Count>0)
                ChangedItem["product_list"] = jarrProduct;
            

            JArray deliveryMan = new JArray();
            foreach (Employee item in DeliveryMan)
            {
                if (item.isChanged) {
                    JObject jobj = new JObject();
                    jobj["employee_id"] = item.Id.Value;
                    jobj["action"] = (int)item.Action.Value;
                    deliveryMan.Add(jobj);
                }
            }
            if (deliveryMan.Count > 0) {
                ChangedItem["delivery_group"] = deliveryMan;
            }
            return ChangedItem;
        }
    }
}
