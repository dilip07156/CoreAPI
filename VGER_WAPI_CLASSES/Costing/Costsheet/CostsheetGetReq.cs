using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class CostsheetGetReq
    {
        public string QRFID { get; set; }

        public long DepartureId { get; set; }

        public long PaxSlabId { get; set; }

        public string EnquiryPipeline { get; set; }
    }
}
