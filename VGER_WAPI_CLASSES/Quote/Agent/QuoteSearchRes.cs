using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class QuoteSearchRes
    {
        public QuoteSearchRes()
        {
            QuoteSearchDetails = new List<QuoteSearchDetails>();
            ResponseStatus = new ResponseStatus();
        }
        public List<QuoteSearchDetails> QuoteSearchDetails { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
        public int TotalCount { get; set; }
    }

    public class QuoteSearchDetails
    {
        public QuoteSearchDetails()
        {
            FollowUpItem = new FollowUpItem(); 
            AgentPassengerInfo = new AgentPassengerInfo();
        }

        public string QRFID { get; set; }
        public string AgentCompanyName { get; set; }
        public string AgentContactName { get; set; }

        public string TourName { get; set; }
        public int? QRFDuration { get; set; }
        public string Destination { get; set; }

        public string AgentMobileNo { get; set; }
        public string AgentEmailAddress { get; set; }

        public AgentPassengerInfo AgentPassengerInfo { get; set; }

        public string TourCode { get; set; }

        public FollowUpItem FollowUpItem { get; set; }

        public DateTime CreateDate { get; set; }

        public List<DateTime?> DeparturesDate { get; set; } = new List<DateTime?>();

        public List<FollowUp> FollowUp { get; set; } = new List<FollowUp>();

    }
}
