using Newtonsoft.Json.Linq;
using PrsimCommonBase;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CommonModel.Model
{
    public class Customer : PrismCommonModelBase
    {
        public ReactiveProperty<int> No { get; set; }
        [JsonPropertyName("cui_name")]
        public ReactiveProperty<string> Name { get; set; }
        [JsonPropertyName("cui_phone")]
        public ReactiveProperty<string> Phone { get; set; }
        [JsonPropertyName("cui_address")]
        public ReactiveProperty<string> Address { get; set; }

        [JsonPropertyName("cui_address_detail")]
        public ReactiveProperty<string> Address1 { get; set; }

        [JsonPropertyName("cui_memo")]
        public ReactiveProperty<string> Memo { get; set; }
        [JsonPropertyName("cui_id")]
        public ReactiveProperty<int> Id { get; set; }

        public Customer()
        {
            this.No = new ReactiveProperty<int>().AddTo(disposable);
            this.Id = new ReactiveProperty<int>().AddTo(disposable);
            this.Name= new ReactiveProperty<string>().AddTo(disposable);
            this.Phone = new ReactiveProperty<string>().AddTo(disposable);
            this.Address = new ReactiveProperty<string>().AddTo(disposable);
            this.Memo = new ReactiveProperty<string>().AddTo(disposable);
            this.Address1  = new ReactiveProperty<string>().AddTo(disposable);
            
        }
        public override void SetObserver()
        {
            this.Name.Subscribe(x => ChangedJson("cui_name", x));
            this.Phone.Subscribe(x => ChangedJson("cui_phone", x));
            this.Address.Subscribe(x => ChangedJson("cui_address", x));
            this.Address1.Subscribe(x => ChangedJson("cui_address_detail", x));
            this.Id.Subscribe(x => ChangedJson("cui_id", x));
        }
        
    }
}
