using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class Integration_Search_Response
    {
        public Integration_Search_Response()
        {
            IntegrationSearchDataList = new List<Integration_Search_Data>();
            Application_DataList = new List<Attributes>();
            ResponseStatus = new ResponseStatus();
        }

        public List<Integration_Search_Data> IntegrationSearchDataList { get; set; }
        public List<Attributes> Application_DataList { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
        public bool isCredentialExist { get; set; }
    }

    public class Integration_Search_Data
    {
        public string Application_Id { get; set; }
        public string Application_Name { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Keys { get; set; }
        public string UserKey { get; set; }
        public string Status { get; set; }
    }
}
