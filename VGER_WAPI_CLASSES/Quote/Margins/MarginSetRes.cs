using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class MarginSetRes
    {
        public MarginSetRes()
        {
            ResponseStatus = new ResponseStatus();
        }

        public ResponseStatus ResponseStatus { get; set; }
        public string QRFID { get; set; }
    }
}
