using Newtonsoft.Json.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModel.Model
{
    public enum AddDelete
    {
        Default = 0,
        Add = 1,
        Remove = 2,
        Update = 3
    }
    public class ContractedProduct : PrismCommonModelBase
    {
        public ReactiveProperty<AddDelete> Action { get; set; }

        public ReactiveProperty<int> Id { get; set; } //id
        
        public ReactiveProperty<int> No { get; set; } // 순서
        public ReactiveProperty<Product> FurnitureInventory { get; set; }

        public ReactiveProperty<int> SellCount { get; set; }//주문수량
        public ReactiveProperty<int> total { get; set; } //개별 물건의 총량

        public ReactiveProperty<string> Memo { get; set; } //메모

        public ContractedProduct() : base()
        {
            this.total = new ReactiveProperty<int>().AddTo(disposable);
            this.Id = new ReactiveProperty<int>().AddTo(disposable);
            this.No = new ReactiveProperty<int>().AddTo(disposable);
            this.SellCount = CreateProperty<int>("수량");
            this.FurnitureInventory = new ReactiveProperty<Product>().AddTo(disposable);
            this.Action = new ReactiveProperty<AddDelete>(AddDelete.Add).AddTo(disposable);
            this.Memo = new ReactiveProperty<string>().AddTo(disposable);
            SetObserver();
        }
        public JObject MakeJson()
        {
            JObject jobj = new JObject();
            jobj["product_order_id"] = this.Id.Value;
            jobj["product_info"] = this.FurnitureInventory.Value.MakeJson();
            jobj["order_count"] = this.SellCount.Value;
            jobj["memo"] = this.Memo.Value;
            jobj["action"] = (int)this.Action.Value;
            return jobj;
        }

        public override void SetObserver()
        {
            this.Memo.Subscribe(x => ChangedJson("memo", x));
            this.Action.Subscribe(x => ChangedJson("action", (int)x));
        }
    }
}
