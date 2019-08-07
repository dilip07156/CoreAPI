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
    [Route("api/Guesstimate")]
    public class GuesstimateController : Controller
    {
        #region Private Variable Declaration
        private readonly IGuesstimateRepository _guesstimateRepository;
        private readonly MongoContext _MongoContext = null;
        private readonly IMSDynamicsRepository _mSDynamicsRepository;
        #endregion

        public GuesstimateController(IGuesstimateRepository guesstimateRepository, IOptions<MongoSettings> settings, IMSDynamicsRepository mSDynamicsRepository)
        {
            _guesstimateRepository = guesstimateRepository;
            _MongoContext = new MongoContext(settings);
            _mSDynamicsRepository = mSDynamicsRepository;
        }


        [Authorize]
        [HttpPost]
        [Route("GetGuesstimate")]
        public async Task<GuesstimateGetRes> GetGuesstimate([FromBody] GuesstimateGetReq request)
        {
            var response = new GuesstimateGetRes();
            try
            {
                if (!string.IsNullOrEmpty(request.QRFID))
                {
                    response = _guesstimateRepository.GetGuesstimate(request);
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
        [Route("SetGuesstimate")]
        public async Task<GuesstimateSetRes> SetGuesstimate([FromBody] GuesstimateSetReq request)
        {
            var response = new GuesstimateSetRes();
            try
            {
                if (request.Guesstimate != null)
                {
                    response = await _guesstimateRepository.SetGuesstimate(request);
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Details can not be blank.";
                }
                if (!string.IsNullOrEmpty(response.ResponseStatus.Status) && response.ResponseStatus.Status == "Success" && !string.IsNullOrEmpty(request.Qrfid) && !string.IsNullOrEmpty(request.VoyagerUserId))
                {
                    Task.Run(() => _mSDynamicsRepository.CreateUpdateOpportnity(request.Qrfid, request.VoyagerUserId).Result);

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
        [Route("GetGuesstimateVersions")]
        public GuesstimateVersionGetRes GetGuesstimateVersions([FromBody] GuesstimateGetReq request)
        {
            var response = new GuesstimateVersionGetRes();
            try
            {
                if (!string.IsNullOrEmpty(request.QRFID))
                {
                    List<GuesstimateVersion> result =  _guesstimateRepository.GetGuesstimateVersions(request);
                    response.ResponseStatus.Status = "Success";
                    response.ResponseStatus.ErrorMessage = result != null ? "" : "No Records Found.";

                    response.GuesstimateVersions = result;
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
        [Route("UpdateGuesstimateVersion")]
        public async Task<GuesstimateSetRes> UpdateGuesstimateVersion([FromBody] GuesstimateVersionSetReq request)
        {
            var response = new GuesstimateSetRes();
            try
            {
                if (!string.IsNullOrEmpty(request.QRFID))
                {
                    response = await _guesstimateRepository.UpdateGuesstimateVersion(request);
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

        [Authorize]
        [HttpPost]
        [Route("GetSupplierPrice")]
        public async Task<GuesstimateGetRes> GetSupplierPrice([FromBody] GuesstimateGetReq request)
        {
            var response = new GuesstimateGetRes();
            try
            {
                if (!string.IsNullOrEmpty(request.QRFID))
                {
                    response = _guesstimateRepository.GetSupplierPrice(request);
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
        [Route("SetGuesstimateChangeRule")]
        public async Task<GuesstimateSetRes> SetGuesstimateChangeRule([FromBody] GuesstimateChangeRuleSetReq request)
        {
            var response = new GuesstimateSetRes();
            try
            {
                if (!string.IsNullOrEmpty(request.GuesstimateId))
                {
                    response = await _guesstimateRepository.SetGuesstimateChangeRule(request);
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
    }
}