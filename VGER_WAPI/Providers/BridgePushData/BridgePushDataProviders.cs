using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI.Proxy;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Providers
{
    public class BridgePushDataProviders
    {
        private readonly IConfiguration _configuration;
        ServiceProxy serviceProxy;

        #region Initializers 
        public BridgePushDataProviders(IConfiguration configuration)
        {
            _configuration = configuration;
            serviceProxy = new ServiceProxy(_configuration);
        }
        #endregion

        #region Bridge Services
        /// <summary>
        /// Generate New Booking Nmuber
        /// </summary>
        /// <param name="objGetCompany_RQ">Takes Type</param>
        /// <returns></returns>
        public async Task<GetCompany_RS> GetLatestSQLReferenceNumber(GetCompany_RQ objGetCompany_RQ)
        {
            GetCompany_RS objGetCompany_RS = new GetCompany_RS();
            objGetCompany_RS = await serviceProxy.PostData(_configuration.GetValue<string>("MPUSH:push:GetLatestSQLReferenceNumber"), objGetCompany_RQ, typeof(GetCompany_RS), "MongoPushUrl");
            return objGetCompany_RS;
        } 
        #endregion
    }
}
