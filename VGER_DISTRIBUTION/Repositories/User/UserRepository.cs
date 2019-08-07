using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_DISTRIBUTION.Helpers;
using VGER_DISTRIBUTION.Models;
using VGER_WAPI_CLASSES;

namespace VGER_DISTRIBUTION.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly MongoContext _MongoContext = null;
        public UserRepository(IOptions<MongoSettings> settings)
        {
            _MongoContext = new MongoContext(settings);
        }

        #region Login

        public IQueryable<dynamic> GetLoginDetails(string UserName)
        {
            var result = _MongoContext.mUsers.AsQueryable().Where(x => x.UserName.ToLower() == UserName.ToLower().Trim());
            return result;
        }

        #endregion

        #region UserDetails

        public UserDetailsResponse GetUserDetails(string UserName)
        {
            var response = new UserDetailsResponse();
            try
            {
                //var UserDetails = (from u in _MongoContext.mUsers.AsQueryable()
                //                   join co in _MongoContext.mCompany.AsQueryable() on u.Company_Id equals co.VoyagerCompany_Id into uco
                //                   join s in _MongoContext.mSystem.AsQueryable() on u.Company_Id equals s.CoreCompany_Id into us
                //                   where u.UserName == UserName
                //                   select new { u.VoyagerUser_Id, u.UserName, u.FirstName, u.LastName, uco.First().Name, us.First().EmergencyPhoneGroups }).FirstOrDefault();
                UserName = UserName.ToLower().Trim();
                var Users = _MongoContext.mUsers.AsQueryable().Where(a => a.UserName.ToLower() == UserName).FirstOrDefault();
                var Contact = _MongoContext.mContacts.AsQueryable().Where(a => !string.IsNullOrEmpty(a.MAIL) && a.MAIL.ToLower() == UserName).FirstOrDefault();
                var Company = _MongoContext.mCompany.AsQueryable().Where(a => Users.Company_Id == a.VoyagerCompany_Id).FirstOrDefault();
                var System = _MongoContext.mSystem.AsQueryable().FirstOrDefault(); //.Where(a => Users.Company_Id == a.CoreCompany_Id)
                var RoleList = _MongoContext.mUsersInRoles.AsQueryable().Where(a => a.UserId == Users.VoyagerUser_Id).Select(a => a.RoleId).ToList();
                var RoleDetails = _MongoContext.mRoles.AsQueryable().Where(a => RoleList.Contains(a.Voyager_Role_Id)).Select(a => new UserRoleDetails
                {
                    Voyager_Role_Id = a.Voyager_Role_Id,
                    RoleName = a.RoleName,
                    LoweredRoleName = a.LoweredRoleName,
                    Description = a.Description
                }).ToList();

                response.FirstName = (Contact.FIRSTNAME == null) ? Users.FirstName : Contact.FIRSTNAME;
                response.LastName = (Contact.LastNAME == null) ? Users.FirstName : Contact.LastNAME;
                response.ContactDisplayMessage = (System.EmergencyPhoneGroups == null) ? "" : System.EmergencyPhoneGroups;
                response.CreditAmount = "10000";
                response.BalanceAmount = "20000";
                response.CompanyName = Company.Name;
                response.Currency = "USD";
                response.UserRoleDetails = RoleDetails;
                response.Status = "Success";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                response.Status = "Error Occured";
            }
            return response;
        }

       

        #endregion
    }
}
