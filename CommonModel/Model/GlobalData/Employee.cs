using Newtonsoft.Json.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModel.Model
{
    public class Employee : PrismCommonModelBase
    {
        public ReactiveProperty<int> No { get; set; }
        public ReactiveProperty<int> Id { get; set; }
        public ReactiveProperty<string> Name { get; set; }
        public ReactiveProperty<string> Phone { get; set; }
        public ReactiveProperty<string> Address { get; set; }
        public ReactiveProperty<string> AddressDetail { get; set; }
        public ReactiveProperty<DateTime> StartWorkTime { get; set; }
        public ReactiveProperty<DateTime> BirthDay { get; set; }
        public ReactiveProperty<string> Memo { get; set; }
        public Employee()
        {
            No = new ReactiveProperty<int>().AddTo(disposable);
            Id = new ReactiveProperty<int>().AddTo(disposable);
            Name = CreateProperty<string>("이름");
            Phone = CreateProperty<string>("번호");
            Address = CreateProperty<string>("주소");
            AddressDetail = CreateProperty<string>("상세주소");
            StartWorkTime = CreateDateTimeProperty("시작일");
            BirthDay  = CreateDateTimeProperty("생일");
            Memo = CreateProperty<string>("메모");
            SetObserver();
        }
        public override void SetObserver()
        {
            Name.Subscribe(x => ChangedJson("employee_name", x));
            Phone.Subscribe(x => ChangedJson("employee_phone", x));
            StartWorkTime.Subscribe(x => ChangedJson("employee_start", x));
            Address.Subscribe(x => ChangedJson("employee_address", x));
            AddressDetail.Subscribe(x => ChangedJson("employee_address_detail", x));
            Memo.Subscribe(x => ChangedJson("employee_memo", x));
            BirthDay.Subscribe(x => ChangedJson("employee_birthday", x));
        }
    }
}
