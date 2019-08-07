using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace VGER_WAPI_CLASSES
{
    public class mServiceDuration
    {
        [BsonId]
        public ObjectId _Id { get; set; }
        [BsonIgnoreIfNull]
        public string ServiceDuration_Id { get; set; }
        [BsonIgnoreIfNull]
        public string ProductTemplate_Id { get; set; }
        [BsonIgnoreIfNull]
        public string ProductTemplate { get; set; }
        [BsonIgnoreIfNull]
        public int Duration { get; set; }
    }
}
