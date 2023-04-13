using GlobalBarModule.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace GlobalBarModule
{
    public class GlobalBarModuleModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionMan = containerProvider.Resolve<IRegionManager>();
            regionMan.RegisterViewWithRegion("GlobalRegion", typeof(Views.GlobalPage));
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {

        }
    }
}