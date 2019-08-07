using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class QuoteMappingReq
    {
        public string QRFID { get; set; }
        public string QRFPrice_Id { get; set; }
        public string PartnerEntityCode { get; set; }
        public string Source { get; set; }
        public string CreatedBy { get; set; }
        public string Type { get; set; }
    }

    public class BookingMappingReq
    {
        public string BookingId { get; set; }
        public string QRFID { get; set; }
        public string QRFPrice_Id { get; set; }
        public string BookingNo { get; set; }
        public string PartnerEntityCode { get; set; }
        public string Source { get; set; }
        public string CreatedBy { get; set; }
        public string Type { get; set; }
    }
}
