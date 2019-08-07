using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VGER_WAPI_CLASSES
{
    public class mBookingPositions
    {
        [BsonId]
        public ObjectId _Id { get; set; }
        [BsonIgnoreIfNull]
        public string Position_Id { get; set; }
        [BsonIgnoreIfNull]
        public string ItineraryLeg_Id { get; set; }
        [BsonIgnoreIfNull]
        public string PositionType { get; set; }
        [BsonIgnoreIfNull]
        public string Booking_Id { get; set; }
        [BsonIgnoreIfNull]
        public string Booking { get; set; }
        [BsonIgnoreIfNull]
        public string Product_Id { get; set; }
        [BsonIgnoreIfNull]
        public string ProductCode { get; set; }
        [BsonIgnoreIfNull]
        public string Product { get; set; }
        [BsonIgnoreIfNull]
        public string City { get; set; }
        [BsonIgnoreIfNull]
        public string Country { get; set; }
        [BsonIgnoreIfNull]
        public string ProductType { get; set; }
        [BsonIgnoreIfNull]
        public string GRIDINFO { get; set; }
        [BsonIgnoreIfNull]
        public string Supplier { get; set; }
        [BsonIgnoreIfNull]
        public string STATUS { get; set; }
        [BsonIgnoreIfNull]
        public string SatusDesc { get; set; }
        [BsonIgnoreIfNull]
        public Int16? ORDERNR { get; set; }
        [BsonIgnoreIfNull]
        public string ProductType_Id { get; set; }
        [BsonIgnoreIfNull]
        public string Resort_Id { get; set; }
        [BsonIgnoreIfNull]
        public string ParentResort_Id { get; set; }
        [BsonIgnoreIfNull]
        public string Supplier_Id { get; set; }
        [BsonIgnoreIfNull]
        public string SENDTYPE { get; set; }
        [BsonIgnoreIfNull]
        public string SENDADDR { get; set; }
        [BsonIgnoreIfNull]

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? STARTDATE { get; set; }
        [BsonIgnoreIfNull]
        public string STARTTIME { get; set; }
        [BsonIgnoreIfNull]
        public string STARTLOC { get; set; }
        [BsonIgnoreIfNull]
        public string DURATION { get; set; }
        [BsonIgnoreIfNull]

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? ENDDATE { get; set; }
        [BsonIgnoreIfNull]
        public string ENDTIME { get; set; }
        [BsonIgnoreIfNull]
        public string ENDLOC { get; set; }
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

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? VOUCHER_DT { get; set; }
        [BsonIgnoreIfNull]
        public Int32? VOUCHER_US { get; set; }
        [BsonIgnoreIfNull]
        public bool? AUTOSPOS { get; set; }
        [BsonIgnoreIfNull]
        public string POSMARKUP { get; set; }
        [BsonIgnoreIfNull]
        public string EmptyLegs { get; set; }
        [BsonIgnoreIfNull]
        public string MarkUp_Id { get; set; }
        [BsonIgnoreIfNull]
        public bool? MealsIncluded { get; set; }
        [BsonIgnoreIfNull]
        public bool? TicketsIncluded { get; set; }
        [BsonIgnoreIfNull]

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EarlyCheckIn { get; set; }
        [BsonIgnoreIfNull]
        public Int32? WashChangeRoom { get; set; }
        [BsonIgnoreIfNull]
        public bool? DiluteFOCPolicy { get; set; }
        [BsonIgnoreIfNull]
        public Int32? HotelStarRating { get; set; }
        [BsonIgnoreIfNull]
        public string MealPlan { get; set; }
        [BsonIgnoreIfNull]

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CancellationDate { get; set; }
        [BsonIgnoreIfNull]
        public string CancellationUser { get; set; }
        [BsonIgnoreIfNull]
        public string CancellationReason { get; set; }
        [BsonIgnoreIfNull]
        public string DriverContactNumber { get; set; }
        [BsonIgnoreIfNull]
        public string SupplierContact_Id { get; set; }
        [BsonIgnoreIfNull]
        public string SupplierTel { get; set; }
        [BsonIgnoreIfNull]
        public string SUPPCONFNR { get; set; }
        [BsonIgnoreIfNull]
        public string StaffName { get; set; }
        [BsonIgnoreIfNull]
        public string PositionUniqueID { get; set; }
        [BsonIgnoreIfNull]
        public bool? PriceFetched { get; set; }
        [BsonIgnoreIfNull]
        public string B2BMarkup_Id { get; set; }
        [BsonIgnoreIfNull]
        public bool? Porterage { get; set; }
        [BsonIgnoreIfNull]
        public bool? IsLocked { get; set; }
        [BsonIgnoreIfNull]
        public bool? IsSendToHotel { get; set; }
        [BsonIgnoreIfNull]
        public string Standard { get; set; }
        [BsonIgnoreIfNull]
        public string HOTELMEALPLAN { get; set; }
        [BsonIgnoreIfNull]
        public bool? ExcludeFromInvoice { get; set; }
        [BsonIgnoreIfNull]
        public string BuyCurrency_Id { get; set; }
        [BsonIgnoreIfNull]
        public string DriverName { get; set; }
        [BsonIgnoreIfNull]
        public string SwitchedPosition_Id { get; set; }
        [BsonIgnoreIfNull]
        public string HotelPLacer_ID { get; set; }
        [BsonIgnoreIfNull]
        public string CancellationPolicy { get; set; }
        [BsonIgnoreIfNull]
        public bool? GrdState { get; set; }
        [BsonIgnoreIfNull]
        public bool? ProductRequested { get; set; }
        [BsonIgnoreIfNull]
        public string InvoiceStatus { get; set; }
        [BsonIgnoreIfNull]
        public string MealPlan_ID { get; set; }
        [BsonIgnoreIfNull]
        public bool? GrdSpclOfferState { get; set; }
        [BsonIgnoreIfNull]
        public string MarkupDetail_ID { get; set; }
        [BsonIgnoreIfNull]
        public double? MarkupPerc { get; set; }
        [BsonIgnoreIfNull]
        public string MarkupCurr { get; set; }
        [BsonIgnoreIfNull]
        public string FlightNumber { get; set; }
        [BsonIgnoreIfNull]
        public bool? ONPackageORService { get; set; }
        [BsonIgnoreIfNull]
        public string MarkUpType { get; set; }
        [BsonIgnoreIfNull]
        public Decimal? MarkUpValue { get; set; }
        [BsonIgnoreIfNull]
        public Int32? ISREQUOTE { get; set; }
        [BsonIgnoreIfNull]
        public Decimal? TotalSAPAmount { get; set; }
        [BsonIgnoreIfNull]
        public string ExchangeRate_ID { get; set; }
        [BsonIgnoreIfNull]
        public string ExchangeRateDetail_ID { get; set; }
        [BsonIgnoreIfNull]
        public Decimal? ExchangeRate { get; set; }
        [BsonIgnoreIfNull]
        public Decimal? ExchangeRateSell { get; set; }

        [BsonIgnoreIfNull]
        public int? Priority { get; set; }

        [BsonIgnoreIfNull]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? OPTIONDATE { get; set; }
        [BsonIgnoreIfNull]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CancelDeadline { get; set; }
        [BsonIgnoreIfNull]
        public string EndSupplier_Id { get; set; }
        [BsonIgnoreIfNull]
        public string EndSupplier { get; set; }


        [BsonIgnoreIfNull]
        public string DriverLicenceNumber { get; set; }
        [BsonIgnoreIfNull]
        public string LicencePlate { get; set; }
        [BsonIgnoreIfNull]
        public string PROPMEMO { get; set; }
        [BsonIgnoreIfNull]
        public string HotelAdvice { get; set; }
    }
}
