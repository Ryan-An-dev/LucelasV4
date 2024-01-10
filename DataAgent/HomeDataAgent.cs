using DataAccess;
using DataAccess.Interface;
using DataAccess.NetWork;
using DataAgent.BaseAgent;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAgent
{
    public class HomeDataAgent : PageDataAgent
    {
        private IHomePageRepository repo = null;

        public HomeDataAgent(IHomePageRepository repo) : base(repo)
        {
            this.repo = repo;
        }
        public void GetHomeData(JObject msg)
        {
            this.repo.GetHomePage(msg);
        }
        
    }
}
