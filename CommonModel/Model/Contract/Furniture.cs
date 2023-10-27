using Newtonsoft.Json.Linq;
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
    public class Furniture : PrismCommonModelBase
    {
        public ReactiveProperty<string> Name { get; set; }
        public ReactiveProperty<int> Price { get; set; }
        public ReactiveProperty<int>Id { get; set; }
        public ReactiveProperty<Company>Company { get; set; }
        public ReactiveProperty<int>ProductType { get; set; }
        public Furniture():base()
        {
            this.Name = new ReactiveProperty<string>("").AddTo(disposable);
            this.Price = new ReactiveProperty<int>().AddTo(disposable);
            this.Id = new ReactiveProperty<int>().AddTo(disposable);
            this.Company = new ReactiveProperty<Company>().AddTo(disposable);
            this.ProductType =  new ReactiveProperty<int>().AddTo(disposable);
        }

        public override void SetObserver()
        {
            this.Name.Subscribe(x => ChangedJson("product_name", x));
            this.Price.Subscribe(x => ChangedJson("product_price", x));
            this.ProductType.Subscribe(x => ChangedJson("product_type", x));
        }
    }
}
