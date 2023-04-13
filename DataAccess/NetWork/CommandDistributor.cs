using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.NetWork
{
    //CMD Distributor로 접속은 한 Class 에서 유지하려면 DI 를 사용해서 하나의 인터페이스로는 부족하다.
    //Receive는 따로 받고 CMD InterFace로 모든 커넥션을 유지시키는게 바람직하다.
    public class CommandDistributor
    {
        public CommandDistributor()
        {

        }



    }
}
