using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class ConfirmBookingGetReq
    {
        public string QRFID { get; set; }
        public List<long> DepatureId { get; set; }
        public string UserName { get; set; }
    }
}
