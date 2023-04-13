using DataAccess;
using DataAccess.NetWork;
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
using System.Text;
using System.Drawing.Text;
using System.Drawing;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Collections.ObjectModel;
using LiveCharts.Wpf;
using LiveCharts;

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
        public ObservableCollection<string> Label { get; set; }
        

        public ReactiveProperty<StatisticsUnit> SelectedUnit { get; set; }
        public IEnumerable<StatisticsUnit> StatisticsUnitList
        {
            get { return Enum.GetValues(typeof(StatisticsUnit)).Cast<StatisticsUnit>(); }
        }

        public SeriesCollection SeriesCollection { get; set; }
        private IContainerProvider ContainerProvider { get; }

        public void ChangedChartUnit(StatisticsUnit args) {
           
        }
        public StatisticsPageViewModel(IRegionManager regionManager, IContainerProvider containerProvider) : base(regionManager)
        {
            this.Label = new ObservableCollection<string>();
            this.SeriesCollection = new SeriesCollection(); 
            this.SelectedUnit = new ReactiveProperty<StatisticsUnit>(0).AddTo(this.disposable);
            this.ContainerProvider = containerProvider;
            this.SelectedUnit.Subscribe(x => { ChangedChartUnit(x); });
            ChartValues<double> values = new ChartValues<double>();
            ChartValues<double> values2 = new ChartValues<double>();
            var r = new Random();
            var t = 0;
           
            for (var i = 0; i < 100; i++)
            {
                t += r.Next(0, 100);
                values.Add(t);
            }
            for (var i = 0; i < 100; i++)
            {
                if (i >= 51) {
                    
                    continue;
                }
                t += r.Next(0, 100);
                values2.Add(t);
            }
            for (int i = 1; i < 101; i++) { 
                Label.Add(i+"일");
            }
            var series1 = new LineSeries
            {
                Title = "지난달",
                Values = values
            };

            var series2 = new LineSeries
            {
                Title = "이번달",
                Values = values2
            };
            SeriesCollection.Add(series2);
            SeriesCollection.Add(series1);
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
