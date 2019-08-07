using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    public class mBookingPositionPricing
    {
        [BsonId]
        public ObjectId _Id { get; set; }
        [BsonIgnoreIfNull] public string PositionPricing_Id { get; set; }
        [BsonIgnoreIfNull] public string Booking_Id { get; set; }
        [BsonIgnoreIfNull] public string BookingNumber { get; set; }
        [BsonIgnoreIfNull] public string Position_Id { get; set; }
        [BsonIgnoreIfNull] public int PositionNumber { get; set; } = 0;
        [BsonIgnoreIfNull] public string ProductTemplate_Id { get; set; }
        [BsonIgnoreIfNull] public string ProductTemplate { get; set; }
        [BsonIgnoreIfNull] public string PersonType_Id { get; set; }
        [BsonIgnoreIfNull] public string PersonType { get; set; }
        [BsonIgnoreIfNull] public string ProductPrice_Id { get; set; }
        [BsonIgnoreIfNull] public string Product_Id { get; set; }
        [BsonIgnoreIfNull] public string ProductName { get; set; }
        [BsonIgnoreIfNull] public string BookingRooms_Id { get; set; }
        [BsonIgnoreIfNull] public int? Quantity { get; set; }
        [BsonIgnoreIfNull] public int? BPOSPRICID { get; set; }
        [BsonIgnoreIfNull] public int? BOOKINGID { get; set; }
        [BsonIgnoreIfNull] public int? BOOKPOSID { get; set; }
        [BsonIgnoreIfNull] public int? SUBPRODID { get; set; }
        [BsonIgnoreIfNull] public int? PERSTYPEID { get; set; }
        [BsonIgnoreIfNull] public string AGEMIN { get; set; }
        [BsonIgnoreIfNull] public string AGEMAX { get; set; }
        [BsonIgnoreIfNull] public string MINQTY { get; set; }
        [BsonIgnoreIfNull] public string MAXQTY { get; set; }
        [BsonIgnoreIfNull] public string BPRICEC { get; set; }
        [BsonIgnoreIfNull] public string BPRICECCUR { get; set; }
        [BsonIgnoreIfNull] public string BPRICE { get; set; }
        [BsonIgnoreIfNull] public string BREQPRICE { get; set; }
        [BsonIgnoreIfNull] public string BQUOTPRICE { get; set; }
        [BsonIgnoreIfNull] public string BPRICECUR { get; set; }
        [BsonIgnoreIfNull] [BsonDateTimeOptions(Kind = DateTimeKind.Local)] public DateTime? CREA_DT { get; set; }
        [BsonIgnoreIfNull] public string CREA_TI { get; set; }
        [BsonIgnoreIfNull] public string CREA_US { get; set; }
        [BsonIgnoreIfNull] [BsonDateTimeOptions(Kind = DateTimeKind.Local)] public DateTime? MODI_DT { get; set; }
        [BsonIgnoreIfNull] public string MODI_TI { get; set; }
        [BsonIgnoreIfNull] public string MODI_US { get; set; }
        [BsonIgnoreIfNull] public decimal? BudgetPrice { get; set; }
        [BsonIgnoreIfNull] public string Action { get; set; }
        [BsonIgnoreIfNull] public Guid? BuyCurrency_Id { get; set; }
        public string Status { get; set; }
        [BsonIgnoreIfNull] public Guid? ProductRange_Id { get; set; }
        [BsonIgnoreIfNull] public bool? ConfirmedReqPrice { get; set; }
        [BsonIgnoreIfNull] [BsonDateTimeOptions(Kind = DateTimeKind.Local)] public DateTime? StartDate { get; set; }
        [BsonIgnoreIfNull] [BsonDateTimeOptions(Kind = DateTimeKind.Local)] public DateTime? EndDate { get; set; }
        [BsonIgnoreIfNull] public string DayPattern { get; set; }
        [BsonIgnoreIfNull] public Guid? Category_Id { get; set; }
        [BsonIgnoreIfNull] public string Category { get; set; }
        [BsonIgnoreIfNull] public decimal? B2BPercMarkup { get; set; }
        [BsonIgnoreIfNull] public decimal? SellBudgetPrice { get; set; }
        [BsonIgnoreIfNull] public string FinalSellPrice { get; set; }
        [BsonIgnoreIfNull] public Guid? BookingSeason_ID { get; set; }
        [BsonIgnoreIfNull] public Guid? MealPlan_Id { get; set; }
        [BsonIgnoreIfNull] public decimal? ContractedSellPrice { get; set; }
        [BsonIgnoreIfNull] public string ContractedSellPriceCurr { get; set; }
        [BsonIgnoreIfNull] public int? Capacity { get; set; }
        [BsonIgnoreIfNull] public Guid? SellContract_Id { get; set; }
        [BsonIgnoreIfNull] public Guid? BuyContract_Id { get; set; }
        [BsonIgnoreIfNull] public decimal? FinalBUYPrice { get; set; }
        [BsonIgnoreIfNull] public Guid? SpecialOfferCombinedEntry_ID { get; set; }
        [BsonIgnoreIfNull] public string InvForPax { get; set; }
        [BsonIgnoreIfNull] public int? InvNumber { get; set; }
        [BsonIgnoreIfNull] public int? AGE { get; set; }

    }
}
