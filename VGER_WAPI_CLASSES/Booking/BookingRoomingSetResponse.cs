using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES.Booking
{
   public  class BookingRoomingSetResponse
    {
        public string PassengerId { get; set; }
        public string BookingId { get; set; }
        public ResponseStatus ResponseStatus { get; set; } = new ResponseStatus();
    }
}
