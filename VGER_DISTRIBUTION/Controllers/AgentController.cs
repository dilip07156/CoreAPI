using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using VGER_DISTRIBUTION.Providers;
using VGER_WAPI_CLASSES;

namespace VGER_DISTRIBUTION.Controllers
{
    [Produces("application/json")]
    [Route("api/Agent")]
    public class AgentController : ControllerBase
    {
        #region Private Variable Declaration

        private readonly IConfiguration _configuration;
        private LoginProviders loginProviders;
        private AgentProviders agentProviders;

        //ServiceProxy serviceProxy;

        #endregion

        public AgentController(IConfiguration configuration)
        {
            _configuration = configuration;
            loginProviders = new LoginProviders(_configuration);
            agentProviders = new AgentProviders(_configuration);
        }

        /// <summary>
        /// Create or update Agent Info(Company) for 3rd party based on provided credentials by 3rd party
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("CreateUpdateAgentInfo")]
        [ProducesResponseType(typeof(ResponseStatus), 200)]
        [ProducesResponseType(typeof(void), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(422)]
        public async Task<IActionResult> CreateUpdateAgentInfo([FromBody] AgentPartnerReq model)
        {
            ManageAgentReq request = new ManageAgentReq();
            var response = new ManageAgentRes();
            try
            {
                if (!ModelState.IsValid)
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Request is not valid";
                    return BadRequest(ModelState);
                }
                else
                {
                    if (model != null)
                    {
                        request = SetAgentInfoWithModel(model);
                        IntegrationLoginResponse objTokenResponse = loginProviders.GetIntegrationToken(request.CredentialInfo).Result;
                        if (string.Compare(objTokenResponse.Message, "Success", true) == 0 && objTokenResponse.Token != "")
                        {
                            request.CreatedUser = objTokenResponse.UserInfo.UserName;
                            request.Application = request.CredentialInfo.Source;
                            request.Token = objTokenResponse.Token;
                            request.LoggedInUserContactId = objTokenResponse.UserInfo.Contact_Id;

                            var countryInfo = agentProviders.GetPartnerCountryDetails(new Attributes { Attribute_Id = request.AgentInfo.Country }, objTokenResponse.Token).Result;
                            if (countryInfo != null && !string.IsNullOrEmpty(countryInfo.ResortInfo.Voyager_Resort_Id))
                            {
                                request.AgentInfo.CountryName = countryInfo.ResortInfo.ResortName;
                            }
                            else
                            {
                                return BadRequest(countryInfo.ResponseStatus);
                            }

                            var cityInfo = agentProviders.GetPartnerCityDetails(new Attributes { Attribute_Id = request.AgentInfo.City }, request.AgentInfo.Country, countryInfo.ResortInfo.ResortCode, objTokenResponse.Token).Result;
                            if (cityInfo != null && !string.IsNullOrEmpty(cityInfo.ResortInfo.Voyager_Resort_Id))
                            {
                                request.AgentInfo.CityName = cityInfo.ResortInfo.ResortName;
                            }
                            else
                            {
                                return BadRequest(cityInfo.ResponseStatus);
                            }

                            request.AgentInfo.Country = countryInfo.ResortInfo.Voyager_Resort_Id;
                            request.AgentInfo.City = cityInfo.ResortInfo.Voyager_Resort_Id;

                            response = agentProviders.CreateUpdatePartnerAgentDetails(request, objTokenResponse.Token).Result;

                            if (response.ResponseStatus != null && !string.IsNullOrEmpty(response.ResponseStatus.Status) && response.ResponseStatus.Status.ToLower() == "duplicate")
                            {
                                return StatusCode(Microsoft.AspNetCore.Http.StatusCodes.Status422UnprocessableEntity, response.ResponseStatus);
                            }
                            if (response.ResponseStatus != null && !string.IsNullOrEmpty(response.ResponseStatus.StatusMessage) && response.ResponseStatus.StatusMessage.ToLower() == "companycodeerror")
                            {
                                return BadRequest(response.ResponseStatus);
                            }
                        }
                        else
                        {
                            return Unauthorized();
                        }
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "AgentInfo details can not be save due to error.";
                        return BadRequest(response.ResponseStatus);
                    }
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An error occurs " + ex.Message.ToString();
                return BadRequest(response.ResponseStatus);
            }
            return Ok(new { Status = response.ResponseStatus.Status, ErrorMessage= response.ResponseStatus.ErrorMessage, StatusMessage= response.ResponseStatus.StatusMessage, Id = response.CompanyInfo.Company_Id });
        }

        [NonAction]
        public ManageAgentReq SetAgentInfoWithModel(AgentPartnerReq model)
        {

            ManageAgentReq res = new ManageAgentReq();
            res.AgentInfo = new AgentThirdPartyInfo();
            res.AgentInfo.Name = model.AgentInfo.Name;
            res.AgentInfo.ApplicationEntityCode = model.AgentInfo.ApplicationEntityCode;
            res.AgentInfo.Address2 = model.AgentInfo.Address2;
            res.AgentInfo.Address1 = model.AgentInfo.Address1;
            res.AgentInfo.City = model.AgentInfo.City;
            res.AgentInfo.Country = model.AgentInfo.Country;
            res.AgentInfo.PostCode = model.AgentInfo.PostCode;

            res.CredentialInfo = new IntegrationLoginRequest();
            res.CredentialInfo.Key = model.CredentialInfo.Key;
            res.CredentialInfo.User = model.CredentialInfo.User;
            res.CredentialInfo.Source = model.CredentialInfo.Source;

            return res;
        }

        /// <summary>
        /// Create Agent Contact Info(add ContactDetails against the Company) for 3rd party based on provided credentials by 3rd party
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("CreateAgentContactInfo")]
        [ProducesResponseType(typeof(ResponseStatus), 200)]
        [ProducesResponseType(typeof(void), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(422)]
        public async Task<IActionResult> CreateAgentContactInfo([FromBody] AgentContactPartnerReq model)
        {
            ManageAgentContactReq request = new ManageAgentContactReq();
            var response = new AgentThirdPartyGetRes();
            try
            {
                if (!ModelState.IsValid)
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Request is not valid";
                    return BadRequest(ModelState);
                }
                else
                {
                    if (model != null)
                    {
                        request = SetAgentContactInfoWithModel(model);
                        IntegrationLoginResponse objTokenResponse = loginProviders.GetIntegrationToken(request.CredentialInfo).Result;
                        if (string.Compare(objTokenResponse.Message, "Success", true) == 0 && objTokenResponse.Token != "")
                        {
                            request.ContactMappingInfo.CreateUser = objTokenResponse.UserInfo.UserName;
                            request.ContactMappingInfo.Application = request.CredentialInfo.Source;
                            request.Token = objTokenResponse.Token;
                            request.LoggedInUserContactId = objTokenResponse.UserInfo.Contact_Id;

                            AgentThirdPartyGetReq CompanyInfoRequest = new AgentThirdPartyGetReq();
                            CompanyInfoRequest.PartnerEntityCode = request.CompanyId;
                            CompanyInfoRequest.Application = request.ContactMappingInfo.Application;

                            var CompanyInfo = agentProviders.GetPartnerAgentDetails(CompanyInfoRequest, objTokenResponse.Token).Result;
                            if (CompanyInfo != null && string.IsNullOrEmpty(CompanyInfo.CompanyId))
                            {
                                response.ResponseStatus.Status = "Failure";
                                response.ResponseStatus.ErrorMessage = "Company/Agent details doesn't exists.";
                                return NotFound(response.ResponseStatus);
                            }
                            request.SelectedCompanyId = CompanyInfo.CompanyId;

                            AgentThirdPartyGetReq ContactInfoRequest = new AgentThirdPartyGetReq();
                            ContactInfoRequest.PartnerEntityCode = request.ContactMappingInfo.PartnerEntityCode;
                            ContactInfoRequest.PartnerEntityName = request.ContactMappingInfo.PartnerEntityName;
                            ContactInfoRequest.Application = request.ContactMappingInfo.Application;

                            var ContactInfo = agentProviders.GetPartnerAgentContactDetails(ContactInfoRequest, objTokenResponse.Token).Result;
                            request.SelectedContactId = ContactInfo.ContactId;
                            if (ContactInfo != null && !string.IsNullOrEmpty(ContactInfo.CompanyId) && !string.IsNullOrEmpty(ContactInfo.ContactId))
                            {
                                response.ResponseStatus.Status = "Failure";
                                response.ResponseStatus.ErrorMessage = "Contact details already exists.";
                                return BadRequest(response.ResponseStatus);
                            }

                            //Insert Contact against the agent/contact
                            response = agentProviders.CreatePartnerAgentContactDetails(request, objTokenResponse.Token).Result;


                        }
                        else
                        {
                            return Unauthorized();
                        }
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "Contact Info details can not be save due to error.";
                        return BadRequest(response.ResponseStatus);
                    }
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An error occurs " + ex.Message.ToString();
                return BadRequest(response.ResponseStatus);
            }
            //return Ok(response.ResponseStatus);
            return Ok(new { Status = response.ResponseStatus.Status, ErrorMessage = response.ResponseStatus.ErrorMessage, StatusMessage = response.ResponseStatus.StatusMessage, Id = response.ContactId });
        }

        /// <summary>
        /// Update Agent Contact Info(edit ContactDetails against the Company) for 3rd party based on provided credentials by 3rd party
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateAgentContactInfo")]
        [ProducesResponseType(typeof(ResponseStatus), 200)]
        [ProducesResponseType(typeof(void), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(422)]
        public async Task<IActionResult> UpdateAgentContactInfo([FromBody] AgentContactPartnerReq model)
        {
            ManageAgentContactReq request = new ManageAgentContactReq();
            var response = new AgentThirdPartyGetRes();
            try
            {
                if (!ModelState.IsValid)
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Request is not valid";
                    return BadRequest(ModelState);
                }
                else
                {
                    if (model != null)
                    {
                        request = SetAgentContactInfoWithModel(model);
                        IntegrationLoginResponse objTokenResponse = loginProviders.GetIntegrationToken(request.CredentialInfo).Result;
                        if (string.Compare(objTokenResponse.Message, "Success", true) == 0 && objTokenResponse.Token != "")
                        {
                            request.ContactMappingInfo.CreateUser = objTokenResponse.UserInfo.UserName;
                            request.ContactMappingInfo.Application = request.CredentialInfo.Source;
                            request.Token = objTokenResponse.Token;
                            request.LoggedInUserContactId = objTokenResponse.UserInfo.Contact_Id;
                            //request.ContactMappingInfo.CreateUser = objTokenResponse.UserInfo.u

                            AgentThirdPartyGetReq CompanyInfoRequest = new AgentThirdPartyGetReq();
                            CompanyInfoRequest.PartnerEntityCode = request.CompanyId;
                            CompanyInfoRequest.Application = request.ContactMappingInfo.Application;

                            var CompanyInfo = agentProviders.GetPartnerAgentDetails(CompanyInfoRequest, objTokenResponse.Token).Result;
                            if (CompanyInfo != null && string.IsNullOrEmpty(CompanyInfo.CompanyId))
                            {
                                response.ResponseStatus.Status = "Failure";
                                response.ResponseStatus.ErrorMessage = "Company/Agent details doesn't exists.";
                                return NotFound(response.ResponseStatus);
                            }
                            request.SelectedCompanyId = CompanyInfo.CompanyId;

                            AgentThirdPartyGetReq ContactInfoRequest = new AgentThirdPartyGetReq();
                            ContactInfoRequest.PartnerEntityCode = request.ContactMappingInfo.PartnerEntityCode;
                            ContactInfoRequest.PartnerEntityName = request.ContactMappingInfo.PartnerEntityName;
                            ContactInfoRequest.Application = request.ContactMappingInfo.Application;

                            var ContactInfo = agentProviders.GetPartnerAgentContactDetails(ContactInfoRequest, objTokenResponse.Token).Result;
                            request.SelectedContactId = ContactInfo.ContactId;
                            if (ContactInfo != null && !string.IsNullOrEmpty(request.SelectedCompanyId) && string.IsNullOrEmpty(ContactInfo.ContactId))
                            {
                                response.ResponseStatus.Status = "Failure";
                                response.ResponseStatus.ErrorMessage = "Contact details doesn't exists.";
                                return NotFound(response.ResponseStatus);
                            }

                            //Update Contact against the agent/contact
                            response = agentProviders.UpdatePartnerAgentContactDetails(request, objTokenResponse.Token).Result;


                        }
                        else
                        {
                            return Unauthorized();
                        }
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "Contact Info details can not be save due to error.";
                        return BadRequest(response.ResponseStatus);
                    }
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An error occurs " + ex.Message.ToString();
                return BadRequest(response.ResponseStatus);
            }
            //return Ok(response.ResponseStatus);
            return Ok(new { Status = response.ResponseStatus.Status, ErrorMessage = response.ResponseStatus.ErrorMessage, StatusMessage = response.ResponseStatus.StatusMessage, Id = response.ContactId });
        }

        [NonAction]
        public ManageAgentContactReq SetAgentContactInfoWithModel(AgentContactPartnerReq model)
        {

            ManageAgentContactReq res = new ManageAgentContactReq();
            res.ContactMappingInfo = new ManageAgentContactMapping();
            res.CompanyId = model.AgentContactInfo.ApplicationAgentEntityCode;
            res.ContactMappingInfo.PartnerEntityCode = model.AgentContactInfo.ApplicationEntityCode;
            res.ContactMappingInfo.PartnerEntityName = model.AgentContactInfo.FirstName + " " + model.AgentContactInfo.LastName;
            res.ContactMappingInfo.Telephone = model.AgentContactInfo.Telephone;
            res.ContactMappingInfo.Application = model.CredentialInfo.Source.ToUpper();
            res.ContactMappingInfo.Title = model.AgentContactInfo.Title;
            res.ContactMappingInfo.FirstName = model.AgentContactInfo.FirstName;
            res.ContactMappingInfo.LastName = model.AgentContactInfo.LastName;
            res.ContactMappingInfo.Email = model.AgentContactInfo.Email;

            res.CredentialInfo = new IntegrationLoginRequest();
            res.CredentialInfo.Key = model.CredentialInfo.Key;
            res.CredentialInfo.User = model.CredentialInfo.User;
            res.CredentialInfo.Source = model.CredentialInfo.Source.ToUpper();

            return res;
        }
    }
}