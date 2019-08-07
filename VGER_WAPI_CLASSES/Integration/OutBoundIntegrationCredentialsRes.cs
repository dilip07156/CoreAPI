using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class OutBoundIntegrationCredentialsRes
    {
        public OutBoundIntegrationCredentialsRes()
        {
            OutBoundIntegrationSearchDataList = new List<OutBoundIntegration_Search_Data>();
            ResponseStatus = new ResponseStatus();
        }
        public List<OutBoundIntegration_Search_Data> OutBoundIntegrationSearchDataList { get; set; }
        public int TotalCount { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
        public bool isConfigExist { get; set; }
    }



    public class OutBoundIntegration_Search_Data
    {
        public string Application_Id { get; set; }
        public string Application_Name { get; set; }
        public string ConfigId { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string DecryptedValue { get; set; }
        public string CreatedUser { get; set; }
        public string EditUser { get; set; }
    }
}
