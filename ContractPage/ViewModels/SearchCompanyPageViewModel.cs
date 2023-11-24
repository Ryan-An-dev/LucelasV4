using AddressSearchManager.Models;
using CommonModel;
using CommonModel.Model;
using DataAccess;
using DataAccess.NetWork;
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
using SettingPage.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace ContractPage.ViewModels
{
    public class SearchCompanyPageViewModel : PrismCommonViewModelBase, INavigationAware, IDisposable, INetReceiver, IDialogAware
    {
        [TypeConverter(typeof(EnumDescriptionTypeConverter))]
        public enum CompanyProductSelect
        {
            [Description("제품명")]
            ProductName = 0,
            [Description("회사명")]
            CompanyName = 1,
        }
        public DelegateCommand<Company> RowDoubleClick { get; set; } 


        public ReactiveCollection<FurnitureType> FurnitureInfos { get; set; } //재품종류리스트가 가지고옴
        public ReactiveProperty<Visibility> SearchVisibility { get; set; } //로딩
        public ReactiveProperty<bool> IsLoading { get; set; } //로딩
        public ReactiveProperty<string> Keyword { get; set; } //검색어
        public ReactiveProperty<CompanyProductSelect> CompanyProductTypeSelect { get; set; } //검색옵션
        public IEnumerable<CompanyProductSelect> CompanyProductTypeSelectValues //검색옵션
        {
            get { return Enum.GetValues(typeof(CompanyProductSelect)).Cast<CompanyProductSelect>(); }
        }


        private DelegateCommand _SearchDialogCommand; //검색버튼
        public DelegateCommand SearchDialogCommand =>
            _SearchDialogCommand ?? (_SearchDialogCommand = new DelegateCommand(SearchCompanyProduct));

        private DelegateCommand<string> _closeDialogCommand;
        public DelegateCommand<string> CloseDialogCommand =>
            _closeDialogCommand ?? (_closeDialogCommand = new DelegateCommand<string>(CloseDialog));

       

        private IContainerProvider ContainerProvider;

        public event Action<IDialogResult> RequestClose;

        public ReactiveCollection<FurnitureInventory> FurnitureList { get; set; } //재고 리스트

        public ReactiveCollection<Company>CompanyList { get; set; } //회사 리스트

        //선택된 제품
        public ReactiveProperty<FurnitureInventory> SelectedFurniture { get; set; }
        //선택된 회사 
        public ReactiveProperty<Company>SelectedCompany { get; set; }

        public string Title => throw new NotImplementedException();

        //선택된 회사 더블클릭시 제품 조회되도록 하는 커맨드
        #region Paging Company
        public ReactiveProperty<int> CurrentPage { get; set; }
        public ReactiveProperty<int> TotalPage { get; set; }
        public ReactiveProperty<int> TotalItemCount { get; set; }
        public ReactiveProperty<int> ListCount { get; set; }
        public ReactiveProperty<int> FirstItem { get; set; }
        public DelegateCommand<object> CmdGoPage { get; }
        public ObservableCollection<int> CountList { get; set; } = new ObservableCollection<int>();
        #endregion
       
        
        #region Paging Product
        public ReactiveProperty<int> CurrentPageProduct { get; set; }
        public ReactiveProperty<int> TotalPageProduct { get; set; }
        public ReactiveProperty<int> TotalItemCountProduct { get; set; }
        public ReactiveProperty<int> FirstItemProduct { get; set; }
        public DelegateCommand<object> CmdGoPageProduct { get; }

        public DelegateCommand<string> AddPage{ get; }
        #endregion
        public IDialogService dialogService { get; set; }

        public SearchCompanyPageViewModel(IContainerProvider con, DialogService dialogService) : base()
        {
            this.dialogService = dialogService;
            this.SearchVisibility = new ReactiveProperty<Visibility>(Visibility.Collapsed).AddTo(disposable);
            this.ContainerProvider = con;
            this.Keyword = new ReactiveProperty<string>().AddTo(disposable);
            this.FurnitureList = new ReactiveCollection<FurnitureInventory>().AddTo(disposable);
            this.CompanyList = new ReactiveCollection<Company>().AddTo(disposable);
            this.CompanyProductTypeSelect = new ReactiveProperty<CompanyProductSelect>(0).AddTo(disposable);
            this.IsLoading = new ReactiveProperty<bool>(false).AddTo(disposable);
            this.IsLoading.Subscribe(x => OnLoadingChanged(x));
            this.SelectedFurniture = new ReactiveProperty<FurnitureInventory>().AddTo(disposable);
            this.SelectedCompany = new ReactiveProperty<Company>().AddTo(disposable);
            SettingPageViewModel temp = this.ContainerProvider.Resolve<SettingPageViewModel>("GlobalData");
            
            FurnitureInfos = temp.FurnitureInfos;


            this.ListCount = new ReactiveProperty<int>(30).AddTo(this.disposable); //Company


            this.FirstItem = new ReactiveProperty<int>(0).AddTo(this.disposable);//Company
            this.TotalPage = new ReactiveProperty<int>(0).AddTo(this.disposable);//Company
            this.TotalItemCount = new ReactiveProperty<int>(0).AddTo(this.disposable);//Company
            this.TotalItemCount.Subscribe(c => this.TotalPage.Value = (c / this.ListCount.Value) + 1);//Company
            this.CurrentPage = new ReactiveProperty<int>(1).AddTo(this.disposable);
            CmdGoPage = new DelegateCommand<object>(ExecCmdGoPage);//Company

            this.CurrentPageProduct = new ReactiveProperty<int>(1).AddTo(this.disposable);//product
            this.FirstItemProduct = new ReactiveProperty<int>(0).AddTo(this.disposable);//product
            this.TotalPageProduct = new ReactiveProperty<int>(0).AddTo(this.disposable);//product
            this.TotalItemCountProduct = new ReactiveProperty<int>(0).AddTo(this.disposable);//product
            this.TotalItemCountProduct.Subscribe(c => this.TotalPageProduct.Value = (c / this.ListCount.Value) + 1);//product
            CmdGoPageProduct = new DelegateCommand<object>(ExecCmdGoPageProduct);//product

            RowDoubleClick = new DelegateCommand<Company>(ExecDoubleClick);

            AddPage = new DelegateCommand<string>(ExecAddPage);
            CountList.Add(30);
            CountList.Add(50);
            CountList.Add(100);
        }

        private void ExecAddPage(string type)
        {
            DialogParameters dialogParameters = new DialogParameters();
            switch (type) {
                case "AddProduct":
                    dialogParameters.Add("object", new FurnitureInventory());

                    dialogService.ShowDialog("ProductAddPage", dialogParameters, r =>
                    {
                        try
                        {
                            if (r.Result == ButtonResult.OK)
                            {
                                FurnitureInventory item = r.Parameters.GetValue<FurnitureInventory>("object");
                                if (item != null)
                                {
                                    using (var network = ContainerProvider.Resolve<DataAgent.ProductDataAgent>())
                                    {
                                        network.SetReceiver(this);
                                        JObject jobj = new JObject();
                                        jobj["product_id"] = (int)0;
                                        jobj["product_type"] = (int)item.ProductType.Value.Id.Value;
                                        jobj["product_name"] = item.Name.Value;
                                        jobj["product_price"] = item.Price.Value;
                                        jobj["company_id"] = item.Company.Value.Id.Value;
                                        network.Create(jobj);
                                        IsLoading.Value = true;
                                    }
                                }
                            }
                        }
                        catch (Exception) { }

                    }, "CommonDialogWindow");

                    break;
                case "AddCompany":
                    
                    dialogParameters.Add("object", new Company());
                    dialogService.ShowDialog("CompanyAddPage", dialogParameters, r =>
                    {
                        try
                        {
                            if (r.Result == ButtonResult.OK)
                            {
                                Company item = r.Parameters.GetValue<Company>("object");
                                if (item != null)
                                {
                                    using (var network = ContainerProvider.Resolve<DataAgent.CompanyDataAgent>())
                                    {
                                        network.SetReceiver(this);
                                        JObject jobj = new JObject();
                                        jobj["company_id"] = (int)0;
                                        jobj["company_name"] = item.CompanyName.Value;
                                        jobj["company_address_detail"] = item.CompanyAddressDetail.Value;
                                        jobj["company_phone"] = item.CompanyPhone.Value;
                                        jobj["company_address"] = item.CompanyAddress.Value;
                                        network.Create(jobj);
                                        IsLoading.Value = true;
                                    }
                                }
                            }
                        }
                        catch (Exception) { }

                    }, "CommonDialogWindow");

                    break;
            }
        }

        private void ExecDoubleClick(Company select)
        {
            using (var network = this.ContainerProvider.Resolve<DataAgent.ProductDataAgent>())
            {
                network.SetReceiver(this);
                JObject jobj = new JObject();
                jobj["page_unit"] = (this.ListCount.Value * CurrentPageProduct.Value) > this.TotalItemCountProduct.Value ? (this.ListCount.Value * CurrentPageProduct.Value) - this.TotalItemCountProduct.Value : this.ListCount.Value;
                jobj["page_start_pos"] = (this.CurrentPageProduct.Value - 1) * this.ListCount.Value;
                JObject inner = new JObject();
                inner["company_name"] = select.CompanyName.Value;
                jobj["search_option"] = inner;
                network.repo.Read(jobj);
                this.IsLoading.Value = true;
            }
        }

        private void ExecCmdGoPageProduct(object param)
        {
            MovePageType moveType = (MovePageType)param;
            if (this.CurrentPageProduct.Value == this.TotalPageProduct.Value && moveType == MovePageType.Next)
            {
                return;
            }
            if (this.CurrentPageProduct.Value == 1 && moveType == MovePageType.Prev)
            {
                return;
            }
            switch (moveType)
            {
                case MovePageType.Next:
                    this.CurrentPageProduct.Value = this.CurrentPageProduct.Value == this.TotalPageProduct.Value ? this.CurrentPageProduct.Value : this.CurrentPageProduct.Value + 1;
                    break;
                case MovePageType.Prev:
                    this.CurrentPageProduct.Value = this.CurrentPageProduct.Value == 1 ? 1 : this.CurrentPageProduct.Value - 1;
                    break;
                default:
                    break;
            }
            UpdatePageItemProduct(moveType, this.ListCount.Value);
        }

        private void UpdatePageItemProduct(MovePageType param, int count)
        {
            using (var network = this.ContainerProvider.Resolve<DataAgent.ProductDataAgent>())
            {
                network.SetReceiver(this);
                JObject jobj = new JObject();
                jobj["next_preview"] = (int)param;
                jobj["page_unit"] = (this.ListCount.Value * CurrentPageProduct.Value) > this.TotalItemCountProduct.Value ? (this.ListCount.Value * CurrentPageProduct.Value) - this.TotalItemCountProduct.Value : this.ListCount.Value;
                jobj["page_start_pos"] = (this.CurrentPageProduct.Value - 1) * this.ListCount.Value;
                JObject inner = new JObject();
                inner["product_name"] = this.Keyword.Value;
                jobj["search_option"] = inner;
                network.repo.Read(jobj);
                this.IsLoading.Value = true;
            }
        }
        private void ExecCmdGoPage(object param)
        {
            MovePageType moveType = (MovePageType)param;
            if (this.CurrentPage.Value == this.TotalPage.Value && moveType == MovePageType.Next)
            {
                return;
            }
            if (this.CurrentPage.Value == 1 && moveType == MovePageType.Prev)
            {
                return;
            }
            switch (moveType)
            {
                case MovePageType.Next:
                    this.CurrentPage.Value = this.CurrentPage.Value == this.TotalPage.Value ? this.CurrentPage.Value : this.CurrentPage.Value + 1;
                    break;
                case MovePageType.Prev:
                    this.CurrentPage.Value = this.CurrentPage.Value == 1 ? 1 : this.CurrentPage.Value - 1;
                    break;
                default:
                    break;
            }
            UpdatePageItem(moveType, this.ListCount.Value);
        }

        private void UpdatePageItem(MovePageType param, int count)
        {
            using (var network = this.ContainerProvider.Resolve<DataAgent.CompanyDataAgent>())
            {
                network.SetReceiver(this);
                JObject jobj = new JObject();
                jobj["next_preview"] = (int)param;
                jobj["page_unit"] = (this.ListCount.Value * CurrentPage.Value) > this.TotalItemCount.Value ? (this.ListCount.Value * CurrentPage.Value) - this.TotalItemCount.Value : this.ListCount.Value;
                jobj["page_start_pos"] = (this.CurrentPage.Value - 1) * this.ListCount.Value;
                JObject inner = new JObject();
                inner["company_name"] = this.Keyword.Value;
                jobj["search_option"] = inner;
                network.repo.Read(jobj);
                this.IsLoading.Value = true;
            }
        }

        internal void SearchCompanyProduct()
        {
            if (Keyword.Value == string.Empty) {
                return;
            }
            switch (this.CompanyProductTypeSelect.Value) {
                case CompanyProductSelect.ProductName :
                    { //제품명 검색
                        this.FirstItemProduct.Value = 0;
                        this.CurrentPageProduct.Value = 1;
                        this.TotalPageProduct.Value = 0;
                        this.TotalItemCountProduct.Value = 0;
                        using (var network = this.ContainerProvider.Resolve<DataAgent.ProductDataAgent>())
                        {
                            JObject jobj = new JObject();
                            network.SetReceiver(this);
                            JObject inner = new JObject();
                            inner["product_name"] = this.Keyword.Value;
                            jobj["search_option"] = inner;
                            jobj["page_unit"] = ListCount.Value;
                            jobj["page_start_pos"] = 0;
                            network.repo.Read(jobj);
                        }
                        this.IsLoading.Value = true;

                    }
                    break;
                case CompanyProductSelect.CompanyName: {
                        //회사명 검색
                        this.FirstItem.Value = 0;
                        this.CurrentPage.Value = 1;
                        this.TotalPage.Value = 0;
                        this.TotalItemCount.Value = 0;
                        using (var network = this.ContainerProvider.Resolve<DataAgent.CompanyDataAgent>())
                        {
                            JObject jobj = new JObject();
                            network.SetReceiver(this);
                            JObject inner = new JObject();
                            inner["company_name"] = this.Keyword.Value;
                            jobj["search_option"] = inner;
                            jobj["page_unit"] = ListCount.Value;
                            jobj["page_start_pos"] = 0;
                            network.repo.Read(jobj);
                        }
                        this.IsLoading.Value = true;
                    }
                    break;
            }
        }

        private void CloseDialog(string parameter)
        {
            DialogResult temp = null;
            ButtonResult result = ButtonResult.None;

            if (parameter?.ToLower() == "true")
            {
                if (this.SelectedFurniture.Value == null)
                    return;
                result = ButtonResult.OK;
                DialogParameters p = new DialogParameters();
                p.Add("object", this.SelectedFurniture.Value);
                temp = new DialogResult(result, p);
            }
            else if (parameter?.ToLower() == "false")
            {
                result = ButtonResult.Cancel;
                temp = new DialogResult(result);
            }
            RaiseRequestClose(temp);
        }
        public virtual void RaiseRequestClose(IDialogResult dialogResult)
        {
            RequestClose?.Invoke(dialogResult);
        }

        private void OnLoadingChanged(bool isLoading)
        {
            SearchVisibility.Value = isLoading ? Visibility.Visible : Visibility.Collapsed;
            
        }
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            throw new NotImplementedException();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            throw new NotImplementedException();
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            throw new NotImplementedException();
        }

        public void OnRceivedData(ErpPacket packet)
        {
            string msg = Encoding.UTF8.GetString(packet.Body);
            JObject jobj = null;
            try { jobj = new JObject(JObject.Parse(msg)); }
            catch (Exception)
            {
                return;
            }
            
            ErpLogWriter.LogWriter.Trace(jobj.ToString());
            switch ((COMMAND)packet.Header.CMD)
            {
                case COMMAND.GETCOMPANYINFO: //데이터 조회
                    if (jobj["company_list"] == null) {
                        return;
                    }
                    foreach(JObject jobject in jobj["company_list"] as JArray) {
                        Company temp=SetCompanyInfo(jobject);
                        Application.Current.Dispatcher.Invoke(() => {
                            this.CompanyList.Add(temp);
                        });
                    }
                    IsLoading.Value = false;
                    break;
                case COMMAND.GETPRODUCTINFO:
                    if (jobj["product_list"] == null)
                        return;

                    foreach (JObject jobject in jobj["product_list"] as JArray)
                    {
                        FurnitureInventory temp = SetProductInfo(jobject);
                        Application.Current.Dispatcher.Invoke(() => {
                            this.FurnitureList.Add(temp);
                        });
                        
                    }
                    IsLoading.Value = false;
                    break;


            }
        }

        private FurnitureInventory SetProductInfo(JObject jobj)
        {
            FurnitureInventory inventory = new FurnitureInventory();
            if (jobj["company"] != null)
                inventory.Company.Value = SetCompanyInfo(jobj["company"] as JObject);
            if (jobj["count"] != null)
                inventory.Count.Value = jobj["count"].ToObject<int>();
            if (jobj["product_type"] != null)
                inventory.ProductType.Value = FurnitureInfos.FirstOrDefault(x => x.Id.Value == jobj["product_type"].ToObject<int>());
            if (jobj["product_name"] != null)
                inventory.Name.Value = jobj["product_name"].ToString();
            if(jobj["product_price"] != null)
                inventory.Price.Value = jobj["product_price"].ToObject<int>();

            return inventory;
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

        public void OnConnected()
        {
          
        }

        public void OnSent()
        {
            
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
           
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
           
        }
    }
}
