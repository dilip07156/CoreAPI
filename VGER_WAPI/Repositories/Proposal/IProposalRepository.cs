using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI
{
    public interface IProposalRepository
    {
        #region Proposal Details

        ProposalGetRes GetProposal(ProposalGetReq request);

        Task<ProposalSetRes> SetProposal(ProposalSetReq request);

        ProposalDocumentGetRes GetProposalDocumentDetailsByQRFID(QuoteAgentGetReq request);

        ProposalDocumentGetRes GetProposalDocumentHeaderDetails(QuoteAgentGetReq request);

        ProposalGetRes GetHotelSummaryByQrfId(ProposalGetReq request);

        #endregion
    }
}
