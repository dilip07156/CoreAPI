using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI_CLASSES
{
    public class mMISMapping
    {
        [BsonId]
        public ObjectId _Id { get; set; }
        
        public string ItemType { get; set; }
        public string Item { get; set; }
        public string ItemUrl { get; set; }
        public int ItemSeq { get; set; }
        public List<string> UserGroups { get; set; } = new List<string>();
    }
}