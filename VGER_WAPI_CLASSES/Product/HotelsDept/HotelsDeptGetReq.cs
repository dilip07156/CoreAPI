using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class ProductSRPHotelGetReq
    {
        public string QRFID { get; set; }
    }

    public class HotelAlternateServicesGetReq
    {
        public string BookingNumber { get; set; }
        public string PositionId { get; set; }
    }

    public class HotelReservationRequestEmail
    {
        public string BookingNumber { get; set; }
        public string PositionId { get; set; }
        public string AltSvcId { get; set; }
        public string WebURLInitial { get; set; }
        public string PlacerUserId { get; set; }
        public string PlacerEmail { get; set; }
        public string SendType { get; set; }
    }
}
