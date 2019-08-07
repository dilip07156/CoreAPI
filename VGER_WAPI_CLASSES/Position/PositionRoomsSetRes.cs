using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class PositionRoomsSetRes
    {
        public string PositionId { get; set; }
        public string ProductId { get; set; }
        public string QRFId { get; set; }
        public ResponseStatus ResponseStatus { get; set; } = new ResponseStatus();
        public List<RoomDetailsInfo> RoomDetailsInfo { get; set; } = new List<RoomDetailsInfo>();
    }
}
