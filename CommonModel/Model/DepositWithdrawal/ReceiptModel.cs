using CommonModel.Model;
using Newtonsoft.Json.Linq;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CommonModel.Model
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum AllocateType
    {
        [Description("미완료")]
        NotYet = 0,
        [Description("완료")]
        FullyCompleted = 1
        
    }
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum ReceiptType
    {
        [Description("모두")]
        All = 0,
        [Description("계좌")]
        Account = 1,
        [Description("카드")]
        Card = 2,
        [Description("현금")]
        Cash =3,
       
    }
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum IncomeCostType {
        [Description("모두")]
        All = 0, 
        [Description("입금내역")]
        Income =1, //카드입금, 계좌 입금 , 현금입금
        [Description("지출내역")]
        Cost =2, //카드긁음, 계좌 출금 ,현금출금
    }
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum FullyCompleted {
        [Description("완료")]
        FullyCompleted = 1,
        [Description("미완료")]
        NotYet =2,
        [Description("모두")]
        All = 0,
    }

    //index
    //날짜
    //구분 income withdrawal
    //구분 카드/계좌이체
    //내용
    //금액
    //승인번호
    //주소
    //완료유무표현
    //선택된 카테고리

    public class ReceiptModel : PrismCommonModelBase
    {
        public ReactiveProperty<bool> IsAutoCategory { get; set; }
        public ReactiveProperty<int> ListNo { get; set; }
        public ReactiveProperty<int> ReceiptNo { get; set; }
        public ReactiveProperty<BankModel> BankInfo { get; set; }
        public ReactiveProperty<DateTime> Month { get; set; }
        public ReactiveProperty<ReceiptType> ReceiptType { get; set; }
        public ReactiveProperty<IncomeCostType> IncomeCostType { get; set; }
        public ReactiveProperty<AllocateType> FullyCompleted { get; set; }
        public ReactiveProperty<CategoryInfo> CategoryInfo { get; set; }
        public ReactiveCollection<Contract> ConnectedContract { get; set; }
        public ReactiveProperty<string> Contents { get; set; }
        public ReactiveProperty<string> Tip { get; set; }
        public ReactiveProperty<string> Memo { get; set; }
        public ReactiveProperty<int> Money { get; set; } //금액
        public ReactiveProperty<int> RemainPrice { get; set; } //남은금액
        public ReactiveProperty<int> AllocatedPrice { get; set; } //할당금액
        public ReactiveProperty<float> CardCharge { get; set; } //카드수수료
        public ReactiveProperty<string> indexKey { get; set; }
        public ReceiptModel() : base()
        {
            this.IsAutoCategory = new ReactiveProperty<bool>(false).AddTo(disposable);
            this.CardCharge = new ReactiveProperty<float>(0).AddTo(disposable);
            this.AllocatedPrice = new ReactiveProperty<int>(0).AddTo(disposable);
            this.Tip = new ReactiveProperty<string>("", mode: ReactivePropertyMode.DistinctUntilChanged).AddTo(disposable);
            this.BankInfo = new ReactiveProperty<BankModel>().AddTo(disposable);
            this.ListNo = new ReactiveProperty<int>().AddTo(disposable);//ListNo
            this.ReceiptNo = new ReactiveProperty<int>().AddTo(disposable); //Refid
            this.Month = new ReactiveProperty<DateTime>().AddTo(disposable); //날짜
            this.ReceiptType = new ReactiveProperty<ReceiptType>().AddTo(disposable);//카드,계좌,현금 중분류
            this.IncomeCostType = new ReactiveProperty<IncomeCostType>((IncomeCostType)1, mode: ReactivePropertyMode.DistinctUntilChanged).AddTo(disposable); //수익 지출 대분류
            this.CategoryInfo = new ReactiveProperty<CategoryInfo>(mode: ReactivePropertyMode.DistinctUntilChanged).AddTo(disposable); // 비용 Category 가 할당됨 , 안됨 소분류
            this.FullyCompleted = new ReactiveProperty<AllocateType>().AddTo(disposable); //계약과의 할당 안됨, 부분완료 , 전체완료 -> 입금들 중 처리해야됨
            this.Contents = new ReactiveProperty<string>().AddTo(disposable); //계좌 혹은 카드에 찍힌 내용
            this.Money = new ReactiveProperty<int>(0, mode: ReactivePropertyMode.DistinctUntilChanged).AddTo(disposable); //금액
            this.ConnectedContract = new ReactiveCollection<Contract>().AddTo(disposable); // Connected 계약 내용
            this.Memo = new ReactiveProperty<string>(mode: ReactivePropertyMode.DistinctUntilChanged).AddTo(disposable);// 메모
            this.RemainPrice = new ReactiveProperty<int>().AddTo(disposable);//할당하지 못하고 남은 금액
            this.indexKey = new ReactiveProperty<string>().AddTo(disposable); // index key
            this.ChangedItem = new JObject();
            SetObserver();
        }
        public ReceiptModel(string Tip, CategoryInfo categoryInfo,string memo,int money,string contents,int incomecost) : base() {
            this.IsAutoCategory = new ReactiveProperty<bool>(false).AddTo(disposable);
            CardCharge = new ReactiveProperty<float>(0).AddTo(disposable);
            this.AllocatedPrice = new ReactiveProperty<int>(0).AddTo(disposable);
            this.Tip = new ReactiveProperty<string>(Tip, mode: ReactivePropertyMode.DistinctUntilChanged).AddTo(disposable); //적요
            this.BankInfo = new ReactiveProperty<BankModel>().AddTo(disposable);
            this.ListNo = new ReactiveProperty<int>().AddTo(disposable);//ListNo
            this.ReceiptNo = new ReactiveProperty<int>().AddTo(disposable); //Refid
            this.Month = new ReactiveProperty<DateTime>().AddTo(disposable); //날짜
            this.ReceiptType = new ReactiveProperty<ReceiptType>().AddTo(disposable);//카드,계좌,현금 중분류
            this.IncomeCostType = new ReactiveProperty<IncomeCostType>((IncomeCostType)incomecost, mode: ReactivePropertyMode.DistinctUntilChanged).AddTo(disposable); //수익 지출 대분류            this.CategoryInfo = new ReactiveProperty<CategoryInfo>(categoryInfo,mode: ReactivePropertyMode.DistinctUntilChanged).AddTo(disposable); // 비용 Category 가 할당됨 , 안됨 소분류
            this.FullyCompleted = new ReactiveProperty<AllocateType>().AddTo(disposable); //계약과의 할당 안됨, 부분완료 , 전체완료 -> 입금들 중 처리해야됨
            this.Contents = new ReactiveProperty<string>(contents, mode: ReactivePropertyMode.DistinctUntilChanged).AddTo(disposable); //계좌 혹은 카드에 찍힌 내용
            this.Money = new ReactiveProperty<int>(money, mode: ReactivePropertyMode.DistinctUntilChanged).AddTo(disposable); //금액
            this.CategoryInfo = new ReactiveProperty<CategoryInfo>(categoryInfo,mode: ReactivePropertyMode.DistinctUntilChanged).AddTo(disposable); // 비용 Category 가 할당됨 , 안됨 소분류
            this.ConnectedContract = new ReactiveCollection<Contract>().AddTo(disposable); // Connected 계약 내용
            this.Memo = new ReactiveProperty<string>(memo, mode: ReactivePropertyMode.DistinctUntilChanged).AddTo(disposable);// 메모
            this.RemainPrice = new ReactiveProperty<int>().AddTo(disposable);//할당하지 못하고 남은 금액
            this.indexKey = new ReactiveProperty<string>().AddTo(disposable); // index key
            this.ChangedItem = new JObject();
            SetObserver();
        }
        public override void SetObserver()
        {
            this.ConnectedContract.ToObservable().Subscribe(x => ChangedConnectedContract(x));
            BindingOperations.EnableCollectionSynchronization(this.ConnectedContract, new object());
            ICollectionView view = CollectionViewSource.GetDefaultView(this.ConnectedContract);
            view.Filter = (x) =>
            {
                Contract contract = x as Contract;
                if (contract == null)
                    return false;
                foreach (Payment pay in contract.Payment) {
                    if (pay.Action.Value == AddDelete.Remove) {
                        return false;
                    }
                }    
                return true;
            };

            this.IsAutoCategory.Subscribe(x => ChangedCategory("shi_auto_category", x));
            this.Contents.Subscribe(x => ChangedJson("shi_use_content", x));
            this.IncomeCostType.Subscribe(x => ChangedJson("shi_type", (int)x));
            this.CategoryInfo.Subscribe(x => ChangedCategoryInfo("shi_biz_type", x.CategoryId.Value)).AddTo(disposable);
            this.Memo.Subscribe(x => ChangedJson("shi_memo", x)).AddTo(disposable);
            this.Tip.Subscribe(x => ChangedJson("shi_use_name", x)).AddTo(disposable);
            this.Money.Subscribe(x => ChangedJson("shi_cost", x));
            this.Contents.Subscribe(x => ChangedJson("shi_use_content", x));
            this.AllocatedPrice.Subscribe(x => ChargeCalc(x));
            this.FullyCompleted.Subscribe(x => ChangedJson("shi_complete", (int)x));
        }

        private void ChangedConnectedContract(Contract x)
        {
            this.isChanged = true;
        }

        private void ChargeCalc(int allocatePrice)
        {
            this.RemainPrice.Value = this.Money.Value - this.AllocatedPrice.Value;
            if (this.ReceiptType.Value == Model.ReceiptType.Card)
            {
                this.CardCharge.Value = (float)( this.AllocatedPrice.Value / this.Money.Value) * 100;
            }
            else
            {
                this.CardCharge.Value = 0;
            }
        }

        private void ChangedCategory(string name, bool args) {
            ChangedItem["shi_use_content"] = this.Contents.Value;
            ChangedJson(name, args);
        }

        private void ChangedCategoryInfo(string name, int categoryId) {
            ChangedJson("shi_biz_type", categoryId);
            if (this.IncomeCostType.Value == Model.IncomeCostType.Cost)
            {
                if (this.CategoryInfo.Value.Name.Value == "기타")
                {
                    this.FullyCompleted.Value = AllocateType.NotYet;
                }
                else
                {
                    this.FullyCompleted.Value = AllocateType.FullyCompleted;
                }
            }
        }
        public JObject GetChangedItem()
        {
            JArray jarr = new JArray();
            if (ChangedItem["connected_contract"] != null)
                jarr = ChangedItem["connected_contract"] as JArray;

            foreach (Contract contract in ConnectedContract)
            {
                foreach (Payment pay in contract.Payment)
                {
                    if (!(pay.Action.Value == AddDelete.Default) && !(pay.Action.Value == AddDelete.Update))
                    {
                        JObject inner = new JObject();
                        inner["con_id"] = contract.Id.Value;
                        inner["payment_id"] = pay.PaymentId.Value;
                        inner["mode"] = (int)pay.Action.Value;
                        jarr.Add(inner);
                    }
                }
            }
            if(jarr.Count>0)
                ChangedItem["connected_contract"] = jarr;
            return ChangedItem;
        }
    }
}
