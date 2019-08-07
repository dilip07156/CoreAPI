using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    public class mBookingRooms
    {
        [BsonId]
        public ObjectId _Id { get; set; }
        [BsonIgnoreIfNull]
        public string BookingRooms_ID { get; set; }
        [BsonIgnoreIfNull]
        public string Booking_Id { get; set; }
        [BsonIgnoreIfNull]
        public string BookingNumber { get; set; }
        [BsonIgnoreIfNull]
        public int? ROOMNO { get; set; }
        [BsonIgnoreIfNull]
        public string ProductTemplate_Id { get; set; }
        [BsonIgnoreIfNull]
        public string ProductTemplate { get; set; }
        [BsonIgnoreIfNull]
        public string ProductTemplateDesc { get; set; }
        [BsonIgnoreIfNull]
        public string Position_Id { get; set; }
        [BsonIgnoreIfNull]
        public string ProductRange_Id { get; set; }
        [BsonIgnoreIfNull]
        public string PersonType_Id { get; set; }
        [BsonIgnoreIfNull]
        public string PersonType { get; set; }
        [BsonIgnoreIfNull]
        public string Status { get; set; }
        [BsonIgnoreIfNull]
        public string AllocationUsed { get; set; }
        [BsonIgnoreIfNull]
        public string CrossPosition_Id { get; set; }
        [BsonIgnoreIfNull]
        public bool? Recurrsive { get; set; }
        [BsonIgnoreIfNull]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? RecDate { get; set; }
        [BsonIgnoreIfNull]
        public Int32? Age { get; set; }
        [BsonIgnoreIfNull]
        public string CrossBookingPax_Id { get; set; }
        [BsonIgnoreIfNull]
        public string Category_Id { get; set; }
        [BsonIgnoreIfNull]
        public bool? ApplyMarkup { get; set; }
        [BsonIgnoreIfNull]
        public bool? ExcludeFromInvoice { get; set; }
        [BsonIgnoreIfNull]
        public string PositionPricing_Id { get; set; }
        [BsonIgnoreIfNull]
        public string MealPlan_Id { get; set; }
        [BsonIgnoreIfNull]
        public string ProductRangeError { get; set; }
        [BsonIgnoreIfNull]
        public string ChargeBasis { get; set; }
        [BsonIgnoreIfNull]
        public Int16? RoomId { get; set; }
        [BsonIgnoreIfNull]
        public string ParentBookingRooms_Id { get; set; }
        [BsonIgnoreIfNull]
        public long? ISREQUOTE { get; set; }
    }
}
