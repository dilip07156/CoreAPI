using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class QuoteThirdPartyGetRes
    {
        public QuoteThirdPartyGetRes()
        {
            ResponseStatus = new ResponseStatus();
        }

        public string QRFID { get; set; }
        public string CurrentPipeline { get; set; }
        public string PartnerEntityCode { get; set; }
        public string ApplicationName { get; set; }
        public string PartnerEntityName { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
    }
}
