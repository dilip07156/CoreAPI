using System.Collections.Generic;

namespace VGER_WAPI_CLASSES.Booking
{
    public class BookingRoomsSetRequest
    {
        public BookingRoomsSetRequest()
        {
            BookingRoomDetails = new List<BookingRoomDetails>();

        }
        public string BookingNumber { get; set; }

        public List<BookingRoomDetails> BookingRoomDetails { get; set; }
    }

    public class BookingRoomDetails
    {
        public string RoomName { get; set; }
        public string RoomQuantity { get; set; }
        public string Difference { get; set; }

    }
}
