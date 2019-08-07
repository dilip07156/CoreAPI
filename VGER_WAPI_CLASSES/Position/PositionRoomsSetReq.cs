using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class PositionRoomsSetReq
    {
        public PositionRoomsSetReq()
        {
            RoomDetailsInfo = new List<RoomDetailsInfo>();
        }
        public string PositionId { get; set; }
        public string ProductId { get; set; }
        public string QRFId { get; set; }
        public string EditUser { get; set; }
        public List<RoomDetailsInfo> RoomDetailsInfo { get; set; }
    }
}
