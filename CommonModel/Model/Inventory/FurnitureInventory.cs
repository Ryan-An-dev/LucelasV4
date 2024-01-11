﻿using CommonModel.Model;
using Newtonsoft.Json.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CommonModel
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum Purpose
    {
        [Description("모두")]
        All = 0,
        [Description("입고예정")]
        PreStored = 1,
        [Description("배달예정")]
        BookingDelivery = 2,
        [Description("창고재고")]
        Stored = 3,
        [Description("DP용")]
        DP = 4,
        [Description("배달완료")]
        Completed = 5,
    }
    //재고
    public class FurnitureInventory: PrismCommonModelBase
    {

        public ReactiveProperty<int> No { get; set; } // 순서

        public ReactiveProperty<Product> Product {get;set;}
        public ReactiveProperty<int> Id { get; set; } // 아이디

        public ReactiveProperty<Purpose> ReceivingType { get; set; } //재고 목적

        public ReactiveProperty<bool> CountEnable { get; set; } //수량 제한여부

        public ReactiveProperty<DateTime?> StoreReachDate { get; set; }//입고일
        public ReactiveProperty<Visibility> RealPriceVis { get; set; } //실제 재고
        public ReactiveProperty<int> RealPrice { get; set; } //실제 재고가격

        public ReactiveProperty<int> Count { get; set; }//수량
        public ReactiveProperty<string> Memo { get; set; } //메모
        
        //연결된 계약을 추가할까? 추가로 받자
        public ReactiveProperty<Contract> ContractedContract { get; set; }

        public FurnitureInventory() : base()
        {
            RealPrice = new ReactiveProperty<int>(0).AddTo(disposable);
            CountEnable = new ReactiveProperty<bool>().AddTo(disposable); 
            RealPriceVis = new ReactiveProperty<Visibility>(Visibility.Collapsed).AddTo(disposable);
            this.No = new ReactiveProperty<int>().AddTo(disposable);
            this.Id = new ReactiveProperty<int>().AddTo(disposable);
            this.Product = new ReactiveProperty<Product>().AddTo(disposable);
            this.ReceivingType = new ReactiveProperty<Purpose>().AddTo(disposable);
            this.StoreReachDate = new ReactiveProperty<DateTime?>().AddTo(disposable);
            this.ContractedContract = new ReactiveProperty<Contract>().AddTo(disposable);   
            this.Memo = CreateProperty<string>("메모");
            this.Count = CreateProperty<int>("재고수량");
            RealPrice = new ReactiveProperty<int>().AddTo(disposable);
            SetObserver();
        }
        public JObject MakeJson()
        {
            JObject jobj = new JObject();
            jobj["product_info"] = this.Product.Value.MakeJson();
            jobj["inventory_id"] = this.Id.Value;
            jobj["receiving_type"] = (int)this.ReceivingType.Value;
            jobj["receiving_date"] = StoreReachDate.Value?.ToString("yyyy-MM-dd");
            jobj["count"] = this.Count.Value;
            jobj["memo"] = this.Memo.Value;
            return jobj;
        }

        public override void SetObserver()
        {
            this.ReceivingType.Subscribe(x => ChangedRecivingType("receiving_type", x));
            this.StoreReachDate.Subscribe(x => ChangedJson("insert_date", x));
            this.Count.Subscribe(x => ChangedJson("count", x));
            this.RealPrice.Subscribe(x => ChangedJson("real_price", x));
            this.Memo.Subscribe(x => ChangedJson("memo", x));
        }
        private void ChangedRecivingType(string name , Purpose purpose)
        {
            if ((int)purpose >= 2) {
                RealPriceVis.Value = Visibility.Visible;
            }
            else{
                RealPriceVis.Value = Visibility.Collapsed;
            }
            ChangedJson("receiving_type", purpose);
        }
    }
}
