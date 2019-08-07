using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class ChangePositionKeepReq
    {
        public string QRFID { get; set; }
        public string ChangeType { get; set; }
        public List<string> PositionIds { get; set; }
        public string EditUser { get; set; }
    }
}
