using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class AccomodationSetReq
    {
        public long QRFID { get; set; }
        public string SaveType { get; set; }
        public List<AccomodationInfo> AccomodationInfo { get; set; }
    }
}
