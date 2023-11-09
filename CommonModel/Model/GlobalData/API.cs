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
    public enum APIType
    {
        [Description("바로빌")]
        BaroBill = 1
    }
    public class API : PrismCommonModelBase
    {
        //제품타입
        public ReactiveProperty<int>No { get; set; }
        public ReactiveProperty<int> Id { get; set; } //DB Id
        public ReactiveProperty<APIType> Type { get; set; } //  타입

        public ReactiveProperty<string> ApiKey { get; set; } // APIKey

        public ReactiveProperty<string> CertNum { get; set; } // 사업자번호
        public ReactiveProperty<string> ApiID { get; set; } // 계정정보

        public API() : base()
        {
            this.Id = new ReactiveProperty<int>().AddTo(disposable);
            this.No = new ReactiveProperty<int>().AddTo(disposable);
            this.Type = new ReactiveProperty<APIType>().AddTo(disposable);
            this.ApiKey = new ReactiveProperty<string>().AddTo(disposable);
            this.CertNum = new ReactiveProperty<string>().AddTo(disposable);
            this.ApiID = new ReactiveProperty<string>().AddTo(disposable);
            SetObserver();
        }
        public override void SetObserver()
        {
            this.Type.Subscribe(x => ChangedJson("api_type", x));
            this.ApiKey.Subscribe(x => ChangedJson("api_key", x));
            this.ApiID.Subscribe(x => ChangedJson("api_account", x));
            this.CertNum.Subscribe(x => ChangedJson("api_cert_num", x));
        }
    }
}
