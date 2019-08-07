using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using VGER_WAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using VGER_WAPI_CLASSES;
using VGER_WAPI.Helpers;

namespace VGER_WAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/Email")]
    public class EmailController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IEmailRepository _emailRepository;

        public EmailController(IConfiguration configuration, IEmailRepository emailRepository)
        {
            _configuration = configuration;          
            _emailRepository = emailRepository;
        }

        /// <summary>
        /// Generic Send email method
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        //[Authorize]
        [HttpPost]
        [Route("GenerateAndSendEmail")]
        public async Task<EmailGetRes> GenerateAndSendEmail([FromBody] EmailGetReq request)
        {
            var response = new EmailGetRes(); 
            try
            {
                response = await _emailRepository.GenerateEmail(request); 
            }
            catch (Exception ex)
            { 
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Generic Send email method
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("SendEmail")]
        public async Task<EmailGetRes> SendEmail([FromBody] EmailTemplateGetRes request)
        {
            var response = new EmailGetRes();

            try
            {
                response = await _emailRepository.SendEmail(request);
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