using System;
using System.Collections.Generic;
using System.Text;


namespace VGER_WAPI_CLASSES
{
    /// <summary>
    /// This element provides the detailed Information of the Booking
    /// </summary>
    public class BookingDetails
    {

        /// <summary>
        /// 
        /// </summary>
        public string BookingReference { get; set; }

        public string Status { get; set; }

        public string BookedDate { get; set; }
        public string GoAheadDate { get; set; }

        public string AgentReference { get; set; }

        public string AgentName { get; set; }

        public string AgentCode { get; set; }

        public string InvoiceCurrency { get; set; }

        public string StartDate { get; set; }

        public string EndDate { get; set; }

        public string Duration { get; set; }

        public string UpdateDate { get; set; }

        public string OPTIONDATE { get; set; }

        public string Operator { get; set; }

        public string OperatorEmail { get; set; }

        public string TourLeader { get; set; }

        public string TourLeaderContact { get; set; }

        public List<BookingPax> BookingPassengers { get; set; }
        public List<BookingRooms> BookingRooms { get; set; }
        public List<BookingPositions> Services { get; set; }
        public List<BookingItineraryDetail> Itinerary { get; set; }

    }

    public class BookingDetailReq
    {
        public string BookingReference { get; set; }
        public string BookingNumber { get; set; }
        public string VoygerCompany_Id { get; set; }
        public string UserName { get; set; }
    }

    public class BookingDetailRes
    {
        public BookingDetailRes()
        {
            Booking = new BookingDetails();
            ResponseStatus = new ResponseStatus();
        }
        public BookingDetails Booking { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
    }

    #region Booking Collection 
    public class BookingInfoRes
    {
        public BookingInfoRes()
        {
            Bookings = new Bookings();
            ResponseStatus = new ResponseStatus();
        }
        public Bookings Bookings { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
    } 

    //The below classes are used in MongoPush in Booking Service and its Microservices
    public class BookingPositionsReq
    {
        public string Pricing_Id { get; set; }
        public string Booking_Id { get; set; }
        public string Position_Id { get; set; }
    }
    public class BookingPositionsInfoReq
    {
        public string Booking_Id { get; set; }
        public List<string> PositionList { get; set; }
    }
    #endregion
}
