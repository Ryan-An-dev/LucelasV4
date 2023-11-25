
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModel.Model
{
    public class FurnitureType : PrismCommonModelBase
    {
        //제품타입
        public ReactiveProperty<int> Id { get; set; } //DB Id
        public ReactiveProperty<string> Name { get; set; } // 사용자지정 Name
        public ReactiveProperty<int>No { get; set; }
        public FurnitureType() : base()
        {
            this.Id = new ReactiveProperty<int>().AddTo(disposable);
            this.Name = new ReactiveProperty<string>().AddTo(disposable);
            this.No = new ReactiveProperty<int>().AddTo(disposable);
            SetObserver();
        }
        public override void SetObserver()
        {
            this.Name.Subscribe(x => ChangedJson("product_type_name", x));
        }
    }
}
