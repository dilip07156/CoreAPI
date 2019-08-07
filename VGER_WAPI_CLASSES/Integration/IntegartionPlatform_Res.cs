using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class IntegartionPlatform_Res
    {
        public IntegartionPlatform_Res()
        {
            ResponseStatus = new ResponseStatus();
            ApplicationList = new List<Attributes>();
            ModuleList = new List<AttributeValues>();
            ActionList = new List<AttributeValues>();
            AppFieldList = new List<AttributeValues>();
            SystemFieldList = new List<AttributeValues>();
            AppModuleActionInfoList = new List<IntegartionPlatform_Req>();
        }

        public string Id { get; set; }
        public int PlatformTotalCount { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
        public List<Attributes> ApplicationList { get; set; }
        public List<AttributeValues> ModuleList { get; set; }
        public List<AttributeValues> ActionList { get; set; }
        public List<AttributeValues> AppFieldList { get; set; }
        public List<AttributeValues> SystemFieldList { get; set; }

        public List<IntegartionPlatform_Req> AppModuleActionInfoList { get; set; }

        /*class AppModuleActionInfoList
        {

        }*/

    }
}
