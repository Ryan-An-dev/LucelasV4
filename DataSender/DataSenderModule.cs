using DataSender.ViewModels;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace DataSender
{
    public class DataSenderModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<DataDistributor>();
        }
    }
}