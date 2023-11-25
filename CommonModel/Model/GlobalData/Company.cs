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
            this.CompanyAddress = CreateProperty<string>("주소");
            this.CompanyPhone = CreateProperty<string>("번호");
            this.CompanyAddressDetail = CreateProperty<string>("상세주소");
            SetObserver();
        }
        public Company(int categoryId, string CompanyName)
        {
            this.Id = new ReactiveProperty<int>(categoryId).AddTo(disposable);
            this.CompanyName = new ReactiveProperty<string>(CompanyName).AddTo(disposable);
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
