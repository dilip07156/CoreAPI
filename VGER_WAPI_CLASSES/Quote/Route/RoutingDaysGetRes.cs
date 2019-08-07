using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class RoutingDaysGetRes
    {
        public RoutingDaysGetRes()
        {
            RoutingDays = new List<RoutingDays>();
            ResponseStatus = new ResponseStatus();
        }

        public string QRFID { get; set; }
        public List<RoutingDays> RoutingDays { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
    }
}
