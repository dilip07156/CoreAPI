using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class MarginSetReq
    {
        public MarginSetReq()
        {
            Margins = new Margins();
        }
        public string QRFID { get; set; }
        public bool IsCostingMargin { get; set; }
        public string VoyagerUserId { get; set; }
        public Margins Margins { get; set; } 
    }
}
