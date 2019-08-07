using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    public class mMongoPushRefesh_Log
    {
        [BsonId]
        public ObjectId _Id { get; set; } = ObjectId.GenerateNewId();
        public string MongoPushRefesh_Log_Id { get; set; }
        public string MongoPushRefesh_Master_Id { get; set; }

        public string ExecutionUser { get; set; }
        public string ExecutionStatus { get; set; }
        public List<string> ErrorMessage { get; set; }
        public string Type { get; set; }
        public string TotalRecords { get; set; }
        public string DeletedRecords { get; set; }
        public string UpsertRecords { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime StartDateTime { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EndDateTime { get; set; }
    }
}