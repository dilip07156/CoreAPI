using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Repositories
{
    public interface IQRFSummaryRepository
    {
        Task<QRFSummaryGetRes> GetQRFSummary(QRFSummaryGetReq request);


        /// <summary>
        /// To update params in quote details
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<QuoteSetRes> SubmitQuote(QuoteSetReq request);

        Task<bool> DeleteOldQRFData(string QRFID);
        Task<bool> SaveDefaultQRFPosition(string QRFID);
        Task<bool> SaveDefaultGuesstimate(string QRFID, string VersionName = "Default", string VersionDescription = "Deafult", string UserName = "");
        Task<string> SaveQRFPrice(string VersionName, string VersionDescription, string QRFID, string UserName);
        Task<bool> SaveQRFCost(string QRFPriceId, string QRFID);
        Task<bool> SaveDefaultProposal(string QRFID, string editUser);
        Task<bool> SaveDefaultItinerary(string editUser, string QRFID, string ItineraryId, bool IsCosting = false);

        #region Copy Quote
        Task<GetQRFForCopyQuoteRes> GetQRFDataForCopyQuote(QuoteAgentGetReq request);
        Task<SetCopyQuoteRes> SetCopyQuote(SetCopyQuoteReq request);
        #endregion
    }
}
