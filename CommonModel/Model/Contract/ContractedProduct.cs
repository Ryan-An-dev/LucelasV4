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
    
    public class ContractedProduct: PrismCommonModelBase
    {
        public ReactiveProperty<int> No { get; set; } // 순서
        public ReactiveProperty<FurnitureInventory> FurnitureInventory { get; set; }

        public ReactiveProperty<int> SellPrice { get; set; } // 판매가격

        public ReactiveProperty<int> SellCount { get; set; }//주문수량

        public ContractedProduct() : base()
        {
            this.No = new ReactiveProperty<int>().AddTo(disposable);
            this.SellCount = new ReactiveProperty<int>(0).AddTo(this.disposable);
            this.SellPrice = new ReactiveProperty<int>(0).AddTo(this.disposable);
            this.FurnitureInventory = new ReactiveProperty<FurnitureInventory>().AddTo(disposable);
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

        public override void SetObserver()
        {
            this.SellPrice.Subscribe(x => ChangedJson("sell_price", x));
            this.SellCount.Subscribe(x => ChangedJson("order_count", x));
        }
    }
}
