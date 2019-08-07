using System;
using System.Collections.Generic;
using System.Text;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI_CLASSES
{ 
    public class ProposalGetRes
    {
        public mProposal Proposal { get; set; }

        public List<mTermsAndConditions> TermsAndConditions { get; set; }


        public List<Hotel> Hotels { get; set; }

        public ResponseStatus ResponseStatus { get; set; }

        public ProposalGetRes()
        {
            TermsAndConditions = new List<mTermsAndConditions>();
            Proposal = new mProposal();
            ResponseStatus = new ResponseStatus();
        }
    }
}
