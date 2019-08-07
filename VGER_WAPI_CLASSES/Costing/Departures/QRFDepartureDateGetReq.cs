using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class QRFDepartureDateGetReq
    {
        public string QRFID { get; set; }
        public DateTime? date { get; set; }
    }
}
