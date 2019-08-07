using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using VGER_WAPI.Repositories;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/Activities")]
    public class ActivitiesController : Controller
    {
        #region Private Variable Declaration
        private readonly IActivitiesRepository _activitiesRepository;
        private readonly IConfiguration _configuration;
        #endregion

        public ActivitiesController(IConfiguration configuration, IActivitiesRepository activitiesRepository)
        {
            _configuration = configuration;
            _activitiesRepository = activitiesRepository;
        }


        /// <summary>
        /// Get Meals Details By QRFID
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetActivityDetailsByQRFID")]
        public async Task<ActivitiesGetRes> GetActivitiesDetailsByQRFID([FromBody] QuoteGetReq request)
        {
            var response = new ActivitiesGetRes();
            try
            {
                if (request != null)
                {
                    response = await _activitiesRepository.GetActivitiesDetailsByQRFID(request);
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Request details can not be Null.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs " + ex.Message;
            }
            response.QRFID = request.QRFID;
            return response;
        }

        /// <summary>
        /// Set Meals Details
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("SetActivityDetails")]
        public async Task<ActivitiesSetRes> SetActivitiesDetails([FromBody] ActivitiesSetReq request)
        {
            var response = new ActivitiesSetRes();
            try
            {
                if (request != null)
                {
                    response = await _activitiesRepository.SetActivitiesDetails(request);
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Request details can not be Null.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs " + ex.Message.ToString();
            }
            response.QRFID = request.QRFID;
            return response;
        }
    }
}