using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class QRFMarginCurrencyGetReq
    {
        public string CurrencyUnit { get; set; }
    }
    public class QRFMarginCurrencyGetRes
    {
        public QRFMarginCurrencyGetRes()
        {
            Currency = new List<Currency>();
            ResponseStatus = new ResponseStatus();
        }
        public List<Currency> Currency { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
    }

}
