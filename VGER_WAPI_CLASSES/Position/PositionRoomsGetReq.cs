using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class PositionRoomsGetReq
    {
        public string PositionId { get; set; }
        public string ProductId { get; set; }
        public string QRFId { get; set; }
        public int RowNo { get; set; }
        public string PositionType { get; set; }
    }
}
