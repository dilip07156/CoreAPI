using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class RoutingSetReq
    {
        public string QRFID { get; set; }
        public List<RoutingInfo> RoutingInfo { get; set; }
        public string Step { get; set; }
        public string SubStep { get; set; }
        public bool IsOverwriteExtPos { get; set; }
        public bool IsSetPrefHotels { get; set; }
        public string VoyagerUserId { get; set; }
    }
}
