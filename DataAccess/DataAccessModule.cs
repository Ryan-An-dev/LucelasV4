using DataAccess.NetWork;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace DataAccess
{
    public class DataAccessModule : IModule
    {
        
        public void OnInitialized(IContainerProvider containerProvider)
        {
            
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            SocketClientV2 Instance = new SocketClientV2();
            containerRegistry.RegisterInstance<SocketClientV2>(Instance);
        }
    }
}