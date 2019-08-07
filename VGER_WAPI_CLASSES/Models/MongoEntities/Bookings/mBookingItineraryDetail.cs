using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    public class mBookingItineraryDetail
    {
        [BsonId]
        public ObjectId _Id { get; set; }

        [BsonIgnoreIfNull] public string ItineraryDetail_Id { get; set; }
        [BsonIgnoreIfNull] public string Booking_Id { get; set; }
        [BsonIgnoreIfNull] public string BookingNumber { get; set; }
        [BsonIgnoreIfNull] public string Position_Id { get; set; }
        [BsonIgnoreIfNull] public string PositionNumber { get; set; }
        [BsonIgnoreIfNull] public string GridInfo { get; set; }
        [BsonIgnoreIfNull] public string ItineraryLeg_Id { get; set; }
        [BsonIgnoreIfNull] public string CountryName { get; set; }
        [BsonIgnoreIfNull] public string CityName { get; set; }
        [BsonIgnoreIfNull] public int? SortOrder { get; set; }
        [BsonIgnoreIfNull] public string Status { get; set; }
        [BsonIgnoreIfNull] public string ProductType { get; set; }
        [BsonIgnoreIfNull] public string Description { get; set; }
        [BsonIgnoreIfNull] public string Details { get; set; }
        [BsonIgnoreIfNull] public string LegType { get; set; }
        [BsonIgnoreIfNull] public bool? ISINCLUDED { get; set; }
        [BsonIgnoreIfNull] [BsonDateTimeOptions(Kind = DateTimeKind.Local)] public DateTime? STARTDATE { get; set; }
        [BsonIgnoreIfNull] public string STARTTIME { get; set; }
        [BsonIgnoreIfNull] public string STARTLOC { get; set; }
        [BsonIgnoreIfNull] [BsonDateTimeOptions(Kind = DateTimeKind.Local)] public DateTime? ENDDATE { get; set; }
        [BsonIgnoreIfNull] public string ENDTIME { get; set; }
        [BsonIgnoreIfNull] public string ENDLOC { get; set; }
        [BsonIgnoreIfNull] public string SHOWTIME { get; set; }
        [BsonIgnoreIfNull] public string PROPMEMO { get; set; }
        [BsonIgnoreIfNull] public bool? ISDELETED { get; set; }
        [BsonIgnoreIfNull] [BsonDateTimeOptions(Kind = DateTimeKind.Local)] public DateTime? CREA_DT { get; set; }
        [BsonIgnoreIfNull] public string CREA_TI { get; set; }
        [BsonIgnoreIfNull] public string CREA_US { get; set; }
        [BsonIgnoreIfNull] [BsonDateTimeOptions(Kind = DateTimeKind.Local)] public DateTime? MODI_DT { get; set; }
        [BsonIgnoreIfNull] public string MODI_TI { get; set; }
        [BsonIgnoreIfNull] public string MODI_US { get; set; }
        [BsonIgnoreIfNull] public bool? IncludeAddress { get; set; }
        [BsonIgnoreIfNull] public bool? LDCTravelRoute { get; set; }
        [BsonIgnoreIfNull] public bool? IsTimeEditable { get; set; }
        [BsonIgnoreIfNull] public string UniqueIdentityValue { get; set; }
        [BsonIgnoreIfNull] public int? DayNo { get; set; }
        [BsonIgnoreIfNull] public string City_Id { get; set; }
        [BsonIgnoreIfNull] public string Country_Id { get; set; }
        [BsonIgnoreIfNull] public string Location { get; set; }
        [BsonIgnoreIfNull] [BsonDateTimeOptions(Kind = DateTimeKind.Local)] public DateTime? Date_Applicable_From { get; set; }
        [BsonIgnoreIfNull] [BsonDateTimeOptions(Kind = DateTimeKind.Local)] public DateTime? Date_Applicable_To { get; set; }
        [BsonIgnoreIfNull] public string Product_Id { get; set; }
        [BsonIgnoreIfNull] public bool? SYNCPARENT { get; set; }
    }
}
