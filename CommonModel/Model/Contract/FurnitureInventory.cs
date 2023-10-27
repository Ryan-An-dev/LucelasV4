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
        public ReactiveProperty<Purpose> Purpose; //재고 목적

        public ReactiveProperty<Furniture> Furniture; //제품

        public ReactiveProperty<DateTime> StoreReachDate;//입고일

        public ReactiveProperty<int> Count;//수량

        public FurnitureInventory() : base()
        {
            this.Purpose = new ReactiveProperty<Purpose>().AddTo(this.disposable);
            this.Furniture = new ReactiveProperty<Furniture>().AddTo(this.disposable);
            this.StoreReachDate = new ReactiveProperty<DateTime>().AddTo(this.disposable);
            this.Count = new ReactiveProperty<int>().AddTo(this.disposable);
        }

        public override void SetObserver()
        {
            this.Purpose.Subscribe(x => ChangedJson("purpose", x));
            this.StoreReachDate.Subscribe(x => ChangedJson("insert_date", x));
            this.Count.Subscribe(x => ChangedJson("count", x));
        }
    }
}
