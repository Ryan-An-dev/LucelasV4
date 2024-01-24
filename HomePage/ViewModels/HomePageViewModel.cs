using CommonModel;
using CommonModel.Model;
using DataAccess;
using DataAccess.NetWork;
using DataAccess.Repository;
using DataAgent;
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
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;

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
        public HomePageViewModel(IRegionManager regionManager, IContainerProvider containerProvider) : base(regionManager)
        {
            HomeSummary = new ReactiveProperty<HomeSummaryModel>(new HomeSummaryModel()).AddTo(this.disposable);
            this.ContainerProvider = containerProvider;
            this.IsLoading = new ReactiveProperty<bool>(false).AddTo(this.disposable);
            
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
            
            return true;
            
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            Exit();
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            SendData();
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
                    break;
            }
            this.IsLoading.Value = false;
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
