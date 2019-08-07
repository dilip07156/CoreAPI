using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MimeKit;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VGER_WAPI.Helpers;
using VGER_WAPI.Models;
using VGER_WAPI.Repositories;
using VGER_WAPI_CLASSES;
using VGER_WAPI_CLASSES.User;

namespace VGER_WAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/User")]
    public class UserController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _UserRepository;
        private readonly IHostingEnvironment _env;
        private readonly IOptions<MongoSettings> _settings;
        private readonly IGenericRepository _genericRepository;
        private readonly IEmailRepository _emailRepository;

        public UserController(IConfiguration configuration, IOptions<MongoSettings> settings, IUserRepository userRepository, IHostingEnvironment env, IGenericRepository genericRepository,
            IEmailRepository emailRepository)
        {
            _env = env;
            _configuration = configuration;
            _UserRepository = userRepository;
            _settings = settings;
            _genericRepository = genericRepository;
            _emailRepository = emailRepository;
        }

        /// <summary>
        /// Method to Authenticate user and Generate JWT token
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("RequestToken")]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        public LoginResponse RequestToken([FromBody] LoginRequest request)
        {
            LoginResponse response = new LoginResponse();
            try
            {
                if (!string.IsNullOrEmpty(request.UserName) && !string.IsNullOrEmpty(request.Password))
                {
                    int tokenTimeout = Convert.ToInt32(_configuration["JWTTokenTimeout"]);
                    string HashedPassword = Encrypt.Sha256encrypt(request.Password);
                    var result = _UserRepository.GetLoginDetails(request.UserName);
                    if (result.Count() > 0 && result.First().Password == HashedPassword)
                    {
                        var curUser = _UserRepository.GetUserDetails(request.UserName);
                        //string SA = _UserRepository.GetUserCompanyType(request.UserName);
                        string userRoles = "";
                        int i = 0;

                        foreach (UserRoleDetails role in curUser.UserRoleDetails)
                        {
                            i++;
                            userRoles = userRoles + role.RoleName;

                            if (i != curUser.UserRoleDetails.Count())
                                userRoles = userRoles + ", ";
                        }
                        var token = CreateToken.GenerateToken(request.UserName, _configuration["SecurityKey"], _configuration["ValidIssuer"], _configuration["ValidAudience"], curUser.CompanyName, userRoles, tokenTimeout);
                        response.Token = new JwtSecurityTokenHandler().WriteToken(token);
                        response.Expiry = tokenTimeout.ToString();
                        response.Message = "Success";

                        return response;
                    }
                    else
                    {
                        response.Token = null;
                        response.Expiry = null;
                        response.Message = "Invalid Credentials";
                        return response;
                    }
                }
                else
                {
                    response.Token = null;
                    response.Expiry = null;
                    response.Message = "UnAuthorized Request";
                    return response;
                }
            }
            catch (Exception ex)
            {
                response.Token = null;
                response.Expiry = null;
                response.Message = ex.Message;
                return response;
            }

        }

        /// <summary>
        /// Method to Authenticate user and Generate JWT token based on Integration Credentials data
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("RequestIntegrationToken")]
        [ProducesResponseType(typeof(IntegrationLoginResponse), 200)]
        public IntegrationLoginResponse RequestIntegrationToken([FromBody] IntegrationLoginRequest request)
        {
            IntegrationLoginResponse response = new IntegrationLoginResponse();
            try
            {
                if (!string.IsNullOrEmpty(request.Key) && !string.IsNullOrEmpty(request.Source) && !string.IsNullOrEmpty(request.User))
                {
                    int tokenTimeout = Convert.ToInt32(_configuration["JWTTokenTimeout"]);

                    var result = _UserRepository.GetIntegrationLoginDetails(request.Key, request.User, request.Source);

                    if (result != null && result.Any())
                    {
                        var curUser = _UserRepository.GetUserDetails(result.First().UserName);
                        string userRoles = "";
                        int i = 0;

                        foreach (UserRoleDetails role in curUser.UserRoleDetails)
                        {
                            i++;
                            userRoles = userRoles + role.RoleName;

                            if (i != curUser.UserRoleDetails.Count)
                                userRoles = userRoles + ", ";
                        }
                        var token = CreateToken.GenerateToken(result.First().UserName, _configuration["SecurityKey"], _configuration["ValidIssuer"], _configuration["ValidAudience"], curUser.CompanyName, userRoles, tokenTimeout);
                        response.Token = new JwtSecurityTokenHandler().WriteToken(token);
                        response.Expiry = tokenTimeout.ToString();
                        response.UserInfo = result.FirstOrDefault();
                        response.Message = "Success";

                        return response;
                    }
                    else
                    {
                        response.Token = null;
                        response.Expiry = null;
                        response.Message = "Invalid Credentials";
                        return response;
                    }
                }
                else
                {
                    response.Token = null;
                    response.Expiry = null;
                    response.Message = "UnAuthorized Request";
                    return response;
                }
            }
            catch (Exception ex)
            {
                response.Token = null;
                response.Expiry = null;
                response.Message = ex.Message;
                return response;
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
        public UserDetailsResponse GetUserDetails([FromBody] UserDetailsRequest request)
        {
            var response = new UserDetailsResponse();
            if (!string.IsNullOrWhiteSpace(request.UserName))
            {
                response = _UserRepository.GetUserDetails(request.UserName);

                if (response != null)
                {
                    response.Status = "Sucess";
                    return response;
                }
                else
                {
                    response.Status = "Invalid UserName";
                    return response;
                }
            }
            else
            {
                response.Status = "Failure";
                return response;
            }
        }

        /// <summary>
        /// Method to get contacts details
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetContactsByEmailId")]
        public ContactDetailsResponse GetContactsByEmailId([FromBody] ContactDetailsRequest request)
        {
            var response = new ContactDetailsResponse();
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                response = _UserRepository.GetContactsByEmailId(request);

                if (response != null)
                {
                    response.Status = "Sucess";
                    return response;
                }
                else
                {
                    response.Status = "Invalid Email";
                    return response;
                }
            }
            else
            {
                response.Status = "Email Id can not be blank";
                return response;
            }
        }


        /// <summary>
        /// Method to Update contacts details
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("UpdateContactDetails")]
        public async Task<ContactDetailsResponse> UpdateContactDetails([FromBody] ContactDetailsRequest request)
        {
            var response = new ContactDetailsResponse();
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                //response = _UserRepository.GetContactsByEmailId(request);
                response = await _UserRepository.UpdateUserContactDetails(request);


                if (response != null)
                {
                    response.Status = "Sucess";

                    return response;
                }
                else
                {
                    response.Status = "Invalid Email";
                    return response;
                }
            }
            else
            {
                response.Status = "Email Id can not be blank";
                return response;
            }
        }



        /// <summary>
        /// Method to Update User Password
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("UpdateUserPassword")]
        public async Task<ContactDetailsResponse> UpdateUserPassword([FromBody] ContactDetailsRequest request)
        {
            var response = new ContactDetailsResponse();
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                //response = _UserRepository.GetContactsByEmailId(request);
                response = await _UserRepository.UpdateUserPassword(request);

                if (response != null)
                {
                    response.Status = "Sucess";

                    return response;
                }
                else
                {
                    response.Status = "Invalid Email";
                    return response;
                }
            }
            else
            {
                response.Status = "Email Id can not be blank";
                return response;
            }
        }

        /// <summary>
        /// Method to Update User Password
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("UpdateUserDetails")]
        public ContactDetailsResponse UpdateUserDetails([FromBody] ContactDetailsRequest request)
        {
            var response = new ContactDetailsResponse();
            if (!string.IsNullOrWhiteSpace(request.Users.VoyagerUser_Id))
            {
                //response = _UserRepository.GetContactsByEmailId(request);
                response = _UserRepository.UpdateUserDetails(request);

                if (response != null)
                {
                    response.Status = "Sucess";

                    return response;
                }
                else
                {
                    response.Status = "Invalid Voyager User Id";
                    return response;
                }
            }
            else
            {
                response.Status = "Voyager User Id can not be blank";
                return response;
            }
        }


        /// <summary>
        /// Method to Authenticate user and Generate JWT token
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("UserPasswordRecover")]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        public async Task<bool> UserPasswordRecover([FromBody] LoginRequest request)
        {
            //LoginResponse response = new LoginResponse();
            bool userExist = false;
            try
            {
                if (!string.IsNullOrEmpty(request.UserName))
                {
                    var result = _UserRepository.GetLoginDetails(request.UserName);
                    if (result.Count() > 0)
                    {
                        var user = result.ToList().FirstOrDefault();
                        string password = Guid.NewGuid().ToString().Replace("-", "");

                        request.ContactId = user.Contact_Id;
                        request.CompanyId = user.Company_Id;
                        request.Password = password;
                        UserDetailsResponse res = _UserRepository.ResetUserPassword(request);
                        if (res != null && res.Status.ToLower() == "success")
                        {
                            #region Send Mail:-UserPasswordRecover
                            var objEmailGetReq = new EmailGetReq()
                            {
                                Remarks = password,
                                UserEmail = request.UserName,
                                DocumentType = DocType.PWDRECOVER,
                                PlacerUserId = res.VoyagerUser_Id,
                                SystemCompany_Id = res.SystemCompany_Id
                            };
                            var responseStatusMail = await _emailRepository.GenerateEmail(objEmailGetReq);
                            if (responseStatusMail == null || responseStatusMail.ResponseStatus == null || string.IsNullOrEmpty(responseStatusMail.ResponseStatus.Status))
                            {
                                responseStatusMail.ResponseStatus = new ResponseStatus();
                                responseStatusMail.ResponseStatus.Status = "Error";
                                responseStatusMail.ResponseStatus.ErrorMessage = "Mail not sent.";
                            }
                            else if (responseStatusMail.ResponseStatus.Status.ToLower() == "success")
                            {
                                userExist = true;
                            }
                            #endregion
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //return response;
            }
            return userExist;
        }

        /// <summary>
        /// Method to get User details by Role
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetUsersByRole")]
        public UserByRoleGetRes GetUsersByRole([FromBody] UserByRoleGetReq request)
        {
            var response = new UserByRoleGetRes();
            if (request.RoleName.Count > 0)
            {
                response = _UserRepository.GetUsersByRole(request);

                return response;
            }
            else
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "User Role can not be blank";
                return response;
            }
        }

        /// <summary>
        /// Method to update users (officers) for quote
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("UpdateUserForQuote")]
        public async Task<CommonResponse> UpdateUserForQuote([FromBody] UpdateUserGetReq request)
        {
            var response = new CommonResponse();
            if (request.ModuleName == "ops")
            {
                if (!string.IsNullOrEmpty(request.BookingNumber))
                {
                    response = await _UserRepository.UpdateUserForQuote(request);
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "BookingNumber can not be blank";
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(request.QRFID))
                {
                    response = await _UserRepository.UpdateUserForQuote(request);
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "QRFId can not be blank";
                }
            }
            return response;
        }

        /// <summary>
        /// Method to update users (officers) for quote
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("UpdateFollowUp")]
        public async Task<CommonResponse> UpdateFollowUp([FromBody] UpdateUserGetReq request)
        {
            var response = new CommonResponse();
            if (!string.IsNullOrEmpty(request.QRFID))
            {
                response = await _UserRepository.UpdateFollowUp(request);

                return response;
            }
            else
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "QRFId can not be blank";
                return response;
            }
        }

        /// <summary>
        ///Get Users And Role Details
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetUserRoleDetails")]
        public async Task<UserRoleGetRes> GetUserRoleDetails([FromBody] UserRoleGetReq request)
        {
            var response = new UserRoleGetRes();
            if (request != null)
            {
                response = await _UserRepository.GetUserRoleDetails(request);
                return response;
            }
            else
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "Request can not be blank";
                return response;
            }
        }

        /// <summary>
        ///Set Users And Role Details
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("SetUserRoleDetails")]
        public async Task<UserRoleSetRes> SetUserRoleDetails([FromBody] UserRoleSetReq request)
        {
            var response = new UserRoleSetRes();
            if (request != null)
            {
                response = await _UserRepository.SetUserRoleDetails(request);
                return response;
            }
            else
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "Request can not be blank";
                return response;
            }
        }

        /// <summary>
        ///Create User
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("CreateUser")]
        public async Task<UserSetRes> CreateUser([FromBody] UserSetReq request)
        {
            var response = new UserSetRes();
            if (request != null)
            {
                response = await _UserRepository.CreateUser(request);
                return response;
            }
            else
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "Request can not be blank";
                return response;
            }
        }

        /// <summary>
        ///Enable disable User
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("EnableDisableUser")]
        public async Task<UserSetRes> EnableDisableUser([FromBody] UserSetReq request)
        {
            var response = new UserSetRes();
            if (request != null)
            {
                response = await _UserRepository.EnableDisableUser(request);
                return response;
            }
            else
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "Request can not be blank";
                return response;
            }
        }

        /// <summary>
        ///Update User
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("UpdateUser")]
        public async Task<UserSetRes> UpdateUser([FromBody] UserSetReq request)
        {
            var response = new UserSetRes();
            if (request != null)
            {
                response = await _UserRepository.UpdateUser(request);
                return response;
            }
            else
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "Request can not be blank";
                return response;
            }
        }

        /// <summary>
        ///Check Existing Email
        /// </summary>
        /// <param name="emailId"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("CheckExistingEmail")]
        public async Task<bool> CheckExistingEmail([FromBody] string emailId)
        {
            bool response = false;
            if (emailId != null)
            {
                response = await _UserRepository.CheckExistingEmail(emailId);
                return response;
            }
            else
            {
                return response;
            }
        }

		/// <summary>
		/// Method to get RoleId by Role Name
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		[Authorize]
		[HttpPost]
		[Route("GetRoleIdByRoleName")]
		public UserByRoleGetRes GetRoleIdByRoleName([FromBody] UserByRoleGetReq request)
		{
			var response = new UserByRoleGetRes();
			if (request.RoleName.Count > 0)
			{
				response = _UserRepository.GetRoleIdByRoleName(request);
				return response;
			}
			else
			{
				response.ResponseStatus.Status = "Failure";
				response.ResponseStatus.ErrorMessage = "User Role can not be blank";
				return response;
			}
		}
        /// <summary>
		/// Method to get Roles
		/// </summary>
		/// <returns>Roles</returns>
		[Authorize]
        [HttpPost]
        [Route("GetRoles")]
        public RoleGetRes GetRoles()
        {
            RoleGetRes response = new RoleGetRes();
            response = _UserRepository.GetRoles();
            if (response == null && !response.RoleDetails.Any())
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "Roles Could Not Be Loaded";
            }
            else {
                response.ResponseStatus.Status = "Success";
            }
            return response;
        }

        /// <summary>
        /// Method to get RoleId by Role Name
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
		[HttpPost]
		[Route("GetUserDetailsByRole")]
		public UserByRoleGetRes GetUserDetailsByRole([FromBody] UserByRoleGetReq request)
		{
			var response = new UserByRoleGetRes();
			if (request.RoleName.Count > 0)
			{
				response = _UserRepository.GetUserDetailsByRole(request);
				return response;
			}
			else
			{
				response.ResponseStatus.Status = "Failure";
				response.ResponseStatus.ErrorMessage = "User Role can not be blank";
				return response;
			}
		}
	}
}