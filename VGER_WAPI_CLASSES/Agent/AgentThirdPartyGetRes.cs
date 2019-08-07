using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class AgentThirdPartyGetRes
    {
        public AgentThirdPartyGetRes()
        {
            ResponseStatus = new ResponseStatus();
        }

        public string CompanyId { get; set; }
        public string ContactId { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
    }
}
