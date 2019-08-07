using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class ConfirmBookingSetRes
    {
        public ConfirmBookingSetRes()
        {
            ResponseStatus = new ResponseStatus();
        }
        public ResponseStatus ResponseStatus { get; set; }
        public string QRFID { get; set; }
    }
}
