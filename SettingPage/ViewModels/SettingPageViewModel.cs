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
using PrsimCommonBase;
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
        public ReactiveCollection<CompanyList> CompanyInfos { get; set; }

        public DelegateCommand AddCompanyCommand { get; set; }

        public IContainerProvider ContainerProvider { get; set; }
        public IDialogService dialogService { get; }
        public SettingPageViewModel(IContainerRegistry containerProvider)
        {
            this.CustomerInfos = new ReactiveCollection<Customer>().AddTo(disposable);
            this.FurnitureInfos = new ReactiveCollection<FurnitureType>().AddTo(this.disposable);
            this.AccountInfos = new ReactiveCollection<BankModel>().AddTo(this.disposable);
            this.CategoryInfos = new ReactiveCollection<CategoryInfo>().AddTo(this.disposable);
            this.CompanyInfos = new ReactiveCollection<CompanyList>().AddTo(this.disposable);
        }
        public SettingPageViewModel(IRegionManager regionManager, IContainerProvider containerProvider, IDialogService dialogService) : base(regionManager)
        {
            this.dialogService = dialogService;
            this.ContainerProvider = containerProvider;
            this.CustomerInfos = new ReactiveCollection<Customer>().AddTo(disposable);
            this.CategoryInfos = new ReactiveCollection<CategoryInfo>().AddTo(this.disposable);
            this.AccountInfos = new ReactiveCollection<BankModel>().AddTo(this.disposable);
            this.CompanyInfos = new ReactiveCollection<CompanyList>().AddTo(this.disposable);
            this.AddCompanyCommand = new DelegateCommand(ExecAddCompanyCommand);
            initData();
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
            try
            {
                using (this.network = this.ContainerProvider.Resolve<DataAgent.SettingDataAgent>())
                {
                    network.SetReceiver(this);
                    //accountList, CategoryList, ProductList 기본으로 요청하기
                    network.GetAccountList();
                }
            }
            catch(Exception ex) { }
          
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
         
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            
        }

        public void OnRceivedData(ErpPacket packet)
        {
            string msg = Encoding.UTF8.GetString(packet.Body);
            if (!msg.Contains("null")) {
                JObject jobj = new JObject(JObject.Parse(msg));
                ErpLogWriter.LogWriter.Trace(jobj.ToString());
                switch ((COMMAND)packet.Header.CMD)
                {
                    case COMMAND.AccountLIst: //데이터 조회
                        SetAccountList(jobj);
                        break;
                    case COMMAND.ProductCategoryList:
                        //SetProductList(jobj);
                        SetProductType(jobj);
                        break;
                    case COMMAND.CategoryList:
                        SetCategoryList(jobj);
                        break;
                    case COMMAND.CustomerList:
                        SetCustomerList(jobj);
                        break;
                }
            }
        }

        private void SetProductType(JObject msg)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.FurnitureInfos.Clear();
            });
            if (msg.ToString().Trim() != string.Empty)
            {
                try {
                    if (msg["product_code_list"] == null)
                        return;
                    JArray jarr = new JArray();
                    jarr = msg["product_code_list"] as JArray;
                    foreach (JObject jobj in jarr) {
                        FurnitureType temp = new FurnitureType();
                        if (jobj["product_code"] != null)
                            temp.ProductCode.Value = msg["product_code"].ToObject<int>();
                        if (jobj["product_name"] != null)
                            temp.Name.Value = msg["product_name"].ToString();
                        this.FurnitureInfos.Add(temp);
                    }
                } catch (Exception) { }
                finally { network.GetCustomerList(); }
            }
        }

        private void SetCustomerList(JObject msg)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.CustomerInfos.Clear();
            });
            if (msg.ToString().Trim() != string.Empty)
            {
                try
                {
                    if (msg["cui_list"].ToString().Equals(""))
                        return;
                    foreach (JObject inner in msg["cui_list"] as JArray)
                    {
                        Customer temp = new Customer();
                        if (inner["cui_id"] != null)
                            temp.Id.Value = inner["cui_id"].ToObject<int>();
                        if (inner["con_name"] != null)
                            temp.Name.Value = inner["con_name"].ToString().Trim();
                        if (inner["con_phone"] != null)
                            temp.Phone.Value = inner["con_phone"].ToString().Trim();
                        if (inner["con_address"] != null)
                            temp.Address.Value = inner["con_address"].ToString().Trim(); 
                        if (inner["memo"] != null)
                            temp.Memo.Value = inner["memo"].ToString().Trim();
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            this.CustomerInfos.Add(temp);
                        });
                    }
                }
                catch (Exception ex) { }
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
            });
            if (msg.ToString().Trim() != string.Empty)
            {
                try
                {
                    if (msg["user_account"].ToString().Equals(""))
                        return;
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
                        temp.IsChecked.Value = true;
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            this.AccountInfos.Add(temp);
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
