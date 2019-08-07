using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace VGER_WAPI_CLASSES
{
    public class mApplications
    {
        [BsonId]
        public ObjectId _Id { get; set; }
        public string Application_Id { get; set; }
        public string Application_Name { get; set; }
        public string Key { get; set; }
        [BsonIgnoreIfNull(true)]
        public List<Integration_Configuration> Configurations { get; set; }
        public string Status { get; set; }
        public string CreateUser { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateDate { get; set; }
        public string EditUser { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime EditDate { get; set; }
    }

    public class Integration_Configuration
    {
        public string ConfigId { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public string Status { get; set; }
        public string CreateUser { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateDate { get; set; }
        public string EditUser { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime EditDate { get; set; }
    }
}
