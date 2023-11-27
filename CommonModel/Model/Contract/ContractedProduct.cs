using Newtonsoft.Json.Linq;
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
    public class ContractedProduct : PrismCommonModelBase
    {
        public event Action<int> ForTotal;
        
        public ReactiveProperty<int> No { get; set; } // 순서
        public ReactiveProperty<Product> FurnitureInventory { get; set; }

        public ReactiveProperty<int> SellPrice { get; set; } // 판매가격

        public ReactiveProperty<int> SellCount { get; set; }//주문수량
        public ReactiveProperty<int> total { get; set; } //개별 물건의 총량

        public ContractedProduct() : base()
        {
            this.total = new ReactiveProperty<int>().AddTo(disposable);
            this.No = new ReactiveProperty<int>().AddTo(disposable);
            this.SellCount = CreateProperty<int>("수량");
            this.SellPrice = CreateProperty<int>("판매금액");
            this.FurnitureInventory = new ReactiveProperty<Product>().AddTo(disposable);
            SetObserver();
        }
        public JObject MakeJson()
        {
            JObject jobj = new JObject();
            jobj["product_info"] = this.FurnitureInventory.Value.MakeJson();
            jobj["sell_price"] = this.SellPrice.Value;
            jobj["order_count"] = this.SellCount.Value;
            return jobj;
        }
        public void GetTotal(string name,int x) {
            int total = 0;
            ChangedJson(name, x);
            total=this.SellCount.Value * this.SellPrice.Value;
            this.total.Value = total;
        }

        public override void SetObserver()
        {
            this.SellPrice.Subscribe(x=> GetTotal("sell_price",x));
            this.SellCount.Subscribe(x => GetTotal("order_count", x));
        }
        public void SetSub() {
            this.total.Subscribe(x => ForTotal(x));
        }
    }
}
