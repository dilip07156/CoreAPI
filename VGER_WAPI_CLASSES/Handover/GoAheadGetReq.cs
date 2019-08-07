using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class GoAheadGetReq
    {
        public string QRFID { get; set; }
        public string GoAheadId { get; set; }
        public long DepatureId { get; set; }
        public string UserName { get; set; } 
    }
}
