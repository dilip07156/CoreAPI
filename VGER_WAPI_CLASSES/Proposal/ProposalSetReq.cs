using System;
using System.Collections.Generic;
using System.Text;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI_CLASSES
{
    public class ProposalSetReq
    {
        public bool IsNewVersion { get; set; } = false;
        public mProposal Proposal { get; set; } = new mProposal();
        public string QRFID { get; set; }
        public string VoyagerUserId { get; set; }
    }

    public class ProposalIncludeRegions
    {
        public bool Costsheet { get; set; } = true;
        public bool IncExc { get; set; } = true;
        public bool RegionMap { get; set; } = true;
        public bool SuggestedHotels { get; set; } = true;
        public bool ItineraryBrief { get; set; } = true;
        public bool ItineraryDetail { get; set; } = false;
    }
}
