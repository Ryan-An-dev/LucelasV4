using PrsimCommonBase;
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
        public ReactiveProperty<int> ProductCode { get; set; } //DB Id
        public ReactiveProperty<string> Name { get; set; } // 사용자지정 Name
        public FurnitureType() : base()
        {
            this.ProductCode = new ReactiveProperty<int>().AddTo(disposable);
            this.Name = new ReactiveProperty<string>().AddTo(disposable);
        }
        public override void SetObserver()
        {
            
        }
    }
}
