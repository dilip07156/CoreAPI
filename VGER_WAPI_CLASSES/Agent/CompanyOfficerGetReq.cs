using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class CompanyOfficerGetReq
    {         
        public string CompanyId { get; set; }
        public string ContactId { get; set; }
        public string UserRole { get; set; }
        public bool IsHeadOfficeUser { get; set; }
    }
}
