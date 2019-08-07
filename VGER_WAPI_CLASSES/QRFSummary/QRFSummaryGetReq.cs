using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class QRFSummaryGetReq
    {
        public string QRFID { get; set; }
        public bool IsCosting { get; set; } = false;
    }
}
