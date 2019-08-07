using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI_CLASSES;


namespace VGER_WAPI.Repositories
{
    public interface ICommercialsRepository
    {
        CommercialsGetRes GetCommercials(CommercialsGetReq request);
        Task<CommonResponse> ChangePositionKeepAs(ChangePositionKeepReq request);
        Task<CommonResponse> SaveCommercials(CommercialsSetReq request);
        Task<string> SetQuoteDetails(QuoteSetReq request);
    }
}
