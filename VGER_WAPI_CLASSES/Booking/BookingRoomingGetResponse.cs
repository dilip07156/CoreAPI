using System.Collections.Generic;

namespace VGER_WAPI_CLASSES
{
    public class BookingRoomingGetResponse
    {
        public BookingRoomingGetResponse()
        {
            Passengers = new List<PassengerDetails>();
            Response = new ResponseStatus();
        }
        public List<PassengerDetails> Passengers { get; set; }
        public ResponseStatus Response { get; set; }
    }

    public class BookingRoomHotelsGetRes
    {
        public BookingRoomHotelsGetRes()
        {
            SendRoomingListToHotelVm = new List<SendRoomingListToHotelVm>();
            Response = new ResponseStatus();
        }
        public List<SendRoomingListToHotelVm> SendRoomingListToHotelVm { get; set; }
        public ResponseStatus Response { get; set; }
    } 
}
