using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class Integration_Search_Request
    {
        public Integration_Search_Request()
        {

        }

        public string Application_Id { get; set; }
        public string UserId { get; set; }
        public bool IsExport { get; set; }
        public string ckUserCompanyId { get; set; }
        public string CreatedUser { get; set; }
        public string EditUser { get; set; }
    }
}
