using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public enum COMMAND : ushort
    {

        #region 로그인&기타공용데이터요청
        LOGIN = 1,
        KeepAlive = 2,
        KeepSession = 3,
        AccountLIst = 4,
        CategoryList = 5,
        ProductCategoryList = 6,
        GetCardTypeList = 7,
        
        #endregion


        #region 입출금페이지
        GetBankHistory =20,
        CreateBankHistory=21,
        UpdateBankHistory=22,
        DeleteBankHistory=23,
        CONNECTED_CONTRACT = 24,
        #endregion

        #region 계약리스트
        GetContractList = 30,
        CREATECONTRACT = 31,
        UPDATECONTRACT = 32,
        DELETECONTRACT = 33,
        GET_CONNECTED_CONTRACT = 34,
        GET_CONTRACT_FOR_RECEIPT = 35,
        #endregion

        #region 직원리스트
        CREATEEMPLOEEINFO = 56,
        GETEMPLOEEINFO ,
        UPDATEEMPLOEEINFO,
        DELETEEMPLOEEINFO,
        #endregion

        #region 회사리스트
        CREATECOMPANYINFO =60,
        GETCOMPANYINFO,
        UPDATECOMPANYINFO,
        DELETECOMPANYINFO,
        #endregion

        #region 제품리스트
        CREATEPRODUCTINFO = 64,
        GETPRODUCTINFO,
        UPDATEPRODUCTINFO,
        DELETEPRODUCTINFO,
        #endregion

        #region 고객리스트
        CREATECUSTOMERINFO = 68,
        GETCUSTOMERINFO,
        UPDATECUSTOMERINFO,
        DELETECUSTOMERINFO,
        #endregion

        #region 제품카테고리
        CREATE_PRODUCTCATEGORY_INFO = 72,
        UPDATE_PRODUCTCATEGORY_INFO,
        DELETE_PRODUCTCATEGORY_INFO,
        #endregion

        #region 계좌/카드
        CREATE_ACCOUNT_INFO = 50,
        UPDATE_ACCOUNT_INFO,
        DELETE_ACCOUNT_INFO,
        #endregion

        #region API
        CREATE_API_INFO = 75,
        READ_API_INFO,
        UPDATE_API_INFO,
        DELETE_API_INFO,
        #endregion

        #region 통계데이터
        GetDailyList = 42,
        #endregion

        #region 재고관리
        GET_INVENTORY_LIST = 80,
        UPDATE_INVENTORY_LIST,
        CREATE_INVENTORY_LIST,
        DELETE_INVENTORY_LIST,
        #endregion
    }
}
