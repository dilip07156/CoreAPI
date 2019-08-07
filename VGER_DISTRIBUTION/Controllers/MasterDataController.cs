using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using VGER_DISTRIBUTION.Repositories.Master;
using VGER_WAPI_CLASSES;


namespace VGER_DISTRIBUTION.Controllers
{
    [Produces("application/json")]
    [Route("api/Master")]
    public class MasterDataController : Controller
    {
        #region Private Variable Declaration 
        private readonly IMasterRepository _MasterRepository;
        #endregion

        public MasterDataController(IMasterRepository MasterRepository)
        {
            _MasterRepository = MasterRepository;
        }

        /// <summary>
        /// Method to get Countries based on Name
        /// </summary>
        /// <param name="countryLookupRequest"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetCountries")]
        [ProducesResponseType(typeof(CountryLookupResponse), 200)]
        [ProducesResponseType(typeof(void), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public IActionResult GetCountryNames([FromBody] CountryLookupRequest countryLookupRequest)
        // public HttpResponseMessage GetCountryNames([FromBody] CountryLookupRequest countryLookupRequest)
        {
            var response = new CountryLookupResponse();
            var resstatus = new ResponseStatus();
            HttpResponseMessage resp = new HttpResponseMessage();
            var res = new HttpResponseMessage();
            try
            {
                if (!ModelState.IsValid)
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Request is not valid";
                    return BadRequest(response.ResponseStatus);
                }
                else
                {
                    if (countryLookupRequest != null)
                    {
                        if (countryLookupRequest.IncludeCities == null || countryLookupRequest.IncludeCities == "Y" || countryLookupRequest.IncludeCities == "N")
                        {
                            List<CountryLookupProperties> result = (List<CountryLookupProperties>)_MasterRepository.GetCountryNames(countryLookupRequest);
                            response.ResponseStatus.Status = "Success";
                            if (result != null && result.Count() > 0)
                            {
                                response.CountryLookupProperties = result.OrderBy(a => a.CountryName).ToList();

                            }
                            else
                            {
                                response.ResponseStatus.ErrorMessage = "No Records Found.";
                                return NotFound(response.ResponseStatus);
                            }
                        }
                        else
                        {
                            response.ResponseStatus.Status = "Failure";
                            response.ResponseStatus.ErrorMessage = "Include Cities parameter only accepts Y / N";
                            return BadRequest(response.ResponseStatus);
                        }
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "Country Name can not be blank.";
                        return BadRequest(response.ResponseStatus);

                    }
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An error occurs " + ex.ToString();
                return StatusCode(400);
            }

           return Ok(response);
             //return Request. (HttpStatusCode.OK, response);
        }

        /// <summary>
        /// Method to get cities based on city name
        /// </summary>
        /// <param name="cityLookupRequest"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetCities")]
        [ProducesResponseType(typeof(CityLookupResponse), 200)]
        [ProducesResponseType(typeof(void), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        //public CityLookupResponse GetCityNames([FromBody] CityLookupRequest cityLookupRequest)
        public IActionResult GetCityNames([FromBody] CityLookupRequestMaster cityLookupRequest)
        {
            var response = new CityLookupResponse();
            bool CallFunction = false;
            try
            {
                if (!ModelState.IsValid)
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Request is not valid";
                    return BadRequest(response.ResponseStatus);
                }
                else
                {
                    if (cityLookupRequest != null)
                    {
                        if(!string.IsNullOrWhiteSpace(cityLookupRequest.VoyagerCountry_Id))
                        {
                            Guid Resort_Id = Guid.Empty;
                            if (!Guid.TryParse(cityLookupRequest.VoyagerCountry_Id, out Resort_Id))
                            {
                                response.ResponseStatus.Status = "Failure";
                                response.ResponseStatus.ErrorMessage = "Voyager Country_Id is not valid";
                                return BadRequest(response.ResponseStatus);
                            }
                            else
                            {
                                if(Resort_Id == Guid.Empty)
                                {
                                    response.ResponseStatus.Status = "Failure";
                                    response.ResponseStatus.ErrorMessage = "Voyager Country_Id is not valid";
                                    return BadRequest(response.ResponseStatus);
                                }
                            }
                            CallFunction = true;
                        }
                        if (!string.IsNullOrWhiteSpace(cityLookupRequest.CountryName))
                        {
                            CallFunction = true;
                        }

                        if (!CallFunction)
                        {
                            response.ResponseStatus.Status = "Failure";
                            response.ResponseStatus.ErrorMessage = "Atleast one parameter of Country Id or Name is mandatory.";
                            return BadRequest(response.ResponseStatus);
                        }
                        else
                        {
                            IQueryable<CityLookupProperties> result = (IQueryable<CityLookupProperties>)_MasterRepository.GetCityNames(cityLookupRequest);
                            response.ResponseStatus.Status = "Success";
                            if (result != null && result.Count() > 0)
                            {
                                response.CityLookupProperties = result.OrderBy(a => a.Lookup).ToList();
                            }
                            else
                            {
                                response.ResponseStatus.ErrorMessage = "No Records Found.";
                                return NotFound(response.ResponseStatus);
                            }
                        }
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "City Name can not be blank.";
                        return BadRequest(response.ResponseStatus);
                    }
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An error occurs " + ex.ToString();
                return StatusCode(400);
            }

            return Ok(response);
        }

        /// <summary>
        /// Method for getting all the currencies
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("GetCurrency")]
        [ProducesResponseType(typeof(CurrencyResponse), 200)]
        [ProducesResponseType(typeof(void), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        //public CurrencyResponse GetCurrencyList()
        public IActionResult GetCurrencyList()
        {
            var response = new CurrencyResponse();
            var resstatus = new ResponseStatus();
            if (!ModelState.IsValid)
            {
                resstatus.Status = "Failure";
                resstatus.ErrorMessage = "Request is not valid";
                return BadRequest(resstatus);
            }
            else
            {
                var result = _MasterRepository.GetCurrencyList();
                response.CurrencyList = result.ToList();
                if(response.CurrencyList.Count == 0)
                {
                    resstatus.Status = "Failure";
                    resstatus.ErrorMessage = "No Records Found";
                    return NotFound(resstatus);
                }
                else
                    return Ok(response);
            }
        }

        /// <summary>
        /// Method for getting all the Status
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("GetStatus")]
        [ProducesResponseType(typeof(StatusMasterResponse), 200)]
        [ProducesResponseType(typeof(void), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        //public StatusMasterResponse GetStatusList()
        public IActionResult GetStatusList()
        {
            var response = new StatusMasterResponse();
            var resstatus = new ResponseStatus();
            if (!ModelState.IsValid)
            {
                resstatus.Status = "Failure";
                resstatus.ErrorMessage = "Request is not valid";
                return BadRequest(resstatus);
            }
            else
            {
                var result = _MasterRepository.GetStatusList();
                response.StatusMaster = result.ToList();
                if (response.StatusMaster.Count == 0)
                {
                    resstatus.Status = "Failure";
                    resstatus.ErrorMessage = "No Records Found";
                    return NotFound(resstatus);
                }
                else
                    return Ok(response);
            }
        }

        /// <summary>
        /// Method for getting all the supported product types
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("GetProductTypes")]
        [ProducesResponseType(typeof(DefProductTypeRes), 200)]
        [ProducesResponseType(typeof(void), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        // public DefProductTypeRes GetProductTypes()
        public IActionResult GetProductTypes()
        {
            var response = new DefProductTypeRes();
            var resstatus = new ResponseStatus();
            response = _MasterRepository.GetProductTypes();
            if (!ModelState.IsValid)
            {
                resstatus.Status = "Failure";
                resstatus.ErrorMessage = "Request is not valid";
                return BadRequest(resstatus);
            }
            else
            {
                if (response != null && response.DefProductType.Count() > 0)
                {
                    response.ResponseStatus.Status = "Success";
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Product Types bot found.";
                    return NotFound(response.ResponseStatus);
                }
                return Ok(response);
            }
        }
    }
}
