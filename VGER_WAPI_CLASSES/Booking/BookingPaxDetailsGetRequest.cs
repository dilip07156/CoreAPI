using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES.Booking
{
    public class BookingPaxDetailsGetRequest
    { 
        public string BookingNumber { get; set; }
        public string PersonType { get; set; }
    }
    public class BookingPaxDetailsGetResponse
    {
        public BookingPaxDetailsGetResponse()
        {
            bookingPaxDetails = new List<TemplateBookingPaxGrid>();
            Response = new ResponseStatus();
        }
        public string BookingNumber { get; set; }
        public List<TemplateBookingPaxGrid> bookingPaxDetails { get; set; }
        public ResponseStatus Response { get; set; }

    }
}
