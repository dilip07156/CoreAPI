using System.Collections.Generic;

namespace VGER_WAPI_CLASSES
{
    public class PaymentScheduleGetReq
    {
        public string BookingNumber { get; set; }
        public string PositionId { get; set; }
    }

    public class PaymentScheduleGetRes
    {
        public string BookingNumber { get; set; }
        public string PositionId { get; set; }
        public List<PaymentSchedule> PaymentSchedule { get; set; } = new List<PaymentSchedule>();
        public ResponseStatus ResponseStatus = new ResponseStatus();
    }
}