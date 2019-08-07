using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES 
{
   public class RoutingDaysSetReq
    {
        public string QRFID { get; set; }
        public string UserName { get; set; } = "";
        public List<RoutingInfo> RoutingInfo { get; set; } = new List<RoutingInfo>();
        public List<RoutingInfo> ExistingRoutingInfo { get; set; } = new List<RoutingInfo>();
    }
}
