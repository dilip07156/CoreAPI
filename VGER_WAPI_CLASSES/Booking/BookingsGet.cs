namespace VGER_WAPI_CLASSES
{
    public class BookingsGetReq
    {
        public string BookingNumber { get; set; }
        public string BookingId { get; set; }
    }

    public class BookingsGetRes
    {
        public ResponseStatus ResponseStatus { get; set; } = new ResponseStatus();
    } 
}
