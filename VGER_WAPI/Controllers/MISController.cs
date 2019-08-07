using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using VGER_WAPI.Repositories;
using VGER_WAPI_CLASSES;
using VGER_WAPI_CLASSES.MIS;

namespace VGER_WAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/MIS")]
    public class MISController : Controller
    {
        private readonly IMISRepository _MISRepository;

        public MISController(IMISRepository MISRepository)
        {
            _MISRepository = MISRepository;
        }

        #region Sales Dashboard

        /// <summary>
        /// Get Sales Dashboard Summary Details
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetSalesDashboardSummary")]
        public async Task<SalesDashboardRes> GetSalesDashboardSummary([FromBody] SalesDashboardReq request)
        {
            var response = new SalesDashboardRes();
            try
            {
                if (request != null)
                {
                    response = await _MISRepository.GetSalesDashboardSummary(request);
                    if (response == null)
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "An error.";
                    }
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
                response.ResponseStatus.ErrorMessage = "An error occurs " + ex.Message.ToString();
            }
            return response;
        }

        /// <summary>
        /// Get Sales Dashboard Filters List
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetSalesDashboardFiltersList")]
        public async Task<SalesDashboardFiltersRes> GetSalesDashboardFiltersList([FromBody] AgentCompanyReq request)
        {
            var response = new SalesDashboardFiltersRes();
            try
            {
                if (request != null)
                {
                    response = await _MISRepository.GetSalesDashboardFiltersList(request);
                    if (response == null)
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "An error.";
                    }
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
                response.ResponseStatus.ErrorMessage = "An error occurs " + ex.Message.ToString();
            }
            return response;
        }

        /// <summary>
        /// Check Mis Mappings Roles
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("CheckMisMappingsRoles")]
        public async Task<MISMappingRes> CheckMisMappingsRoles([FromBody] AgentCompanyReq request)
        {
            var response = new MISMappingRes();
            try
            {
                if (request != null)
                {
                    response = await _MISRepository.CheckMisMappingsRoles(request);
                    if (response == null)
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "An error.";
                    }
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
                response.ResponseStatus.ErrorMessage = "An error occurs " + ex.Message.ToString();
            }
            return response;
        }
        /// <summary>
        ///Search Mis Data
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("SearchMisData")]
        public async Task<MisSearchGetResList> SearchMisData([FromBody] SearchMisReqGet request)
        {
            var response = new MisSearchGetResList();
            try
            {
                if (request != null)
                {
                    response = await _MISRepository.SearchMisData(request);
                    if (response == null)
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "An error.";
                    }
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
                response.ResponseStatus.ErrorMessage = "An error occurs " + ex.Message.ToString();
            }
            return response;
        }
        /// <summary>
        ///Save Mis Data
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("SaveMisData")]
        public async Task<MisSaveResponse> SaveMisData([FromBody] SearchMisReqGet request)
        {
            var response = new MisSaveResponse();
            try
            {
                if (request != null)
                {
                    response = await _MISRepository.SaveMisData(request);
                    if (response == null)
                    {
                        response.Response.Status = "Failure";
                        response.Response.ErrorMessage = "An error.";
                    }
                }
                else
                {
                    response.Response.Status = "Failure";
                    response.Response.ErrorMessage = "Request details can not be blank.";
                }
            }
            catch (Exception ex)
            {
                response.Response.Status = "Failure";
                response.Response.ErrorMessage = "An error occurs " + ex.Message.ToString();
            }
            return response;
        }
        /// <summary>
        ///Delete Mis Data
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("DeleteMisArtifactData")]
        public async Task<MisSaveResponse> DeleteMisArtifactData([FromBody] SearchMisReqGet request)
        {
            var response = new MisSaveResponse();
            try
            {
                if (request != null)
                {
                    response = await _MISRepository.DeleteMisArtifactData(request);
                    if (response == null)
                    {
                        response.Response.Status = "Failure";
                        response.Response.ErrorMessage = "An error.";
                    }
                }
                else
                {
                    response.Response.Status = "Failure";
                    response.Response.ErrorMessage = "Request details can not be blank.";
                }
            }
            catch (Exception ex)
            {
                response.Response.Status = "Failure";
                response.Response.ErrorMessage = "An error occurs " + ex.Message.ToString();
            }
            return response;
        }

        #endregion

        #region Bookings Dashboard

        /// <summary>
        /// Get Sales Dashboard Summary Details
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetBookingsDashboardSummary")]
        public BookingsDashboardRes GetBookingsDashboardSummary([FromBody] SalesDashboardReq request)
        {
            var response = new BookingsDashboardRes();
            try
            {
                if (request != null)
                {
                    response = _MISRepository.GetBookingsDashboardSummary(request);
                    if (response == null)
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "An error.";
                    }
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
                response.ResponseStatus.ErrorMessage = "An error occurs " + ex.Message.ToString();
            }
            return response;
        }
        
        #endregion
    }
}
