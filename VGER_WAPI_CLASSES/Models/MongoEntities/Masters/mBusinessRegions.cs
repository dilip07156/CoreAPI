using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace VGER_WAPI_CLASSES
{
    public class mBusinessRegions
    {
        [BsonId]
        public ObjectId _Id { get; set; } = ObjectId.GenerateNewId();
        public string BusinessRegion_Id { get; set; }
        public string BusinessRegion { get; set; }
        public string Status { get; set; }
        [BsonIgnoreIfNull]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? Crea_Dt { get; set; }
        [BsonIgnoreIfNull]
        public string Crea_US { get; set; }
        [BsonIgnoreIfNull]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? Modi_Dt { get; set; }
        [BsonIgnoreIfNull]
        public string Modi_US { get; set; }
    }
}
