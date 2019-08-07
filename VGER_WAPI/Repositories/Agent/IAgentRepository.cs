using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Repositories
{
    public interface IAgentRepository
    {
        Task<AgentGetRes> GetAgentDetails(AgentGetReq request);

        Task<AgentGetRes> GetAgentDetailedInfo(AgentGetReq request);

        Task<AgentSetRes> SetAgentDetailedInfo(AgentSetReq request);

        Task<List<mStatus>> GetStatusForAgents();

        Task<List<mDefStartPage>> GetStartPageForAgents();

        Task<mUsers> GetUserDetailsByContactId(AgentGetReq request);

        Task<List<Attributes>> GetDefDocumentTypes();

        Task<List<Attributes>> GetProductTypes();
		bool GetSystemCompany(string LoggedInUserContact_Id, out string SystemCompanyId);

		Task<CompanyOfficerGetRes> GetCompanyOfficers(CompanyOfficerGetReq request);

        Task<CompanyOfficerGetRes> GetCompanyContacts(CompanyOfficerGetReq request);

		Task<CompanyOfficerGetRes> GetSalesOfficesOfSystemCompany();

        Task<CompanyOfficerGetRes> GetSalesOfficesByCompanyId(string CompanyId);

        Task<TargetGetRes> GetCompanyTargets(AgentGetReq request);

        #region 3rd party Search Agent Details

        Task<AgentThirdPartyGetRes> GetPartnerAgentDetails(AgentThirdPartyGetReq request);

        Task<AgentThirdPartyGetRes> GetPartnerAgentContactDetails(AgentThirdPartyGetReq request);

        #endregion

        #region 3rd party AgentInfo

        Task<ManageAgentRes> CreateUpdatePartnerAgentDetails(ManageAgentReq request);

        Task<AgentThirdPartyGetRes> CreatePartnerAgentContactDetails(ManageAgentContactReq request);

        Task<AgentThirdPartyGetRes> UpdatePartnerAgentContactDetails(ManageAgentContactReq request);

        #endregion
    }
}
