using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_DISTRIBUTION.Models;
using MongoDB.Driver;
using VGER_WAPI_CLASSES;

namespace VGER_DISTRIBUTION.Helpers
{
    public class CommonFunction
    {
        private readonly MongoContext _MongoContext = null;
        public CommonFunction(MongoContext settings)
        {
            _MongoContext = settings;
        }
        public void GetUserCompanyType(ref UserCookieDetail userdetails)
        {
            string UserName = userdetails.UserName.ToLower().Trim();
            var Users = _MongoContext.mUsers.AsQueryable().Where(a => a.UserName.ToLower() == UserName).FirstOrDefault();
            var Company = _MongoContext.mCompany.AsQueryable().Where(a => Users.Company_Id == a.VoyagerCompany_Id).FirstOrDefault();

            if ((Company.Issupplier ?? false))
                userdetails.IsSupplier = true;
            if ((Company.Iscustomer ?? false) || (Company.Issubagent ?? false))
                userdetails.IsAgent = true;

            //res.UserName = UserName;
            //res.RoleName = userdetails.RoleName;
            //res.CompanyName = userdetails.CompanyName;
            userdetails.Company_Id = Company.VoyagerCompany_Id;
            userdetails.Contact_Id = Users.Contact_Id;
           // return res;
        }
    }
}
