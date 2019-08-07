using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class CostsheetVerSetReq
    {
        public string QRFID { get; set; }
        public string QRFPriceId { get; set; }
        public string Create_User { get; set; }
        public string Pipeline { get; set; }
    }
}
