using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES.Booking
{
    public class PositionsFromBookingGetReq
    {
        public string BookingNumber { get; set; }
        public string PositionId { get; set; }
        public string PositionName { get; set; }
        public string PositionType { get; set; }
		public bool? IsPlaceholder { get; set; } = null;
	}
}
