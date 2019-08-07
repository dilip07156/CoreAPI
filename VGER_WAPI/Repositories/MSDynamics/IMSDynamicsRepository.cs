using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Repositories
{
    public interface IMSDynamicsRepository
    {
        Task<ResponseStatus> CreateOpportunity(string QRFId, string ParentQRFId, string ckLoginUser_Id);

        Task<ResponseStatus> CreateUpdateOpportnity(string QRFId, string ckLoginUser_Id);

        Task<ResponseStatus> CreateUpdateQuotation(string QRFId, string VoyagerUserId);

        Task<bool> UpdatemQuoteNmQRFPriceMapping(QuoteMappingReq request);

        Task<ResponseStatus> CreateUpdateBooking(string BookingNo, string VoyagerUser);

        Task<bool> UpdatemBookingMapping(BookingMappingReq request);

        Task<bool> AddOpportunityMapping(QuoteMappingReq request);

        Task<ResponseStatus> RejectOpportunityInfo(string QRFId, string VoyagerUserId);

        Task<IntegartionPlatform_Req> GetIntegrationConfigInfo(IntegartionPlatform_Req request);

        Task<IntegrationMappingDataRes> GetAllApplicationMappingDataList(IntegrationMappingDataReq request);

        Task<mCompanies> GetCompanyInfo(string CompanyId);
    }
}
