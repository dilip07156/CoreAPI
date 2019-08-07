using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_DISTRIBUTION.Proxy;
using VGER_WAPI_CLASSES;

namespace VGER_DISTRIBUTION.Providers
{
    public class QuoteProviders
    {
        #region Private Variable Declaration

        private readonly IConfiguration _configuration;
        ServiceProxy serviceProxy;

        #endregion

        public QuoteProviders(IConfiguration configuration)
        {
            _configuration = configuration;
            serviceProxy = new ServiceProxy(_configuration);
        }

        public async Task<OpportunityPartnerRes> UpdatePartnerOpportunityDetails(ManageOpportunityReq request, string ticket)
        {
            OpportunityPartnerRes response = new OpportunityPartnerRes();
            response = await serviceProxy.PostData(_configuration.GetValue<string>("QuoteService:UpdatePartnerOpportunityDetails"), request, typeof(OpportunityPartnerRes), ticket);
            return response;
        }

    }
}
