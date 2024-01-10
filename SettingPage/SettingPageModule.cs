using CommonModule.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using SettingPage.ViewModels;
using SettingPage.Views;
using System.Threading;

namespace SettingPage
{
    public class SettingPageModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<SettingPageViewModel>();
            containerRegistry.RegisterDialog<CompanyAddPage>("CompanyAddPage");
            containerRegistry.RegisterDialog<CustomerAddPage>("CustomerAddPage");
            containerRegistry.RegisterDialog<EmployeeAddPage>("EmployeeAddPage");
            containerRegistry.RegisterDialog<ProductAddPage>("ProductAddPage");
            containerRegistry.RegisterDialog<AccountAddPage>("AccountAddPage");
            containerRegistry.RegisterDialog<ApiAddPage>("ApiAddPage");
            containerRegistry.RegisterDialog<CompanySearchList>("CompanySearchList");
            containerRegistry.RegisterDialog<ProductCategoryAddPage>("ProductCategoryAddPage");
            
        }
    }
}