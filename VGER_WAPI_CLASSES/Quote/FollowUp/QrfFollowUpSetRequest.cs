using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class QrfFollowUpSetRequest
    {
        public string QRFID { get; set; }
        public string QRFStatus { get; set; }
        public FollowUpItem FollowUpItem { get; set; }
    }
}
