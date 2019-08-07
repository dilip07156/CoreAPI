using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class AgentCompanyRes
    {
        public AgentCompanyRes()
        {
            AgentProperties = new List<AgentProperties>();
            ResponseStatus = new ResponseStatus();
        }

        public List<AgentProperties> AgentProperties;
        public ResponseStatus ResponseStatus { get; set; }
    }

    public class AgentProperties
    {
        public string Name { get; set; }
        public string VoyagerCompany_Id { get; set; }
        public string Code { get; set; }
    }
}
