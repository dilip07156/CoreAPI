using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class QRFPaxSlabSetReq
    {
        public QRFPaxSlabDetails PaxSlabDetails { get; set; }
        public string QRFID { get; set; }

        public QRFPaxSlabSetReq()
        {
            PaxSlabDetails = new QRFPaxSlabDetails();
        }
    }
}
