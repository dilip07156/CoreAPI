using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class OpsHeaderGetReq
    {
        public string BookingNumber { get; set; }
        public string PositionId { get; set; }
        public Bookings Bookings { get; set; }
        public string UIProductType { get; set; }
    }
}
