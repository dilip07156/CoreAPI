using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using VGER_DISTRIBUTION.Helpers;
using VGER_DISTRIBUTION.Models;
using VGER_DISTRIBUTION.Repositories;
using VGER_WAPI_CLASSES;
using Microsoft.Extensions.Logging;
using NLog;
using Microsoft.AspNetCore.Mvc.Filters;

namespace VGER_DISTRIBUTION.Controllers
{
    [ServiceFilter(typeof(LogFilter))]
    [Produces("application/json")]
    [Route("api/User")]

    public class UserController : Controller
    {
        private ILogger<UserController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _UserRepository;
        public UserController(IConfiguration configuration, IOptions<MongoSettings> settings, IUserRepository userRepository, ILogger<UserController> logger)
        {
            _configuration = configuration;
            _UserRepository = userRepository;
            _logger = logger;
        }

        /// <summary>
        /// Method to Authenticate user and Generate JWT token
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [ServiceFilter(typeof(LogFilter))]
        [AllowAnonymous]
        [HttpPost]
        [Route("RequestToken")]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        [ProducesResponseType(typeof(void), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public IActionResult RequestToken([FromBody] LoginRequest request)
        {
            LoginResponse response = new LoginResponse();
            try
            {
                if (!ModelState.IsValid)
                {
                    response.Message = "Request is not valid";
                    return BadRequest(response);
                }
                else
                {
                    if (!string.IsNullOrEmpty(request.UserName) && !string.IsNullOrEmpty(request.Password))
                    {
                        string HashedPassword = Encrypt.Sha256encrypt(request.Password);
                        var result = _UserRepository.GetLoginDetails(request.UserName);
                        if (result.Count() > 0 && result.First().Password == HashedPassword)
                        {
                            var curUser = _UserRepository.GetUserDetails(request.UserName);
                            //string SA = _UserRepository.GetUserCompanyType(request.UserName);
                            string userRoles = "";
                            int i = 0;
                            
                            foreach(UserRoleDetails role in curUser.UserRoleDetails)
                            {
                                i++;
                                userRoles = userRoles + role.RoleName;

                                if (i != curUser.UserRoleDetails.Count())
                                    userRoles = userRoles + ", ";
                            }
                            var token = CreateToken.GenerateToken(request.UserName, _configuration["SecurityKey"], _configuration["ValidIssuer"], _configuration["ValidAudience"], curUser.CompanyName, userRoles);
                            response.Token = new JwtSecurityTokenHandler().WriteToken(token);
                            response.Expiry = "60";
                            response.Message = "Success";

                            return Ok(response);
                        }
                        else
                        {
                            response.Token = null;
                            response.Expiry = null;
                            response.Message = "Invalid Credentials";
                            return BadRequest(response);
                        }
                    }
                    else
                    {
                        response.Token = null;
                        response.Expiry = null;
                        response.Message = "UnAuthorized Request";
                        return Unauthorized();
                    }
                }
            }
            catch (Exception ex)
            {
                response.Token = null;
                response.Expiry = null;
                response.Message = ex.Message;
                return BadRequest(response);
            }

        }

        /// <summary>
        /// Method to get user details
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetUserDetails")]
        [ProducesResponseType(typeof(UserDetailsResponse), 200)]
        [ProducesResponseType(typeof(void), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public IActionResult GetUserDetails([FromBody] UserDetailsRequest request)
        {
            _logger.LogCritical("nlog is working from a controller");
            var response = new UserDetailsResponse();
            if (!ModelState.IsValid)
            {
                response.Status = "Request is not valid";
                return BadRequest(response);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(request.UserName))
                {
                    response = _UserRepository.GetUserDetails(request.UserName);

                    if (response != null)
                    {
                        return Ok(response);
                    }
                    else
                    {
                        response.Status = "Invalid UserName";
                        return NotFound(response);
                    }
                }
                else
                {
                    response.Status = "Failure";
                    return BadRequest(response);
                }
            }
        }
    }
}
