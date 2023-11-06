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
    public class EmployeeDataAgent : PageDataAgent
    {
        private IEmployeeRepository repo = null;

        public EmployeeDataAgent(IEmployeeRepository repo) : base(repo)
        {
            this.repo = repo;
        }
        public void GetEmployeeList(JObject msg)
        {
            this.repo.GetEmployee(msg);
        }
        public void CreateEmployeeList(JObject msg)
        {
            this.repo.CreateEmployee(msg);
        }
        public void UpdateEmployeeList(JObject msg)
        {
            this.repo.UpdateEmployee(msg);
        }
        public void DeleteEmployeeList(JObject msg)
        {
            this.repo.DeleteEmployee(msg);
        }
    }
}
