using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class QuoteThirdPartyGetReq
    {
        public string PartnerEntityCode { get; set; }
        public string PartnerEntityName { get; set; }
        public string PartnerEntityContactCode { get; set; }
        public string Module { get; set; }
        public string Operation { get; set; }
        public string Application { get; set; }
        public string QrfID { get; set; }

    }
}
