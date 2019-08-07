using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class AgentCompanyReq
    {
        public string AgentName { get; set; }
        public string CompanyId { get; set; }
        public string UserId { get; set; }
        public string SpecificFilterName { get; set; }
        public string[] Roles { get; set; }
    }
}
