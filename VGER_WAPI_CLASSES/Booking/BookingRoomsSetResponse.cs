using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES.Booking
{
   public class BookingRoomsSetResponse
    {
        public BookingRoomsSetResponse()
        {
            Response = new ResponseStatus();
        }
        public ResponseStatus Response { get; set; }
    }
}
