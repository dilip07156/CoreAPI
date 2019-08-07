using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES.MIS
{
    public class MisSearchGetRes
    {
        public MisSearchGetRes()
        {
            Groups = new List<string>();

        }
        public string Type { get; set; }
        [BsonId]
        public ObjectId _Id;
        public string Item { get; set; }
        public int ItemSeq { get; set; }
        public List<string> Groups { get; set; }
        public string ItemUrl { get; set; }
    }
}
