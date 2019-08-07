using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace VGER_WAPI_CLASSES
{
    public class mFOCDilution
    {
        [BsonId]
        public ObjectId _Id { get; set; }
        [BsonIgnoreIfNull]
        public string FOCDilution_Id { get; set; }
        [BsonIgnoreIfNull]
        public string Booking_Id { get; set; }
        [BsonIgnoreIfNull]
        public string BookingNumber { get; set; }
        [BsonIgnoreIfNull]
        public string Position_Id { get; set; }
        [BsonIgnoreIfNull]
        public int? PositionNumber { get; set; }
        [BsonIgnoreIfNull]
        public string ProductTemplate_Id { get; set; }
        [BsonIgnoreIfNull]
        public string ProductTemplate { get; set; }
        [BsonIgnoreIfNull]
        public decimal? TotalUnits { get; set; }
        [BsonIgnoreIfNull]
        public decimal? FreeUnits { get; set; }
        [BsonIgnoreIfNull]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? FOCDate { get; set; }
        [BsonIgnoreIfNull]
        public decimal? FOCValue { get; set; }
    }
}
