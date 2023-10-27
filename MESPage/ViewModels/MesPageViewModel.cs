using DataAccess;
using DataAccess.NetWork;
using Newtonsoft.Json.Linq;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using PrsimCommonBase;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MESPage.ViewModels
{
    public enum MovePageType { Next = 1, Prev }
    public class MesPageViewModel : PrismCommonViewModelBase, INavigationAware, INetReceiver
    {
        
        public MesPageViewModel()
        {

        }


        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return false;
        }

        public void OnConnected()
        {
            
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            
        }

        public void OnRceivedData(ErpPacket packet)
        {
            
        }

        public void OnSent()
        {
            
        }
    }
}
