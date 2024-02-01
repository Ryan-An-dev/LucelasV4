using DataAccess.Interface;
using DataAgent.BaseAgent;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAgent
{
    public class StatisticsDataAgent : PageDataAgent
    {
        IStatisticsRepository repo = null;
        public StatisticsDataAgent(IStatisticsRepository repo) : base(repo)
        {
            this.repo = repo;
        }

        public void GetDailyList(JObject msg)
        {
            this.repo.GetDailyList(msg);
        }
        public void GetComparisonList(JObject msg)
        {
            this.repo.GetComparisonList(msg);
        }

    }
}
