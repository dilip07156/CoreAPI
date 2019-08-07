using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace VGER_WAPI_CLASSES
{
    /// <summary>
    /// Mongo Object for Booking Collection on its whole
    /// </summary>
    public class Bookings
    {
        /// <summary>
        /// SQL Column: Booking_Id
        /// </summary>
        [BsonId]
        [Newtonsoft.Json.JsonProperty("_id")]
        public string Booking_Id { get; set; }
        /// <summary>
        /// SQL Column: Booking
        /// </summary>
        public string BookingNumber { get; set; }

        public string SystemCompany_Id { get; set; }
        /// <summary>
        /// SQL Column: NOTE
        /// </summary>
        public string QRFID { get; set; }
        /// <summary>
        /// SQL Column: CustRef
        /// </summary>
        public string CustRef { get; set; }
        /// <summary>
        /// SQL Column: GridInfo
        /// </summary>
        public string GridInfo { get; set; }
        /// <summary>
        /// SQL Column: ParentBooking_Id
        /// </summary>
        public string ParentBooking_Id { get; set; }
        public string ParentBookingNumber { get; set; }
        public string ParentBooking_Name { get; set; }

        /// <summary>
        /// SQL Column: BOOKINGDAT
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? BookingDate { get; set; }
        /// <summary>
        /// SQL Column: BUSITYPE
        /// </summary>
        public string BusiType { get; set; }
        /// <summary>
        /// SQL Column: BusinessType
        /// </summary>
        public string BusinessType { get; set; }

        /// <summary>
        /// SQL Column: Destination
        /// </summary>
        public string Destination { get; set; }

        /// <summary>
        /// SQL Column: STARTDATE
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? STARTDATE { get; set; }
        /// <summary>
        /// SQL Column: ENDDATE
        /// </summary>
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? ENDDATE { get; set; }
        /// <summary>
        /// SQL Column: TotalNights
        /// </summary>
        public int? Duration { get; set; }
        /// <summary>
        /// SQL Column: STATUS
        /// </summary>
        public string STATUS { get; set; }

        public BookingCompanyInfo AgentInfo { get; set; }
        public BookingStaffDetails StaffDetails { get; set; }

        /// <summary>
        /// SQL Column: Currency_Id
        /// </summary>
        public string SellCurrency_Id { get; set; }
        public string SellCurrency_Name { get; set; }
        public BookingCommercials Commercials { get; set; }

        public BookingPreferences Preferences { get; set; }
        public BookingGuestDetails GuestDetails { get; set; }

        public string TourLeader_Name { get; set; }
        public string TourLeader_Contact { get; set; }

        public List<TemplateBookingPaxGrid> BookingPax { get; set; }
        public List<TemplateBookingRoomsGrid> BookingRooms { get; set; }
        public List<BookingHeaderPaxBreak> PaxSlabs { get; set; }
        public List<PassengerDetails> RoomingList { get; set; } = new List<PassengerDetails>();
        public List<Positions> Positions { get; set; }
        public ExchangeRateSnapshot ExchangeRateSnapshot { get; set; }
        public List<Fixes> Fixes { get; set; }
        public GoAheadDetails GoAheadDetails { get; set; }
        public List<ItineraryDetails> ItineraryDetails { get; set; }
        public AuditTrail AuditTrail { get; set; }

        [BsonIgnoreIfNull(true)]
        public List<BookingMapping> Mappings { get; set; }
    }

    public class PassengerDetails
    {
        public PassengerDetails()
        {
            AuditTrail = new AuditTrail();
            DietaryRequirements = new List<string>();
            SpecialAssistanceRequirements = new List<string>();
        }
        [BsonIgnoreIfNull(true)]
        public string Passenger_Id { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PassengerName_LocalLanguage { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? DateOfBirth { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? DateOfAnniversary { get; set; }
        public string Notes { get; set; }
        public string PassportNumber { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? PassportIssued { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? PassportExpiry { get; set; }
        public string VisaNumber { get; set; }
        public string Sex { get; set; }
        public bool? ISTourLeader { get; set; }
        public List<string> DietaryRequirements { get; set; }
        public List<string> SpecialAssistanceRequirements { get; set; }
        public string RoomType { get; set; }
        public string PersonType { get; set; }
        public int? RoomAssignment { get; set; }
        public AuditTrail AuditTrail { get; set; }
        public int PassengerNumber { get; set; }
        public string Age { get; set; }

    }
    public class BookingCommercials
    {
        [BsonIgnoreIfNull(true)]
        public string Booking_Id { get; set; }
        [BsonIgnoreIfNull(true)]
        public string Position_Id { get; set; }

        /// <summary>
        /// SQL Column: Markup_Id
        /// </summary>
        public string Markup_Id { get; set; }
        public string Markup_Name { get; set; }
        public string MarkupDetail_Id { get; set; }
        public string MarkupPercAmt { get; set; }
        public string MarkupCurrency_Id { get; set; }
        public string MarkupCurrency { get; set; }
    }

    public class BookingCompanyInfo
    {
        [BsonIgnoreIfNull(true)]
        public string Booking_Id { get; set; }
        [BsonIgnoreIfNull(true)]
        public string Position_Id { get; set; }

        /// <summary>
        /// SQL Column for Agent: Partner_Id
        /// SQL Column for Supplier: Supplier_Id
        /// </summary>
        public string Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        /// <summary>
        /// SQL Column for Agent: Contact_Id
        /// SQL Column for Supplier: SupplierContact_Id
        /// </summary>
        public string Contact_Id { get; set; }
        /// <summary>
        /// SQL Column for Agent: CUSTCONT
        /// </summary>
        public string Contact_Name { get; set; }
        /// <summary>
        /// SQL Column for Agent: SENDADDR
        /// SQL Column for Supplier: SENDADDR
        /// </summary>
        public string Contact_Email { get; set; }
        /// <summary>
        /// SQL Column for Supplier: SupplierTel
        /// </summary>
        public string Contact_Tel { get; set; }
        /// <summary>
        /// SQL Column for Agent: SENDTYPE
        /// SQL Column for Supplier: SENDTYPE
        /// </summary>
        public string Contact_SendType { get; set; }
        /// <summary>
        /// SQL Column for Agent: ISSUBAGENT
        /// Null for Supplier
        /// </summary>
        public bool? ISSUBAGENT { get; set; }
        /// <summary>
        /// SQL Column for Agent: SalesOffice_Id
        /// SQL Column for Supplier: EndSupplier_Id
        /// </summary>
        public string ParentCompany_Id { get; set; }
        public string ParentCompany_Name { get; set; }

        public string Division_ID { get; set; }
        public string Division_Name { get; set; }
    }

    public class BookingStaffDetails
    {
        [BsonIgnoreIfNull(true)]
        public string Booking_Id { get; set; }
        /// <summary>
        /// SQL Column: Sales_Id
        /// </summary>
        public string Staff_SalesUser_Id { get; set; }
        public string Staff_SalesUser_Name { get; set; }
        public string Staff_SalesUser_Email { get; set; }
        public string Staff_SalesUser_Company_Id { get; set; }
        public string Staff_SalesUser_Company_Name { get; set; }
        /// <summary>
        /// SQL Column: SalesSupport_Id
        /// </summary>
        public string Staff_SalesSupport_Id { get; set; }
        public string Staff_SalesSupport_Name { get; set; }
        public string Staff_SalesSupport_Email { get; set; }
        /// <summary>
        /// SQL Column: Staff_Id
        /// </summary>
        public string Staff_OpsUser_Id { get; set; }
        public string Staff_OpsUser_Name { get; set; }
        public string Staff_OpsUser_Email { get; set; }
        public string Staff_OpsUser_Company_Id { get; set; }
        public string Staff_OpsUser_Company_Name { get; set; }
        /// <summary>
        /// SQL Column: SalesProductAccountant_ID
        /// </summary>
        public string Staff_PAUser_Id { get; set; }
        public string Staff_PAUser_Name { get; set; }
        public string Staff_PAUser_Email { get; set; }
        public string Staff_PAUser_Company_Id { get; set; }
        public string Staff_PAUser_Company_Name { get; set; }
    }

    public class BookingGuestDetails
    {
        [BsonIgnoreIfNull(true)]
        public string Booking_Id { get; set; }
        /// <summary>
        /// SQL Column: Nationality_Id
        /// </summary>
        public string Nationality_Id { get; set; }
        public string Nationality_Name { get; set; }
        /// <summary>
        /// SQL Column: GroupLanguage_Id
        /// </summary>
        public string GroupLanguage_Id { get; set; }
        public string GroupLanguage_Name { get; set; }
        /// <summary>
        /// SQL Column: TravelReason
        /// </summary>
        public string TravelReason { get; set; }
    }

    public class BookingPreferences
    {
        [BsonIgnoreIfNull(true)]
        public string Booking_Id { get; set; }
        /// <summary>
        /// SQL Column: HotelStarRating
        /// </summary>
        public string PreferredStarRating { get; set; }
        /// <summary>
        /// SQL Column: HotelBoard
        /// </summary>
        public string PreferredBoardBasis { get; set; }
        /// <summary>
        /// SQL Column: HotelBreakfastType
        /// </summary>
        public string PreferredBreakfastType { get; set; }
        /// <summary>
        /// SQL Column: HotelLocation
        /// </summary>
        public string PreferredLocation { get; set; }
        /// <summary>
        /// SQL Column: Standard
        /// </summary>
        public string PreferredBudgetCategory { get; set; }
        /// <summary>
        /// SQL Column: CATEGORY_ID
        /// </summary>
        public string PreferredBudgetCategory_ID { get; set; }
        /// <summary>
        /// SQL Column: IsFreePlace
        /// </summary>
        public bool? IsFreePlace { get; set; }
        /// <summary>
        /// SQL Column: ServicesOnly
        /// </summary>
        public string ServicesOnly { get; set; }
        /// <summary>
        /// SQL Column: PRIORITY
        /// </summary>
        public string PRIORITY { get; set; }
        /// <summary>
        /// SQL Column: TravelReason
        /// </summary>
        public string TravelReason { get; set; }
        /// <summary>
        /// SQL Column: Destination_Id
        /// </summary>
        public string Destination_Id { get; set; }
        public string Destination_Name { get; set; }
    }

    public class Positions
    {
        [BsonIgnoreIfNull(true)]
        public string Booking_Id { get; set; }
        public string Position_Id { get; set; }
        public string PositionType { get; set; }
        public string GRIDINFO { get; set; }
        public string STATUS { get; set; }
        public string InvoiceStatus { get; set; }
        public string OrderNr { get; set; }
        public int? InterConnectingRooms { get; set; }
        public DateTime? LateCheckout { get; set; }

        public string ProductType_Id { get; set; }
        public string ProductType { get; set; }
        public string Product_Id { get; set; }
        public string ProductCode { get; set; }
        public string Product_Name { get; set; }
        public string Country_Id { get; set; }
        public string Country { get; set; }
        public string City_Id { get; set; }
        public string City { get; set; }

        public List<BookingRoomsAndPrices> BookingRoomsAndPrices { get; set; }
        public List<BookingSeason> BookingSeason { get; set; }
        public List<PositionFOC> PositionFOC { get; set; }
        public HotelAdditionalInfo Attributes { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? STARTDATE { get; set; }
        public string STARTTIME { get; set; }
        public string STARTLOC { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? ENDDATE { get; set; }
        public string ENDTIME { get; set; }
        public string ENDLOC { get; set; }
        public string DURATION { get; set; }

        public string BuyCurrency_Id { get; set; }
        public string BuyCurrency_Name { get; set; }
        public string ExchangeRate_ID { get; set; }
        public string ExchangeRateDetail_ID { get; set; }
        public Decimal? ExchangeRate { get; set; }
        public Decimal? ExchangeRateSell { get; set; }

        public BookingCompanyInfo SupplierInfo { get; set; }
        /// <summary>
        /// SQL Column: SUPPCONFNR
        /// </summary>
        public string Supplier_Confirmation { get; set; }
        public string SupplierInvoice_Id { get; set; }

        public string HotelPLacer_ID { get; set; }
        public string HotelPLacer_Name { get; set; }
        public List<BookingCommercials> Commercials { get; set; }

        /// <summary>
        /// AUTOSPOS
        /// </summary>
        public bool? StandardRooming { get; set; }
        public bool? IsLocked { get; set; }
        public bool? IsSendToHotel { get; set; }
        public bool? IsSuppPosLocked { get; set; }
        /// <summary>
        /// SQL Column: HotelAdvice
        /// </summary>
        public string Special_Requests { get; set; }

        public string EmptyLegs { get; set; }
        public bool? Porterage { get; set; }
        public bool? MealsIncluded { get; set; }
        public bool? TicketsIncluded { get; set; }
        public int? HotelStarRating { get; set; }
        public string HOTELMEALPLAN { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EarlyCheckIn { get; set; }
        public int? WashChangeRoom { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? OPTIONDATE { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CancelDeadline { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CancellationDate { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? ConfirmDate { get; set; }
        public string CancellationUser { get; set; }
        public string CancellationReason { get; set; }

        public string FlightNumber { get; set; }
        public string SwitchedPosition_Id { get; set; }
        public string BreakFastType { get; set; }
        public string VoucherNote { get; set; }
        public string CancellationPolicy { get; set; }
        public string CityTaxAdvise { get; set; }
        public Decimal? TotalSAPAmount { get; set; }

        /// <summary>
        /// This should only contain the latest of all Pricing. (Status = 'A'). 
        /// When new pricing is generated, exisitng pricing should be updated to status = 'H' and
        /// then it should be the transffered to a mPricing collection for Archive purpose. 
        /// </summary>
        public List<Pricing> Pricing { get; set; }

        #region Nullable Fields
        [BsonIgnoreIfNull(true)]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CREA_DT { get; set; }

        [BsonIgnoreIfNull(true)]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? MODI_DT { get; set; }

        [BsonIgnoreIfNull(true)]
        public string CREA_US { get; set; }

        [BsonIgnoreIfNull(true)]
        public string MODI_US { get; set; }

        [BsonIgnoreIfNull(true)]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? STATUS_DT { get; set; }

        [BsonIgnoreIfNull(true)]
        public string STATUS_US { get; set; }
        #endregion

        public AuditTrail AuditTrail { get; set; }
        public List<AlternateServices> AlternateServices { get; set; }

        [BsonIgnoreIfNull(true)]
        public List<BudgetSupplements> BudgetSupplements { get; set; }

        [BsonIgnoreIfNull(true)]
        public List<PaymentSchedule> PaymentSchedule { get; set; }
        
        public string TypeOfRoom { get; set; }
        public string TLRemarks { get; set; }
        public string OPSRemarks { get; set; }

        #region Attraction
        public string TicketLocation { get; set; }
        public string MeetingPoint { get; set; }
        public string TrainNumber { get; set; }
        public bool GuidePurchaseTicket { get; set; }
        #endregion

        #region Bus  
        public string DriverName { get; set; }
        public string DriverContactNumber { get; set; }
        public string DriverLicenceNumber { get; set; }
        public string DriverLanguage { get; set; }
        public int NumberOfDriverRooms { get; set; }
        public string VehicleRegistration { get; set; }
        public string LicencePlate { get; set; }
        public DateTime? ManufacturedDate { get; set; }
        public bool Parking { get; set; }
        public bool CityPermits { get; set; }
        public bool RoadTolls { get; set; }
        public bool AC { get; set; }
        public bool WC { get; set; }
        public bool Safety { get; set; }
        public bool GPS { get; set; }
        public bool AV { get; set; }
        public string Itinerary { get; set; }
        #endregion

        #region Meal
        public string Floor { get; set; }
        public bool CoachParkingAvailable { get; set; }
        public string MealStyle { get; set; }
        public string Course { get; set; }
        public bool Tea { get; set; }
        public bool Dessert { get; set; }
        public bool Water { get; set; }
        public bool Bread { get; set; }
        public string Menu { get; set; }
        #endregion
    }

    public class AlternateServices
    {
        [BsonIgnoreIfNull(true)]
        public string Position_Id { get; set; }

        [BsonIgnoreIfNull(true)]
        public string Booking_Id { get; set; }

        public string AlternateServies_Id { get; set; }
        public int? SortOrder { get; set; }
        public bool? IsBlackListed { get; set; }

        public string Product_Id { get; set; }
        public string Product_Name { get; set; }
        public string Country_Id { get; set; }
        public string Country { get; set; }
        public string City_Id { get; set; }
        public string City { get; set; }

        public HotelAdditionalInfo Attributes { get; set; } = new HotelAdditionalInfo();
        public BookingCompanyInfo SupplierInfo { get; set; } = new BookingCompanyInfo();

        public string Request_Status { get; set; }
        public string Availability_Status { get; set; }

        [BsonIgnoreIfNull(true)]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? OptionDate { get; set; }

        [BsonIgnoreIfNull(true)]
        public string CancellationDeadline { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? Requested_On { get; set; }

        public Decimal? PPTwin_Rate { get; set; }
        public List<BookingRoomsAndPrices> Request_RoomsAndPrices { get; set; } = new List<BookingRoomsAndPrices>();

        #region Nullable Fields
        [BsonIgnoreIfNull(true)]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CREA_DT { get; set; }

        [BsonIgnoreIfNull(true)]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? MODI_DT { get; set; }

        [BsonIgnoreIfNull(true)]
        public string CREA_US { get; set; }

        [BsonIgnoreIfNull(true)]
        public string MODI_US { get; set; }

        [BsonIgnoreIfNull(true)]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? STATUS_DT { get; set; }

        [BsonIgnoreIfNull(true)]
        public string STATUS_US { get; set; }
        #endregion

        public AuditTrail AuditTrail { get; set; } = new AuditTrail();
        [BsonIgnoreIfNull(true)]
        public List<BudgetSupplements> BudgetSupplements { get; set; }
    }

    public class AuditTrail
    {
        [BsonIgnoreIfNull(true)]
        public string Booking_Id { get; set; }
        [BsonIgnoreIfNull(true)]
        public string Position_Id { get; set; }
        [BsonIgnoreIfNull(true)]
        public string AlternateServies_Id { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CREA_DT { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? MODI_DT { get; set; }
        public string CREA_US { get; set; }
        public string MODI_US { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? STATUS_DT { get; set; }
        public string STATUS_US { get; set; }
    }

    public class BookingRoomsAndPrices
    {
        [BsonIgnoreIfNull(true)]
        public string Booking_Id { get; set; }
        [BsonIgnoreIfNull(true)]
        public string Position_Id { get; set; }

        public string BookingRooms_Id { get; set; }
        public string PositionPricing_Id { get; set; }
        /// <summary>
        /// SQL Column: ROOMNO
        /// </summary>
        public int? Req_Count { get; set; }
        public string ChargeBasis { get; set; }
        public string Status { get; set; }

        public string ProductRange_Id { get; set; }
        public string Category_Id { get; set; }
        public string CategoryName { get; set; }
        public string ProductTemplate_Id { get; set; }
        /// <summary>
        /// SQL Column: SUBPROD
        /// </summary>
        public string RoomShortCode { get; set; }
        /// <summary>
        /// SQL Column: NAME
        /// </summary>
        public string RoomName { get; set; }
        public int? Capacity { get; set; }

        public string PersonType_Id { get; set; }
        public string PersonType { get; set; }
        public int? Age { get; set; }

        public bool? ApplyMarkup { get; set; }
        public bool? ExcludeFromInvoice { get; set; }
        public string AllocationUsed { get; set; }
        public string Allocation_Id { get; set; }
        public List<RoomsAndPricesAllocation> RoomsAndPricesAllocation { get; set; }

        public string CrossPosition_Id { get; set; }
        public string CrossBookingPax_Id { get; set; }

        /// <summary>
        /// SQL Column: Recurrsive
        /// </summary>
        public bool? IsRecursive { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? OneOffDate { get; set; }

        public string ParentBookingRooms_Id { get; set; }
        public string MealPlan_Id { get; set; }
        public string MealPlan { get; set; }

        // PositionPricing Columns Begin
        public bool? ConfirmedReqPrice { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? StartDate { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EndDate { get; set; }

        public string BuyCurrency_Id { get; set; }
        public string BuyCurrency_Name { get; set; }
        public string Action { get; set; }

        public string BuyContract_Id { get; set; }
        public string BuyPositionPrice_Id { get; set; }
        public string SellContract_Id { get; set; }
        public string SellPositionPrice_Id { get; set; }
        public string SellContractCurrency_Id { get; set; }
        public string SellContractCurrency_Name { get; set; }
        public decimal? ContractedSellPrice { get; set; }

        public decimal? BudgetPrice { get; set; }
        /// <summary>
        /// SQL Column: BREQPRICE
        /// </summary>
        public decimal? RequestedPrice { get; set; }
        /// <summary>
        /// SQL Column: BPRICE
        /// </summary>
        public decimal? BuyPrice { get; set; }
        public decimal? ContractedBuyPrice { get; set; }

        public string BookingSeason_Id { get; set; }
        public string InvForPax { get; set; }
        public int? InvNumber { get; set; }
        // PositionPricing Columns End

        public bool IsAdditionalYN { get; set; }

        #region Nullable Fields
        [BsonIgnoreIfNull(true)]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CREA_DT { get; set; }

        [BsonIgnoreIfNull(true)]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? MODI_DT { get; set; }

        [BsonIgnoreIfNull(true)]
        public string CREA_US { get; set; }

        [BsonIgnoreIfNull(true)]
        public string MODI_US { get; set; }

        [BsonIgnoreIfNull(true)]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? STATUS_DT { get; set; }

        [BsonIgnoreIfNull(true)]
        public string STATUS_US { get; set; }
        #endregion

        public AuditTrail AuditTrail { get; set; }
    }

    public class RoomsAndPricesAllocation
    {
        [BsonIgnoreIfNull(true)]
        public string Booking_Id { get; set; }
        [BsonIgnoreIfNull(true)]
        public string Position_Id { get; set; }
        [BsonIgnoreIfNull(true)]
        public string BookingRooms_Id { get; set; }

        public string BookingRoomDetail_ID { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? AllocationDate { get; set; }
        public int? OnReqQty { get; set; }
        public int? OnAllocQty { get; set; }
        public int? OnFreeSellQty { get; set; }

        public AuditTrail AuditTrail { get; set; }
    }

    public class PositionFOC
    {
        [BsonIgnoreIfNull(true)]
        public string Booking_Id { get; set; }
        [BsonIgnoreIfNull(true)]
        public string Position_Id { get; set; }

        public string PositionFOC_Id { get; set; }
        public string BuyBookingRooms_ID { get; set; }
        public string BuyRoomShortCode { get; set; }
        public int? BuyQuantity { get; set; }

        public string GetBookingRooms_ID { get; set; }
        public string GetRoomShortCode { get; set; }
        public int? GetQuantity { get; set; }

        #region Nullable Fields
        [BsonIgnoreIfNull(true)]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CREA_DT { get; set; }

        [BsonIgnoreIfNull(true)]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? MODI_DT { get; set; }

        [BsonIgnoreIfNull(true)]
        public string CREA_US { get; set; }

        [BsonIgnoreIfNull(true)]
        public string MODI_US { get; set; }

        [BsonIgnoreIfNull(true)]
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? STATUS_DT { get; set; }

        [BsonIgnoreIfNull(true)]
        public string STATUS_US { get; set; }
        #endregion

        public AuditTrail AuditTrail { get; set; }
    }

    public class ExchangeRateSnapshot
    {
        [BsonIgnoreIfNull(true)]
        public string Booking_Id { get; set; }

        public string ExchangeRateSnapshot_ID { get; set; }
        public string ExchangeRate_id { get; set; }
        public string EXRATE { get; set; }
        public string REFCUR { get; set; }
        public DateTime? DATEMIN { get; set; }
        public DateTime? DATEMAX { get; set; }
        public decimal? VATRATE { get; set; }
        public string Currency_Id { get; set; }
        public DateTime? CREA_DT { get; set; }
        public List<ExchangeRateDetailSnapshot> ExchangeRateDetail { get; set; }
    }

    public class ExchangeRateDetailSnapshot
    {
        [BsonIgnoreIfNull(true)]
        public string ExchangeRateSnapshot_ID { get; set; }
        public string ExchangeRateDetailSnapshot_Id { get; set; }
        public string ExchangeRateDetail_Id { get; set; }
        public string Currency_Id { get; set; }
        public string CURRENCY { get; set; }
        public decimal? RATE { get; set; }
        public decimal? ROUNDTO { get; set; }
        public decimal? ROUNDOPT { get; set; }
        public decimal? SECURITY { get; set; }
        public DateTime? CREA_DT { get; set; }
    }

    public class Fixes
    {
        public string BookingFixes_Id { get; set; }
        public string Position_Id { get; set; }
        public string Position_ProductType { get; set; }
        public string Position_CityName { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? Position_StartDate { get; set; }
        public string Booking_Id { get; set; }
        public string PositionPricing_Id { get; set; }
        public string Document_Id { get; set; }
        public string FixDescription { get; set; }
        public string Status { get; set; }
        public bool? Completed { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CREA_DT { get; set; }
        public string CREA_US { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? Position_DueDate { get; set; }
    }

    public class GoAheadDetails
    {
        public string GoAhdMaterialisation_Id { get; set; }
        public string Booking_Id { get; set; }
        public bool? BookingMaterialised { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? BookingMaterialised_Date { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? HotelConfirmation_Date { get; set; }
        public string Remarks { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? GoAhead_Date { get; set; }
        public string Document_Id { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? STARTDATE { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? ENDDATE { get; set; }
        public string Status { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? NEW_STARTDATE { get; set; }
        public List<GoAhead_MealDetails> MealDetails { get; set; }
        public List<GoAhead_PositionDetails> ServiceDetails { get; set; }
    }

    public class GoAhead_MealDetails
    {
        public string MealDetails_Id { get; set; }
        public string GoAhdMaterialisation_Id { get; set; }
        public int? DayNo { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? Date { get; set; }
        public string BreakFast { get; set; }
        public string Lunch { get; set; }
        public string Dinner { get; set; }
    }

    public class GoAhead_PositionDetails
    {
        public string PositionDetails_Id { get; set; }
        public string GoAhdMaterialisation_Id { get; set; }
        public int? DayNo { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? Date { get; set; }
        public string Position_Id { get; set; }
        public string City_Id { get; set; }
        public string City_Name { get; set; }
        public string Selected_Product_Id { get; set; }
        public string Selected_Product_Name { get; set; }
        public string Product_Type { get; set; }
        public string PositionGridInfo { get; set; }
        public string REMARKS { get; set; }

    }

    public class ItineraryDetails
    {
        public string ItineraryDetail_Id { get; set; }
        public string Booking_Id { get; set; }
        public string Position_Id { get; set; }
        public int? ORDERNR { get; set; }
        public string GridInfo { get; set; }
        public string CountryName { get; set; }
        public string CityName { get; set; }
        public int? SortOrder { get; set; }
        public string Status { get; set; }
        public string ProductType { get; set; }
        public string Description { get; set; }
        public string Details { get; set; }
        public string LegType { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? STARTDATE { get; set; }
        public string STARTTIME { get; set; }
        public string StartingLocation { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? ENDDATE { get; set; }
        public string ENDTIME { get; set; }
        public string EndingLocation { get; set; }
        public string UniqueIdentityValue { get; set; }
        public int? DayNo { get; set; }
        public string City_Id { get; set; }
        public string Country_Id { get; set; }
        public string TLRemarks { get; set; }
        public string OPSRemarks { get; set; }
        public string ItineraryRemarks { get; set; }
        public string ItineraryNote { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class BudgetSupplements
    {
        [BsonIgnoreIfNull(true)]
        public string Position_Id { get; set; }
        [BsonIgnoreIfNull(true)]
        public string AlternateServies_Id { get; set; }

        public string BudgetSupplement_Id { get; set; }
        public string BookingRooms_Id { get; set; }
        public string PositionPricing_Id { get; set; }
        public string RoomShortCode { get; set; }
        public string PersonType { get; set; }
        [Required, DisplayName("Budget Supplement Amount")]
        public decimal? BudgetSupplementAmount { get; set; }
        [Required, DisplayName("Budget Supplement Reason")]
        public string BudgetSupplementReason { get; set; }
        public string BudgetSuppCurrencyId { get; set; }
        public string BudgetSuppCurrencyName { get; set; }
        public bool ApplyMarkUp { get; set; }
        public bool AgentConfirmed { get; set; }
        public string SupplementFor { get; set; }
        public int? SupplementNumber { get; set; }
        public string status { get; set; }

        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CREA_DT { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? MODI_DT { get; set; }
        public string CREA_US { get; set; }
        public string MODI_US { get; set; }
    }

    public class BookingMapping
    {
        public string Application_Id { get; set; }
        public string Application { get; set; }
        public string PartnerEntityCode { get; set; }
        public string PartnerEntityName { get; set; }
        public string PartnerEntityType { get; set; }
        public string Action { get; set; }
        public string Status { get; set; }
        public string AdditionalInfoType { get; set; }
        public string AdditionalInfo { get; set; }
        public string CreateUser { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CreateDate { get; set; }
        public string EditUser { get; set; }
        [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EditDate { get; set; }
    }

    public class PaymentSchedule
    {
        [BsonIgnoreIfNull(true)]
        public string Booking_Id { get; set; }

        public string BookingPaymentSchedule_Id { get; set; }
        public string Company_Id { get; set; }
        public string Company_Name { get; set; }
        public string Position_Id { get; set; }
        public DateTime PaymentDueDate { get; set; }
        public decimal Amount { get; set; }
        public string Currency_Id { get; set; }
        public string Currency_Name { get; set; }
        public string Status { get; set; }
        public bool VoucherReleased { get; set; }
        public DateTime CREA_DT { get; set; }
        public string CREA_US { get; set; }
        public DateTime? MODI_DT { get; set; }
        public string MODI_US { get; set; }
        public string ClientInvoice_ID { get; set; }
        public bool? IsInvoiced { get; set; }
        public string Remark { get; set; }
    }
}