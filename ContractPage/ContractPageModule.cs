using ContractPage.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace ContractPage
{
    public class ContractPageModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            containerProvider.Resolve<IRegionManager>();
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }
    }
}