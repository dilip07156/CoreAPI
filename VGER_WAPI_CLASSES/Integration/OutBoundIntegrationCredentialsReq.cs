using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class OutBoundIntegrationCredentialsReq
    {
        public string Application_Id { get; set; }
        public string Application_Name { get; set; }
        public string ConfigId { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }

        public string CreatedUser { get; set; }
        public string EditUser { get; set; }

        public int Length { get; set; }
        public int Start { get; set; }
    }
}
