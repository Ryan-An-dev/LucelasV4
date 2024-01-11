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
        public ReactiveProperty<AddDelete> Action { get; set; }
        public ReactiveProperty<bool> IsChecked { get; set; }
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
            Action = new ReactiveProperty<AddDelete>(0).AddTo(disposable);
            IsChecked = new ReactiveProperty<bool>(false).AddTo(disposable);
            No = new ReactiveProperty<int>().AddTo(disposable);
            Id = new ReactiveProperty<int>().AddTo(disposable);
            Name = CreateProperty<string>("이름");
            Phone = CreateProperty<string>("번호");
            Address = CreateProperty<string>("주소");
            AddressDetail = CreateProperty<string>("상세주소");
            StartWorkTime = CreateDateTimeProperty("시작일");
            BirthDay  = CreateDateTimeProperty("생일");
            Memo = new ReactiveProperty<string>().AddTo(disposable);
            SetObserver();
        }
        public override void SetObserver()
        {
            IsChecked.Subscribe(x => ActionChecked(x));
            Name.Subscribe(x => ChangedJson("employee_name", x));
            Phone.Subscribe(x => ChangedJson("employee_phone", x));
            StartWorkTime.Subscribe(x => ChangedJson("employee_start", x));
            Address.Subscribe(x => ChangedJson("employee_address", x));
            AddressDetail.Subscribe(x => ChangedJson("employee_address_detail", x));
            Memo.Subscribe(x => ChangedJson("employee_memo", x));
            BirthDay.Subscribe(x => ChangedJson("employee_birthday", x));
        }

        public void ActionChecked(bool check)
        {
            isChanged = true;
            if (check)
            {
                this.Action.Value = AddDelete.Add;
            }
            else { 
                this.Action.Value = AddDelete.Remove;
            }
        }
        public Employee Copy() { 
            Employee employee = new Employee();
            employee.Action.Value = this.Action.Value;
            employee.IsChecked.Value = this.IsChecked.Value;
            employee.Id.Value = this.Id.Value;
            employee.Name.Value = this.Name.Value;
            employee.Phone.Value = this.Phone.Value;
            employee.Address.Value = this.Address.Value;
            employee.AddressDetail.Value = this.AddressDetail.Value;
            employee.StartWorkTime.Value = this.StartWorkTime.Value;
            employee.BirthDay.Value = this.BirthDay.Value;
            employee.Memo.Value = this.Memo.Value;
            return employee;
        }
        
    }
}
