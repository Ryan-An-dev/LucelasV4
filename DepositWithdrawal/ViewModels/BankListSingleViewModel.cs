using CommonModel.Model;
using ControlzEx.Standard;
using DataAccess;
using DataAccess.NetWork;
using DepositWithdrawal.Views;
using LogWriter;
using MaterialDesignThemes.Wpf;
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
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace DepositWithdrawal.ViewModels
{
    //여기 삭제랑 업데이트 할때 로딩아이콘 넣어야된다. 무조껀
    public class BankListSingleViewModel : PrismCommonModelBase, INavigationAware, IDisposable, INetReceiver
    {
        public IEnumerable<IncomeCostType> SearchIncomeCostTypeValues
        {
            get { return Enum.GetValues(typeof(IncomeCostType)).Cast<IncomeCostType>().Skip(1); }
        }
        public IEnumerable<ReceiptType> SearchReceiptTypeValues
        {
            get { return Enum.GetValues(typeof(ReceiptType)).Cast<ReceiptType>().Skip(1); }
        }
        public IEnumerable<FullyCompleted> SearchFullyCompletedValues
        {
            get { return Enum.GetValues(typeof(FullyCompleted)).Cast<FullyCompleted>().Skip(1); }
        }
        public ReactiveProperty<string> Title { get; } = new();
        public DelegateCommand SaveButton { get; }
        public ReactiveProperty<ReceiptModel> ReceiptModel { get; set; }

        private readonly CompositeDisposable _disposable = new();
        public DelegateCommand DeleteButton { get; }
        public ReactiveProperty<Visibility> VisibilityAddButton { get; } = new();
        public ReactiveProperty<Visibility> VisibilityContract { get; } = new();
        public ReactiveProperty<bool> IsEnableTab { get; } = new();
        public ReadOnlyReactiveProperty<bool> IsReverseEnableTab { get; }
        public ReactiveProperty<bool> IsCashOnly { get; set; }
        public ReactiveCollection<CategoryInfo> CategoryInfos { get; set; }
        public IContainerProvider ContainerProvider { get; }
        public DelegateCommand<string> AddContractItemButton { get; }
        public IDialogService dialogService { get; }

        public BankListSingleViewModel(IRegionManager regionManager,IContainerProvider containerProvider,IDialogService dialogService):base(regionManager)
        {
            this.dialogService = dialogService;
            this.ContainerProvider = containerProvider;
            Title.AddTo(_disposable);
            SaveButton = new DelegateCommand(SaveButtonExecute);
            DeleteButton = new DelegateCommand(DeleteButtonExecute);
            AddContractItemButton = new DelegateCommand<string>(ExcuteContractButton);
            ReceiptModel = new ReactiveProperty<ReceiptModel>().AddTo(_disposable);
            CategoryInfos = new ReactiveCollection<CategoryInfo>().AddTo(_disposable);
            IsEnableTab.AddTo(_disposable);
            IsReverseEnableTab = IsEnableTab.Select(x=>!x).ToReadOnlyReactiveProperty();
            IsCashOnly = new ReactiveProperty<bool>(false).AddTo(this.disposable);
        }

        private void ExcuteContractButton(string obj)
        {
            
            if (obj == "Add")
            {
                if (this.ReceiptModel.Value.RemainPrice.Value == 0)
                { // 남은 금액이 없으면 메시지박스 띄워줌 (-버튼만 활성화 됩니다. 등);
                    //return;
                }
                DialogParameters p = new DialogParameters();
                p.Add("AccountName",this.ReceiptModel.Value);
                this.dialogService.ShowDialog("FindItemPage", p, r => FindContractItem(r), "CommonDialogWindow");
            }
            else {  //Delete
                
            }
        }
        private void FindContractItem(IDialogResult r) {
            //Contract ID 받아야되는데 
            if(r == null) return;
            if (r.Result == ButtonResult.OK)
            {
                if (!r.Parameters.ContainsKey("SelectedPaymentItem")) return;
                else {
                    Payment temp = null;
                    r.Parameters.TryGetValue("SelectedPaymentItem", out temp);
                }
            }
            else 
            { 
                
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
        public bool IsNavigationTarget(NavigationContext navigationContext) => false;

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            
        }
        
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            var Receipt = navigationContext.Parameters["ReceiptModel"] as ReactiveProperty<ReceiptModel>;
            SettingPageViewModel temp = this.ContainerProvider.Resolve<SettingPageViewModel>("GlobalData");
            foreach (var item in temp.CategoryInfos) {
                this.CategoryInfos.Add(item);
            }
            this.VisibilityContract.Value = Visibility.Visible; //contract 추가 
            if (Receipt == null)
            {
                Title.Value = "현금내역 추가";
                VisibilityAddButton.Value = Visibility.Collapsed;
                IsEnableTab.Value = false; // 현금내역일때
                this.ReceiptModel.Value = new ReceiptModel();
                this.ReceiptModel.Value.Month.Value =DateTime.Now;
                this.ReceiptModel.Value.ReceiptType.Value = ReceiptType.Cash;
                this.ReceiptModel.Value.Tip.Value = "현금";
            }
            else
            {
                Title.Value = "내역 수정";
                IsEnableTab.Value = true; // 현금내역 아닐때
                this.ReceiptModel.Value = Receipt.Value.Copy();
                if (Receipt.Value.IncomeCostType.Value == IncomeCostType.Cost) {
                    this.VisibilityContract.Value = Visibility.Collapsed;
                }
                //하나하나에 값 재할당 해줘야한다. 벨류 안바뀌게 
            }
            if (this.ReceiptModel.Value.ReceiptType.Value == ReceiptType.Cash) {
                IsCashOnly.Value = true;
            }
          
        }
        private void Save(int param) {
            using (var network = this.ContainerProvider.Resolve<DataAgent.BankListDataAgent>())
            {
                JObject jobj = new JObject();
                network.SetReceiver(this);
                if (param == 0) { // Update
                    if (this.Title.Value=="현금내역 추가") //신규등록일경우
                    { 
                        JObject inner = new JObject();
                        inner["shi_type"] = (int)this.ReceiptModel.Value.IncomeCostType.Value;
                        inner["shi_biz_type"] = this.ReceiptModel.Value.CategoryInfo.Value.CategoryId.Value;
                        inner["shi_cost"] = this.ReceiptModel.Value.Money.Value;
                        inner["shi_time"] = this.ReceiptModel.Value.Month.Value.ToString("yyyy-MM-dd HH:mm:ss");
                        inner["shi_use_content"] = this.ReceiptModel.Value.Contents.Value;
                        inner["shi_memo"] = this.ReceiptModel.Value.Memo.Value;
                        inner["shi_use_name"] = this.ReceiptModel.Value.Tip.Value;
                        jobj["create_history"] = inner;
                        network.CreateBankHistory(jobj);
                    }
                    else { //내역수정일경우
                        if (this.ReceiptModel.Value.IsChanged.Value) {
                            JObject inner = new JObject();
                            inner["shi_id"] = this.ReceiptModel.Value.ReceiptNo.Value;
                            inner["changed_property"] = this.ReceiptModel.Value.ChangedItem;
                            jobj["update_history"] = inner;
                            network.UpdateBankHistory(jobj);
                        }
                    }
                }
                else
                {
                    jobj["delete_history"] = this.ReceiptModel.Value.ReceiptNo.Value;
                    network.DeleteBankHistory(jobj);
                }
            }
        }

        public void OnRceivedData(ErpPacket packet)
        {
            string msg = Encoding.UTF8.GetString(packet.Body);
            try {
                JObject jobj = new JObject(JObject.Parse(msg));
                //ErpLogWriter.LogWriter.Trace(jobj.ToString());
            } catch (Exception e) { }
            switch ((COMMAND)packet.Header.CMD)
            {
                case COMMAND.UpdateBankHistory: //데이터 업데이트 완료
                case COMMAND.DeleteBankHistory: //데이터 삭제완료
                    Application.Current.Dispatcher.Invoke(() => {
                        DrawerHost.CloseDrawerCommand.Execute(Dock.Right, null);
                        this.ReceiptModel.Value.CompleteChangedData(); //변경완료 후 변수 초기화
                        Dispose();
                        regionManager.RequestNavigate("ContentRegion", nameof(BankListPage));
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

        public override JObject GetChangedItem()
        {
            throw new NotImplementedException();
        }
    }
}
