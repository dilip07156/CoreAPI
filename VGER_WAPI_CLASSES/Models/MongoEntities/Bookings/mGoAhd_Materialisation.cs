using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    public class mGoAhd_Materialisation
    {
        [BsonId]
        public ObjectId _Id { get; set; }
        [BsonIgnoreIfNull]

        public string BookingPax_Id { get; set; }
        [BsonIgnoreIfNull]
        public string Booking_Id { get; set; }
        [BsonIgnoreIfNull]
        public string PersonType_Id { get; set; }
        [BsonIgnoreIfNull]
        public string PersonType { get; set; }
        [BsonIgnoreIfNull]
        public string PERSONS { get; set; }
        [BsonIgnoreIfNull]
        public string AGE { get; set; }
        [BsonIgnoreIfNull]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? STARTDATE { get; set; }
        [BsonIgnoreIfNull]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? ENDDATE { get; set; }
        [BsonIgnoreIfNull]
        public bool? AdHocPassenger { get; set; }
        [BsonIgnoreIfNull]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CREA_DT { get; set; }
        [BsonIgnoreIfNull]
        public string CREA_US { get; set; }
        [BsonIgnoreIfNull]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? MODI_DT { get; set; }
        [BsonIgnoreIfNull]
        public string MODI_US { get; set; }
        [BsonIgnoreIfNull]
        public string Status { get; set; }
        [BsonIgnoreIfNull]
        public string CrossPosition_Id { get; set; }
        [BsonIgnoreIfNull]
        public string RoomType_Id { get; set; }
        [BsonIgnoreIfNull]
        public int? ISREQUOTE { get; set; }
        [BsonIgnoreIfNull]
        public string ClientName { get; set; }
    }
}
