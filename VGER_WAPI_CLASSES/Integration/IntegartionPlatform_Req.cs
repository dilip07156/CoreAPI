using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class IntegartionPlatform_Req
    {
        public IntegartionPlatform_Req()
        {
            Configurations = new List<IntegrationConfigurationInfo>();
        }

        public string Application { get; set; }
        public string ApplicationName { get; set; }

        public string Module { get; set; }
        public string ModuleName { get; set; }

        public string Action { get; set; } //Operation
        public string ActionName { get; set; } //OperationName

        public string ApplicationFieldId { get; set; }
        public string ApplicationFieldName { get; set; }

        public string SystemFieldId { get; set; }
        public string SystemFieldName { get; set; }

        public string BoundTypeId { get; set; }
        public string BoundType { get; set; }

        public string EntityName { get; set; }

        public string URL { get; set; }

        public string CreateUser { get; set; }
        public string EditUser { get; set; }

        public List<IntegrationConfigurationInfo> Configurations { get; set; }

        public int Start { get; set; }
        public int Length { get; set; }
        public string TypeName { get; set; }
    }
}
