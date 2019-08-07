using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using VGER_WAPI_CLASSES;
//using Voyager.App.Contracts;
//using VGER_Communicator.Library;
//usingVGER_Communicator.Models;
using VGER_Communicator.Providers;

namespace VGER_DISTRIBUTION.Controllers
{
    public class VoyagerController : Controller
    {
        private readonly IConfiguration _configuration;

        public VoyagerController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string token
        {
            get
            {
                return Request.Cookies["JWTToken"];
            }
        }

        public string ckUserEmailId
        {
            get
            {
                return Request.Cookies["EmailId"];
            }
        }

        private string _UserRoles;
        public string[] UserRoles
        {
            get
            {
                _UserRoles = Request.Cookies["UserRoles"];
                if (!string.IsNullOrEmpty(_UserRoles))
                    return _UserRoles.Split(',');
                else
                    return null;
            }
        }

        #region Set/Remove Cookies
        public bool SetCookiesForLoginUser(UserDetailsResponse objUserDetailsResponse, string EmailId)
        {
            Response.Cookies.Append("EmailId", EmailId);
            Response.Cookies.Append("UserName", objUserDetailsResponse.FirstName + " " + objUserDetailsResponse.LastName);
            Response.Cookies.Append("ContactDisplayMessage", objUserDetailsResponse.ContactDisplayMessage);
            Response.Cookies.Append("CompanyName", objUserDetailsResponse.CompanyName);
            Response.Cookies.Append("Currency", objUserDetailsResponse.Currency);
            Response.Cookies.Append("BalanceAmount", objUserDetailsResponse.BalanceAmount);
            Response.Cookies.Append("CreditAmount", objUserDetailsResponse.CreditAmount);
            Response.Cookies.Append("UserRoles", string.Join(",", objUserDetailsResponse.UserRoleDetails.Select(a => a.RoleName)));
            return true;
        }

        public bool DeleteAllCookies()
        {
            Response.Cookies.Delete("JWTToken");
            Response.Cookies.Delete("EmailId");
            Response.Cookies.Delete("UserName");
            Response.Cookies.Delete("ContactDisplayMessage");
            Response.Cookies.Delete("CompanyName");
            Response.Cookies.Delete("Currency");
            Response.Cookies.Delete("BalanceAmount");
            Response.Cookies.Delete("CreditAmount");
            Response.Cookies.Delete("UserRoles");
            return true;
        }
        #endregion
    }
}
