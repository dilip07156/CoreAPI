using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class IntegrationMappingDataReq
    {
        public IntegrationMappingDataReq()
        {
            TypeEntityInfoList = new List<IntegrationMappingTypeEntityInfo>();
        }

        public string Application { get; set; }
        public string ApplicationName { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public string EditUser { get; set; }
        public string CreateUser { get; set; }

        //Search on Entity level
        public string IntegrationApplicationMapping_Id { get; set; }
        public string Type { get; set; }
        public string Entity { get; set; }

        //Search on item level
        public string IntegrationApplicationMappingItem_Id { get; set; }
        public string PartnerEntityName { get; set; }//3rd party Name
        public string PartnerEntityCode { get; set; }//3rd party Value/Code
        public string SystemEntityName { get; set; }//System Name
        public string SystemEntityCode { get; set; }//System value/Code

        //Search based on Entity and Type
        public List<IntegrationMappingTypeEntityInfo> TypeEntityInfoList { get; set; }
         

    }

    public class IntegrationMappingTypeEntityInfo
    {
        public string Entity { get; set; }
        public string Type { get; set; }
    }
}
