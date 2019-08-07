using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI.Proxy;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Providers
{
    public class AgentProviders
    {
        private readonly IConfiguration _configuration;
        ServiceProxy serviceProxy;

        public AgentProviders(IConfiguration configuration)
        {
            _configuration = configuration;
            serviceProxy = new ServiceProxy(_configuration);
        }

        #region Bridge Service
        public async Task<GetCompany_RS> GetLatestSQLReferenceNumber(GetCompany_RQ request, string ticket)
        {
            GetCompany_RS response = new GetCompany_RS();
            response = await serviceProxy.PostData(_configuration.GetValue<string>("Bridge:Company:GetLatestSQLReferenceNumber"), request, typeof(GetCompany_RS), ticket, "Bridge");
            return response;
        }

        public async Task<ResponseStatus> SetCompany(SetCompany_RQ request, string ticket)
        {
            ResponseStatus response = new ResponseStatus();
            response = await serviceProxy.PostData(_configuration.GetValue<string>("Bridge:Company:SetCompany"), request, typeof(ResponseStatus), ticket, "Bridge");
            return response;
        }

        public async Task<ResponseStatus> SetCompanyContact(SetCompanyContact_RQ request, string ticket)
        {
            ResponseStatus response = new ResponseStatus();
            response = await serviceProxy.PostData(_configuration.GetValue<string>("Bridge:Company:SetCompanyContact"), request, typeof(ResponseStatus), ticket, "Bridge");
            return response;
        }

        #endregion
    }
}
