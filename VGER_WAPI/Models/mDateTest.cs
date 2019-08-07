using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace VGER_WAPI.Models
{
    public class mDateTest
    {
        [BsonId]
        public ObjectId _Id { get; set; }

        public string Type { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
    }
}
