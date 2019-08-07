using System;
using System.Collections.Generic;

namespace VGER_WAPI_CLASSES
{
	public class OpsBookingItineraryGetReq
	{
		public string BookingNumber { get; set; }
		public string BookingRemark { get; set; }
		public string DayName { get; set; }
		public string ProductType { get; set; }
		public string Status { get; set; }
		public string PositionId { get; set; }
		public string ItineraryDetailId { get; set; }
	}

	public class OpsBookingItineraryGetRes
	{
		public OpsItineraryDetails OpsItineraryDetails = new OpsItineraryDetails();
		public ItineraryDetails ItineraryDetails = new ItineraryDetails();
		public ResponseStatus ResponseStatus = new ResponseStatus();
	}

	public class OpsBookingItinerarySetReq
	{
		public string BookingNumber { get; set; }
		public string Type { get; set; }
		public ItineraryDetails ItineraryDetails = new ItineraryDetails();
		public List<ItineraryDetails> lstItineraryDetails = new List<ItineraryDetails>();
	}

	public class OpsBookingItinerarySetRes
	{
		public string ItineraryDetailId { get; set; }
		public ResponseStatus ResponseStatus = new ResponseStatus();
	}

	public class OpsItineraryDetails
	{
		//The below List will contain the DayList of given Booking Positions
		public List<string> Days = new List<string>();

		//The below List will contain the CityList of given Booking Positions
		public List<string> Cities = new List<string>();

		//The below List will contain the list of ProductType/ServiceType of given Booking Positions
		public List<string> ServiceType = new List<string>();

		//The below List will contain a DISTINCT LIST of POSITION STATUS on the given BOOKING
		public List<string> BookingStaus = new List<string>();

		//The below class contains Day Wise details of ProductTypes with CityName,Alloc,Start/End DateTime,etc
		public List<OpsItinenraryDays> OpsItinenraryDays = new List<OpsItinenraryDays>();

		//The below List contain position description
		public List<OpsPositionDetails> OpsPositions = new List<OpsPositionDetails>();
	}

	public class OpsItinenraryDays
	{
		public string DayName { get; set; }
		public int? DayNo { get; set; }
		public string CityNames { get; set; }
		public string ItineraryDesc { get; set; }
		public List<OpsItineraryDayDetails> OpsItineraryDayDetails { get; set; }
	}

	public class OpsItineraryDayDetails
	{
		public string ItineraryDetailId { get; set; }
		public string PositionId { get; set; }
		public int? DayNo { get; set; }
		public string CityId { get; set; }
		public string CityName { get; set; }
		public string CountryId { get; set; }
		public string CountryName { get; set; }
		public string DayName { get; set; }
		public string STARTDateLongFormat { get; set; }
		public string STARTDayOfWeek { get; set; }
		public DateTime? STARTDATE { get; set; }
		public string STARTTIME { get; set; }
		public DateTime? ENDDATE { get; set; }
		public string ENDTIME { get; set; }
		public string ProductType { get; set; }
		public string ProductName { get; set; }
		public string Supplier { get; set; }
		public string Allocation { get; set; }
		public string Status { get; set; }
		public string StatusDescription { get; set; }
		public string NoOfPax { get; set; }
		public string Price { get; set; }
		public string OPSRemarks { get; set; }
		public string TLRemarks { get; set; }
		public bool IsDeleted { get; set; }
		public string ItineraryRemarks { get; set; }
		public string UniqueIdentityValue { get; set; }		
	}

	public class OpsPositionDetails
	{	
		public string Position_Id { get; set; }
		public string STARTTIME { get; set; }
		public string ENDTIME { get; set; }
		public string HOTELMEALPLAN { get; set; }
		public string BreakFastType { get; set; }
		public bool Porterage { get; set; }
		public string VoucherNote { get; set; }
		public string STARTLOC { get; set; }
		public string ENDLOC { get; set; }
		public string DriverName { get; set; }
		public string DriverContactNumber { get; set; }
		public string LicencePlate { get; set; }
		public string Menu { get; set; }
		public string MealStyle { get; set; }
		public string Course { get; set; }
		public string TicketLocation { get; set; }
		public bool GuidePurchaseTicket { get; set; }
		public string TrainNumber { get; set; }
		public string Itinerary { get; set; }
		public string ProdDescription { get; set; }
		public string DayNo { get; set; }
		public string ProductType { get; set; }
		public string PositionType { get; set; }
		public bool? Placeholder { get; set; }
		public string PositionStatus { get; set; }
		public string systemCompanyId { get; set; }
	}

    public class ProductContractsGetReq
    {
        public string SupplierId { get; set; }
        public string ProductId { get; set; }
        public string BuySellType { get; set; }
        public string AgentId { get; set; }
    }

    public class ProductContractsGetRes
    {
        public Contracts ProductContract { get; set; }
        public ResponseStatus ResponseStatus { get; set; } = new ResponseStatus();
    }
}
