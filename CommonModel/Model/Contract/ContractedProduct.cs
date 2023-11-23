using Newtonsoft.Json.Linq;
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
    
    public class ContractedProduct: PrismCommonModelBase
    {
        public ReactiveProperty<int> No { get; set; } // 순서
        public ReactiveProperty<FurnitureInventory> Product { get; set; }

        public ReactiveProperty<int> Price { get; set; } // 판매가격

        public ReactiveProperty<int> Count;//주문수량

        public ContractedProduct() : base()
        {
            this.No = new ReactiveProperty<int>().AddTo(disposable);
            this.Count = new ReactiveProperty<int>().AddTo(this.disposable);
            this.Price = new ReactiveProperty<int>().AddTo(this.disposable);
            SetObserver();
        }

        public override void SetObserver()
        {
            this.Price.Subscribe(x => ChangedJson("sell_price", x));
            this.Count.Subscribe(x => ChangedJson("order_count", x));
        }
    }
}
