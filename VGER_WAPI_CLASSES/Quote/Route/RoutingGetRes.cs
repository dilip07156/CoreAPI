using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class RoutingGetRes
    {
        public RoutingGetRes()
        {
            RoutingInfo = new List<RoutingInfo>();
            ResponseStatus = new ResponseStatus();
        }

        public string QRFID { get; set; }
        public List<RoutingInfo> RoutingInfo { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
    }
}
