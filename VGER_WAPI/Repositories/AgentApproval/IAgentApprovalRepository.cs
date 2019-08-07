using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Repositories
{
    public interface IAgentApprovalRepository
    {
        #region Send to client Mail
        Task<SendToClientSetRes> SendToClientMail(SendToClientSetReq request);

        Task<SendToClientSetRes> GetSendToClientDetails(SendToClientGetReq request);

        Task<AcceptSendToClientSetRes> AcceptSendToClient(SendToClientGetReq request);

        Task<SendToClientGetRes> SetSuggestSendToClient(SendToClientGetReq request);

        Task<CostingGetRes> GetSuggestSendToClient(GetSuggestionReq request);
        #endregion 

        Task<CommonResponse> AcceptWithoutProposal(EmailGetReq request);

        Task<CommonResponse> AmendmentQuote(AmendmentQuoteReq request,bool IsSuggestion=false);

        Task<CommonResponse> CheckProposalGenerated(QuoteGetReq request);
    }
}
