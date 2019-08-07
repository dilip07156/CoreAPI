using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class GetQRFForCopyQuoteRes
    {
        public string QRFID;
        public string TourName { get; set; }
        public AgentInfo AgentInfo { get; set; } = new AgentInfo();
        public List<ExisitingDepatures> ExisitingDepatures { get; set; } = new List<ExisitingDepatures>();
        public ResponseStatus ResponseStatus { get; set; } = new ResponseStatus();

    }

    public class SetCopyQuoteReq
    {
        public string QRFID { get; set; }
        public string TourName { get; set; }
        public string AgentId { get; set; }
        public string AgentName { get; set; }
        public string MobileNo { get; set; }
        public string Email { get; set; }
        public string ContactPerson { get; set; }
        public List<CopyQuoteDepartures> CopyQuoteDepartures { get; set; } = new List<CopyQuoteDepartures>();
        public string CreateUser { get; set; }
        public string VoyagerUserId { get; set; }
    }

    public class SetCopyQuoteRes
    {
        public string QRFID { get; set; }
        public ResponseStatus ResponseStatus { get; set; } = new ResponseStatus();
    }

    public class CopyQuoteDepartures
    {
        [Required]
        public DateTime NewDepartureDate { get; set; }
        public long DepartureId { get; set; }
    }
}
