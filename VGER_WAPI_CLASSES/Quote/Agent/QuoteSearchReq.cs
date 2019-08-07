using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class QuoteSearchReq
    {
        //Tour Details
        public string CurrentPipeline { get; set; }
        public string AgentName { get; set; }
        public string QRFID { get; set; }
        public string TourCode { get; set; }
        public string TourName { get; set; }
        public string QRFStatus { get; set; }
        public string Priority { get; set; }
        public string QuoteResult { get; set; }
        public string Division { get; set; }
        public string Destination { get; set; }
		public string UserId { get; set; }
		public string UserName { get; set; }

		//Followup details
		public string Date { get; set; } 
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string Month { get; set; }
        public int Year { get; set; }
    }
}
