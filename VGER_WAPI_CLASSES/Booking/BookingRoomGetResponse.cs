using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES.Booking
{
  public  class BookingRoomGetResponse
    {
        public BookingRoomGetResponse()
        {
            BookingRoomsDetails = new List<TemplateBookingRoomsGrid>();
            Response = new ResponseStatus();
        }
        public string BookingNumber { get; set; }
        public string QrfId { get; set; }
        public List<TemplateBookingRoomsGrid> BookingRoomsDetails { get; set; }
        public ResponseStatus Response { get; set; }
    }
}
