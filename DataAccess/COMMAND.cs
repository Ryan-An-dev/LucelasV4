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
        CustomerList = 7,
        #endregion


        #region 입출금페이지
        GetBankHistory =20,
        CreateBankHistory=21,
        UpdateBankHistory=22,
        DeleteBankHistory=23,
        GetConnectedContract = 24,
        #endregion

        #region 계약리스트
        GetContractList = 30,
        CREATECONTRACT = 31,
        #endregion


        #region 통계데이터
        GetDailyList = 42,
        #endregion
    }
}
