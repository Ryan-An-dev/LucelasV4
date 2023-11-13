using CommonModule.Views;
using DataAccess.Interface;
using DataAccess.Repository;
using DepositWithdrawal.ViewModels;
using DepositWithdrawal.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace DepositWithdrawal
{
    public class DepositWithdrawalModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterDialog<FindItemPage>("FindItemPage");
            
        }
    }
}