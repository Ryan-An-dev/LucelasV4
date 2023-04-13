using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataSender.ViewModels
{
    //이 클래스가 데이터를 분류해서 각 페이지에 뿌려준다. 

    public class DataDistributor 
    {
        IContainerProvider ContainerProvider;
        public DataDistributor(IContainerProvider containerProvider)
        {
            ContainerProvider = containerProvider;
        }
        public void OnRceivedData(ushort num,byte[] msg)
        {
            switch (num) {

                case 1: //Login Session
                    
                    break;
                case 2: //
                    break;
                case 3: // 
                    break;
            }
        }
    }
}
