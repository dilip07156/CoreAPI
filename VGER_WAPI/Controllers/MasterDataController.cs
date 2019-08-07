using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI.Repositories;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Controllers
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
        /// Method for getting Product Attributes
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("GetGenericMasterForType")]
        public MasterTypeResponse GetGenericMasterForType([FromBody] MasterTypeRequest request)
        {
            var response = new MasterTypeResponse();
            if (!string.IsNullOrWhiteSpace(request.Property))
            {
                if (!string.IsNullOrWhiteSpace(request.Name))
                {
                    var result = _MasterRepository.GetGenericMasterForTypeByName(request);
                    var Attribute = new List<Attributes>();
                    var Property = new Properties();
                    if (result.Count() > 0)
                    {
                        foreach (var x in result)
                        {
                            Property.Attribute = x;
                        }
                        Property.PropertyName = request.Property;
                        response.PropertyList.Add(Property);
                        response.Status = "Success";
                        return response;
                    }
                    else
                    {
                        response.Status = "Invalid AttributeName";
                        return response;
                    }
                }
                else
                {
                    var results = _MasterRepository.GetGenericMasterForTypeByProperty(request);
                    if (results.Count() > 0)
                    {
                        foreach (var x in results)
                        {
                            response.PropertyList.Add(x);
                        }
                        response.PropertyList.ForEach(a => a.Attribute.ForEach(b => b.Values = b.Values.OrderBy(c => c.Value).ToList()));
                        response.Status = "Success";
                        return response;
                    }
                    else
                    {
                        response.Status = "Invalid PropertyName";
                        return response;
                    }
                }
            }
            else
            {
                response.Status = "Failure";
                return response;
            }
        }

        /// <summary>
        /// Method to get cities based on city name
        /// </summary>
        /// <param name="cityLookupRequest"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetCityNames")]
        public CityLookupResponse GetCityNames([FromBody] CityLookupRequest cityLookupRequest)
        {
            var response = new CityLookupResponse();
            try
            {
                if (cityLookupRequest != null)
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
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "City Name can not be blank.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An error occurs " + ex.ToString();
            }

            return response;
        }

        /// <summary>
        /// Method for getting all the currencies
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("GetCurrencyList")]
        public CurrencyResponse GetCurrencyList()
        {
            var response = new CurrencyResponse();
            var result = _MasterRepository.GetCurrencyList();
            response.CurrencyList = result.OrderBy(a => a.CurrencyName).ToList();
            return response;
        }


        /// <summary>
        /// Method for getting Coach Sizes
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("GetCoachSizes")]
        public CoachesGetResponse GetCoachSizes()
        {
            var result = _MasterRepository.GetCoachSizes();
            result.Status = "Success";
            return result;
        }

        /// <summary>
        ///Get DefMealPlan
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetDefMealPlan")]
        public DefMealPlanGetRes GetDefMealPlan([FromBody] DefMealPlanGetReq request)
        {
            var response = new DefMealPlanGetRes();
            try
            {
                response = _MasterRepository.GetDefMealPlan(request);
                response.ResponseStatus.Status = "Success";
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }

            return response;
        }

        /// <summary>
        ///Get User System and Contact Details
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetUserSystemContactDetails")]
        public UserSystemContactGetRes GetUserSystemContactDetails()
        {
            var response = new UserSystemContactGetRes();
            try
            {
                response.UserSystemContactDetails = _MasterRepository.GetUserSystemContactDetails();
                response.ResponseStatus.Status = "Success";
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Method to get all countries
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("GetAllCountries")]
        public List<Attributes> GetAllCountries()
        {
            var response = new List<Attributes>();
            response = _MasterRepository.GetAllCountries();
            return response;
        }

        /// <summary>
        /// Method to get all cities by country id
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("GetAllCitiesByCountryId")]
        public List<Attributes> GetAllCitiesByCountryId([FromBody] string CountryId)
        {
            var response = new List<Attributes>();
            response = _MasterRepository.GetAllCitiesByCountryId(CountryId);
            return response;
        }
        [HttpPost]
        [Authorize]
        [Route("GetAllStatesByCountryId")]
        public List<Attributes> GetAllStatesByCountryId([FromBody] string CountryId)
        {
            var response = new List<Attributes>();
            response = _MasterRepository.GetAllStatesByCountryId(CountryId);
            return response;
        }

        /// <summary>
        /// Method to get all markkups from mMarkup collection
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("GetMarkups")]
        public List<Attributes> GetMarkups()
        {
            var response = new List<Attributes>();
            response = _MasterRepository.GetMarkups();
            return response;
        }

        /// <summary>
        ///Get ChargeBasis from Master
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetDefChargeBasis")]
        public DefChargeBasisRes GetDefChargeBasis()
        {
            var response = new DefChargeBasisRes();
            try
            {
                response.DefChargeBasis = _MasterRepository.GetChargeBasis();
                response.ResponseStatus.Status = "Success";
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }
            return response;
        }

        /// <summary>
        ///Get Person Type from Master
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetPersonType")]
        public DefPersonTypeRes GetPersonType()
        {
            var response = new DefPersonTypeRes();
            try
            {
                response.DefPersonType = _MasterRepository.GetPersonType();
                response.ResponseStatus.Status = "Success";
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }
            return response;
        }

        /// <summary>
        ///Get Meal Type from Master
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetMealType")]
        public DefMealTypeRes GetMealType(DefMealTypeGetReq request)
        {
            var response = new DefMealTypeRes();
            try
            {
                response.DefMealType = _MasterRepository.GetMealType(request);
                response.ResponseStatus.Status = "Success";
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }
            return response;
        }

        /// <summary>
        ///Get Product Templates from mProductTemplates collection
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetProductTemplates")]
        public ProductTemplatesGetRes GetProductTemplates(ProductTemplatesGetReq request)
        {
            var response = new ProductTemplatesGetRes();
            try
            {
                response.ProductTemplates = _MasterRepository.GetProductTemplates(request);
                response.ResponseStatus.Status = "Success";
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Get Data from Workflow_Action
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetWorkflowAction")]
        public async Task<WorkflowActionGetRes> GetWorkflowAction([FromBody] WorkflowActionGetReq request)
        {
            var response = new WorkflowActionGetRes();
            try
            {
                if (request != null)
                {
                    response = await _MasterRepository.GetWorkflowAction(request);
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "request can not be Null/Empty.";
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