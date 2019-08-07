using System.Collections.Generic;

namespace VGER_WAPI_CLASSES
{
    public class ConfirmBookingSetReq
    {
        public string QRFID { get; set; }
        public List<long> DepatureId { get; set; }
        public string UserName { get; set; }
    }
}
