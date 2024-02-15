using CommonModel.Model;
using DataAccess;
using DataAccess.NetWork;
using DataAgent;
using LogWriter;
using Newtonsoft.Json.Linq;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using CommonModel;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Interop;

namespace SettingPage.ViewModels
{
    public class SettingPageViewModel : PrismCommonViewModelBase, INavigationAware, INetReceiver
    {
        private SettingDataAgent network { get; set; }
        public ReactiveCollection<FurnitureType> FurnitureInfos { get; set; }
        public ReactiveCollection<CategoryInfo> CategoryInfos { get; set; }
        public ReactiveCollection<BankModel> AccountInfos { get; set; }
        public ReactiveCollection<Customer> CustomerInfos { get; set; }
        public ReactiveCollection<Employee> EmployeeInfos { get; set; }
        public ReactiveCollection<PayCardType> PayCardTypeInfos { get; set; }

        public DelegateCommand AddCompanyCommand { get; set; }

        public CustomerListViewModel CustomerListViewModel { get; set; }

        public EmployeeListViewModel EmployeeListViewModel { get; set; }
        public ProductCategoryListViewModel ProductCategoryListViewModel { get; set; }

        public CompanyListViewModel CompanyListViewModel { get; set; }

        public ProductListViewModel ProductListViewModel { get; set; }
        public AccountListViewModel AccountListViewModel { get; set; }

        public PaymentCardListViewModel PaymentCardListViewModel { get; set; }



        public ApiListViewModel ApiListViewModel { get; set; }

        public ReactiveProperty<Visibility> SearchVisibility { get; set; }

        public IContainerProvider ContainerProvider { get; set; }
        public IDialogService dialogService { get; }
        public ReactiveProperty<bool> IsLoading { get; set; } //로딩

        private void initViewModel() {
            ProductCategoryListViewModel = new ProductCategoryListViewModel(ContainerProvider, regionManager, dialogService);
            EmployeeListViewModel = new EmployeeListViewModel(ContainerProvider, regionManager, dialogService);
            CustomerListViewModel = new CustomerListViewModel(ContainerProvider, regionManager, dialogService);
            CompanyListViewModel = new CompanyListViewModel(ContainerProvider, regionManager, dialogService);
            ProductListViewModel = new ProductListViewModel(ContainerProvider, regionManager, dialogService);
            AccountListViewModel = new AccountListViewModel(ContainerProvider, regionManager, dialogService);
            ApiListViewModel = new ApiListViewModel(ContainerProvider, regionManager, dialogService);
            PaymentCardListViewModel = new PaymentCardListViewModel(ContainerProvider, regionManager, dialogService);

        }
        public SettingPageViewModel()
        {
            this.CustomerInfos = new ReactiveCollection<Customer>().AddTo(disposable);
            this.FurnitureInfos = new ReactiveCollection<FurnitureType>().AddTo(this.disposable);
            this.AccountInfos = new ReactiveCollection<BankModel>().AddTo(this.disposable);
            this.CategoryInfos = new ReactiveCollection<CategoryInfo>().AddTo(this.disposable);
            this.EmployeeInfos = new ReactiveCollection<Employee>().AddTo(this.disposable);
            this.PayCardTypeInfos = new ReactiveCollection<PayCardType>().AddTo(disposable);
            this.SearchVisibility = new ReactiveProperty<Visibility>(Visibility.Collapsed);
            this.IsLoading = new ReactiveProperty<bool>(true).AddTo(disposable);
            this.IsLoading.Subscribe(x => OnLoadingChanged(x));
            

        }
        public SettingPageViewModel(IRegionManager regionManager, IContainerProvider containerProvider, IDialogService dialogService) : base(regionManager)
        {
            this.SearchVisibility = new ReactiveProperty<Visibility>(Visibility.Collapsed);
            this.dialogService = dialogService;
            this.ContainerProvider = containerProvider;
            this.PayCardTypeInfos = new ReactiveCollection<PayCardType>().AddTo(disposable);
            this.EmployeeInfos = new ReactiveCollection<Employee>().AddTo(this.disposable);
            this.FurnitureInfos = new ReactiveCollection<FurnitureType>().AddTo(this.disposable);
            this.CustomerInfos = new ReactiveCollection<Customer>().AddTo(disposable);
            this.CategoryInfos = new ReactiveCollection<CategoryInfo>().AddTo(this.disposable);
            this.AccountInfos = new ReactiveCollection<BankModel>().AddTo(this.disposable);
            this.AddCompanyCommand = new DelegateCommand(ExecAddCompanyCommand);
            this.IsLoading = new ReactiveProperty<bool>(true).AddTo(disposable);
            this.IsLoading.Subscribe(x => OnLoadingChanged(x));
            initViewModel();
            initData();
        }
        private void OnLoadingChanged(bool isLoading)
        {
            SearchVisibility.Value = isLoading ? Visibility.Visible : Visibility.Collapsed;
        }
        private void ExecAddCompanyCommand()
        {
            this.dialogService.ShowDialog("CompanyAddPage", null, r => 
            {
                try {
                    if (r.Result == ButtonResult.OK)
                    {
                        //CompanyList item = r.Parameters.GetValue<CompanyList>("Company");
                        //this.CompanyInfos.Add(item);
                    }
                } catch (Exception) { }
                
            }, "CommonDialogWindow");
        }

        public void initData()
        {
            IsLoading.Value = true;
            try
            {
                using (this.network = this.ContainerProvider.Resolve<DataAgent.SettingDataAgent>())
                {
                    network.SetReceiver(this);
                    //accountList, CategoryList, ProductList 기본으로 요청하기
                    network.GetAccountList();
                }
            }
            catch (Exception ex) { }

        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return false;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
         
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            initData();
        }

        public void OnRceivedData(ErpPacket packet)
        {
            string msg = Encoding.UTF8.GetString(packet.Body);
            if (!msg.Contains("null")) {
                JObject jobj = new JObject(JObject.Parse(msg));
                ErpLogWriter.LogWriter.Trace((COMMAND)packet.Header.CMD+ " \r\n " +jobj.ToString());
                try
                {
                    switch ((COMMAND)packet.Header.CMD)
                    {
                        case COMMAND.AccountLIst: //데이터 조회
                            SetAccountList(jobj);
                            break;
                        case COMMAND.ProductCategoryList:
                            SetProductType(jobj);
                            break;
                        case COMMAND.CategoryList:
                            SetCategoryList(jobj);
                            break;
                        case COMMAND.GETCUSTOMERINFO:
                            SetCustomerList(jobj);
                            break;
                        case COMMAND.GETEMPLOEEINFO:
                            SetEmployeeList(jobj);
                            break;
                        case COMMAND.GETCOMPANYINFO:
                            SetCompanyList(jobj);
                            break;
                        case COMMAND.GETPRODUCTINFO:
                            SetProductList(jobj);
                            break;
                        case COMMAND.READ_API_INFO:
                            SetApiList(jobj);
                            break;

                        case COMMAND.GetCardTypeList:
                            SetPayCardType(jobj);
                            break;
                    }
                } catch (Exception e) { }
               
            }
        }

        private void SetApiList(JObject msg)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (this.ApiListViewModel != null)
                    this.ApiListViewModel.List.Clear();
            });
            if (msg.ToString().Trim() != string.Empty)
            {
                try
                {
                    if (msg["api_list"] == null)
                        return;
                    JArray jarr = new JArray();
                    jarr = msg["api_list"] as JArray;
                    if (msg["history_count"] != null)
                        this.ApiListViewModel.TotalItemCount.Value = msg["history_count"].ToObject<int>();
                    int i = 1;
                    foreach (JObject jobj in jarr)
                    {
                        API temp = new API();
                        temp.No.Value = i++;
                        if (jobj["api_id"] != null)
                            temp.Id.Value = jobj["api_id"].ToObject<int>();
                        if (jobj["api_type"] != null)
                            temp.Type.Value = jobj["api_type"].ToObject<APIType>();
                        if (jobj["api_key"] != null)
                            temp.ApiKey.Value = jobj["api_key"].ToString();
                        if (jobj["api_account"] != null)
                            temp.ApiID.Value = jobj["api_account"].ToString();
                        if (jobj["api_cert_num"] != null)
                            temp.CertNum.Value = jobj["api_cert_num"].ToString();
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            this.ApiListViewModel.List.Add(temp);
                        });
                    }
                }
                catch (Exception e) { LogWriter.ErpLogWriter.LogWriter.Debug(e.ToString()); }
                finally {
                    if (this.CustomerListViewModel.ContainerProvider != null)
                    {
                        this.CustomerListViewModel.SendBasicData(this);
                    }
                }
            }
        }

        private void SetProductList(JObject msg)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (this.ProductListViewModel != null)
                    this.ProductListViewModel.List.Clear();
            });
            if (msg.ToString().Trim() != string.Empty)
            {
                try
                {
                    if (msg["product_list"] == null)
                        return;
                    JArray jarr = new JArray();
                    jarr = msg["product_list"] as JArray;
                    if (msg["history_count"] != null)
                        this.ProductListViewModel.TotalItemCount.Value = msg["history_count"].ToObject<int>();
                    int i = 1;
                    foreach (JObject jobj in jarr)
                    {
                        Product inventory = new Product();
                        inventory.No.Value = i++;
                        if (jobj["company"] != null)
                            inventory.Company.Value = SetCompanyInfo(jobj["company"] as JObject);
                        if (jobj["product_type"] != null)
                            inventory.ProductType.Value = FurnitureInfos.FirstOrDefault(x => x.Id.Value == jobj["product_type"].ToObject<int>());
                        if (jobj["product_name"] != null)
                            inventory.Name.Value = jobj["product_name"].ToString();
                        if (jobj["product_price"] != null)
                            inventory.Price.Value = jobj["product_price"].ToObject<int>();
                        if (jobj["product_id"] != null)
                            inventory.Id.Value = jobj["product_id"].ToObject<int>();
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            this.ProductListViewModel.List.Add(inventory);
                        });

                    }
                }
                catch (Exception e) { LogWriter.ErpLogWriter.LogWriter.Debug(e.ToString()); }
                finally
                {
                    if (this.ApiListViewModel.ContainerProvider != null)
                    {
                        this.ApiListViewModel.SendBasicData(this);
                    }
                }
            }
        }

        private Company SetCompanyInfo(JObject jobj)
        {
            Company temp = new Company();
            if (jobj["company_name"] != null)
                temp.CompanyName.Value = jobj["company_name"].ToString();
            if (jobj["company_phone"] != null)
                temp.CompanyPhone.Value = jobj["company_phone"].ToString();
            if (jobj["company_id"] != null)
                temp.Id.Value = jobj["company_id"].ToObject<int>();
            if (jobj["company_address"] != null)
                temp.CompanyAddress.Value = jobj["company_address"].ToString();
            return temp;

        }

        private void SetCompanyList(JObject msg)
        {
            
            if (msg.ToString().Trim() != string.Empty)
            {
                try
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (this.CompanyListViewModel != null)
                            this.CompanyListViewModel.List.Clear();
                    });
                    if (msg["company_list"] == null)
                        return;
                    JArray jarr = new JArray();
                    jarr = msg["company_list"] as JArray;
                    if (msg["history_count"] != null)
                        this.CompanyListViewModel.TotalItemCount.Value = msg["history_count"].ToObject<int>();
                    int i = 1;
                    foreach (JObject jobj in jarr) {
                        Company temp = new Company();
                        temp.No.Value = i++;
                        if (jobj["company_name"] != null)
                            temp.CompanyName.Value = jobj["company_name"].ToString();
                        if (jobj["company_phone"] != null)
                            temp.CompanyPhone.Value = jobj["company_phone"].ToString();
                        if (jobj["company_id"] != null)
                            temp.Id.Value = jobj["company_id"].ToObject<int>();
                        if (jobj["company_address"] != null)
                            temp.CompanyAddress.Value = jobj["company_address"].ToString();
                        if (jobj["company_address_detail"] != null)
                            temp.CompanyAddressDetail.Value = jobj["company_address_detail"].ToString();

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            this.CompanyListViewModel.List.Add(temp);
                        });
                    }
                    
                }
                catch (Exception e) { LogWriter.ErpLogWriter.LogWriter.Debug(e.ToString()); }
                finally {
                    if (this.ProductListViewModel.ContainerProvider != null) {
                        this.ProductListViewModel.SendBasicData(this);
                    }
                }
            }
        }
        private void SetPayCardType(JObject msg) {
            if (msg.ToString().Trim() != string.Empty)
            {
                try
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (this.PaymentCardListViewModel != null)
                            this.PaymentCardListViewModel.List.Clear();
                        PayCardTypeInfos.Clear();
                    });
                    if (msg["card_company_list"] == null)
                        return;
                    JArray jarr = new JArray();
                    jarr = msg["card_company_list"] as JArray;
                    if (msg["history_count"] != null)
                    {
                        if (this.PaymentCardListViewModel != null)
                            this.PaymentCardListViewModel.TotalItemCount.Value = msg["history_count"].ToObject<int>();
                    }
                    int i = 1;
                    foreach (JObject jobj in jarr)
                    {
                        PayCardType temp = new PayCardType();
                        temp.No.Value = i++;
                        if (jobj["card_id"] != null)
                            temp.Id.Value = jobj["card_id"].ToObject<int>();
                        if (jobj["card_company_name"] != null)
                            temp.Name.Value = jobj["card_company_name"].ToString();
                        
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (PaymentCardListViewModel != null)
                                this.PaymentCardListViewModel.List.Add(temp);
                            PayCardTypeInfos.Add(temp);
                        });

                    }
                }
                catch (Exception e) { LogWriter.ErpLogWriter.LogWriter.Debug(e.ToString()); }
                finally
                {
                   
                    if (this.CompanyListViewModel != null)
                    {
                        this.CompanyListViewModel.SendBasicData(this);
                    }
                }
            }
        }

        private void SetEmployeeList(JObject msg)
        {
            
            if (msg.ToString().Trim() != string.Empty)
            {
                try
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if(this.EmployeeListViewModel != null)
                            this.EmployeeListViewModel.List.Clear();
                        //EmployeeInfos.Clear();
                    });
                    if (msg["employee_list"] == null)
                        return;
                    JArray jarr = new JArray();
                    jarr = msg["employee_list"] as JArray;
                    if (msg["history_count"] != null) { 
                        if(this.EmployeeListViewModel != null)
                            this.EmployeeListViewModel.TotalItemCount.Value = msg["history_count"].ToObject<int>();
                    } 
                    int i = 1;
                    foreach (JObject jobj in jarr)
                    {
                        Employee temp = new Employee();
                        temp.No.Value = i++;
                        if (jobj["employee_id"] != null)
                            temp.Id.Value = jobj["employee_id"].ToObject<int>();
                        if (jobj["employee_name"] != null)
                            temp.Name.Value = jobj["employee_name"].ToString();
                        if (jobj["employee_phone"] != null)
                            temp.Phone.Value = jobj["employee_phone"].ToString();
                        if (jobj["employee_start"] != null)
                            temp.StartWorkTime.Value = jobj["employee_start"].ToObject<DateTime>();
                        if (jobj["employee_address"] != null)
                            temp.Address.Value = jobj["employee_address"].ToString();
                        if (jobj["employee_address_detail"] != null)
                            temp.AddressDetail.Value = jobj["employee_address_detail"].ToString();
                        if (jobj["employee_memo"] != null)
                            temp.Memo.Value = jobj["employee_memo"].ToString();
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if(EmployeeListViewModel != null)
                                this.EmployeeListViewModel.List.Add(temp);
                            EmployeeInfos.Add(temp);
                        });

                    }
                }
                catch (Exception e) { LogWriter.ErpLogWriter.LogWriter.Debug(e.ToString()); }
                finally {
                    JObject jobj = new JObject();
                    jobj["page_unit"] = 30;
                    jobj["page_start_pos"] = 0;
                    jobj["all_mode"] = 1;
                    network.GetCardTypeList(jobj);
                }
            }
        }

        private void SetProductType(JObject msg)
        {
           
            if (msg.ToString().Trim() != string.Empty)
            {
                try
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (this.ProductCategoryListViewModel != null)
                            this.ProductCategoryListViewModel.List.Clear();
                        this.FurnitureInfos.Clear();
                    });
                    if (msg["category_list"] == null)
                        return;
                    if (msg["history_count"] != null) {
                        if (ProductCategoryListViewModel != null)
                        {
                            this.ProductCategoryListViewModel.TotalItemCount.Value = msg["history_count"].ToObject<int>();
                        }
                    }
                       
                    JArray jarr = new JArray();
                    jarr = msg["category_list"] as JArray;
                    
                    int i = 1;
                    foreach (JObject jobj in jarr) {
                        FurnitureType temp = new FurnitureType();
                        if (jobj["product_type_id"] != null)
                            temp.Id.Value = jobj["product_type_id"].ToObject<int>();
                        if (jobj["product_type_name"] != null)
                            temp.Name.Value = jobj["product_type_name"].ToString();
                        temp.No.Value = i++;
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (ProductCategoryListViewModel != null) {

                                this.ProductCategoryListViewModel.List.Add(temp);
                            }
                            this.FurnitureInfos.Add(temp);
                        });
                      
                    }
                } catch (Exception e) { LogWriter.ErpLogWriter.LogWriter.Debug(e.ToString());  }
                finally {
                    JObject jobj = new JObject();
                    jobj["page_unit"] = 30;
                    jobj["page_start_pos"] = 0;
                    jobj["all_mode"] = 1;
                    network.GetEmployeeList(jobj);
                    
                }
            }
        }

        private void SetCustomerList(JObject msg)
        {
            
            if (msg.ToString().Trim() != string.Empty)
            {
                try
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (this.CustomerListViewModel != null)
                            this.CustomerListViewModel.List.Clear();
                    });
                    if (msg["customer_list"] == null)
                        return;
                    if (msg["history_count"] != null)
                        this.CustomerListViewModel.TotalItemCount.Value = msg["history_count"].ToObject<int>();
                    int i = 1;
                    foreach (JObject inner in msg["customer_list"] as JArray)
                    {
                        Customer temp = new Customer();
                        temp.No.Value = i++;
                        if (inner["cui_id"] != null)
                            temp.Id.Value = inner["cui_id"].ToObject<int>();
                        if (inner["cui_name"] != null)
                            temp.Name.Value = inner["cui_name"].ToString().Trim();
                        if (inner["cui_phone_num"] != null)
                            temp.Phone.Value = inner["cui_phone_num"].ToString().Trim();
                        if (inner["cui_address"] != null)
                            temp.Address.Value = inner["cui_address"].ToString().Trim();
                        if (inner["cui_address_detail"] != null)
                            temp.Address1.Value = inner["cui_address_detail"].ToString().Trim();
                        if (inner["cui_memo"] != null)
                            temp.Memo.Value = inner["cui_memo"].ToString().Trim();
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            this.CustomerListViewModel.List.Add(temp);
                        });
                    }
                }
                catch (Exception ex) { }
                finally {
                    IsLoading.Value = false;
                }
            }
        }
        private void SetCategoryList(JObject msg)
        {
            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    this.CategoryInfos.Clear();
                });
            } catch (Exception) { }
           
            if (msg.ToString().Trim() != string.Empty)
            {
                try
                {
                    if (msg["category_list"].ToString().Equals(""))
                        return;
                    foreach (JObject inner in msg["category_list"] as JArray)
                    {
                        CategoryInfo temp = new CategoryInfo();
                        if (inner["sbt_biz_name"] != null)
                            temp.OriginName.Value = inner["sbt_biz_name"].ToString().Trim();
                        if (inner["sbt_name"] != null)
                            temp.Name.Value = inner["sbt_name"].ToString().Trim();
                        if (inner["sbt_id"] != null)
                            temp.CategoryId.Value = inner["sbt_id"].ToObject<int>();
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            this.CategoryInfos.Add(temp);
                        });
                    }
                }
                catch (Exception ex) { }
                finally {    
                    network.GetProductCategory();
                }
            }
        }

        private void SetAccountList(JObject msg)
        {
            
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.AccountInfos.Clear();
                if (AccountListViewModel != null)
                    this.AccountListViewModel.List.Clear();
            });
            if (msg.ToString().Trim() != string.Empty)
            {
                try
                {
                    if (AccountListViewModel != null)
                        if (msg["history_count"] != null)
                            this.AccountListViewModel.TotalItemCount.Value = msg["history_count"].ToObject<int>();
                    if (msg["user_account"].ToString().Equals(""))
                        return;
                    int i = 1;
                    foreach (JObject inner in msg["user_account"] as JArray)
                    {
                        BankModel temp = new BankModel();
                        if (inner["account_name"] != null)
                            temp.Name.Value = inner["account_name"].ToObject<string>().Trim();
                        if (inner["account_num"] != null)
                            temp.AccountNum.Value = inner["account_num"].ToString().Trim();
                        if (inner["account_serial"] != null)
                            temp.AccountSerial.Value = inner["account_serial"].ToObject<int>();
                        if (inner["account_type"] != null)
                            temp.Type.Value = (BankType)inner["account_type"].ToObject<int>();
                        try {
                            if (inner["last_update"] != null)
                                temp.LastUpdate.Value = inner["last_update"].ToObject<DateTime>();
                        }
                        catch(Exception e){
                            temp.LastUpdate.Value = null;
                        }
                        
                        temp.IsChecked.Value = true;
                        temp.No.Value = i++;
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            this.AccountInfos.Add(temp);
                            if (AccountListViewModel != null)
                                this.AccountListViewModel.List.Add(temp);
                        });
                    }
                }
                catch (Exception ex) { }
                finally { network.GetCategory(); }
            }
        }

        public void OnConnected()
        {
            throw new NotImplementedException();
        }

        public void OnSent()
        {
            throw new NotImplementedException();
        }

       
    }
}
