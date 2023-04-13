using HomePageModule.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace HomePageModule
{
    public class HomePageModuleModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionMan = containerProvider.Resolve<IRegionManager>();
            regionMan.RegisterViewWithRegion("ContentRegion", typeof(Views.HomePage));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }
    }
}