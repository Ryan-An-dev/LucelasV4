using CommonModule.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using SettingPage.ViewModels;
using SettingPage.Views;

namespace SettingPage
{
    public class SettingPageModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            SettingPageViewModel instance=containerProvider.Resolve<SettingPageViewModel>("GlobalData");
            instance.ContainerProvider = containerProvider;
            instance.initData();
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            SettingPageViewModel instance = new SettingPageViewModel(containerRegistry);
            containerRegistry.RegisterInstance<SettingPageViewModel>(instance,"GlobalData");
            containerRegistry.RegisterDialog<CompanyAddPage>("CompanyAddPage");
            containerRegistry.RegisterDialog<CustomerAddPage>("CustomerAddPage");
            containerRegistry.RegisterDialog<EmployeeAddPage>("EmployeeAddPage");
            containerRegistry.RegisterDialog<ProductAddPage>("ProductAddPage");
            containerRegistry.RegisterDialog<AccountAddPage>("AccountAddPage");
            containerRegistry.RegisterDialog<ApiAddPage>("ApiAddPage");
            containerRegistry.RegisterDialog<ProductCategoryAddPage>("ProductCategoryAddPage");
            
        }
    }
}