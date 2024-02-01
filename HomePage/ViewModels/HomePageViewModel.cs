using CommonModel;
using CommonModel.Model;
using DataAccess;
using DataAccess.NetWork;
using DataAccess.Repository;
using DataAgent;
using LiveCharts;
using LiveCharts.Wpf;
using LogWriter;
using Newtonsoft.Json.Linq;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SettingPage.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace HomePage.ViewModels
{
    public class HomePageViewModel : PrismCommonViewModelBase, INavigationAware, INetReceiver
    {
        //뷰모델 부를 때마다 서버에 요청해서 데이터 가져오기 
        //뷰모델이 Dispose 될때 수정내역이 있다면 전달하자.

        public ReactiveCommand<string> MenuSelectCommand { get; set; }
        public ReactiveProperty<HomeSummaryModel> HomeSummary { get; set; }
        public ReactiveProperty<bool> IsLoading { get; set; }
        private IContainerProvider ContainerProvider { get; }
        public ChartValues<int> ProfitDailyData { get; set; }
        public ChartValues<int> SalesDailyData { get; set; }
        public ChartValues<int> CostDailyData { get; set; }
        public ChartValues<string> ProfitDailyDate { get; set; }
        public ChartValues<int> ProfitData { get; set; }
        public ChartValues<int> SalesData { get; set; }
        public ChartValues<string> ProfitDate { get; set; }
        public ReactiveProperty<int> MaxDaily { get; set; }
        public ReactiveProperty<int> MaxMonthly { get; set; }

        public ChartValues<string> ComparisonDate { get; set; }

        public ReactiveProperty<int> MaxComparison { get; set; }

        public ChartValues<int> ComparisonPreviousData { get; set; }
        public ChartValues<int> ComparisonPresentData { get; set; }

        public ReactiveProperty<DateTime> Date { get; set; }

        public HomePageViewModel(IRegionManager regionManager, IContainerProvider containerProvider) : base(regionManager)
        {
            ComparisonPreviousData = new ChartValues<int>();
            ComparisonPresentData = new ChartValues<int>();
            MaxComparison = new ReactiveProperty<int>().AddTo(this.disposable);
            ComparisonDate = new ChartValues<string>();
            CostDailyData = new ChartValues<int>();
            SalesData = new ChartValues<int>();
            MaxMonthly = new ReactiveProperty<int>(0).AddTo(this.disposable);
            Date = new ReactiveProperty<DateTime>(DateTime.Now).AddTo(this.disposable);
            MaxDaily = new ReactiveProperty<int>(0).AddTo(this.disposable);
            SalesDailyData = new ChartValues<int>();
            ProfitDailyData = new ChartValues<int>();
            ProfitDailyDate = new ChartValues<string>();
            ProfitDate = new ChartValues<string>();
            ProfitData = new ChartValues<int>();
            HomeSummary = new ReactiveProperty<HomeSummaryModel>(new HomeSummaryModel()).AddTo(this.disposable);
            this.ContainerProvider = containerProvider;
            this.IsLoading = new ReactiveProperty<bool>(false).AddTo(this.disposable);

            
            Timer();
        }
        private void Timer() {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) => {
                Date.Value = DateTime.Now;
            };
            timer.Start();
        }

        private void SendData() {
            this.IsLoading.Value = true;
            using (var network = this.ContainerProvider.Resolve<HomeDataAgent>())
            {
                network.SetReceiver(this);
                JObject jobj = new JObject();
                jobj["date_time"] = HomeSummary.Value.Month.Value.ToString("yyyy-MM-dd 23:59:59");
                network.GetHomeData(jobj);
            }
        }

        public void Exit() {

            //변경된 내용 다시 Save 시키는 로직 넣기
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {

            return false;

        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            Exit();
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            SendData();
        }
        private void SendData(int mode, int count)
        {
            using (var network = this.ContainerProvider.Resolve<StatisticsDataAgent>())
            {
                network.SetReceiver(this);
                JObject jobject = new JObject();
                jobject["get_mode"] = mode;
                jobject["select_count"] = count;
                network.GetDailyList(jobject);
            }
        }

        public void OnRceivedData(ErpPacket packet)
        {
            string msg = Encoding.UTF8.GetString(packet.Body);
            JObject jobj = new JObject(JObject.Parse(msg));
            ErpLogWriter.LogWriter.Trace(jobj.ToString());
            switch ((COMMAND)packet.Header.CMD)
            {
                case COMMAND.GETHOMESUMMARY: //데이터 조회
                    SetHomeSummary(jobj);
                    SendData(1, 12);
                    break;
                case COMMAND.GetDailyList:
                    SetHomeStatisticList(jobj);

                    break;
                case COMMAND.GetPreviousDailyList:
                    SetPreviousDailyList(jobj);
                    break;
            }
        }

        private void SetPreviousDailyList(JObject jobj) {
            MaxComparison.Value = 1000;
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.ComparisonDate.Clear();
                this.ComparisonPreviousData.Clear();
                this.ComparisonPresentData.Clear();
            });
            for (int i = 1; i < 32; i++)
            {
                ComparisonDate.Add(i.ToString() + "일");
            }
            if (jobj["befor_summary_list"] != null)
            {
                JArray jarray = jobj["befor_summary_list"] as JArray;
                foreach (JObject inner in jarray)
                {
                    int Sum = 0;
                    if (inner["sum_con_sales"] != null) { 
                        Sum += int.Parse(inner["sum_con_sales"].ToString());
                        this.ComparisonPreviousData.Add(Sum);
                        MaxComparison.Value = Sum;
                    }
                    
                }
            }
            if (jobj["now_summary_list"] != null)
            {
                JArray jarray = jobj["now_summary_list"] as JArray;
                foreach (JObject inner in jarray)
                {
                    int Sum = 0;
                    if (inner["sum_con_sales"] != null)
                    {
                        Sum += int.Parse(inner["sum_con_sales"].ToString());
                        this.ComparisonPresentData.Add(Sum);
                        MaxComparison.Value = Sum;
                    }
                }
            }
            this.IsLoading.Value = false;
        }

        public Func<double, string> YFormatter
        {
            get
            {
                return value => (value / 10000).ToString("C0", CultureInfo.CreateSpecificCulture("ko-KR")) + "만원";
            }
        }

        private void SetHomeStatisticList(JObject jobj)
        {
            if (jobj["get_mode"] != null) {
                switch (jobj["get_mode"].ToObject<int>())
                {
                    case 1:
                        SetMonthlyList(jobj);
                        break;
                    case 2:
                        SetDailyList(jobj);
                        break;
                }
            }
        }

        private void SetDailyList(JObject jobj)
        {
            MaxDaily.Value = 0;
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.ProfitDailyData.Clear();
                this.ProfitDailyDate.Clear();
                this.SalesDailyData.Clear();
                this.CostDailyData.Clear();
            });
            if (jobj["summary_list"] != null)
            {
                JArray jarray = jobj["summary_list"] as JArray;
                foreach (JObject inner in jarray)
                {
                    if (inner["ssi_con_profits"] != null)
                        this.ProfitDailyData.Add(int.Parse(inner["ssi_con_profits"].ToString()));
                    if (inner["sum_con_sales"] != null) {
                        if (int.Parse(inner["sum_con_sales"].ToString()) >= MaxDaily.Value) { 
                            MaxDaily.Value = int.Parse(inner["sum_con_sales"].ToString())*2;
                        }
                        this.SalesDailyData.Add(int.Parse(inner["sum_con_sales"].ToString()));
                    }
                    if (inner["date_time"] != null)
                    {
                        DateTime dateTime = inner["date_time"].ToObject<DateTime>();
                        this.ProfitDailyDate.Add(dateTime.ToString("dd") + dateTime.ToString("ddd"));
                    }
                    this.CostDailyData.Add(int.Parse(inner["sum_con_sales"].ToString()) - int.Parse(inner["ssi_con_profits"].ToString()));
                }
            }
            using (var network = this.ContainerProvider.Resolve<StatisticsDataAgent>())
            {
                network.SetReceiver(this);
                JObject jobject = new JObject();
                jobject["get_mode"] = DateTime.Now.Month;
                network.GetComparisonList(jobject);
            }
        }

        private void SetMonthlyList(JObject jobj)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.ProfitData.Clear();
                this.ProfitDate.Clear();
                this.SalesData.Clear();
            });
            if (jobj["summary_list"] != null)
            {
                JArray jarray = jobj["summary_list"] as JArray;
                foreach (JObject inner in jarray)
                {
                    if (inner["ssi_con_profits"] != null)
                        this.ProfitData.Add(int.Parse(inner["ssi_con_profits"].ToString()));
                    if (inner["sum_con_sales"]!=null)
                    {
                        if (int.Parse(inner["sum_con_sales"].ToString()) >= MaxMonthly.Value)
                        {
                            MaxMonthly.Value = int.Parse(inner["sum_con_sales"].ToString()) * 2;
                        }
                        this.SalesData.Add(int.Parse(inner["sum_con_sales"].ToString()));
                    }
                    if (inner["date_time"] != null)
                    {
                        DateTime dateTime = inner["date_time"].ToObject<DateTime>();
                        this.ProfitDate.Add(dateTime.ToString("yy") + "년 " + dateTime.ToString("MM") + "월");
                    }
                }
            }
            SendData(2, 7);
        }

        private void SetHomeSummary(JObject msg)
        {
            if (msg["summary"]!= null && !msg["summary"].ToString().Equals("")){
                JObject inner = msg["summary"] as JObject;
                
                if (inner["complete"] != null && !inner["complete"].ToString().Equals("")) { 
                    if (inner["complete"]["contract_count"]!=null)
                        HomeSummary.Value.CompleteContract.Value = int.Parse(inner["complete"]["contract_count"].ToString());
                    if (inner["complete"]["contract_count"] != null)
                        HomeSummary.Value.CompleteDistribute.Value = int.Parse(inner["complete"]["payment_count"].ToString());
                    if (inner["complete"]["contract_count"] != null)
                        HomeSummary.Value.CompleteDelevery.Value = int.Parse(inner["complete"]["delivery_count"].ToString());
                }
                if (inner["incomplete"] != null && !inner["incomplete"].ToString().Equals(""))
                {
                    if (inner["incomplete"]["contract_count"] != null)
                        HomeSummary.Value.NotCompleteContract.Value = int.Parse(inner["incomplete"]["contract_count"].ToString());
                    if (inner["incomplete"]["contract_count"] != null)
                        HomeSummary.Value.NotCompleteDistribute.Value = int.Parse(inner["incomplete"]["payment_count"].ToString());
                    if (inner["incomplete"]["contract_count"] != null)
                        HomeSummary.Value.NotCompleteDelivery.Value = int.Parse(inner["incomplete"]["delivery_count"].ToString());
                }
                if (inner["today_delivery_count"]!=null)
                    HomeSummary.Value.TodayDelevery.Value = int.Parse(inner["today_delivery_count"].ToString());
                if (inner["delivery_finalize"]!=null)
                    HomeSummary.Value.DeliveryUnFinalizeCount.Value = int.Parse(inner["delivery_finalize"].ToString());
                if (inner["not_order_count"]!=null)
                    HomeSummary.Value.NotOrderCount.Value = int.Parse(inner["not_order_count"].ToString());
            }
        }

        public void OnConnected()
        {
            
        }

        public void OnSent()
        {
            
        }
    }
}
