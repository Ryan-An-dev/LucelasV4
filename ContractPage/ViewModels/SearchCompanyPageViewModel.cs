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

        public SearchCompanyPageViewModel(IContainerProvider con) : base()
        {
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
        }
        

        internal void SearchCompanyProduct()
        {
            if (Keyword.Value == string.Empty) {
                return;
            }
            switch (this.CompanyProductTypeSelect.Value) {
                case CompanyProductSelect.ProductName :
                    { //제품명 검색
                        using (var network = this.ContainerProvider.Resolve<DataAgent.ProductDataAgent>())
                        {
                            JObject jobj = new JObject();
                            network.SetReceiver(this);
                            JObject inner = new JObject();
                            inner["product_name"] = this.Keyword.Value;
                            jobj["search_option"] = inner;
                            network.repo.Read(jobj);
                        }
                        this.IsLoading.Value = true;

                    }
                    break;
                case CompanyProductSelect.CompanyName: {
                        //회사명 검색
                        using (var network = this.ContainerProvider.Resolve<DataAgent.CompanyDataAgent>())
                        {
                            JObject jobj = new JObject();
                            network.SetReceiver(this);
                            JObject inner = new JObject();
                            inner["company_name"] = this.Keyword.Value;
                            jobj["search_option"] = inner;
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
                if (this.SelectedFurniture == null)
                    return;
                result = ButtonResult.OK;
                DialogParameters p = new DialogParameters();
                p.Add("SelectedFurniture", this.SelectedFurniture);
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
                    foreach(JObject jobject in jobj["company_list"] as JArray) {
                        Company temp=SetCompanyInfo(jobject);
                        this.CompanyList.Add(temp);
                    }
                    IsLoading.Value = false;
                    break;
                case COMMAND.GETPRODUCTINFO:
                    foreach (JObject jobject in jobj["product_list"] as JArray)
                    {
                        FurnitureInventory temp = SetProductInfo(jobject);
                        this.FurnitureList.Add(temp);
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
                inventory.ProductType.Value = FurnitureInfos.FirstOrDefault(x => x.ProductCode.Value == jobj["product_type"].ToObject<int>());
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
