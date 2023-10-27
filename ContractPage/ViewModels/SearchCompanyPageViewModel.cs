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
using PrsimCommonBase;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace ContractPage.ViewModels
{
    public class SearchCompanyPageViewModel : PrismCommonViewModelBase, INavigationAware, IDisposable, INetReceiver
    {
        [TypeConverter(typeof(EnumDescriptionTypeConverter))]
        public enum CompanyProductSelect
        {
            [Description("제품명")]
            ProductName = 0,
            [Description("회사명")]
            CompanyName = 1,
        }
        public ReactiveProperty<Visibility> SearchVisibility { get; set; }
        public ReactiveProperty<bool> IsLoading { get; set; }
        public ReactiveProperty<string> Keyword { get; set; }
        public ReactiveProperty<CompanyProductSelect> CompanyProductTypeSelect { get; set; }
        public IEnumerable<CompanyProductSelect> CompanyProductTypeSelectValues
        {
            get { return Enum.GetValues(typeof(CompanyProductSelect)).Cast<CompanyProductSelect>(); }
        }
        private DelegateCommand _SearchDialogCommand;
        public DelegateCommand SearchDialogCommand =>
            _SearchDialogCommand ?? (_SearchDialogCommand = new DelegateCommand(SearchAddress));
        private IContainerProvider ContainerProvider;

        public ReactiveCollection<Furniture> FurnitureList { get; set; }

        public ReactiveCollection<Company>CompanyList { get; set; }


        public SearchCompanyPageViewModel(IContainerProvider con) : base()
        {
            this.SearchVisibility = new ReactiveProperty<Visibility>(Visibility.Collapsed).AddTo(disposable);
            this.ContainerProvider = con;
            this.Keyword = new ReactiveProperty<string>().AddTo(disposable);
            this.FurnitureList = new ReactiveCollection<Furniture>().AddTo(disposable);
            this.CompanyList = new ReactiveCollection<Company>().AddTo(disposable);
            this.CompanyProductTypeSelect = new ReactiveProperty<CompanyProductSelect>(0).AddTo(disposable);
            this.IsLoading = new ReactiveProperty<bool>(false).AddTo(disposable);
            this.IsLoading.Subscribe(x => OnLoadingChanged(x));
        }
        internal async void SearchAddress()
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
                        SetCompanyInfo(jobject);
                    }
                    break;
                case COMMAND.GETPRODUCTINFO:
                    SetProductInfo(jobj);
                    break;


            }
        }

        private void SetProductInfo(JObject jobj)
        {
            Company temp = new Company();
            temp.CompanyName.Value = jobj["company_name"].ToString();
            temp.CompanyPhone.Value = jobj["company_phone"].ToString();
            temp.Id.Value = jobj["company_id"].ToObject<int>();
            temp.CompanyAddress.Value = jobj["company_address"].ToString();
            this.CompanyList.Add(temp);
        }

        private void SetCompanyInfo(JObject jobj)
        {
            throw new NotImplementedException();
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
