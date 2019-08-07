using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    public class mProdCatDef
    {
        [BsonId]
        public ObjectId _Id { get; set; }
        public string VoyagerDefProductCategoryId { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public bool ForHotel { get; set; }
        public bool IsDefault { get; set; }
        public bool Standard { get; set; }
        public string CreateUser { get; set; }
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateDate { get; set; }
        public string EditUser { get; set; }
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime EditDate { get; set; }
    }
}
