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
    public class Furniture: PrismCommonModelBase
    {
        public ReactiveProperty<Company> Company;

        public ReactiveProperty<Product> SelectedProduct;

        public ReactiveProperty<DateTime> DeliveryDate; //고객 배송 요청날짜

        public ReactiveProperty<DateTime> FactoryOrderDate;//공장 오더 날짜

        public ReactiveProperty<DateTime> StoreReachDate;//매장 도착 날짜

        public ReactiveProperty<bool> Completed;//개별 상품 완료 여부

        public Furniture()
        {
            this.Company = new ReactiveProperty<Company>().AddTo(this.disposable);
            this.SelectedProduct = new ReactiveProperty<Product>().AddTo(this.disposable);
            this.DeliveryDate = new ReactiveProperty<DateTime>().AddTo(this.disposable);
            this.FactoryOrderDate = new ReactiveProperty<DateTime>().AddTo(this.disposable);
            this.StoreReachDate = new ReactiveProperty<DateTime>().AddTo(this.disposable);
            this.Completed = new ReactiveProperty<bool>().AddTo(this.disposable);
        }

    }
}
