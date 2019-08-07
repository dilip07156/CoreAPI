using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI.Proxy;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Providers
{
    public class MSDynamicsProviders
    {

        private readonly IConfiguration _configuration;
        ServiceProxy serviceProxy;

        public MSDynamicsProviders(IConfiguration configuration)
        {
            _configuration = configuration;
            serviceProxy = new ServiceProxy(_configuration);
        }

        /// <summary>
        /// the below Integration service will 
        /// UPSERT details in MSDynamics (CRM) 
        /// </summary>
        /// <param name="request">Takes CredentialInfo, mQuote, PartnerEntityCode, status </param>
        /// <returns></returns>
        public async Task<ResponseStatus> CreateOpportunity(IntegrationOpportunityReq request)
        {
            ResponseStatus objResponseStatus = new ResponseStatus();
            objResponseStatus = await serviceProxy.PostData(_configuration.GetValue<string>("IntegrationMSDynamics:CreateOpportunity"), request, typeof(ResponseStatus), "IntegrationMSDynamics");
            return objResponseStatus;
        }

        /// <summary>
        /// the below Integration service will 
        /// UPSERT details in MSDynamics (CRM) 
        /// </summary>
        /// <param name="request">Takes CredentialInfo, mQuote, PartnerEntityCode, status </param>
        /// <returns></returns>
        public async Task<ResponseStatus> CreateNewOpportunityInfo(IntegrationOpportunityReq request)
        {
            ResponseStatus objResponseStatus = new ResponseStatus();
            objResponseStatus = await serviceProxy.PostData(_configuration.GetValue<string>("IntegrationMSDynamics:CreateNewOpportunityInfo"), request, typeof(ResponseStatus), "IntegrationMSDynamics");
            return objResponseStatus;
        }

        /// <summary>
        /// the below Integration service will 
        /// UPSERT details in MSDynamics (CRM) 
        /// </summary>
        /// <param name="request">Takes CredentialInfo, mQuote, mQRFPrice, PartnerEntityCode, status </param>
        /// <returns></returns>
        public async Task<ResponseStatus> CreateUpdateQuotation(IntegrationQuotationReq request)
        {
            ResponseStatus objResponseStatus = new ResponseStatus();
            objResponseStatus = await serviceProxy.PostData(_configuration.GetValue<string>("IntegrationMSDynamics:CreateUpdateQuotation"), request, typeof(ResponseStatus), "IntegrationMSDynamics");
            return objResponseStatus;
        }

        /// <summary>
        /// the below Integration service will to update reject Booking Opportunity
        /// UPSERT details in MSDynamics (CRM) 
        /// </summary>
        /// <param name="request">Takes CredentialInfo, mQuote, mQRFPrice, PartnerEntityCode, status </param>
        /// <returns></returns>
        public async Task<ResponseStatus> RejectOpportunityInfo(IntegrationOpportunityReq request)
        {
            ResponseStatus objResponseStatus = new ResponseStatus();
            objResponseStatus = await serviceProxy.PostData(_configuration.GetValue<string>("IntegrationMSDynamics:RejectOpportunityInfo"), request, typeof(ResponseStatus), "IntegrationMSDynamics");
            return objResponseStatus;
        }

        /// <summary>
        /// the below Integration service will 
        /// UPSERT details in MSDynamics (CRM) 
        /// </summary>
        /// <param name="request">Takes CredentialInfo, mQuote, mQRFPrice, PartnerEntityCode, status </param>
        /// <returns></returns>
        public async Task<ResponseStatus> CreateUpdateBooking(IntegrationBookingReq request)
        {
            ResponseStatus objResponseStatus = new ResponseStatus();
            objResponseStatus = await serviceProxy.PostData(_configuration.GetValue<string>("IntegrationMSDynamics:CreateUpdateBooking"), request, typeof(ResponseStatus), "IntegrationMSDynamics");
            return objResponseStatus;
        }

    }
}
