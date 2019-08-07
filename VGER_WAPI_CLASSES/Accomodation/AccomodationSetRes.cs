using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class AccomodationSetRes
    {
        public AccomodationSetRes()
        {
            ResponseStatus = new ResponseStatus();
        }
        public ResponseStatus ResponseStatus { get; set; }
        public long QRFId { get; set; }
        public List<PositionInfo> PositionInfo { get; set; }
    }
}
