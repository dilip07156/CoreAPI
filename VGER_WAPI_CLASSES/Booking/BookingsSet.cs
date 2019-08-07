using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class BookingSetReq
    {
        public string BookingNumber { get; set; }
        public string PositionId { get; set; }
        public string AlternateServiceId { get; set; }
        public string User { get; set; }
        public string Status { get; set; }
    }

    public class BookingPosAltSetReq
    {
        public string BookingNumber { get; set; }
        public string PositionId { get; set; }
        public string DocumentType { get; set; }
        public string User { get; set; }
        public string ModuleType { get; set; }
    }

	public class BookingsSetRes
	{
		public Bookings resBooking = new Bookings();
		public string PositionId { get; set; } 
	}

    public class BookingPosSetReq
    {
        public List<string> PositionIds { get; set; }
        public string UserEmail { get; set; }
        public string BookingNumber { get; set; }
    }

    public class BridgeBookingReq
    {
        public string BookingNumber { get; set; }
        public string PositionId { get; set; }
        public string User { get; set; }
        public string SupplierId { get; set; }
        public string DocType { get; set; }
    }
}
