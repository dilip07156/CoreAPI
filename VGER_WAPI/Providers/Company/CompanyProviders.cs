using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI.Proxy;
using VGER_WAPI_CLASSES;
using Microsoft.Extensions.Configuration;

namespace VGER_WAPI.Providers
{
    public class CompanyProviders
    {
        private readonly IConfiguration _configuration;
        ServiceProxy serviceProxy;

        #region Initializers 
        public CompanyProviders(IConfiguration configuration)
        {
            _configuration = configuration;
            serviceProxy = new ServiceProxy(_configuration);
        }
        #endregion

        #region Bridge Services
        /// <summary>
        /// Insert/Update Company->Contact in SQL
        /// </summary>
        /// <param name="objSetCompanyContact_RQ">Takes ContactId and Username</param>
        /// <returns></returns>
        public async Task<ResponseStatus> SetCompanyContact(SetCompanyContact_RQ objSetCompanyContact_RQ)
        {
            ResponseStatus objResponseStatus = new ResponseStatus();
            objResponseStatus = await serviceProxy.PostData(_configuration.GetValue<string>("Bridge:Company:SetCompanyContact"), objSetCompanyContact_RQ, typeof(ResponseStatus), "Bridge");
            return objResponseStatus;
        }

        /// <summary>
        /// Insert/Update ProductSuppliers table in SQL
        /// </summary>
        /// <param name="objSetProduct_RQ">Takes ProductSupplier_Id and Username</param>
        /// <returns></returns>
        public async Task<ResponseStatus> SetProductSuppliers(SetProduct_RQ objSetProduct_RQ)
        {
            ResponseStatus objResponseStatus = new ResponseStatus();
            objResponseStatus = await serviceProxy.PostData(_configuration.GetValue<string>("Bridge:Company:SetProductSuppliers"), objSetProduct_RQ, typeof(ResponseStatus), "Bridge");
            return objResponseStatus;
        }
        #endregion
    }
}
