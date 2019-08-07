using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VGER_WAPI.Repositories;
using VGER_WAPI.Helpers;
using Microsoft.Extensions.Options;
using VGER_WAPI.Models;
using VGER_WAPI_CLASSES;
using Microsoft.AspNetCore.Authorization;

namespace VGER_WAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/Itinerary")]
    public class ItineraryController : Controller
    {
        #region Private Variable Declaration
        private readonly IItineraryRepository _itineraryRepository;
        private readonly MongoContext _MongoContext = null;
        private readonly IMSDynamicsRepository _mSDynamicsRepository;
        #endregion

        public ItineraryController(IItineraryRepository itineraryRepository, IOptions<MongoSettings> settings, IMSDynamicsRepository mSDynamicsRepository)
        {
            _itineraryRepository = itineraryRepository;
            _MongoContext = new MongoContext(settings);
            _mSDynamicsRepository = mSDynamicsRepository;
        }

        [Authorize]
        [HttpPost]
        [Route("GetItinerary")]
        public async Task<ItineraryGetRes> GetItinerary([FromBody] ItineraryGetReq request)
        {
            var response = new ItineraryGetRes();
            try
            {
                if (!string.IsNullOrEmpty(request.QRFID) && request != null)
                {
                    //response = await _itineraryRepository.GetItinerary(request);
                    //response.ResponseStatus.Status = "Success";

                    mItinerary result = await _itineraryRepository.GetItinerary(request);
                    response.ResponseStatus.Status = "Success";
                    response.ResponseStatus.ErrorMessage = result != null ? "" : "No Records Found.";

                    response.Itinerary = result;

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
        [Route("GetItineraryDetails")]
        public async Task<ItineraryGetRes> GetItineraryDetails([FromBody] ItineraryGetReq request)
        {
            var response = new ItineraryGetRes();
            try
            {
                if (!string.IsNullOrEmpty(request.QRFID) && request != null)
                {
                    mItinerary result = await _itineraryRepository.GetItineraryDetails(request);
                    response.ResponseStatus.Status = "Success";
                    response.ResponseStatus.ErrorMessage = result != null ? "" : "No Records Found.";
                    response.Itinerary = result;
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
        [Route("SetItinerary")]
        public async Task<ItinerarySetRes> SetItinerary([FromBody] ItinerarySetReq request)
        {
            var response = new ItinerarySetRes();
            try
            {
                if (request.itinerary != null)
                {
                    response = await _itineraryRepository.SetItinerary(request);
                    response.ResponseStatus.Status = "Success";
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Details can not be blank.";
                }
                if (!string.IsNullOrEmpty(response.ResponseStatus.Status) && response.ResponseStatus.Status == "Success" && !string.IsNullOrEmpty(request.QRFId) && !string.IsNullOrEmpty(request.VoyagerUserId))
                {
                    Task.Run(() => _mSDynamicsRepository.CreateUpdateOpportnity(request.QRFId, request.VoyagerUserId).Result);

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
        [Route("EnableDisablePosition")]
        public async Task<ItinerarySetRes> EnableDisablePosition([FromBody] ItinerarySetReq request)
        {
            var response = new ItinerarySetRes();
            try
            {
                if (request.itinerary != null)
                {
                    response = await _itineraryRepository.EnableDisablePosition(request);
                    response.ResponseStatus.Status = "Success";
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Details not updated.";
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
        [Route("SaveRemarks")]
        public async Task<ItinerarySetRes> SaveRemarks([FromBody] ItinerarySetReq request)
        {
            var response = new ItinerarySetRes();
            try
            {
                if (request.itinerary != null)
                {
                    response = await _itineraryRepository.SaveRemarks(request);
                    response.ResponseStatus.Status = "Success";
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Details not updated.";
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
        [Route("GetQRFPosition")]
        public async Task<ItineraryGetRes> GetQRFPosition([FromBody] ItineraryGetReq request)
        {
            var response = new ItineraryGetRes();
            try
            {
                if (request != null && !string.IsNullOrEmpty(request.QRFID))
                {
                    response = await _itineraryRepository.GetQRFPosition(request);
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
    }
}