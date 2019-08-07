using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using VGER_DISTRIBUTION.Providers;
using VGER_WAPI_CLASSES;

namespace VGER_DISTRIBUTION.Controllers
{
    [Produces("application/json")]
    [Route("api/Quote")]
    public class QuoteController : Controller
    {
        #region Private Variable Declaration

        private readonly IConfiguration _configuration;
        private LoginProviders loginProviders;
        private QuoteProviders quoteProviders;

        #endregion

        public QuoteController(IConfiguration configuration)
        {
            _configuration = configuration;
            loginProviders = new LoginProviders(configuration);
            quoteProviders = new QuoteProviders(configuration);
        }

        /// <summary>
        /// Update Opportunity Info(Booking Opportunity "Status" with "Reason") for 3rd party based on provided credentials by 3rd party
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateOpportunityInfo")]
        [ProducesResponseType(typeof(ResponseStatus), 200)]
        [ProducesResponseType(typeof(void), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        [ProducesResponseType(422)]
        public async Task<IActionResult> UpdateOpportunityInfo([FromBody] OpportunityPartnerReq model)
        {
            ManageOpportunityReq request = new ManageOpportunityReq();
            OpportunityPartnerRes response = new OpportunityPartnerRes();
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
                        request = SetOpportunityInfoWithModel(model);

                        IntegrationLoginResponse objTokenResponse = loginProviders.GetIntegrationToken(request.CredentialInfo).Result;
                        if (string.Compare(objTokenResponse.Message, "Success", true) == 0 && objTokenResponse.Token != "")
                        {
                            request.CreatedUser = objTokenResponse.UserInfo.UserName;
                            request.Application = request.CredentialInfo.Source;
                            request.Token = objTokenResponse.Token;
                            request.LoggedInUserContactId = objTokenResponse.UserInfo.Contact_Id;

                            response = quoteProviders.UpdatePartnerOpportunityDetails(request, objTokenResponse.Token).Result;

                            if (response.ResponseStatus != null && !string.IsNullOrEmpty(response.ResponseStatus.Status) && response.ResponseStatus.Status.ToLower() == "failed")
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
                        response.ResponseStatus.ErrorMessage = "Opportunity details can not be save due to error.";
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
            return Ok(new { Status = response.ResponseStatus.Status, ErrorMessage = response.ResponseStatus.ErrorMessage, StatusMessage = response.ResponseStatus.StatusMessage, Id = request.OpportunityInfo.OpportunityId });
        }

        [NonAction]
        public ManageOpportunityReq SetOpportunityInfoWithModel(OpportunityPartnerReq model)
        {

            ManageOpportunityReq req = new ManageOpportunityReq();
            req.OpportunityInfo = model.OpportunityInfo;

            req.CredentialInfo = new IntegrationLoginRequest();
            req.CredentialInfo.Key = model.CredentialInfo.Key;
            req.CredentialInfo.User = model.CredentialInfo.User;
            req.CredentialInfo.Source = model.CredentialInfo.Source;

            return req;
        }

    }
}