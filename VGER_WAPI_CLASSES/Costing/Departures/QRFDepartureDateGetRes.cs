using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class QRFDepartureDateGetRes
    {
        public List<QRFDepartureDates> DepartureDates { get; set; }
        public string Status { get; set; }

        public QRFDepartureDateGetRes()
        {
            DepartureDates = new List<QRFDepartureDates>();
        }
    }
}
