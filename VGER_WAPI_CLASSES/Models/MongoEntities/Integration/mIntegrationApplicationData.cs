using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class mIntegrationApplicationData
    {
        [BsonId]
        [Newtonsoft.Json.JsonProperty("_id")]
        public string IntegrationApplicationData_Id { get; set; }
        public string Application { get; set; }
        public string ApplicationName { get; set; }
        [BsonIgnoreIfNull(true)]
        public List<ApplicationMapping> ApplicationMappings { get; set; }
        public string Status { get; set; }
        public string CreateUser { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateDate { get; set; }
        public string EditUser { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime EditDate { get; set; }
    }

    public class ApplicationMapping
    {
        [BsonId]
        [Newtonsoft.Json.JsonProperty("_id")]
        public string IntegrationApplicationMapping_Id { get; set; }
        public string Type { get; set; }//Master|Picklist etc.
        public string Entity { get; set; }//Country|City|Title etc.
        [BsonIgnoreIfNull(true)]
        public List<ApplicationMappingData> Mappings { get; set; }
        public string Status { get; set; }
        public string CreateUser { get; set; }
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public string CreateDate { get; set; }
        public string EditUser { get; set; }
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public string EditDate { get; set; }
    }

    public class ApplicationMappingData
    {
        [BsonId]
        [Newtonsoft.Json.JsonProperty("_id")]
        public string IntegrationApplicationMappingItem_Id { get; set; }
        public string PartnerEntityName { get; set; }//3rd party Name
        public string PartnerEntityCode { get; set; }//3rd party Value/Code
        public string SystemEntityName { get; set; }//System Name
        public string SystemEntityCode { get; set; }//System value/Code
        public string Status { get; set; }
        public string CreateUser { get; set; }
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public string CreateDate { get; set; }
        public string EditUser { get; set; }
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public string EditDate { get; set; }
    }
}
