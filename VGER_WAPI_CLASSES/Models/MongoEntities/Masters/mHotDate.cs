using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    public class mHotDate
    {
        [BsonId]
        public ObjectId _Id { get; set; } = ObjectId.GenerateNewId();
        public string VoyagerEvents_Id { get; set; }
        public string Resort_Id { get; set; }
        public string Name { get; set; }
        public string CountryName { get; set; }
        public string CityName { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime StartDate { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime EndDate { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string ImportYear { get; set; }
        public string ParentResort_Id { get; set; }
        public string CreateUser { get; set; }
       // [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime CreateDate { get; set; }
        public string EditUser { get; set; }
       // [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime EditDate { get; set; }
    }
}
