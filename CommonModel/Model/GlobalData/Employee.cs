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
        public ReactiveProperty<int> EmployeeId { get; set; }

        public ReactiveProperty<string> CompanyName { get; set; }

        public Employee(ReactiveProperty<int> employeeId, ReactiveProperty<string> companyName)
        {
            EmployeeId = employeeId.AddTo(disposable);
            CompanyName = companyName.AddTo(disposable);
            
        }
        public override void SetObserver()
        {
            throw new NotImplementedException();
        }
    }
}
