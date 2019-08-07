using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace VGER_WAPI_CLASSES
{
    public class mMongoPushRefesh_Master
    {
        [BsonId]
        public ObjectId _Id { get; set; } = ObjectId.GenerateNewId();
        public string MongoPushRefesh_Master_Id { get; set; }
        public string EntityType { get; set; }
        public string EntityName { get; set; }
        //The below field used for when service is running then it will fetched the DateTime of service started.
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime LastRefreshDate { get; set; }
        public string LastRefreshUser { get; set; }
        public int RefreshFrequency { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime NextRefreshDate { get; set; }
        public string ExecutionStatus { get; set; }
        public string NoOfRecords { get; set; }

        //The below field used for when service execution is done succesfully then it will fetched the DateTime.
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime LastSuccessfulRefresh { get; set; }
        public bool Status { get; set; } 
        public string CreateUser { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateDate { get; set; }
        public string EditUser { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EditDate { get; set; }
    }
}
