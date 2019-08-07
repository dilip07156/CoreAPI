using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class BookingDocumentGetReq
    {
        public string Booking_Id { get; set; }

        public string Document_Id { get; set; }

        public string User { get; set; }

        public string Type { get; set; }
    }
}
