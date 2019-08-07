using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class IntegrationMappingDataRes
    {
        public IntegrationMappingDataRes()
        {
            ResponseStatus = new ResponseStatus();
            IntegrationMappingList = new List<IntegrationMappingInfo>();
            IntegrationMappingItemList = new List<IntegrationMappingItemInfo>();
        }

        public int TotalCount { get; set; }
        public List<IntegrationMappingInfo> IntegrationMappingList { get; set; }
        public List<IntegrationMappingItemInfo> IntegrationMappingItemList { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
    }

    public class IntegrationMappingInfo
    {
        public string IntegrationApplicationData_Id { get; set; }
        public string Application { get; set; }
        public string ApplicationName { get; set; }
        public string IntegrationApplicationMapping_Id { get; set; }
        public string Type { get; set; }
        public string Entity { get; set; }
    }

    public class IntegrationMappingItemInfo
    {
        public string Application { get; set; }
        public string ApplicationName { get; set; }
        public string Type { get; set; }
        public string Entity { get; set; }
        public string IntegrationApplicationMappingItem_Id { get; set; }
        public string PartnerEntityName { get; set; }//3rd party Name
        public string PartnerEntityCode { get; set; }//3rd party Value/Code
        public string SystemEntityName { get; set; }//System Name
        public string SystemEntityCode { get; set; }//System value/Code
    }

    //Can be used is any export to excel is needed
    public class IntegrationMappingDataInfo
    {
        public string IntegrationApplicationData_Id { get; set; }
        public string Application { get; set; }
        public string ApplicationName { get; set; }
        public string IntegrationApplicationMapping_Id { get; set; }
        public string Type { get; set; }
        public string Entity { get; set; }

        public string IntegrationApplicationMappingItem_Id { get; set; }
        public string PartnerEntityName { get; set; }//3rd party Name
        public string PartnerEntityCode { get; set; }//3rd party Value/Code
        public string SystemEntityName { get; set; }//System Name
        public string SystemEntityCode { get; set; }//System value/Code
    }
}
