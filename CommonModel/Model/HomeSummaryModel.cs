﻿using Newtonsoft.Json.Linq;
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
        
        public ReactiveProperty<DateTime> Month { get; set; }
        public ReactiveProperty<int?> CompleteContract { get; set; }
        public ReactiveProperty<int?> CompleteDistribute { get; set; }
        public ReactiveProperty<int?> CompleteDelevery { get; set; }
        public ReactiveProperty<int?> NotCompleteContract { get; set; }
        public ReactiveProperty<int?> NotCompleteDistribute { get; set; }
        public ReactiveProperty<int?> NotCompleteDelivery { get; set; }
        public ReactiveProperty<int?> TodayDelevery { get; set; }

        public ReactiveProperty<int?> DeliveryUnFinalizeCount { get; set; }

        public ReactiveProperty<int?> NotOrderCount { get; set; }

        public HomeSummaryModel() : base()
        {
            DeliveryUnFinalizeCount= new ReactiveProperty<int?>().AddTo(this.disposable);
            NotOrderCount = new ReactiveProperty<int?>().AddTo(this.disposable);
            this.Month = new ReactiveProperty<DateTime>(DateTime.Now).AddTo(this.disposable);
            this.CompleteContract = new ReactiveProperty<int?>(0).AddTo(this.disposable);
            this.CompleteDistribute = new ReactiveProperty<int?>(0).AddTo(this.disposable);
            this.CompleteDelevery = new ReactiveProperty<int?>(0).AddTo(this.disposable);
            this.NotCompleteContract = new ReactiveProperty<int?>(0).AddTo(this.disposable);
            this.NotCompleteDistribute = new ReactiveProperty<int?>(0).AddTo(this.disposable);
            this.TodayDelevery = new ReactiveProperty<int?>(0).AddTo(this.disposable);
            this.NotCompleteDelivery = new ReactiveProperty<int?>().AddTo(this.disposable);
        }

        public override void SetObserver()
        {
            
        }
    }
}
