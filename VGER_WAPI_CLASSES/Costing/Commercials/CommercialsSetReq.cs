using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class CommercialsSetReq
    {
        public string QRFPriceId { get; set; }
        public double PercentSoldOptional { get; set; }
        public string EditUser { get; set; }
        public string QrfId { get; set; }
        public string VoyagerUserId { get; set; }
    }
}
