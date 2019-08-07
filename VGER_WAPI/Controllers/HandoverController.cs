using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VGER_WAPI.Repositories;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/Handover")]
    public class HandoverController : Controller
    {
        #region Private Variable Declaration
        private readonly IHandoverRepository _handoverRepository;
        private readonly IMSDynamicsRepository _mSDynamicsRepository;
        #endregion

        public HandoverController(IHandoverRepository handoverRepository, IMSDynamicsRepository mSDynamicsRepository)
        {
            _handoverRepository = handoverRepository;
            _mSDynamicsRepository = mSDynamicsRepository;
        }

        #region Get Set mGoAhead 
        /// <summary>
        /// Get mGoHead
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetGoAhead")]
        public async Task<GoAheadGetRes> GetGoAhead([FromBody] GoAheadGetReq request)
        {
            var response = new GoAheadGetRes();
            try
            {
                if (!string.IsNullOrEmpty(request?.QRFID))
                {
                    response = await _handoverRepository.GetGoAhead(request);
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
        /// Get mGoAhead depatures
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetGoAheadDepature")]
        public async Task<GetGoAheadDepatureRes> GetGoAheadDepature([FromBody] GoAheadGetReq request)
        {
            var response = new GetGoAheadDepatureRes();
            try
            {
                if (!string.IsNullOrEmpty(request?.QRFID))
                {
                    response = await _handoverRepository.GetGoAheadDepature(request);
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
        /// Set mGoHead
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("SetGoAhead")]
        public async Task<GoAheadSetRes> SetGoAhead([FromBody] GoAheadSetReq request)
        {
            var response = new GoAheadSetRes();
            try
            {
                if (request != null)
                {
                    response = await _handoverRepository.SetGoAhead(request);

                    if (response != null && response.ResponseStatus != null && !string.IsNullOrEmpty(response.ResponseStatus.Status) && response.ResponseStatus.Status.ToLower() == "Success".ToLower() && !string.IsNullOrEmpty(request.mGoAhead.QRFID) && !string.IsNullOrEmpty(request.VoyagerUserId))
                    {
                        Task.Run(() => _mSDynamicsRepository.CreateUpdateOpportnity(request.mGoAhead.QRFID, request.VoyagerUserId).Result);
                    }
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
        /// Set Materialisation
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("SetMaterialisation")]
        public async Task<SetMaterialisationRes> SetMaterialisation([FromBody] SetMaterialisationReq request)
        {
            var response = new SetMaterialisationRes();
            try
            {
                if (request != null)
                {
                    response = await _handoverRepository.SetMaterialisation(request);
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
        #endregion

        #region Handover
        /// <summary>
        ///SetGoAheadConfirmMessage
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("SetGoAheadConfirmMessage")]
        public async Task<ConfirmBookingSetRes> SetGoAheadConfirmMessage([FromBody] ConfirmBookingSetReq request)
        {
            var response = new ConfirmBookingSetRes();
            try
            {
                if (!string.IsNullOrEmpty(request?.QRFID))
                {
                    response = await _handoverRepository.SetGoAheadConfirmMessage(request);
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
        ///GoAheadQuotes will generate the BookingNo and pushed the data to SQL and update the Booking No in mGoAhead collection.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GoAheadQuotes")]
        public async Task<ConfirmBookingGetRes> GoAheadQuotes([FromBody] ConfirmBookingGetReq request)
        {
            var response = new ConfirmBookingGetRes();
            try
            {
                if (!string.IsNullOrEmpty(request?.QRFID))
                {
                    response =await _handoverRepository.GoAheadQuotes(request);
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
        ///Get GoAhead Departures Details
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetGoAheadDeparturesDetails")]
        public async Task<HandoverGetRes> GetGoAheadDeparturesDetails([FromBody] GoAheadGetReq request)
        {
            var response = new HandoverGetRes();
            try
            {
                if (!string.IsNullOrEmpty(request?.QRFID))
                {
                    response = await _handoverRepository.GetGoAheadDeparturesDetails(request);
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
        #endregion

        #region Add New Departures
        /// <summary>
        ///GoAheadQuotes
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetGoAheadExistDepartures")] 
        public async Task<GoAheadNewDeptGetRes> GetGoAheadExistDepartures([FromBody] GoAheadGetReq request)
        {
            var response = new GoAheadNewDeptGetRes();
            try
            {
                if (!string.IsNullOrEmpty(request?.QRFID))
                {
                    response =await _handoverRepository.GetGoAheadExistDepartures(request);
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
        ///GoAheadQuotes
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("SetGoAheadNewDepartures")]
        public async Task<GoAheadNewDeptSetRes> SetGoAheadNewDepartures([FromBody] GoAheadNewDeptSetReq request)
        {
            var response = new GoAheadNewDeptSetRes();
            try
            {
                if (!string.IsNullOrEmpty(request?.QRFID))
                {
                    response = await _handoverRepository.SetGoAheadNewDepartures(request);
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
        #endregion
    }
}
