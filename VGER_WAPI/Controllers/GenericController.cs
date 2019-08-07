using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VGER_WAPI.Repositories;
using Microsoft.Extensions.Configuration;
using VGER_WAPI_CLASSES;
using Microsoft.AspNetCore.Authorization;

namespace VGER_WAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/Generic")]
    public class GenericController : Controller
    {
        private readonly IGenericRepository _genericRepository;
        private readonly ICostingRepository _costingRepository;
        private readonly IConfiguration _configuration;

        public GenericController(IConfiguration configuration, IGenericRepository agentRepository, ICostingRepository costingRepository)
        {
            _configuration = configuration;
            _genericRepository = agentRepository;
            _costingRepository = costingRepository;
        }

        /// <summary>
        /// Method to get reference number based on CounterType
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("GetNextReferenceNumber")]
        public QRFCounterResponse GetNextReferenceNumber([FromBody] QRFCounterRequest request)
        {
            if (!string.IsNullOrWhiteSpace(request.CounterType))
            {
                var response = _genericRepository.GetNextReferenceNumber(request);
                if (response.LastReferenceNumber != 0)
                {
                    response.Status = "Success";
                }
                else
                {
                    response.Status = "Invalid CounterType";
                }
                return response;
            }
            else
            {
                var response = new QRFCounterResponse();
                response.Status = "Failure";
                return response;
            }
        }

        /// <summary>
        /// Get Departures for a QRF
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("GetDepartureDatesForCostingByQRF_Id")]
        public QRFDepartureDateGetRes GetDepartureDatesForCostingByQRF_Id([FromBody] QRFDepartureDateGetReq request)
        {
            var response = new QRFDepartureDateGetRes();
            if (request != null)
            {
                var res = _costingRepository.GetDepartureDatesForCostingByQRF_Id(request);
                return res;
            }
            else
            {
                response.Status = "Primary key is blank";
                return response;
            }
        }

        /// <summary>
        /// Get PaxSlabDetails for a QRF
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("GetPaxSlabDetailsForCostingByQRF_Id")]
        public QRFPaxGetResponse GetPaxSlabDetailsForCostingByQRF_Id([FromBody] QRFPaxSlabGetReq request)
        {
            var response = new QRFPaxGetResponse();
            if (request != null)
            {
                var res = _costingRepository.GetPaxSlabDetailsForCostingByQRF_Id(request);
                return res;
            }
            else
            {
                response.Status = "Primary key is blank";
                return response;
            }
        }

        /// <summary>
        /// Get costing details by QRF ID
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetCostingDetailsByQRFID")]
        public async Task<CostingGetRes> GetCostingDetailsByQRFID([FromBody] CostingGetReq request)
        {
            var response = new CostingGetRes();
            try
            {
                if (request != null && !string.IsNullOrEmpty(request.QRFID))
                {
                    response = await _costingRepository.GetCostingDetailsByQRFID(request);
                    response.ResponseStatus.Status = "Success";
                    response.ResponseStatus.ErrorMessage = response.CostingGetProperties != null ? "" : "No Records Found.";
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
                response.ResponseStatus.ErrorMessage = "An Error Occurs " + ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Get Lat Lon by Address
        /// </summary>
        /// <param name="request">Provide Address and Type in request parameter. Type is optional. Do not provide anything in Type</param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetLatLogByAddress")]
        public async Task<GeocoderLocationGetRes> GetLatLogByAddress([FromBody] GeocoderLocationGetReq request)
        {
            var response = new GeocoderLocationGetRes();
            try
            {
                if (!string.IsNullOrEmpty(request.Address))
                {
                    response = _genericRepository.GetLocationLatLon(request.Address);
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Address can not be blank.";
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
        /// Get Lat Lon by Address and Types
        /// </summary>
        /// <param name="request">Provide Address and Type in request parameter. Type is optional. You can provide multiple types by | seperated</param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetLatLogByAddressAndType")]
        public async Task<GeocoderLocationGetRes> GetLatLogByAddressAndType([FromBody] GeocoderLocationGetReq request)
        {
            var response = new GeocoderLocationGetRes();
            try
            {
                if (!string.IsNullOrEmpty(request.Address))
                {
                    response = _genericRepository.GetLocationLatLonByType(request.Address, request.Type);
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Address can not be blank.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }
            return response;
        }


        ///// <summary>
        ///// Get Distance Matrix by Lat Lon
        ///// </summary>
        ///// <param name="request"></param>
        ///// <returns></returns>
        //[HttpPost]
        //[Route("GetDistanceMatrix")]
        //public async Task<DistanceMatrixGetRes> GetDistanceMatrix([FromBody] DistanceMatrixGetReq request)
        //{
        //    var response = new DistanceMatrixGetRes();

        //    response = await _genericRepository.GetDistanceMatrix(request);

        //    return response;
        //}

        /// <summary>
        /// Get Distance Matrix between Cities.
        /// Provide GUId in FromId and ToId
        /// </summary>
        /// <param name="request">Pass CityId in FromId and ToId</param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetDistanceMatrixForCity")]
        public async Task<DistanceMatrixGetRes> GetDistanceMatrixForCity([FromBody] DistanceMatrixReq request)
        {
            var response = new DistanceMatrixGetRes();

            try
            {
                if (!(string.IsNullOrEmpty(request.FromId) || string.IsNullOrEmpty(request.ToId)))
                {
                    response = await _genericRepository.GetDistanceMatrixForCity(request.FromId, request.ToId);
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
        /// Get Distance Matrix between Products.
        /// Provide GUId in FromId and ToId
        /// </summary>
        /// <param name="request">Pass ProductId in FromId and ToId</param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetDistanceMatrixForProduct")]
        public async Task<DistanceMatrixGetRes> GetDistanceMatrixForProduct([FromBody] DistanceMatrixReq request)
        {
            var response = new DistanceMatrixGetRes();

            try
            {
                if (!(string.IsNullOrEmpty(request.FromId) || string.IsNullOrEmpty(request.ToId)))
                {
                    response = await _genericRepository.GetDistanceMatrixForProduct(request.FromId, request.ToId);
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
        /// Get Partner Country Mapping info
        /// </summary>
        /// <param name="request">Provide Attribute_Id and AttributeName(not mandatory) in request parameter. returns mResort Info</param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetPartnerCountryDetails")]
        public async Task<PartnerCountryCityRes> GetPartnerCountryDetails([FromBody] Attributes request)
        {
            var response = new PartnerCountryCityRes();
            try
            {
                if (!string.IsNullOrEmpty(request.Attribute_Id))
                {
                    response = await _genericRepository.GetPartnerCountryDetails(request);
                    if (string.IsNullOrEmpty(response.ResortInfo.Voyager_Resort_Id))
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "Country not found.";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Country not found.";
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
        /// Get Partner City Mapping info
        /// </summary>
        /// <param name="request">Provide Attribute_Id and AttributeName(not mandatory) in request parameter. returns mResort Info</param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetPartnerCityDetails")]
        public async Task<PartnerCountryCityRes> GetPartnerCityDetails([FromBody] Attributes request)
        {
            var response = new PartnerCountryCityRes();
            try
            {
                if (!string.IsNullOrEmpty(request.Attribute_Id))
                {
                    response = await _genericRepository.GetPartnerCityDetails(request);
                    if (string.IsNullOrEmpty(response.ResortInfo.Voyager_Resort_Id))
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "City not found.";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "City not found.";
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
        /// Get Partner Country Mapping info
        /// </summary>
        /// <param name="request">Provide CountryCode in request parameter. returns mResort Info</param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetPartnerCountryDetailsBasedOnCode")]
        public async Task<PartnerCountryCityRes> GetPartnerCountryDetailsBasedOnCode([FromBody] string request)
        {
            var response = new PartnerCountryCityRes();
            try
            {
                if (!string.IsNullOrEmpty(request))
                {
                    response = await _genericRepository.GetPartnerCountryDetailsBasedOnCode(request);
                    if (string.IsNullOrEmpty(response.ResortInfo.Voyager_Resort_Id))
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "Country not found.";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Country not found.";
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
        /// Get Partner City Mapping info
        /// </summary>
        /// <param name="request">Provide CityCode in request parameter. returns mResort Info</param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetPartnerCityDetailsBasedOnCode")]
        public async Task<PartnerCountryCityRes> GetPartnerCityDetailsBasedOnCode([FromBody] string request)
        {
            var response = new PartnerCountryCityRes();
            try
            {
                if (!string.IsNullOrEmpty(request))
                {
                    response = await _genericRepository.GetPartnerCityDetailsBasedOnCode(request);
                    if (string.IsNullOrEmpty(response.ResortInfo.Voyager_Resort_Id))
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "City not found.";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "City not found.";
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
        /// Get Partner City Mapping info
        /// </summary>
        /// <param name="request">Provide TravelogiCountryCityRes request parameter. returns mResort Info</param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetPartnerCityDetailsBasedOnName")]
        public async Task<PartnerCountryCityRes> GetPartnerCityDetailsBasedOnName([FromBody] TravelogiCountryCityRes request)
        {
            var response = new PartnerCountryCityRes();
            try
            {
                if (request != null)
                {
                    response = await _genericRepository.GetPartnerCityDetailsBasedOnName(request);
                    if (string.IsNullOrEmpty(response.ResortInfo.Voyager_Resort_Id))
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "City not found.";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "City not found.";
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