using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES.Booking
{
   public class QrfPackagePriceGetRes
    {
        public QrfPackagePriceGetRes()
        {
            RoomType = new List<string>();
            Response = new ResponseStatus();
        }
        public List<string> RoomType { get; set; }
        public ResponseStatus Response { get; set; }
    }
}
