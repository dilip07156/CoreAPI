using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI_CLASSES;


namespace VGER_WAPI.Repositories
{
    public interface IGuesstimateRepository
    {
        GuesstimateGetRes GetGuesstimate(GuesstimateGetReq request);

        Task<GuesstimateSetRes> SetGuesstimate(GuesstimateSetReq request, bool fromGet = false);

        List<GuesstimateVersion> GetGuesstimateVersions(GuesstimateGetReq request);

        Task<GuesstimateSetRes> UpdateGuesstimateVersion(GuesstimateVersionSetReq request);

        GuesstimateGetRes GetSupplierPrice(GuesstimateGetReq request);

        Task<GuesstimateSetRes> SetGuesstimateChangeRule(GuesstimateChangeRuleSetReq request);
    }
}
