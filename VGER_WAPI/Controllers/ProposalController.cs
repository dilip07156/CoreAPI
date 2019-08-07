using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;
using VGER_WAPI.Helpers;
using VGER_WAPI.Models;
using VGER_WAPI.Repositories;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/Proposal")]
    public class ProposalController : Controller
    {
        #region Private Variable Declaration
        private readonly IProposalRepository _proposalRepository;
        private readonly MongoContext _MongoContext = null;
        private readonly IMSDynamicsRepository _mSDynamicsRepository;
        #endregion

        public ProposalController(IProposalRepository proposalRepository, IOptions<MongoSettings> settings, IMSDynamicsRepository mSDynamicsRepository)
        {
            _proposalRepository = proposalRepository;
            _MongoContext = new MongoContext(settings);
            _mSDynamicsRepository = mSDynamicsRepository;
        }

        /// <summary>
        /// Method to Get Proposal
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetProposal")]
        public async Task<ProposalGetRes> GetProposal([FromBody] ProposalGetReq request)
        {
            var response = new ProposalGetRes();
            try
            {
                if (!string.IsNullOrEmpty(request.QRFID) && request != null)
                {
                    //response = await _itineraryRepository.GetItinerary(request);
                    //response.ResponseStatus.Status = "Success";

                    ProposalGetRes result = _proposalRepository.GetProposal(request);

                    response.ResponseStatus.Status = "Success";
                    response.ResponseStatus.ErrorMessage = result != null ? "" : "No Records Found.";

                    response = result;

                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "QRFId can not be Null/Zero.";
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
        /// Method to Insert/Update Proposal
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("SetProposal")]
        public async Task<ProposalSetRes> SetProposal([FromBody] ProposalSetReq request)
        {
            var response = new ProposalSetRes();
            try
            {
                if (request.Proposal != null)
                { 
                    response = await _proposalRepository.SetProposal(request);
                    response.ResponseStatus.Status = "Success";
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Details can not be blank.";
                }
                if (!string.IsNullOrEmpty(response.ResponseStatus.Status) && response.ResponseStatus.Status == "Success" && !string.IsNullOrEmpty(request.QRFID) && !string.IsNullOrEmpty(request.VoyagerUserId))
                {
                    Task.Run(() => _mSDynamicsRepository.CreateUpdateOpportnity(request.QRFID, request.VoyagerUserId).Result);

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
        /// Method to Get Proposal Document Details By QRFID
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetProposalDocumentDetailsByQRFID")]
        public ProposalDocumentGetRes GetProposalDocumentDetailsByQRFID([FromBody] QuoteAgentGetReq request)
        {
            var response = new ProposalDocumentGetRes();
            try
            {
                if (!string.IsNullOrEmpty(request.QRFID) && request != null)
                {
                    response = _proposalRepository.GetProposalDocumentDetailsByQRFID(request);
                    response.ResponseStatus.Status = "Success";
                    response.ResponseStatus.ErrorMessage = response != null ? "" : "No Records Found.";
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "QRFId can not be Null/Zero.";
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
        /// Method to Get Proposal Document Header Footer Details By QRFID
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetProposalDocumentHeaderDetails")]
        public ProposalDocumentGetRes GetProposalDocumentHeaderDetails([FromBody] QuoteAgentGetReq request)
        {
            var response = new ProposalDocumentGetRes();
            try
            {
                if (!string.IsNullOrEmpty(request.QRFID) && request != null)
                {
                    response = _proposalRepository.GetProposalDocumentHeaderDetails(request);
                    response.ResponseStatus.Status = "Success";
                    response.ResponseStatus.ErrorMessage = response != null ? "" : "No Records Found.";
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "QRFId can not be Null/Zero.";
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
        /// Method to Get Hotel Summary By ProposalId
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetHotelSummaryByQrfId")]
        public async Task<ProposalGetRes> GetHotelSummaryByQrfId([FromBody] ProposalGetReq request)
        {
            var response = new ProposalGetRes();
            try
            {
                if (!string.IsNullOrEmpty(request.QRFID) && request != null)
                {
                    //response = await _itineraryRepository.GetItinerary(request);
                    //response.ResponseStatus.Status = "Success";

                    ProposalGetRes result = _proposalRepository.GetHotelSummaryByQrfId(request);

                    response.ResponseStatus.Status = "Success";
                    response.ResponseStatus.ErrorMessage = result != null ? "" : "No Records Found.";

                    response = result;

                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "QRFId can not be Null/Zero.";
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