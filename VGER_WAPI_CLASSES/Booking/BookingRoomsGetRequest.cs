using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES.Booking
{
    public class BookingRoomsGetRequest
    {
        public string BookingNumber { get; set; }
        public string BookingRoom_id { get; set; }
        public string RoomType { get; set; }
    }
}
