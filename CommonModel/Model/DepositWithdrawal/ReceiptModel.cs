using CommonModel.Model;
using Newtonsoft.Json.Linq;
using PrsimCommonBase;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModel.Model
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum AllocateType
    {
        [Description("완료")]
        FullyCompleted = 1,
        [Description("미완료")]
        NotYet = 0,
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
        public ReactiveProperty<int> Money { get; set; }
        public ReactiveProperty<string> Tip { get; set; }
        public ReactiveProperty<string> Memo { get; set; }
        public ReactiveProperty<int> RemainPrice { get; set; }
        public ReactiveProperty<string> indexKey { get; set; }
        public ReactiveProperty<bool> IsChanged { get; set; }
        public JObject ChangedItem { get; set; }
        public ReceiptModel() : base()
        {
            this.IsChanged = new ReactiveProperty<bool>(false).AddTo(disposable);
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
            this.IncomeCostType.Subscribe(x => ChangedJson("shi_type", (int)x));
            this.CategoryInfo.Subscribe(x => ChangedJson("shi_biz_type", x.CategoryId.Value)).AddTo(disposable);
            this.Memo.Subscribe(x => ChangedJson("shi_memo", x)).AddTo(disposable);
            this.Tip.Subscribe(x => ChangedJson("shi_use_name", x)).AddTo(disposable);
            this.Money.Subscribe(x => ChangedJson("shi_cost", x));
            this.Contents.Subscribe(x => ChangedJson("shi_use_content", x));
        }
        public ReceiptModel(string Tip, CategoryInfo categoryInfo,string memo,int money,string contents,int incomecost) : base() {
            this.IsChanged = new ReactiveProperty<bool>(false).AddTo(disposable);
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
            this.IncomeCostType.Subscribe(x => ChangedJson("shi_type", (int)x));
            this.CategoryInfo.Subscribe(x => ChangedJson("shi_biz_type", x.CategoryId.Value)).AddTo(disposable);
            this.Memo.Subscribe(x => ChangedJson("shi_memo", x)).AddTo(disposable);
            this.Tip.Subscribe(x => ChangedJson("shi_use_name", x)).AddTo(disposable);
            this.Money.Subscribe(x => ChangedJson("shi_cost", x));
            this.Contents.Subscribe(x => ChangedJson("shi_use_content", x));
        }
        private void ChangedJson(string name, object value) {
            if (value != null) {
                if (value is int)
                {
                    ChangedItem[name] = (int)value;
                }
                else if (value is string)
                {
                    ChangedItem[name] = value.ToString();
                }
                this.IsChanged.Value = true;
            }
        }
        public void CompleteChangedData() {
            this.ChangedItem.RemoveAll();
            this.IsChanged.Value = false;
            
        }

        public ReceiptModel Copy() {
            ReceiptModel tmp = new ReceiptModel(this.Tip.Value, this.CategoryInfo.Value, this.Memo.Value,this.Money.Value,this.Contents.Value,(int)this.IncomeCostType.Value);
            tmp.IsChanged.Value = this.IsChanged.Value;
            tmp.ChangedItem= this.ChangedItem;
            tmp.BankInfo.Value = this.BankInfo.Value;
            tmp.ListNo.Value = this.ListNo.Value;
            tmp.ReceiptNo.Value = this.ReceiptNo.Value;
            tmp.Month.Value = this.Month.Value;
            tmp.ReceiptType.Value = this.ReceiptType.Value;
            tmp.FullyCompleted.Value = this.FullyCompleted.Value;
            tmp.RemainPrice.Value = this.RemainPrice.Value;
            tmp.indexKey.Value = this.indexKey.Value;
            if (this.ConnectedContract.Count > 0) { 
                foreach (Contract item in this.ConnectedContract) {
                    tmp.ConnectedContract.Add(item);
                }
            }
            return tmp;
        }
    }
}
