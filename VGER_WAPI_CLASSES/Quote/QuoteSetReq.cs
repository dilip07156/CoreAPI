using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class QuoteSetReq
    {
        public string QRFID { get; set; }

        //public string CurrentPipeline { get; set; }
        //public string CurrentPipelineStep { get; set; }
        //public string CurrentPipelineSubStep { get; set; }
        //public string Status { get; set; }
        //public string QuoteResult { get; set; }
        public string Remarks { get; set; }
        public string PlacerUser { get; set; }
        public string PlacerUserId { get; set; }
        public string PlacerEmail { get; set; }
        //public string EditDate { get; set; }
        public string CostingOfficer { get; set; }
        //public string ProductAccountant { get; set; }
        public string EnquiryPipeline { get; set; }
        public string MailStatus { get; set; }
        public string QRFPriceID { get; set; }
        public bool IsApproveQuote { get; set; }
		public bool IsUI { get; set; }
		public string UserId { get; set; }
        public bool IsCopyQuote { get; set; }
        public string VoyagerUserID { get; set; }
    } 

    public class DatePaxDetailsSetRequest
    {
        public List<PaxSlabs> PaxSlabs { get; set; }
        public List<DepartureDates> DepartureDates { get; set; }
        public List<PaxSlabs> PaxSlabsNew { get; set; }
        public List<DepartureDates> DepartureDatesNew { get; set; }

        public string QRFID { get; set; }
        public string UserEmail { get; set; } 

        public DatePaxDetailsSetRequest()
        {
            PaxSlabs = new List<PaxSlabs>();
            DepartureDates = new List<DepartureDates>();
            PaxSlabsNew = new List<PaxSlabs>();
            DepartureDatesNew = new List<DepartureDates>();
        }
    }
}
