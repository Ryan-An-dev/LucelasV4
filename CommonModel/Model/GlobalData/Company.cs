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

namespace CommonModel.Model
{
    public class Company : PrismCommonModelBase
    {
        public ReactiveProperty<int> No { get; set; }
        public ReactiveProperty<int> Id { get; set; }
        public ReactiveProperty<string> CompanyName { get; set; }  
        public ReactiveProperty<string> CompanyPhone { get; set; }
        public ReactiveProperty<string> CompanyAddress { get; set; }
        public ReactiveProperty<string> CompanyAddressDetail { get; set; }
        public Company():base()
        {
            this.No = new ReactiveProperty<int>().AddTo(disposable);
            this.Id = new ReactiveProperty<int>().AddTo(disposable);
            this.CompanyName = CreateProperty<string>("이름");
            this.CompanyAddress = new ReactiveProperty<string>().AddTo(disposable);
            this.CompanyPhone = new ReactiveProperty<string>().AddTo(disposable);
            this.CompanyAddressDetail = new ReactiveProperty<string>().AddTo(disposable);
            SetObserver();
        }
        public Company(int categoryId, string CompanyName)
        {
            this.Id = new ReactiveProperty<int>(categoryId).AddTo(disposable);
            this.CompanyName = new ReactiveProperty<string>(CompanyName).AddTo(disposable);
        }
        public JObject MakeJson()
        {
            JObject jobj = new JObject();
            jobj["company_id"] = this.Id.Value;
            jobj["company_name"] = this.CompanyName.Value;
            jobj["company_phone"] = this.CompanyPhone.Value;
            jobj["company_address"] = this.CompanyAddress.Value;
            jobj["company_address_detail"] = this.CompanyAddressDetail.Value;
            return jobj;
        }
        public override void SetObserver()
        {
            this.CompanyName.Subscribe(x => ChangedJson("company_name", x));
            this.CompanyPhone.Subscribe(x => ChangedJson("company_phone", x));
            this.CompanyAddress.Subscribe(x => ChangedJson("company_address", x));
            this.CompanyAddressDetail.Subscribe(x => ChangedJson("company_address_detail", x));
        }
    }
}
