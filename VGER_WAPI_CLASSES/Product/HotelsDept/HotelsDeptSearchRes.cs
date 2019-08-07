using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class HotelsDeptSearchRes
    {
        public List<HotelSearchResult> BookingsDetails { get; set; } = new List<HotelSearchResult>();
		public int HotelsTotalCount { get; set; }
		public ResponseStatus ResponseStatus { get; set; } = new ResponseStatus();
    }

    public class HotelSearchResult
    {
        public string BookingNumber { get; set; }
        public string AgentName { get; set; }
        public string ContactName { get; set; }
        public string BookingName { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Duration { get; set; }
        public string Status { get; set; }
		public string StrBookingRooms { get; set; }
        public List<TemplateBookingRoomsGrid> BookingRooms { get; set; }
	}

    public class HotelsByBookingGetRes
    {
        public string QRFID { get; set; }
        public Bookings Bookings { get; set; }
        public ResponseStatus ResponseStatus { get; set; } = new ResponseStatus();
        public List<ProductSRPDetails> ProductSRPDetails { get; set; } = new List<ProductSRPDetails>();
    }

    public class HotelAlternateServicesGetRes
    {
        public ResponseStatus ResponseStatus { get; set; } = new ResponseStatus();
        public List<AlternateServices> AlternateServices { get; set; } = new List<AlternateServices>();
    }

    public class HotelReservationEmailRes
    {
        public ResponseStatus ResponseStatus { get; set; } = new ResponseStatus();
    }

    public class HotelSwitchSetRes
    {
        public ResponseStatus ResponseStatus { get; set; } = new ResponseStatus();
        public Bookings Bookings { get; set; }
    }

    public class SupplierContactDetails
    {
        public string AltSvcId { get; set; }
        public string SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string SupplierEmail { get; set; }
        public string ProdName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
    }
}
