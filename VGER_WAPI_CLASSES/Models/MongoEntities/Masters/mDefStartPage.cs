using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    public class mDefStartPage
    {
        [BsonId]
        public ObjectId _Id { get; set; } = ObjectId.GenerateNewId();
        public string StartPage_Id { get; set; }
        public string Name { get; set; }
        public string URL { get; set; }
        public bool? ForInternalUser { get; set; }
        public bool? ForExternalUser { get; set; }
    }
}
