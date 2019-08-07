namespace VGER_WAPI_CLASSES
{
    public class BookingPositionGetReq
    {
        public string PositionId { get; set; }
    }

    public class BookingPositionGetRes
    {
        public ResponseStatus ResponseStatus { get; set; } = new ResponseStatus();
    }
}