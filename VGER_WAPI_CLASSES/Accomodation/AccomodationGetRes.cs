using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class AccomodationGetRes
    {
        public AccomodationGetRes()
        { 
            ResponseStatus = new ResponseStatus();
        }

        public long QRFID { get; set; } 
        public ResponseStatus ResponseStatus { get; set; }
    }
}
