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
    public class Employee : PrismCommonModelBase
    {
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
            Id = new ReactiveProperty<int>().AddTo(disposable);
            Name = new ReactiveProperty<string>().AddTo(disposable);
            Phone = new ReactiveProperty<string>().AddTo(disposable);
            Address = new ReactiveProperty<string>().AddTo(disposable);
            AddressDetail = new ReactiveProperty<string>().AddTo(disposable);
            StartWorkTime = new ReactiveProperty<DateTime>().AddTo(disposable);
            BirthDay  = new ReactiveProperty<DateTime>().AddTo(disposable);
            Memo = new ReactiveProperty<string>().AddTo(disposable);
        }
        public override void SetObserver()
        {
            Name.Subscribe(x => ChangedJson("employee_name", x));
            Phone.Subscribe(x => ChangedJson("employee_phone", x));
            StartWorkTime.Subscribe(x => ChangedJson("employee_start", x));
            Address.Subscribe(x => ChangedJson("employee_address", x));
            AddressDetail.Subscribe(x => ChangedJson("employee_address_detail", x));
            Memo.Subscribe(x => ChangedJson("employee_memo", x));
        }
    }
}
