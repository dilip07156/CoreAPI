using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class RejectOpportunityReq
    {
        public RejectOpportunityReq()
        {
            CredentialInfo = new PartnerLoginRequest();
            OpportunityInfo = new mQuote();
            OpportunityQRFPriceInfo = new mQRFPrice();
        }

        public PartnerLoginRequest CredentialInfo { get; set; }
        public mQuote OpportunityInfo { get; set; }
        public mQRFPrice OpportunityQRFPriceInfo { get; set; }
        public string PartnerEntityCode { get; set; }
        public string StatusCode { get; set; }
    }
}
