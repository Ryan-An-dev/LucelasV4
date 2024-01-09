using AddressSearchManager.Models;
using CommonModel;
using CommonModel.Model;
using DataAccess;
using DataAccess.NetWork;
using LogWriter;
using MaterialDesignThemes.Wpf;
using MESPage.Views;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog.Targets;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SettingPage.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace MESPage.ViewModels
{

    public class MesSingleViewModel : PrsimListViewModelBase, INavigationAware, IDisposable, INetReceiver
    {
        public ReactiveProperty<CompanyProductSelect> CompanyProductTypeSelect { get; set; } //검색옵션
        public IEnumerable<CompanyProductSelect> CompanyProductTypeSelectValues //검색옵션
        {
            get { return Enum.GetValues(typeof(CompanyProductSelect)).Cast<CompanyProductSelect>(); }
        }
        public ReactiveProperty<Purpose> PurposeTypeSelect { get; set; } //검색옵션
        public IEnumerable<Purpose> PurposeTypeSelectValues //검색옵션
        {
            get { return Enum.GetValues(typeof(Purpose)).Cast<Purpose>().Skip(1);  }
        }
        public ReactiveCommand List_MouseDoubleClick { get; set; }

        [TypeConverter(typeof(EnumDescriptionTypeConverter))]
        public enum CompanyProductSelect
        {
            [Description("제품명")]
            ProductName = 0,
            [Description("회사명")]
            CompanyName = 1,
        }
        public ReactiveCollection<FurnitureType> FurnitureInfos { get; set; }


        /// <summary>
        /// 새로운 계약 : Collapse  // 기존계약 : Visible
        /// </summary>
        public ReactiveProperty<Visibility> IsNewInventory { get; set; }
        /// <summary>
        /// 새로운 계약 : Visible  // 기존계약 : Collapse
        /// </summary>
        public ReactiveProperty<Visibility> IsNewContractReverse { get; set; }
        /// <summary>
        /// 편집,완료 버튼
        /// </summary>
        public ReactiveProperty<string> ButtonName { get; set; }
        /// <summary>
        /// 인적사항 편집모드 버튼 커멘드
        /// </summary>
        public DelegateCommand<string> SetEditMode { get; set; }
        /// <summary>
        ///  편집모드 = false // 읽기전용 = true;
        /// </summary>

        public DelegateCommand SaveButton { get; }
        public ReactiveProperty<string> Title { get; } = new();
        public ReactiveProperty<FurnitureInventory> Inventory { get; set; }

        public ReactiveProperty<Visibility> RealPriceVis { get; set; }
        public ReactiveProperty<int> RealyPrice { get; set; }

        public DelegateCommand DeleteButton { get; }
        public DelegateCommand SearchAddress { get; }
        public DelegateCommand<string> SearchName { get; }
        public DelegateCommand<string> AddContractItemButton { get; }
        private IRegionManager RegionManager { get; }
        public IContainerProvider ContainerProvider { get; }

        public MesSingleViewModel(IContainerProvider containerprovider, IRegionManager regionManager, IDialogService dialogService) : base(regionManager, containerprovider, dialogService)
        {

            //listItem
            List_MouseDoubleClick = new ReactiveCommand().WithSubscribe(() => RowDoubleClickEvent()).AddTo(disposable);
            FurnitureInfos = new ReactiveCollection<FurnitureType>().AddTo(disposable);
            CompanyProductTypeSelect = new ReactiveProperty<CompanyProductSelect>(CompanyProductSelect.ProductName).AddTo(disposable);

            IsNewInventory = new ReactiveProperty<Visibility>(Visibility.Collapsed).AddTo(disposable);
            PurposeTypeSelect = new ReactiveProperty<Purpose>().AddTo(disposable);
            ButtonName = new ReactiveProperty<string>().AddTo(disposable);
            IsNewContractReverse = new ReactiveProperty<Visibility>().AddTo(disposable);
            ContainerProvider = containerprovider;
            AddContractItemButton = new DelegateCommand<string>(ExecAddContractItemButton);
            RegionManager = regionManager;
            SaveButton = new DelegateCommand(SaveButtonExecute);
            DeleteButton = new DelegateCommand(DeleteButtonExecute);
            Inventory = new ReactiveProperty<FurnitureInventory>().AddTo(disposable);
            Title.Value = "신규등록";
            
            
        }


        public override void RowDoubleClickEvent()
        {
            if (SelectedItem.Value == null)
                return;
            DialogParameters dialogParameters = new DialogParameters();
            SelectedItem.Value.ClearJson();
            dialogParameters.Add("object", SelectedItem.Value as Product);

            dialogService.ShowDialog("ProductAddPage", dialogParameters, r =>
            {
                try
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        Product item = r.Parameters.GetValue<Product>("object");
                        if (item != null)
                        {
                            using (var network = ContainerProvider.Resolve<DataAgent.ProductDataAgent>())
                            {
                                network.SetReceiver(this);
                                JObject jobj = new JObject();
                                jobj["changed_item"] = item.ChangedItem;
                                jobj["product_id"] = item.Id.Value;
                                network.Update(jobj);
                            }
                        }
                    }
                }
                catch (Exception) { }

            }, "CommonDialogWindow");
        }
        private void ExecAddContractItemButton(string obj)
        {
            switch (obj)
            {
                case "AddProduct":
                    break;
                case "DeleteProduct":
                    
                    break;
                case "AddPayment":
                  
                   
                    break;
                case "DeletePayment":
                    
                    break;
            }
        }     
       
        public bool IsNavigationTarget(NavigationContext navigationContext) => false;

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            this.Dispose();
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            FurnitureInventory Inventory = navigationContext.Parameters["object"] as FurnitureInventory;

            if (Inventory == null)
            {
                Title.Value = "신규 재고 추가";
                this.Inventory.Value = new FurnitureInventory();
                IsNewInventory.Value = Visibility.Visible;
            }
            else
            {
                Title.Value = "재고 내역 수정";
                this.Inventory.Value = Inventory;
                SelectedItem.Value = Inventory.Product.Value;
                this.Inventory.Value.ClearJson();
                //하나하나에 값 재할당 해줘야한다. 벨류 안바뀌게 
            }
        }
        private void SaveButtonExecute()
        {
            Save(0);
        }

        private void DeleteButtonExecute()
        {
            Save(1);
        }

        private void Save(int param)
        {
            using (var network = this.ContainerProvider.Resolve<DataAgent.InventoryDataAgent>())
            {
                JObject jobj = new JObject();
                network.SetReceiver(this);
                if (param == 0)
                { // Update
                    if (this.Title.Value == "신규 재고 추가") //신규등록일경우
                    {

                        if (this.Inventory.Value.isChanged)
                        {
                            this.Inventory.Value.Product.Value = this.SelectedItem.Value as Product;
                            network.Create(this.Inventory.Value.MakeJson());
                        }
                    }
                    else
                    { //내역수정일경우
                        if (this.Inventory.Value.isChanged)
                        {
                            JObject inner = new JObject();
                            inner["con_id"] = this.Inventory.Value.Id.Value;
                            inner["changed_item"] = this.Inventory.Value.GetChangedItem();
                            ErpLogWriter.LogWriter.Debug(inner);
                            network.Update(inner);
                        }
                    }
                }
                else
                {
                    jobj["con_id"] = this.Inventory.Value.Id.Value;
                    network.Delete(jobj);
                }
            }
        }

        public void OnRceivedData(ErpPacket packet) //완료 커맨드가 들어와야 첫화면으로 넘어간다.
        {
            string msg = Encoding.UTF8.GetString(packet.Body);
            try
            {
                JObject jobj = new JObject(JObject.Parse(msg));
                //ErpLogWriter.LogWriter.Trace(jobj.ToString());
            }
            catch (Exception e) { }
            switch ((COMMAND)packet.Header.CMD)
            {
                case COMMAND.CREATE_INVENTORY_LIST:
                case COMMAND.UPDATE_INVENTORY_LIST: //데이터 업데이트 완료
                case COMMAND.DELETE_INVENTORY_LIST: //데이터 삭제완료
                    Application.Current.Dispatcher.Invoke(() => {
                        DrawerHost.CloseDrawerCommand.Execute(Dock.Right, null);
                        //this.Inventory.Value.CompleteChangedData(); //변경완료 후 변수 초기화
                        Dispose();
                        regionManager.RequestNavigate("ContentRegion", nameof(MesPage));
                    });
                    break;
                case COMMAND.UPDATEPRODUCTINFO:
                    SearchTitle(this.Keyword.Value);
                    break;
                case COMMAND.GETPRODUCTINFO:
                    JObject jobject = null;
                    try { jobject = new JObject(JObject.Parse(msg)); }
                    catch (Exception)
                    {
                        return;
                    }
                    ErpLogWriter.LogWriter.Trace(jobject.ToString());
                    SettingPageViewModel temp = this.ContainerProvider.Resolve<SettingPageViewModel>("GlobalData");
                    FurnitureInfos = temp.FurnitureInfos;
                    Application.Current.Dispatcher.Invoke(() => { List.Clear(); });
                    if (jobject.ToString().Trim() != string.Empty)
                    {
                        try
                        {
                            if (jobject["product_list"] == null)
                                return;
                            JArray jarr = new JArray();
                            jarr = jobject["product_list"] as JArray;
                            if (jobject["history_count"] != null)
                                TotalItemCount.Value = jobject["history_count"].ToObject<int>();

                            int i = CurrentPage.Value == 1 ? 1 : ListCount.Value * (CurrentPage.Value - 1) + 1;

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
                                    this.List.Add(inventory);
                                });
                            }
                        }
                        catch (Exception e) { LogWriter.ErpLogWriter.LogWriter.Debug(e.ToString()); }
                    }
                    break;
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

        public void OnConnected()
        {
        }

        public void OnSent()
        {
        }
        public override void AddButtonClick()
        {
            DialogParameters dialogParameters = new DialogParameters();
            dialogParameters.Add("object", new Product());

            dialogService.ShowDialog("ProductAddPage", dialogParameters, r =>
            {
                try
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        Product item = r.Parameters.GetValue<Product>("object");
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
        }

        public override void DeleteButtonClick(PrismCommonModelBase selecteditem)
        {
            using (var network = ContainerProvider.Resolve<DataAgent.ProductDataAgent>())
            {
                network.SetReceiver(this);
                JObject jobj = new JObject();
                jobj["company_id"] = (int)(selecteditem as Product).Company.Value.Id.Value;
                jobj["product_id"] = (int)(selecteditem as Product).Id.Value;
                network.Delete(jobj);
                IsLoading.Value = true;
            }
        }

        public override void SearchTitle(string Keyword)
        {
            using (var network = ContainerProvider.Resolve<DataAgent.ProductDataAgent>())
            {
                network.SetReceiver(this);
                JObject jobj = new JObject();
                JObject search = new JObject();
                if (this.CompanyProductTypeSelect.Value == CompanyProductSelect.ProductName)
                {
                    search["product_name"] = Keyword;
                }
                else
                {
                    search["company_name"] = Keyword;
                }
                jobj["page_unit"] = (ListCount.Value);
                jobj["page_start_pos"] = (CurrentPage.Value - 1) * ListCount.Value;
                jobj["search_option"] = search;
                network.Get(jobj);
            }
        }

        public override void UpdatePageItem(CommonModel.MovePageType param, int count)
        {
            using (var network = ContainerProvider.Resolve<DataAgent.ProductDataAgent>())
            {
                network.SetReceiver(this);
                JObject jobj = new JObject();
                jobj["next_preview"] = (int)param;
                jobj["page_unit"] = (ListCount.Value * CurrentPage.Value) > TotalItemCount.Value ? TotalItemCount.Value - (ListCount.Value * (CurrentPage.Value - 1)) : ListCount.Value;
                jobj["page_start_pos"] = (CurrentPage.Value - 1) * ListCount.Value;
                network.repo.Read(jobj);
            }
        }
    }
}
