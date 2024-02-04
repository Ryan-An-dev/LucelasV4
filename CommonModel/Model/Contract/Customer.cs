using Newtonsoft.Json.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        [Required(ErrorMessage = "번호를 입력하세요.")]
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
            this.No = new ReactiveProperty<int>(mode: ReactivePropertyMode.IgnoreInitialValidationError).AddTo(disposable);
            this.Id = new ReactiveProperty<int>(mode: ReactivePropertyMode.IgnoreInitialValidationError).AddTo(disposable);
            this.Name = CreateProperty<string>("이름");
            this.Phone = CreateProperty<string>("번호");
            this.Address = CreateProperty<string>("주소");
            this.Memo = new ReactiveProperty<string>().AddTo(disposable);
            this.Address1 = CreateProperty<string>("상세주소");
            SetObserver();
        }
       
        public override void SetObserver()
        {
            this.Name.Subscribe(x => ChangedJson("cui_name", x));
            this.Phone.Subscribe(x => ChangedJson("cui_phone", x));
            this.Address.Subscribe(x => ChangedJson("cui_address", x));
            this.Memo.Subscribe(x => ChangedJson("cui_memo", x));
            this.Address1.Subscribe(x => ChangedJson("cui_address_detail", x));
            this.Id.Subscribe(x => ChangedJson("cui_id", x));
        }
        
    }
}
