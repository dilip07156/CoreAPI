using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class OpsBookingSearchReq
    {
        public string AgentId { get; set; }
        public string AgentName { get; set; }
        public string BookingNumber { get; set; }
        public string AgentCode { get; set; }
        public string AgentTourName { get; set; }
        public string DateType { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string Status { get; set; }
        public string BusinessType { get; set; }
        public string Destination { get; set; }
        public string SalesOffice { get; set; }
        public string FileHandler { get; set; }
        public int Start { get; set; }
		public int Length { get; set; }
	}

    public class OpsBookingSearchRes
    {
        public List<OpsBookingsSearchResult> Bookings { get; set; } = new List<OpsBookingsSearchResult>();
		public int BookingTotalCount { get; set; }
		public ResponseStatus ResponseStatus { get; set; } = new ResponseStatus();
    }

    public class OpsBookingsSearchResult
    {
        public string BookingNumber { get; set; }
        public string OutstandingCount { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public string CompanyContact { get; set; }
        public string CompanyName { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public string TourName { get; set; }
        public string Duration { get; set; }
        public string PaxCount { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Destination { get; set; }
        public decimal AccomPercent { get; set; }
        public decimal TransportPercent { get; set; }
        public decimal ServicesPercent { get; set; }
        public string SalesOfficerEmail { get; set; }
        public string FileHandlerEmail { get; set; }
        public string ProductAccountantEmail { get; set; }
        public string SalesOffice { get; set; }
        public List<TemplateBookingRoomsGrid> BookingRooms { get; set; }
    }

    public class ServicePercentages
    {
        public string BookingNumber { get; set; }
        public string PositionId { get; set; }
        public decimal AccomPercent { get; set; }
        public decimal TransportPercent { get; set; }
        public decimal ServicesPercent { get; set; }
        public decimal TotalPercent { get; set; }
    }

    public class OpsBookingSummaryGetRes
    {
        public OpsBookingSummaryDetails OpsBookingSummaryDetails { get; set; } = new OpsBookingSummaryDetails();
        public ResponseStatus ResponseStatus { get; set; } = new ResponseStatus();
    }

    public class OpsBookingSummaryDetails
    {
        #region Header Details
        //public COHeaderViewModel COHeader { get; set; } = new COHeaderViewModel();
        public string QRFID { get; set; }
        public decimal ConfirmationPerc { get; set; }
        public string BookingNumber { get; set; }
        public int NoOfDays { get; set; }
        public int NoOfNights { get; set; }
        public string SalesOfficerEmail { get; set; }
        public string CostingOfficerEmail { get; set; }
        public string FileHandlerEmail { get; set; }
        public string ProductAccountantEmail { get; set; }
        public string Destination { get; set; }
        #endregion

        #region Client Key Info
        public string AgentId { get; set; }
        public string AgentName { get; set; }
        public string ContactId { get; set; }
        public string ContactName { get; set; }
        public string ContactTel { get; set; }
        public string ContactEmail { get; set; }
        public string TourName { get; set; }
        public string TourType { get; set; }
        public string Division { get; set; }
        public string Nationality { get; set; }
        public string PRIORITY { get; set; }
        public string TravelReason { get; set; }
        #endregion

        #region Timelines
        public string GoAheadDate { get; set; }
        public string HotelConfirmationDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string PaymentDueDate { get; set; }
        #endregion

        #region Workflow Counts
        public int PlacingPendingCnt { get; set; }
        public int BookingPendingCnt { get; set; }
        public int BookingConfirmedCnt { get; set; }
        public int BookingVoucheredCnt { get; set; }
        #endregion

        #region Materialisation
        public List<BookingRooms> PaxRooms { get; set; }
        public List<BookingRooms> TourStaffRooms { get; set; }
        public List<Fixes> BookingFixes { get; set; } = new List<Fixes>();
        #endregion 
    }
}
