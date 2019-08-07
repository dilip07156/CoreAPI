using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VGER_WAPI.Helpers;
using VGER_WAPI.Models;
using VGER_WAPI.Repositories;
using VGER_WAPI_CLASSES;
namespace VGER_WAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/Agent")]
    public class AgentController : Controller
    {
        #region Private Variable Declaration
        private readonly IAgentRepository _agentRepository;
        private readonly IUserRepository _userRepository;
        private readonly MongoContext _MongoContext = null;
        #endregion

        public AgentController(IAgentRepository agentRepository, IUserRepository userRepository, IOptions<MongoSettings> settings)
        {
            _agentRepository = agentRepository;
            _userRepository = userRepository;
            _MongoContext = new MongoContext(settings);
        }

        [Authorize]
        [HttpPost]
        [Route("GetAgentDetails")]
        public async Task<AgentGetRes> GetAgentDetails([FromBody] AgentGetReq request)
        {
            var response = new AgentGetRes();
            try
            {
                response = await _agentRepository.GetAgentDetails(request);
                response.ResponseStatus.Status = "Success";
                response.ResponseStatus.ErrorMessage = response != null ? "" : "No Records Found.";
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }

            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("GetAgentDetailedInfo")]
        public async Task<AgentGetRes> GetAgentDetailedInfo([FromBody] AgentGetReq request)
        {
            var response = new AgentGetRes();
            UserRoleGetReq req = new UserRoleGetReq();
            req.UserID = request.UserId;
            try
            {
                response = await _agentRepository.GetAgentDetailedInfo(request);
                //response.StatusList = await _agentRepository.GetStatusForAgents();
                //response.DefStartPageList = await _agentRepository.GetStartPageForAgents();
                //response.DefDocumentTypes = await _agentRepository.GetDefDocumentTypes();
                //response.ProductList = await _agentRepository.GetProductTypes();

                //response.UserRolesDetails =  _userRepository.GetUserRoleDetails(req).Result.UserRolesDetails;
                //response.UserId = request.UserId;

                response.ResponseStatus.Status = "Success";
                response.ResponseStatus.ErrorMessage = response != null ? "" : "No Records Found.";
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }

            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("GetStatusForAgents")]
        public async Task<List<mStatus>> GetStatusForAgents()
        {
            var response = new List<mStatus>();
            try
            {
                response = await _agentRepository.GetStatusForAgents();
            }
            catch (Exception ex)
            {
                response = null;
            }
            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("GetDefDocumentTypes")]
        public async Task<List<Attributes>> GetDefDocumentTypes()
        {
            var response = new List<Attributes>();
            try
            {
                response = await _agentRepository.GetDefDocumentTypes();
            }
            catch (Exception ex)
            {
                response = null;
            }
            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("GetProductTypes")]
        public async Task<List<Attributes>> GetProductTypes()
        {
            var response = new List<Attributes>();
            try
            {
                response = await _agentRepository.GetProductTypes();
            }
            catch (Exception ex)
            {
                response = null;
            }
            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("SetAgentDetailedInfo")]
        public async Task<AgentSetRes> SetAgentDetailedInfo([FromBody] AgentSetReq request)
        {
            var response = new AgentSetRes();
            try
            {
                response = await _agentRepository.SetAgentDetailedInfo(request);
                response.ResponseStatus.Status = response.ResponseStatus.Status == null ? "Success" : response.ResponseStatus.Status;
                response.ResponseStatus.ErrorMessage = response.ResponseStatus.ErrorMessage == null ? "" : response.ResponseStatus.ErrorMessage;
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }

            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("GetUserDetailsByContactId")]
        public async Task<AgentGetRes> GetUserDetailsByContactId([FromBody] AgentGetReq request)
        {
            var response = new AgentGetRes();
            try
            {
                response.User = await _agentRepository.GetUserDetailsByContactId(request);
                response.UserId = response.User.VoyagerUser_Id;
                response.ResponseStatus.Status = "Success";
                response.ResponseStatus.ErrorMessage = response != null ? "" : "No Records Found.";
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }

            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("GetStartPageForAgents")]
        public async Task<List<mDefStartPage>> GetStartPageForAgents()
        {
            var response = new List<mDefStartPage>();
            try
            {
                response = await _agentRepository.GetStartPageForAgents();
            }
            catch (Exception ex)
            {
                response = null;
            }
            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("GetCompanyOfficers")]
        public async Task<CompanyOfficerGetRes> GetCompanyOfficers([FromBody] CompanyOfficerGetReq request)
        {
            var response = new CompanyOfficerGetRes();
            try
            {
                if (!string.IsNullOrEmpty(request.CompanyId))
                {
                    response = await _agentRepository.GetCompanyOfficers(request);
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "CompanyId can not be Null/Empty.";
                }
            }
            catch (Exception ex)
            {
                response = null;
            }
            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("GetCompanyContacts")]
        public async Task<CompanyOfficerGetRes> GetCompanyContacts([FromBody] CompanyOfficerGetReq request)
        {
            var response = new CompanyOfficerGetRes();
            try
            {
                if (!string.IsNullOrEmpty(request.CompanyId))
                {
                    response = await _agentRepository.GetCompanyContacts(request);
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "CompanyId can not be Null/Empty.";
                }
            }
            catch (Exception ex)
            {
                response = null;
            }
            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("GetSystemCompany")]
        public Response GetSystemCompany([FromBody] string LoggedInUserContact_Id)
        {
            var response = new Response();
            try
            {
                bool flag = false;
                flag = _agentRepository.GetSystemCompany(LoggedInUserContact_Id, out string SystemCompanyId);
                response.SystemCompanyId = SystemCompanyId;
                if (flag == true)
                {
                    response.ResponseStatus.Status = "Success";
                }
                else
                {
                    response.ResponseStatus.Status = "Error";
                    response.ResponseStatus.ErrorMessage = "SystemCompanyId not found for logged in user";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Error";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }
            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("GetSalesOfficesOfSystemCompany")]
        public async Task<CompanyOfficerGetRes> GetSalesOfficesOfSystemCompany()
        {
            var response = new CompanyOfficerGetRes();
            try
            {
                response = await _agentRepository.GetSalesOfficesOfSystemCompany();
            }
            catch (Exception ex)
            {
                response = null;
            }
            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("GetSalesOfficesByCompanyId")]
        public async Task<CompanyOfficerGetRes> GetSalesOfficesByCompanyId([FromBody] string CompanyId)
        {
            var response = new CompanyOfficerGetRes();
            try
            {
                response = await _agentRepository.GetSalesOfficesByCompanyId(CompanyId);
            }
            catch (Exception ex)
            {
                response = null;
            }
            return response;
        }

        #region mComapnies->Target, mCompnies->Contacts->Targets
        [Authorize]
        [HttpPost]
        [Route("GetCompanyTargets")]
        public async Task<TargetGetRes> GetCompanyTargets([FromBody] AgentGetReq request)
        {
            TargetGetRes response = new TargetGetRes() { ResponseStatus = new ResponseStatus(), TargetList = new List<Targets>() };
            try
            {
                response = await _agentRepository.GetCompanyTargets(request); 
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }
            return response;
        }
        #endregion

        #region 3rd party search agent

        [Authorize]
        [HttpPost]
        [Route("GetPartnerAgentDetails")]
        public async Task<AgentThirdPartyGetRes> GetPartnerAgentDetails([FromBody] AgentThirdPartyGetReq request)
        {
            var response = new AgentThirdPartyGetRes();
            try
            {
                response = await _agentRepository.GetPartnerAgentDetails(request);
                response.ResponseStatus.ErrorMessage = response.CompanyId != null ? "" : "No Records Found.";
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }

            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("GetPartnerAgentContactDetails")]
        public async Task<AgentThirdPartyGetRes> GetPartnerAgentContactDetails([FromBody] AgentThirdPartyGetReq request)
        {
            var response = new AgentThirdPartyGetRes();
            try
            {
                response = await _agentRepository.GetPartnerAgentContactDetails(request);
                response.ResponseStatus.ErrorMessage = response.ContactId != null ? "" : "No Records Found.";
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }

            return response;
        }
        #endregion

        #region 3rd party AgentInfo or ContactInfo

        [Authorize]
        [HttpPost]
        [Route("CreateUpdatePartnerAgentDetails")]
        public async Task<ManageAgentRes> CreateUpdatePartnerAgentDetails([FromBody] ManageAgentReq request)
        {
            var response = new ManageAgentRes();
            try
            {
                response = await _agentRepository.CreateUpdatePartnerAgentDetails(request);
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }

            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("CreatePartnerAgentContactDetails")]
        public async Task<AgentThirdPartyGetRes> CreatePartnerAgentContactDetails([FromBody] ManageAgentContactReq request)
        {
            var response = new AgentThirdPartyGetRes();
            try
            {
                response = await _agentRepository.CreatePartnerAgentContactDetails(request);
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }

            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("UpdatePartnerAgentContactDetails")]
        public async Task<AgentThirdPartyGetRes> UpdatePartnerAgentContactDetails([FromBody] ManageAgentContactReq request)
        {
            var response = new AgentThirdPartyGetRes();
            try
            {
                response = await _agentRepository.UpdatePartnerAgentContactDetails(request);
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }

            return response;
        }

        #endregion
    }
}