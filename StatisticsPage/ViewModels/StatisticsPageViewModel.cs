using DataAccess;
using DataAccess.NetWork;
using LiveChartsCore;
using Newtonsoft.Json.Linq;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using PrsimCommonBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using CommonModel;
using CommonModel.Model;
using SkiaSharp;
using System.Text;
using System.Drawing.Text;
using System.Drawing;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using LiveCharts.Wpf;
using LiveCharts;
using System.Collections.ObjectModel;

namespace StatisticsPage.ViewModels
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum StatisticsUnit {
        [Description("일간")]
        Daily = 0,
        [Description("월간")]
        Monthly = 1
    }

    public class ChartDataBase {
        private string _Name;

        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        private int myVar;

        public int MyProperty
        {
            get { return myVar; }
            set { myVar = value; }
        }


    }

    public class StatisticsPageViewModel : PrismCommonModelBase, INavigationAware, INetReceiver
    {
        public ReactiveProperty<ObservableCollection<string>> Label { get; set; }
        public ReactiveProperty<Axis> YAxes { get; set; }

        public ReactiveProperty<StatisticsUnit> SelectedUnit { get; set; }
        public IEnumerable<StatisticsUnit> StatisticsUnitList
        {
            get { return Enum.GetValues(typeof(StatisticsUnit)).Cast<StatisticsUnit>(); }
        }
        public ReactiveProperty<LineSeries> PreviousMonth { get; set; }
        public ReactiveProperty<LineSeries> ThisMonth { get; set; }

        public ISeries[] SeriesCollection { get; set; }
        private IContainerProvider ContainerProvider { get; }

        public void ChangedChartUnit(StatisticsUnit args) {
           
        }
        public StatisticsPageViewModel(IRegionManager regionManager, IContainerProvider containerProvider) : base(regionManager)
        {
            this.Label = new ReactiveProperty<ObservableCollection<string>>().AddTo(this.disposable);
            this.SelectedUnit = new ReactiveProperty<StatisticsUnit>(0).AddTo(this.disposable);
            this.ContainerProvider = containerProvider;
            this.SelectedUnit.Subscribe(x => { ChangedChartUnit(x); });

            IList<int?> values = new int?[100];
            IList<int?> values2 = new int?[100];
            var r = new Random();
            var t = 0;

            for (var i = 0; i < 100; i++)
            {
                t += r.Next(0, 100);
                values[i] = t;
            }
            for (var i = 0; i < 100; i++)
            {
                if (i >= 51) {
                    values2[i] = null;
                    continue;
                }
                t += r.Next(0, 100);
                values2[i] = t;
            }
            Label.Value = new ObservableCollection<string>();
            LineSeries temp = new LineSeries();
            temp.Values = (IChartValues)values;
            this.PreviousMonth.Value = temp;
            LineSeries temp2 = new LineSeries();
            temp.Values = (IChartValues)values2;
            this.ThisMonth.Value = temp2;
        }
        
        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnConnected()
        {
            throw new NotImplementedException();
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            SendData();
        }

        public void OnRceivedData(ErpPacket packet)
        {
            throw new NotImplementedException();
        }

        public void OnSent()
        {
            throw new NotImplementedException();
        }
        private void SendData()
        {
            using (var network = this.ContainerProvider.Resolve<DataAgent.BankListDataAgent>())
            {
                network.SetReceiver(this);
                JObject jobj = new JObject();
                
                //network.GetBankHistory(jobj);
                
            }
        }
    }
}
