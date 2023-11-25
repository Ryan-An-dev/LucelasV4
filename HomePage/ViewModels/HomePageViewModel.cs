using CommonModel;
using CommonModel.Model;
using Newtonsoft.Json.Linq;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HomePage.ViewModels
{
    public class HomePageViewModel : PrismCommonViewModelBase, INavigationAware
    {
        //뷰모델 부를 때마다 서버에 요청해서 데이터 가져오기 
        //뷰모델이 Dispose 될때 수정내역이 있다면 전달하자.
        public ReactiveProperty<int> Month { get; set; }


        public HomePageViewModel()
        {
            this.Month = new ReactiveProperty<int>(1).AddTo(this.disposable);
            init();
        }
        public void init()
        {  //서버랑 통신하는 로직 넣기
            this.Month.Value = DateTime.Now.Month;
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
         
        }
    }
}
