using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class mIntegrationPlatform
    {
        [BsonId]
        [Newtonsoft.Json.JsonProperty("_id")]
        public string IntegrationPlatform_Id { get; set; }
        public string Application { get; set; }
        public string ApplicationName { get; set; }
        [BsonIgnoreIfNull(true)]
        public List<ModuleActionInfo> Modules { get; set; }
    }

    public class ModuleActionInfo
    {
        public string Module { get; set; }
        public string ModuleName { get; set; }
        [BsonIgnoreIfNull(true)]
        public List<ActionInfo> Actions { get; set; }
        public string Status { get; set; }
        public string CreateUser { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateDate { get; set; }
        public string EditUser { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime EditDate { get; set; }
    }

    public class IntegrationConfigurationInfo
    {   
        public string ConfigId { get; set; }
        public string URL { get; set; }
        public string ApplicationFieldName { get; set; }
        public string SystemFieldName { get; set; }
        public string PlatformTypeName { get; set; }
        public string BoundType { get; set; }
        public string EntityName { get; set; }
        public string Status { get; set; }
        public string CreateUser { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateDate { get; set; }
        public string EditUser { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime EditDate { get; set; }
    }

    public class ActionInfo
    {
        public string Action { get; set; }
        public string ActionName { get; set; }
        public List<IntegrationConfigurationInfo> Configurations { get; set; }
        public string Status { get; set; }
        public string CreateUser { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateDate { get; set; }
        public string EditUser { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime EditDate { get; set; }
        public string TypeName { get; set; }
    }
}
