using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class GuesstimateGetReq
    {
        public string QRFID { get; set; }
        public string PositionId { get; set; }
        public long DepartureId { get; set; }
        public long PaxSlabId { get; set; }
        public string SupplierId { get; set; }
        public string CalculateFor { get; set; }
        public string LoginUserId { get; set; }
    }
}
