using DataAccess.NetWork;
using Newtonsoft.Json.Linq;

namespace DataAccess.Interface
{
    public interface IBaseRepositoryV2
    {
        public void SetReceiver(INetReceiver netReceiver);

        public void Create(JObject msg);
        public void Read(JObject msg);

        public void Update(JObject msg);
        public void Delete(JObject msg);
    }
}