using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using VGER_WAPI.Repositories;
using VGER_WAPI_CLASSES;
using VGER_WAPI.Helpers;
using Microsoft.Extensions.Options;
using VGER_WAPI.Models;
using MongoDB.Driver;

namespace VGER_WAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/Commercials")]
    public class CommercialsController : Controller
    {
        #region Private Variable Declaration
        private readonly ICommercialsRepository _commercialsRepository;
        private readonly MongoContext _MongoContext = null;
        private readonly IMSDynamicsRepository _mSDynamicsRepository;
        #endregion

        public CommercialsController(ICommercialsRepository commercialsRepository, IOptions<MongoSettings> settings, IMSDynamicsRepository mSDynamicsRepository)
        {
            _commercialsRepository = commercialsRepository;
            _MongoContext = new MongoContext(settings);
            _mSDynamicsRepository = mSDynamicsRepository;
        }


        [Authorize]
        [HttpPost]
        [Route("GetCommercials")]
        public async Task<CommercialsGetRes> GetCommercials([FromBody] CommercialsGetReq request)
        {
            var response = new CommercialsGetRes();
            try
            {
                if (!string.IsNullOrEmpty(request.QRFID))
                {
                    response = _commercialsRepository.GetCommercials(request);
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
        
        [Authorize]
        [HttpPost]
        [Route("ChangePositionKeepAs")]
        public async Task<CommonResponse> ChangePositionKeepAs([FromBody] ChangePositionKeepReq request)
        {
            var response = new CommonResponse();
            try
            {
                if (!string.IsNullOrEmpty(request.QRFID))
                {
                    response = await _commercialsRepository.ChangePositionKeepAs(request);
                }
                else
                {
                     response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "QRFId can not be blank.";
                }
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
        [Route("SaveCommercials")]
        public async Task<CommonResponse> SaveCommercials([FromBody] CommercialsSetReq request)
        {
            var response = new CommonResponse();
            try
            {
                if (!string.IsNullOrEmpty(request.QRFPriceId))
                {
                    response = await _commercialsRepository.SaveCommercials(request);
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "QRFId can not be blank.";
                }
                if (!string.IsNullOrEmpty(response.ResponseStatus.Status) && response.ResponseStatus.Status == "Success" && !string.IsNullOrEmpty(request.QrfId) && !string.IsNullOrEmpty(request.VoyagerUserId))
                {
                    Task.Run(() => _mSDynamicsRepository.CreateUpdateOpportnity(request.QrfId, request.VoyagerUserId).Result);

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
        /// Update Quote details
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("SetQuoteDetails")]
        public async Task<CommonResponse> SetQuoteDetails([FromBody] QuoteSetReq request)
        {
            var response = new CommonResponse();
            try
            {
                if (request != null)
                {
                    string result = await _commercialsRepository.SetQuoteDetails(request);
                    response.QRFID = result;
                    response.ResponseStatus.Status = "Success";
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Details can not be blank.";
                }
                if (!string.IsNullOrEmpty(response.ResponseStatus.Status) && response.ResponseStatus.Status == "Success" && !string.IsNullOrEmpty(request.QRFID) && !string.IsNullOrEmpty(request.VoyagerUserID))
                {
                    Task.Run(() => _mSDynamicsRepository.CreateUpdateOpportnity(request.QRFID, request.VoyagerUserID).Result);

                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs " + ex.Message;
            }
            return response;
        }
    }
}