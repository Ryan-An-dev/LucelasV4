﻿using DataAccess.NetWork;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interface
{
    public interface IStatisticsRepository : IBaseRepository
    {
        #region LIST CRUD

        public void GetDailyList(JObject msg);

        public void GetComparisonList(JObject msg);

        #endregion

    }
}
