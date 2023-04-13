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
    public class HomeSummaryModel : PrismCommonModelBase
    {
        
        ReactiveProperty<int?> Month { get; set; }
        ReactiveProperty<int?> CompleteContract { get; set; }
        ReactiveProperty<int?> CompleteDistribute { get; set; }
        ReactiveProperty<int?> CompleteDelevery { get; set; }
        ReactiveProperty<int?> NotCompleteContract { get; set; }
        ReactiveProperty<int?> NotCompleteDistribute { get; set; }
        ReactiveProperty<int?> TodayDelevery { get; set; }

        public HomeSummaryModel() : base()
        {
            this.Month = new ReactiveProperty<int?>(0).AddTo(this.disposable);
            this.CompleteContract = new ReactiveProperty<int?>(0).AddTo(this.disposable);
            this.CompleteDistribute = new ReactiveProperty<int?>(0).AddTo(this.disposable);
            this.CompleteDelevery = new ReactiveProperty<int?>(0).AddTo(this.disposable);
            this.NotCompleteContract = new ReactiveProperty<int?>(0).AddTo(this.disposable);
            this.NotCompleteDistribute = new ReactiveProperty<int?>(0).AddTo(this.disposable);
            this.TodayDelevery = new ReactiveProperty<int?>(0).AddTo(this.disposable);
        }
    }
}
