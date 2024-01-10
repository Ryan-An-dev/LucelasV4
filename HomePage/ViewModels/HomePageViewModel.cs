using CommonModel;
using CommonModel.Model;
using DataAccess;
using DataAccess.NetWork;
using DataAgent;
using LogWriter;
using Newtonsoft.Json.Linq;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HomePage.ViewModels
{
    public class HomePageViewModel : PrismCommonViewModelBase, INavigationAware, INetReceiver
    {
        //뷰모델 부를 때마다 서버에 요청해서 데이터 가져오기 
        //뷰모델이 Dispose 될때 수정내역이 있다면 전달하자.
        public ReactiveProperty<int> Month { get; set; }

        


        private IContainerProvider ContainerProvider { get; }
        public HomePageViewModel(IRegionManager regionManager, IContainerProvider containerProvider) : base(regionManager)
        {
            this.ContainerProvider = containerProvider;
            this.Month = new ReactiveProperty<int>(1).AddTo(this.disposable);
            init();
        }
        public void init()
        {  //서버랑 통신하는 로직 넣기
            this.Month.Value = DateTime.Now.Month;

        }
        private void SendData() {
            //using (var network = this.ContainerProvider.Resolve<HomeDataAgent>())
            //{
            //    network.SetReceiver(this);
            //    JObject jobj = new JObject();
            //    jobj["date_time"] = this.Month.Value.ToString("yyyy-MM-dd-HH-mm-ss");
            //    network.GetHomeData(jobj);
            //}
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
        }

        private void SetHomeSummary(JObject msg)
        {
            if (msg["summary"]!= null && !msg["summary"].ToString().Equals("")){
                JObject inner = msg["summary"] as JObject;
                if (inner["complete"] != null && !inner["complete"].ToString().Equals("")) { 
                    
                }
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
