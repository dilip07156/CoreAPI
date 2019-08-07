using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_DISTRIBUTION.Proxy;
using VGER_WAPI_CLASSES;

namespace VGER_DISTRIBUTION.Providers
{
    public class LoginProviders
    {

        #region MyRegion

        private readonly IConfiguration _configuration;
        ServiceProxy serviceProxy;

        #endregion

        public LoginProviders(IConfiguration configuration)
        {
            _configuration = configuration;
            serviceProxy = new ServiceProxy(_configuration);
        }

        public async Task<IntegrationLoginResponse> GetIntegrationToken(IntegrationLoginRequest objTokenRequest)
        {
            IntegrationLoginResponse objLoginResponse = new IntegrationLoginResponse();
            objLoginResponse = await serviceProxy.PostData(_configuration.GetValue<string>("ServiceGetIntegrationToken"), objTokenRequest, typeof(IntegrationLoginResponse));
            return objLoginResponse;
        }
    }
}
