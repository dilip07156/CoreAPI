using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VGER_WAPI.Models;
using VGER_WAPI.Repositories;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/AgentApproval")]
    public class AgentApprovalController : Controller
    {
        #region Private Variable Declaration
        private readonly IAgentApprovalRepository _agentApprovalRepository;
        private readonly IMSDynamicsRepository _mSDynamicsRepository;
        private readonly IQuoteRepository _quoteRepository;
        #endregion

        public AgentApprovalController(IAgentApprovalRepository agentApprovalRepository, IMSDynamicsRepository mSDynamicsRepository, IQuoteRepository quoteRepository)
        {
            _agentApprovalRepository = agentApprovalRepository;
            _mSDynamicsRepository = mSDynamicsRepository;
            _quoteRepository = quoteRepository;
        }

        #region Send To Client Mail  
        /// <summary>
        /// Send To Client Mail
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("SendToClientMail")]
        public async Task<SendToClientSetRes> SendToClientMail([FromBody] SendToClientSetReq request)
        {
            var response = new SendToClientSetRes();
            try
            {
                if (request != null)
                {
                    response = await _agentApprovalRepository.SendToClientMail(request);
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Details can not be blank.";
                }
                if (!string.IsNullOrEmpty(response.ResponseStatus.Status) && response.ResponseStatus.Status == "Success" && !string.IsNullOrEmpty(request.QRFID) && !string.IsNullOrEmpty(request.VoyagerUserId))
                {
                    //Task.Run(() => _mSDynamicsRepository.CreateUpdateOpportnity(request.QRFID, request.VoyagerUserId).Result);
                    Task.Run(() => _mSDynamicsRepository.CreateUpdateQuotation(request.QRFID, request.VoyagerUserId).Result);

                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }
            return response;
        }

        /// <summary>
        ///Get SendToClient Details
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetSendToClientDetails")]
        public async Task<SendToClientSetRes> GetSendToClientDetails([FromBody] SendToClientGetReq request)
        {
            var response = new SendToClientSetRes();
            try
            {
                if (request != null)
                {
                    response = await _agentApprovalRepository.GetSendToClientDetails(request);
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Details can not be blank.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }
            return response;
        }

        /// <summary>
        ///Accept SendToClient
        /// </summary>
        /// <param name="request"></param> 
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("AcceptSendToClient")]
        public async Task<AcceptSendToClientSetRes> AcceptSendToClient([FromBody] SendToClientGetReq request)
        {
            var response = new AcceptSendToClientSetRes();
            try
            {
                if (!string.IsNullOrEmpty(request.QRFID) && request.QRFID != "0" && !string.IsNullOrEmpty(request.QRFPriceID))
                {
                    response = await _agentApprovalRepository.AcceptSendToClient(request);
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Request details can not be blank.";
                }

                if (string.IsNullOrEmpty(request.VoyagerUserId) && response.ResponseStatus != null && !string.IsNullOrEmpty(response.ResponseStatus.Status) && response.ResponseStatus.Status == "Success" && !string.IsNullOrEmpty(request.QRFID))
                {
                    var quoteInfo = _quoteRepository.getQuoteInfo(request.QRFID).Result;
                    request.VoyagerUserId = quoteInfo.SalesPerson;
                }

                if (response.ResponseStatus != null && !string.IsNullOrEmpty(response.ResponseStatus.Status) && response.ResponseStatus.Status == "Success" && !string.IsNullOrEmpty(request.QRFID) && !string.IsNullOrEmpty(request.VoyagerUserId))
                {
                    Task.Run(() => _mSDynamicsRepository.CreateUpdateQuotation(request.QRFID, request.VoyagerUserId).Result);

                }

            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }

            return response;
        }

        /// <summary>
        ///set Suggest SendToClient
        /// </summary>
        /// <param name="request"></param> 
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("SetSuggestSendToClient")]
        public async Task<SendToClientGetRes> SetSuggestSendToClient([FromBody] SendToClientGetReq request)
        {
            var response = new SendToClientGetRes();
            try
            {
                if (!string.IsNullOrEmpty(request.QRFID) && request.QRFID != "0" && !string.IsNullOrEmpty(request.QRFPriceID))
                {
                    response = await _agentApprovalRepository.SetSuggestSendToClient(request);
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Request details can not be blank.";
                }

            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }

            return response;
        }

        /// <summary>
        ///Get Suggest SendToClient
        /// </summary>
        /// <param name="request"></param> 
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("GetSuggestSendToClient")]
        public async Task<CostingGetRes> GetSuggestSendToClient([FromBody] GetSuggestionReq request)
        {
            var response = new CostingGetRes();
            try
            {
                if (!string.IsNullOrEmpty(request.QRFID) && request.QRFID != "0" && !string.IsNullOrEmpty(request.QRFPriceID))
                {
                    response = await _agentApprovalRepository.GetSuggestSendToClient(request);
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Request details can not be blank.";
                }

            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }

            return response;
        }
        #endregion 

        /// <summary>
        /// Accept Without Proposal
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("AcceptWithoutProposal")]
        public async Task<CommonResponse> AcceptWithoutProposal([FromBody] EmailGetReq request)
        {
            var response = new CommonResponse();
            try
            {
                if (request != null && !string.IsNullOrEmpty(request.QrfId))
                {
                    response = await _agentApprovalRepository.AcceptWithoutProposal(request);
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "QRF ID can not be Null/Blank.";
                }

                if (!string.IsNullOrEmpty(response.ResponseStatus.Status) && response.ResponseStatus.Status == "Success" && !string.IsNullOrEmpty(request.QrfId) && !string.IsNullOrEmpty(request.PlacerUserId))
                {
                    Task.Run(() => _mSDynamicsRepository.CreateUpdateQuotation(request.QrfId, request.PlacerUserId).Result);
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }

            return response;
        }

        /// <summary>
        /// AmendmentQuote
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("AmendmentQuote")]
        public async Task<CommonResponse> AmendmentQuote([FromBody] AmendmentQuoteReq request)
        {
            var response = new CommonResponse();
            try
            {
                if (request != null && !string.IsNullOrEmpty(request.QRFID))
                {
                    response = await _agentApprovalRepository.AmendmentQuote(request);

                    if (!string.IsNullOrEmpty(response.ResponseStatus.Status) && response.ResponseStatus.Status == "Success" && !string.IsNullOrEmpty(request.QRFID) && !string.IsNullOrEmpty(response.QRFID) && !string.IsNullOrEmpty(request.VoyagerUserId))
                    {
                        Task.Run(() =>
                        {
                            try
                            {
                                var returnResult = _mSDynamicsRepository.CreateOpportunity(response.QRFID, request.QRFID, request.VoyagerUserId).Result;
                            }
                            catch (Exception ex)
                            {
                                response.ResponseStatus.Status = "Failure";
                                response.ResponseStatus.ErrorMessage = "While Creating New Booking Opportunity in CRM system. \n" + ex.Message;
                            }
                        });
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "QRF ID can not be Null/Zero.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }

            return response;
        }

        /// <summary>
        /// CheckProposalGenerated
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("CheckProposalGenerated")]
        public async Task<CommonResponse> CheckProposalGenerated([FromBody] QuoteGetReq request)
        {
            var response = new CommonResponse();
            try
            {
                if (request != null && !string.IsNullOrEmpty(request.QRFID))
                {
                    response = await _agentApprovalRepository.CheckProposalGenerated(request);
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "QRF ID can not be Null/Blank.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }

            return response;
        }
    }
}