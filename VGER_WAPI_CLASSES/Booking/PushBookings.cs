using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    /// <summary>
    /// Request object for sending the QRF to OPS module.
    /// Pass the QRFID as BookingNumber
    /// </summary>
    public class PushBookingsSetReq
    {
        /// <summary>
        /// QRFID of the quote
        /// </summary>
        public string QRFID { get; set; }
        /// <summary>
        /// BookingNumber of the quote
        /// </summary>
        public string BookingNumber { get; set; }
        /// <summary>
        /// DepartureId of the quote
        /// </summary>
        public string DepartureId { get; set; }
        /// <summary>
        /// Requesting User
        /// </summary>
        public string UserName { get; set; }
		/// <summary>
		/// GoAheadId
		/// </summary>
		public string GoAheadId { get; set; }
	}

    /// <summary>
    /// Response object for sending the QRF to OPS module.
    /// </summary>
    public class PushBookingsSetRes
    {
        public PushBookingsSetRes()
        {
            ResponseStatus = new ResponseStatus();
        }
        /// <summary>
        /// Status of the Request
        /// </summary>
        public ResponseStatus ResponseStatus { get; set; }

        /// <summary>
        /// QRFID of the quote
        /// </summary>
        public string QuoteNumber { get; set; }

        /// <summary>
        /// BookingNumber generated in the OPS for the given Departure
        /// </summary>
        public string BookingNumber { get; set; }
    }

    public class BookingHeader
    {
        public string NAME { get; set; }
        public string Booking_Id { get; set; }
        public string BUSITYPE { get; set; }
        public string BOOKING { get; set; }

        public DateTime? BOOKINGDAT { get; set; }
        public string CUSTCONT { get; set; }
        public string CUSTREF { get; set; }
        public string SENDTYPE { get; set; }
        public string SENDADDR { get; set; }
        public string Markup_Id { get; set; }
        public string Partner_Id { get; set; }
        public string Contact_Id { get; set; }


        public DateTime? STARTDATE { get; set; }
        public DateTime? ENDDATE { get; set; }
        public string STATUS { get; set; }
        public string PROPMEMO { get; set; }
        public DateTime? STATUS_DT { get; set; }
        public DateTime? CREA_DT { get; set; }
        public DateTime? MODI_DT { get; set; }


        public DateTime? POSMODI_DT { get; set; }
        public Int32? POSMODI_US { get; set; }
        public string CREA_US { get; set; }
        public string MODI_US { get; set; }
        public string Nationality_Id { get; set; }
        public string Staff_Id { get; set; }
        public string STATUS_US { get; set; }
        public DateTime? QuoteBy { get; set; }


        public string HotelStarRating { get; set; }
        public bool? ETNPreferredSuppliers { get; set; }
        public string GroupLanguage_Id { get; set; }
        public string HotelBoard { get; set; }
        public bool? ApplyMarketRate { get; set; }
        public Int32? TotalNights { get; set; }
        public string HotelBreakfastType { get; set; }
        public string HotelLocation { get; set; }
        public string WeightBookingChange { get; set; }
        public string Standard { get; set; }
        public bool? GrpHeaderViewState { get; set; }
        public bool? GrpDetailViewState { get; set; }
        public string BuildBy { get; set; }
        public bool? ISSUBAGENT { get; set; }

        public bool? IsPackagedPositionExists { get; set; }



        public string PRIORITY { get; set; }
        public string Currency_Id { get; set; }
        public string ServicesOnly { get; set; }
        public string FixedDates { get; set; }
        public string RoomAssignmentReqd { get; set; }
        public string TourLeader_Name { get; set; }
        public string TourLeader_Contact { get; set; }
        public string STATUSDesc { get; set; }
        public string GridInfo { get; set; }

        public string TravelReason { get; set; }
        public string Destination_Id { get; set; }
        public string Descriptor { get; set; }
        public string CATEGORY_ID { get; set; }
        public string IsSubAgent { get; set; }
        public string STA { get; set; }
        public string ProfitShare { get; set; }
        public string ProcessStage { get; set; }
        public string ParentBooking_Id { get; set; }


        public string OperatorEmail { get; set; }
        public string Sales_Id { get; set; }
        public string PARTNER { get; set; }
        public string IsNewMode { get; set; }
        public string CostSheetBased { get; set; }
        public string IsLocked { get; set; }

        public string NOTE { get; set; }

        public string subBusinessType { get; set; }
        public string ContactEmail { get; set; }
        public string SalesSupport_ID { get; set; }
        public string IsFreePlace { get; set; }
        public string CoachOverrideType { get; set; }
        public string IsCoachOverride { get; set; }
        public string CoachOverridePax { get; set; }
        public string IsBookingSeason { get; set; }
        public string IsLDC { get; set; }
        public string IsPorterage { get; set; }
        public string IsTip { get; set; }
        public string IsDRVMeal { get; set; }
        public string IsDRVAcco { get; set; }
        public string IsEmptyLeg { get; set; }
        public string SystemCompany_Id {get;set; }
        public string SalesProductAccountant_ID { get; set; }
        public string BusinessType { get; set; }
        public string Division_ID { get; set; }
    }

    public class TemplateBookingPaxGrid
    {
        public string BookingPax_Id { get; set; }
        public string Booking_Id { get; set; }
        public string PersonType_Id { get; set; }
        public int PERSONS { get; set; }
        public string PERSTYPE { get; set; }
        public int Editable { get; set; }
        public int ID { get; set; }
        public int? AGE { get; set; }
        public string Status { get; set; }
    }

    public class TemplateBookingRoomsGrid
    {
        public string BookingRooms_ID { get; set; }
        public string Booking_Id { get; set; }
        public int? ROOMNO { get; set; }
        public string ProductTemplate_Id { get; set; }
        public string PersonType_Id { get; set; }
        public string Category_Id { get; set; }
        public string SUBPROD { get; set; }
        public string PersonType { get; set; }
        public string Name { get; set; }
        public Int16? ID { get; set; }
        public int? Age { get; set; }
        public string Status { get; set; }
    }

    public class BookingHeaderPaxBreak
    {
        public string BookingHeaderPaxBreak_ID { get; set; }
        public string Booking_ID { get; set; }
        public int? PAXBREAK { get; set; }
    }

    public class PushBookingsSetDBRes
    {
        public bool Column1 { get; set; }
    }

    public class PushBookingsSetIDRes
    {
        public Guid? ID { get; set; }
    }
    public class PushBookingsSetIDNameRes
    {
        public Guid? ID { get; set; }
        public string Name { get; set; }
    }

    public class PushMarkupSetDBRes
    {
        public Guid? Markup_Id { get; set; }
        public Guid? MarkupDetail_Id { get; set; }
    }

    public class PositionIdMappings
    {
        public string QRFID { get; set; }
        public Guid? Booking_Id { get; set; }
        public Guid? SalesPosition_Id { get; set; }
        public Guid? OpsPosition_Id { get; set; }

    }

    public class PositionMarkupMappings
    {

        public string QRFID { get; set; }
        public Guid? Booking_Id { get; set; }
        public Guid? SalesPosition_Id { get; set; }
        public Guid? OpsPosition_Id { get; set; }
        public Guid? Markup_Id { get; set; }
        public Guid? MarkupDetail_Id { get; set; }
    }

    //public class BookingFixes
    //{
    //    public string BookingFixes_Id { get; set; }
    //    public string position_id { get; set; }
    //    public string positionprice_id { get; set; }
    //    public string Description { get; set; }
    //    public string Status { get; set; }
    //}
    //public class UpdateOperationDetailSetDbRes
    //{
    //    public bool UpdateOperationDetailId { get; set; }
    //    public UpdateOperationDetails_RQ Request { get; set; }
    //    public string CurrentBookingStatus { get; set; }

    //    public List<UpdateOperationDetails_RS_ByPosition> UpdateOperationDetails { get; set; }
    //    //public List<BookingFixes> BookingFixes { get; set; }
    //}
}
