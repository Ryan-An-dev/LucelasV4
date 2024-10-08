﻿using LucelasV4.Views;
using SettingPage.Views;
using Prism.Ioc;
using Prism.Modularity;
using System.Windows;
using Prism.Regions;
using Prism.Unity;
using ContractPage.Views;
using LoginPage.Views;
using LoginPage.ViewModels;
using DataAccess.Interface;
using DataAccess.Repository;
using LogWriter;
using SettingPage;
using MESPage;
using DataAccess.NetWork;
using LucelasV4.ViewModels;
using CommonModule.Views;
using HomePage.ViewModels;
using Prism.Events;
using CommonServiceLocator;
using SettingPage.ViewModels;

namespace LucelasV4
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        IContainerRegistry regMan = null;
        IModuleCatalog moduleCatalog = null;

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void OnInitialized()
        {
            ErpLogWriter.LogWriter.Debug("============= OnInitialized =============");
            var login = Container.Resolve<Login>();
            var result = login.ShowDialog();
            if (result.Value)
            {
                LoadAllModule();
                base.OnInitialized();
                //여기서 모듈 로드 및 navigation 등록을 해야된다.
                RegisterAllNavigation();
                var regionManager = Container.Resolve<IRegionManager>();
                regionManager.RequestNavigate("ContentRegion", "HomePage");
                SocketClientV2 temp =Container.Resolve<SocketClientV2>();
                temp.SetMainConnectCheck(Container.Resolve<MainWindowViewModel>());
                Container.Resolve<MainWindowViewModel>().initLoadingTimer();
                Container.Resolve<MainWindowViewModel>().loginInfo();
                Container.Resolve<MainWindowViewModel>().initTimeLeft();
            }
        }

        public void LoadAllModule() {
            //Setting내역 내려받고
            ErpLogWriter.LogWriter.Debug("============= HomePageModule initialize =============");
            moduleCatalog.AddModule<HomePage.HomePageModule>();
            ErpLogWriter.LogWriter.Debug("============= SettingPageModule initialize =============");
            ErpLogWriter.LogWriter.Debug("============= DepositWithdrawalModule initialize =============");
            moduleCatalog.AddModule<DepositWithdrawal.DepositWithdrawalModule>();
            ErpLogWriter.LogWriter.Debug("============= ContractPageModule initialize =============");
            moduleCatalog.AddModule<ContractPage.ContractPageModule>();
            ErpLogWriter.LogWriter.Debug("============= Statictics initialize =============");
            moduleCatalog.AddModule<StatisticsPage.StatisticsPageModule>();
            ErpLogWriter.LogWriter.Debug("============= Mes initialize =============");
            moduleCatalog.AddModule<MESPage.MESPageModule>();
            ErpLogWriter.LogWriter.Debug("============= DeliveryPage initialize =============");
            moduleCatalog.AddModule<DeliveryPage.DeliveryPageModule>();
            moduleCatalog.AddModule<SettingPage.SettingPageModule>();
            this.InitializeModules();

        }

        public void RegisterAllNavigation() {
            
            regMan.RegisterForNavigation<HomePage.Views.HomePage>();
            regMan.RegisterForNavigation<SettingPage.Views.SettingPage>();
            regMan.RegisterForNavigation<ContractPage.Views.ContractPage>();
            regMan.RegisterForNavigation<ContractPage.Views.ContractSingle>();
            regMan.RegisterForNavigation<ContractPage.Views.SearchAdressPage>();
            regMan.RegisterForNavigation<ContractPage.Views.SearchNamePage>();
            regMan.RegisterForNavigation<ContractPage.Views.FindInventoryItem>();
            regMan.RegisterForNavigation<DepositWithdrawal.Views.BankListPage>();
            regMan.RegisterForNavigation<DepositWithdrawal.Views.BankListSingle>();
            regMan.RegisterForNavigation<DepositWithdrawal.Views.FindItemPage>();
            regMan.RegisterForNavigation<StatisticsPage.Views.StatisticsPage>();
            regMan.RegisterForNavigation<MESPage.Views.MesPage>();
            regMan.RegisterForNavigation<MESPage.Views.MesSingle>();
            regMan.RegisterForNavigation<DeliveryPage.Views.DeliveryPage>();
            regMan.RegisterForNavigation<DeliveryPage.Views.DeliverySinglePage>();
        }


        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            this.regMan = containerRegistry;
            containerRegistry.RegisterSingleton<IEventAggregator, EventAggregator>();
            containerRegistry.RegisterSingleton<LoginViewModel>();
            containerRegistry.RegisterSingleton<Login>();
            containerRegistry.RegisterSingleton<MainWindowViewModel>();
            containerRegistry.RegisterSingleton<HomePageViewModel>();
            containerRegistry.RegisterSingleton<SettingPageViewModel>();
            containerRegistry.Register<IReceiptRepository, ReceiptRepository>();
            containerRegistry.Register<IStatisticsRepository, StatisticsRepository>();
            containerRegistry.Register<IHomePageRepository, HomePageRepository>();
            containerRegistry.Register<IContractRepository, ContractRepository>();
            containerRegistry.Register<ILoginRepository, LoginRepository>();
            containerRegistry.Register<ISettingRepository, SettingRepository>();
            containerRegistry.Register<ICustomerRepository, CustomerRepository>();
            containerRegistry.Register<IInventoryRepository, InventoryRepository>();
            containerRegistry.Register<IDeliveryRepository, DeliveryRepository>();
            containerRegistry.Register<IProductRepository, ProductRepository>();
            containerRegistry.Register<ICompanyRepository, CompanyRepository>();
            containerRegistry.Register<IEmployeeRepository, EmployeeRepository>();
            containerRegistry.Register<IProductCategoryRepository, ProductCategoryRepository>();
            containerRegistry.Register<IAccountRepository, AccountRepository>();
            containerRegistry.Register<IApiRepository, APIRepository>();


            containerRegistry.RegisterDialogWindow<CommonDialogWindow>("CommonDialogWindow");
            containerRegistry.RegisterDialogWindow<AlertWindow1>("AlertWindow");
            containerRegistry.RegisterDialogWindow<AlertWindow2>("AlertWindow2");
            containerRegistry.RegisterDialogWindow<AlertWindowBasic>("AlertWindowBasic");
            containerRegistry.RegisterDialog<SearchAdressPage>("SearchAdressPage");
            containerRegistry.RegisterDialog<SearchNamePage>("SearchNamePage");
            containerRegistry.RegisterDialog<AddPaymentPage>("AddPaymentPage");
            containerRegistry.RegisterDialog<CompletedPaymentPage>("CompletedPaymentPage");

            containerRegistry.RegisterDialog<PaymentCardAddPage>("PaymentCardAddPage");
        }
        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            ErpLogWriter.CreateLogger(LogType.Page);
            this.moduleCatalog = moduleCatalog;
            ErpLogWriter.LogWriter.Debug("============= DataAccessModule initialize =============");
            moduleCatalog.AddModule<DataAccess.DataAccessModule>();
            ErpLogWriter.LogWriter.Debug("============= LoginPageModule initialize =============");
            moduleCatalog.AddModule<LoginPage.LoginPageModule>();
            base.ConfigureModuleCatalog(moduleCatalog);
        }
    }
}
