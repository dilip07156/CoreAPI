using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class QRFMarginSetReq
    {
        public QRFMarginSetReq()
        {
            Margins = new QRFMargins();
        }
        public string QRFID { get; set; }
        public QRFMargins Margins { get; set; }
    }

    public class QRFMarginSetRes
    {
        public QRFMarginSetRes()
        {
            ResponseStatus = new ResponseStatus();
        }

        public ResponseStatus ResponseStatus { get; set; }
        public string QRFID { get; set; }
    }
}
