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
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum Purpose {
        [Description("입고예정")]
        PreStored = 1,
        [Description("배달예정")]
        BookingDelivery= 2,
        [Description("배달완료")]
        Completed=3,
        [Description("창고재고")]
        Stored = 4,
        [Description("DP용")]
        DP = 5
    }
    //재고와 제품은 다른것이다.

    public class FurnitureInventory: PrismCommonModelBase
    {
        public ReactiveProperty<int> No { get; set; } // 순서
        public ReactiveProperty<string> Name { get; set; } //제품명
        public ReactiveProperty<int> Price { get; set; } // 입고가격
        public ReactiveProperty<int> Id { get; set; } // 아이디
        public ReactiveProperty<Company> Company { get; set; } //회사
        public ReactiveProperty<FurnitureType> ProductType { get; set; } //분류

        public ReactiveProperty<Purpose> Purpose; //재고 목적

        public ReactiveProperty<DateTime> StoreReachDate;//입고일

        public ReactiveProperty<int> Count;//수량

        public FurnitureInventory() : base()
        {
            this.No = new ReactiveProperty<int>().AddTo(disposable);
            this.Purpose = new ReactiveProperty<Purpose>().AddTo(this.disposable);
            this.StoreReachDate = new ReactiveProperty<DateTime>().AddTo(this.disposable);
            this.Count = new ReactiveProperty<int>().AddTo(this.disposable);
            this.Name = new ReactiveProperty<string>("").AddTo(disposable);
            this.Price = new ReactiveProperty<int>().AddTo(disposable);
            this.Id = new ReactiveProperty<int>().AddTo(disposable);
            this.Company = new ReactiveProperty<Company>(new Model.Company()).AddTo(disposable);
            this.ProductType = new ReactiveProperty<FurnitureType>(new FurnitureType()).AddTo(disposable);
            SetObserver();
        }

        public JObject MakeJson()
        {
            JObject jobj = new JObject();
            jobj["company_id"] = (int)this.Company.Value.Id.Value;
            jobj["product_name"] = this.Name.Value;
            jobj["product_price"] = (int)Price.Value;
            jobj["product_type"] = (int)this.ProductType.Value.Id.Value;
            jobj["purpose"] = (int)this.Purpose.Value;
            return jobj;
        }

        public override void SetObserver()
        {
            this.Company.Subscribe(x => ChangedJson("company_id", x.Id));
            this.Name.Subscribe(x => ChangedJson("product_name", x));
            this.Price.Subscribe(x => ChangedJson("product_price", x));
            this.ProductType.Subscribe(x => ChangedJson("product_type", x.Id));
            this.Purpose.Subscribe(x => ChangedJson("purpose", x));
            this.StoreReachDate.Subscribe(x => ChangedJson("insert_date", x));
            this.Count.Subscribe(x => ChangedJson("count", x));
        }
    }
}
