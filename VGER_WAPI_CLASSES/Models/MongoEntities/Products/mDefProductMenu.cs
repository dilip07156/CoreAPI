using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    public class mDefProductMenu
    {
        [BsonId]
        public ObjectId _Id { get; set; }
        [BsonIgnoreIfNull]
        public string ProductMenu_Id { get; set; }
        [BsonIgnoreIfNull]
        public string Product_Id { get; set; }
        [BsonIgnoreIfNull]
        public string ProductName { get; set; }
        [BsonIgnoreIfNull]
        public string ProductMenuDesc { get; set; }
        [BsonIgnoreIfNull]
        public bool? Status { get; set; }
    }
}
