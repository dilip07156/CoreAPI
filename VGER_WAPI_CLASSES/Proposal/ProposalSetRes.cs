using System;
using System.Collections.Generic;
using System.Text;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI_CLASSES
{
    public class ProposalSetRes
    {
        public ProposalSetRes()
        {
            ResponseStatus = new ResponseStatus();
        }
        public ResponseStatus ResponseStatus { get; set; }
        public string QRFProposalId { get; set; }
        public List<mProposal> mQRFProposal { get; set; }
    }
}
