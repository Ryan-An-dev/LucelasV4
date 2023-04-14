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
using LogWriter;
using System.Windows;
using System.Windows.Media;
using LiveCharts.Definitions.Charts;
using System.Windows.Controls.Primitives;

namespace StatisticsPage.ViewModels
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum StatisticsUnit {
        [Description("일간")]
        Daily = 0,
        [Description("월간")]
        Monthly = 1
    }

    public class StatisticsPageViewModel : PrismCommonModelBase, INavigationAware, INetReceiver
    {
        public ChartValues<int> PreviousMonthSalesValues { get; set; }
        public ChartValues<int> CurrentMonthSalesValues { get; set; }

        public ReactiveProperty<StatisticsUnit> SelectedUnit { get; set; }
        public IEnumerable<StatisticsUnit> StatisticsUnitList
        {
            get { return Enum.GetValues(typeof(StatisticsUnit)).Cast<StatisticsUnit>(); }
        }
        public Func<DateTime, string> XFormatter { get; set; }
        public SeriesCollection SeriesCollection { get; set; }
        public DefaultTooltip DataToolTip { get; set; }
        private IContainerProvider ContainerProvider { get; }

        public void ChangedChartUnit(StatisticsUnit args) {
            if (args == StatisticsUnit.Daily)
            {

            }
        }
        public SeriesCollection PieSeries { get; set; }
        //public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> YFormatter { get; set; }
        public StatisticsPageViewModel(IRegionManager regionManager, IContainerProvider containerProvider) : base(regionManager)
        {
            YFormatter = value => value.ToString("C");
            XFormatter = value => value.ToString("dd") + "일";
            this.PieSeries = new SeriesCollection();
            this.PreviousMonthSalesValues = new ChartValues<int>();
            this.CurrentMonthSalesValues = new ChartValues<int>();
            this.SeriesCollection = new SeriesCollection();
            this.SelectedUnit = new ReactiveProperty<StatisticsUnit>(0).AddTo(this.disposable);
            this.ContainerProvider = containerProvider;
            this.SelectedUnit.Subscribe(x => { ChangedChartUnit(x); });

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
            Application.Current.Dispatcher.Invoke(() => { this.SeriesCollection.Clear(); this.PieSeries.Clear(); });
            SendData();
        }

        public void OnRceivedData(ErpPacket packet)
        {
            string msg = Encoding.UTF8.GetString(packet.Body);
            JObject jobj = new JObject(JObject.Parse(msg));
            ErpLogWriter.LogWriter.Trace(jobj.ToString());
            switch ((COMMAND)packet.Header.CMD)
            {
                case COMMAND.GetDailyList: //데이터 조회
                    MakeStatisticsChart(jobj);
                    break;

            }
        }

        private void MakeStatisticsChart(JObject jobj)
        {
            Application.Current.Dispatcher.Invoke(() => {
                ChartValues<int> Target = new ChartValues<int>();
                List<string> labelsList = new List<string>(); // Labels 배열 대신 List<string>을 사용합니다.
                int Sum = 0;
                List<Series> PieItems = new List<Series>();
                if (jobj["biz_type_statistics_list"] != null)
                {
                    if (jobj["period_type"].ToObject<int>() == 0) // 이번달
                    {
                        foreach (JObject jobj in jobj["biz_type_statistics_list"] as JArray) {
                            ChartValues<double> percent = new ChartValues<double>();
                            percent.Add(jobj["sum_rate_cost"].ToObject<int>() / 100.0);
                            Series Pie = new PieSeries()
                            {
                                PushOut=5,
                                Values = percent,
                                Title = jobj["sbt_biz_name"].ToString()
                            };
                            PieSeries.Add(Pie);
                        }
                    }
                }
                if (jobj["cost_day_list"] != null)
                {
                    LineSeries Series = null;
                    JArray innerCostDayList = jobj["cost_day_list"] as JArray;
                    foreach (JObject InnerJobj in innerCostDayList)
                    {
                        int value = InnerJobj["sum_cost"].ToObject<int>();
                        Sum = Sum + value;
                        Target.Add(Sum);
                        string date = InnerJobj["shi_time_day"].ToString(); // 날짜를 가져옵니다.
                        labelsList.Add(date); // 리스트에 날짜를 추가합니다.
                    }
                    if (jobj["period_type"].ToObject<int>() == 0) // 이번달
                    {
                        SendData(false);
                        Series = new LineSeries()
                        {
                            Values = Target,
                            Title = "이번달",
                            ScalesXAt = 0,
                            ScalesYAt = 0,
                            Stroke = Brushes.Red,
                            Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(50 , 255,0 , 0))
                        };
                    }
                    else // 지난달
                    {
                        Series = new LineSeries()
                        {
                            Values = Target,
                            Title = "지난달",
                            ScalesXAt = 0,
                            ScalesYAt = 0,
                            Stroke = Brushes.Gray,
                            Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 220, 220,220))
                        };
                    }
                    SeriesCollection.Add(Series);
                    Labels = labelsList.ToArray(); // List<string>을 배열로 변환합니다.
                    
                }
            });
            //Application.Current.Dispatcher.Invoke(() => {

            //    ChartValues<int> Target = new ChartValues<int>();
            //    int Sum = 0;
            //    //사용처별 퍼센트
            //    if (jobj["biz_type_statistics_list"] != null)
            //    {

            //    }
            //    if (jobj["cost_day_list"] != null)
            //    {
            //        LineSeries Series = null;
            //        JArray innerCostDayList = jobj["cost_day_list"] as JArray;
            //        foreach (JObject InnerJobj in innerCostDayList)
            //        {
            //            int value=jobj["sum_cost"].ToObject<int>();
            //            Sum = Sum + value;
            //            Target.Add(Sum);
            //        }
            //        if (jobj["period_type"].ToObject<int>() == 0) //이번달
            //        {
            //            SendData(false);
            //            Series = new LineSeries()
            //            {
            //                Values = Target,
            //                Title = "이번달"
            //            };
            //        }
            //        else //지난달
            //        {
            //            Series = new LineSeries()
            //            {
            //                Values = Target,
            //                Title = "지난달"
            //            };
            //        }
            //        SeriesCollection.Add(Series);
            //        for (int j = 0; j <= 31; j++) {
            //            Labels[j] = (j + 1) + "일";
            //        }
            //    }

            //});
        }

        public void OnSent()
        {
            throw new NotImplementedException();
        }
        private void SendData()
        {
            using (var network = this.ContainerProvider.Resolve<DataAgent.StatisticsDataAgent>())
            {
                network.SetReceiver(this);
                JObject jobj = new JObject();
                DateTime now = DateTime.Now;  // 현재 날짜와 시간

                jobj["period_type"] = 0;
                DateTime firstDayOfMonth = new DateTime(now.Year, now.Month, 1); // 이번 달의 1일
                jobj["start_time"] = firstDayOfMonth.ToString("yyyy-MM-dd HH:mm:ss");
                jobj["end_time"] = firstDayOfMonth.AddMonths(1).ToString("yyyy-MM-dd HH:mm:ss");
                network.GetDailyList(jobj);
            }
        }

        private void SendData(bool thisMonth) {
            using (var network = this.ContainerProvider.Resolve<DataAgent.StatisticsDataAgent>())
            {
                network.SetReceiver(this);
                JObject jobj = new JObject();
                DateTime now = DateTime.Now;  // 현재 날짜와 시간
                if (thisMonth)
                {
                    jobj["period_type"] = 0;
                    DateTime firstDayOfMonth = new DateTime(now.Year, now.Month, 1); // 이번 달의 1일
                    jobj["start_time"] = firstDayOfMonth.ToString("yyyy-MM-dd HH:mm:ss");
                    jobj["end_time"] = firstDayOfMonth.AddMonths(1).ToString("yyyy-MM-dd HH:mm:ss");
                    network.GetDailyList(jobj);
                }
                else {
                    DateTime PreviousOfMonth = new DateTime(now.Year, now.Month - 1, 1); // 저번 달의 1일
                    jobj["period_type"] = 1;
                    jobj["start_time"] = PreviousOfMonth.ToString("yyyy-MM-dd HH:mm:ss");
                    jobj["end_time"] = PreviousOfMonth.AddMonths(1).ToString("yyyy-MM-dd HH:mm:ss");
                    network.GetDailyList(jobj);
                }
            }
        }


        private int CountDaily(int year, int month) {
            
            int daysInMonth = DateTime.DaysInMonth(year, month);
            return daysInMonth;
        }
    }
}
