using System.Collections.Generic;
using System.Threading.Tasks;
using VGER_WAPI_CLASSES;
using VGER_WAPI_CLASSES.MIS;

namespace VGER_WAPI.Repositories
{
    public interface IMISRepository
    {
        #region Hotels Search & View by Booking
        Task<MISMappingRes> CheckMisMappingsRoles(AgentCompanyReq request);
        Task<SalesDashboardRes> GetSalesDashboardSummary(SalesDashboardReq request);
        Task<SalesDashboardFiltersRes> GetSalesDashboardFiltersList(AgentCompanyReq request);
        BookingsDashboardRes GetBookingsDashboardSummary(SalesDashboardReq request);
        Task<MisSearchGetResList> SearchMisData(SearchMisReqGet request);
        Task<MisSaveResponse> SaveMisData(SearchMisReqGet request);
        Task<MisSaveResponse> DeleteMisArtifactData(SearchMisReqGet request);
        #endregion
    }
}
