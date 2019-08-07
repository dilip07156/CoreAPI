using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI.Models;
using VGER_WAPI_CLASSES;
using VGER_WAPI_CLASSES.User;

namespace VGER_WAPI.Repositories
{
    public interface IUserRepository
    {
        IQueryable<dynamic> GetLoginDetails(string UserName);
        UserDetailsResponse GetUserDetails(string UserName);
        ContactDetailsResponse GetContactsByEmailId(ContactDetailsRequest request);

        Task<ContactDetailsResponse> UpdateUserContactDetails(ContactDetailsRequest request);

        Task<ContactDetailsResponse> UpdateUserPassword(ContactDetailsRequest request);

        ContactDetailsResponse UpdateUserDetails(ContactDetailsRequest request);

        UserDetailsResponse ResetUserPassword(LoginRequest request);

        UserByRoleGetRes GetUsersByRole(UserByRoleGetReq request);

        Task<CommonResponse> UpdateUserForQuote(UpdateUserGetReq request);

        Task<CommonResponse> UpdateFollowUp(UpdateUserGetReq request);

        Task<UserRoleGetRes> GetUserRoleDetails(UserRoleGetReq request);

        Task<UserRoleSetRes> SetUserRoleDetails(UserRoleSetReq request);

        Task<UserSetRes> CreateUser(UserSetReq request);

        Task<UserSetRes> EnableDisableUser(UserSetReq request);

        Task<UserSetRes> UpdateUser(UserSetReq request);

        Task<bool> CheckExistingEmail(string emailId);

        List<UserSystemContactDetails> GetActiveUserSystemContactDetailsByRole(string RoleName);

		UserByRoleGetRes GetRoleIdByRoleName(UserByRoleGetReq request);

		UserByRoleGetRes GetUserDetailsByRole(UserByRoleGetReq request);

        RoleGetRes GetRoles();

        IQueryable<dynamic> GetIntegrationLoginDetails(string applicationKey, string userKey, string source);

        void GetUserCompanyType(ref UserCookieDetail userdetails);
    }
}
