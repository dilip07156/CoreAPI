using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    public class mProductType
    {
        [BsonId]
        public ObjectId _Id { get; set; }
        public string VoyagerProductType_Id { get; set; }
        public string Prodtype { get; set; }
        public string Busitypes { get; set; }
        public string ProductTypeInitial { get; set; }
        public string ChargeBasis { get; set; }
        public string ChargeBasisName { get; set; }
        public string Name { get; set; }
        public string CreateUser { get; set; }
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CreateDate { get; set; }
        public string EditUser { get; set; }
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EditDate { get; set; }
    }
}
