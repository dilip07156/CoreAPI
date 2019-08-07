using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Repositories
{
    public interface IActivitiesRepository
    {
        Task<ActivitiesGetRes> GetActivitiesDetailsByQRFID(QuoteGetReq request);

        Task<ActivitiesSetRes> SetActivitiesDetails(ActivitiesSetReq request);
    }
}
