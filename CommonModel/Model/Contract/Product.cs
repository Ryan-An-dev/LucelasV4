using CommonModel.Model;
using Newtonsoft.Json.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CommonModel
{
    public class Product : PrismCommonModelBase
    {
        public ReactiveProperty<int> No { get; set; } // 순서
        public ReactiveProperty<string> Name { get; set; } //제품명
        public ReactiveProperty<int> Price { get; set; } // 매입단가
        public ReactiveProperty<int> Id { get; set; } // 아이디
        public ReactiveProperty<Company> Company { get; set; } //회사
        public ReactiveProperty<FurnitureType> ProductType { get; set; } //분류

        public Product() : base()
        {
            this.No = new ReactiveProperty<int>().AddTo(disposable);
            this.Id = new ReactiveProperty<int>().AddTo(disposable);
            this.Name = CreateProperty<string>("이름");
            this.Price = CreateProperty<int>("매입단가");
            this.Company = new ReactiveProperty<Company>(mode: ReactivePropertyMode.IgnoreInitialValidationError).SetValidateNotifyError(x => {
                if (x == null || x.Id.Value == 0)
                {
                    return $"회사을(를) 입력하세요.";
                }
                return null;
            });

            this.ProductType = CreateProperty<FurnitureType>("제품타입");
            SetObserver();
        }
        public JObject MakeJson()
        {
            JObject jobj = new JObject();
            jobj["company_id"] = (int)this.Company.Value.Id.Value;
            jobj["product_name"] = this.Name.Value;
            jobj["product_price"] = (int)Price.Value;
            jobj["product_type"] = (int)this.ProductType.Value.Id.Value;
            return jobj;
        }

        public override void SetObserver()
        {
            this.Company.Subscribe(x => ChangedJson("company_id", x.Id));
            this.Name.Subscribe(x => ChangedJson("product_name", x));
            this.Price.Subscribe(x => ChangedJson("product_price", x));
            this.ProductType.Subscribe(x => ChangedJson("product_type", x.Id));
        }
    }
}
