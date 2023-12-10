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
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace MESPage.ViewModels
{

    public class MesSingleViewModel : PrismCommonViewModelBase, INavigationAware, IDisposable, INetReceiver
    {
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

        private readonly CompositeDisposable _disposable = new();

        public ReactiveProperty<ContractedProduct> Inventory { get; set; }
        public DelegateCommand DeleteButton { get; }
        public DelegateCommand SearchAddress { get; }
        public DelegateCommand<string> SearchName { get; }
        public DelegateCommand<string> AddContractItemButton { get; }
        private IRegionManager RegionManager { get; }
        public IContainerProvider ContainerProvider { get; }
        public ReactiveProperty<Payment> SelectedPayment { get; set; }
        public ReactiveProperty<ContractedProduct> SelectedProduct { get; set; }
        public ReactiveCollection<Employee> EmployeeInfos { get; set; }
        public ReactiveProperty<Employee> SelectedEmployee { get; set; }
        public IDialogService dialogService { get; }
        public ReactiveProperty<PrismCommonModelBase> SelectedItem { get; set; }
        public DelegateCommand RowDoubleClick { get; }
        public MesSingleViewModel(IRegionManager regionManager, IContainerProvider containerProvider, IDialogService dialogService) : base(regionManager)
        {
            this.SelectedItem = new ReactiveProperty<PrismCommonModelBase>().AddTo(this.disposable);
            ButtonName = new ReactiveProperty<string>().AddTo(disposable);
            IsNewContractReverse = new ReactiveProperty<Visibility>().AddTo(disposable);
            ContainerProvider = containerProvider;
            this.dialogService = dialogService;
            SelectedProduct = new ReactiveProperty<ContractedProduct>().AddTo(disposable);
            AddContractItemButton = new DelegateCommand<string>(ExecAddContractItemButton);
            RegionManager = regionManager;
            SaveButton = new DelegateCommand(SaveButtonExecute);
            DeleteButton = new DelegateCommand(DeleteButtonExecute);
            Inventory = new ReactiveProperty<ContractedProduct>().AddTo(disposable);
            Title.Value = "신규등록";
            EmployeeInfos = new ReactiveCollection<Employee>().AddTo(disposable);
            SelectedEmployee = new ReactiveProperty<Employee>().AddTo(disposable);
            this.RowDoubleClick = new DelegateCommand(RowDoubleClickEvent);
        }
        public void RowDoubleClickEvent()
        {
            if (SelectedItem.Value == null)
                return;
            DialogParameters dialogParameters = new DialogParameters();
            SelectedItem.Value.ClearJson();
            dialogParameters.Add("object", SelectedItem.Value as ContractedProduct);

            dialogService.ShowDialog("FindInventoryItem", dialogParameters, r =>
            {
                try
                {
                    if (r.Result == ButtonResult.OK)
                    {
                        ContractedProduct item = r.Parameters.GetValue<ContractedProduct>("object");
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
       
        public void Dispose()
        {
            _disposable.Dispose();
        }
        public bool IsNavigationTarget(NavigationContext navigationContext) => false;

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            this.Dispose();
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            ContractedProduct Inventory = navigationContext.Parameters["object"] as ContractedProduct;

            if (Inventory == null)
            {
                Title.Value = "신규 재고 추가";
                this.Inventory.Value = new ContractedProduct();
            }
            else
            {
                Title.Value = "재고 내역 수정";
                IsNewInventory.Value = Visibility.Visible;
                this.Inventory.Value = Inventory;
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
                    if (this.Title.Value == "신규계약 추가") //신규등록일경우
                    {

                        if (this.Inventory.Value.isChanged)
                        {
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
                case COMMAND.UpdateBankHistory: //데이터 업데이트 완료
                case COMMAND.DeleteBankHistory: //데이터 삭제완료
                    Application.Current.Dispatcher.Invoke(() => {
                        DrawerHost.CloseDrawerCommand.Execute(Dock.Right, null);
                        //this.Inventory.Value.CompleteChangedData(); //변경완료 후 변수 초기화
                        Dispose();
                        regionManager.RequestNavigate("ContentRegion", nameof(MESPage));
                    });
                    break;
            }
        }

        public void OnConnected()
        {
        }

        public void OnSent()
        {
        }

    }
}
