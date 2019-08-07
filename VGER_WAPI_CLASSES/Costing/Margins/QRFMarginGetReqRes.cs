using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class QRFMarginGetReq
    {
        public string QRFID { get; set; }
    }

    public class QRFMarginGetRes
    {
        public QRFMarginGetRes()
        {
            ResponseStatus = new ResponseStatus();
            Margins = new QRFMargins();
            Margins.Package = new QRFMarginPackage();
            Margins.Product = new QRFMarginProduct();
        }
        public ResponseStatus ResponseStatus { get; set; }
        public string QRFID { get; set; }
        public QRFMargins Margins { get; set; }
    }
}
