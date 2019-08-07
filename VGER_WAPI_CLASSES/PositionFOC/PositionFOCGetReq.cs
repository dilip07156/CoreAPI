using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class PositionFOCGetReq
    {
        public string QRFID { get; set; }
        public string PositionId { get; set; }
        public string ProductID { get; set; }
        public bool IsClone { get; set; } = false;
    }
}
