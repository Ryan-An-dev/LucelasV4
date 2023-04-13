using DataAccess.NetWork;

namespace DataAccess.Interface
{
    public interface IBaseRepository
    {
        public void SetReceiver(INetReceiver netReceiver);
    }
}