using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class PositionPriceGetReq
    {
        public string QRFID { get; set; }
        public string PositionId { get; set; }
        public long DepartureId { get; set; }
        public long PaxSlabId { get; set; }
        public string ProductType { get; set; }
        public string ProductID { get; set; }
        public bool IsClone { get; set; } = false;
        public string LoginUser { get; set; }
    }
}
