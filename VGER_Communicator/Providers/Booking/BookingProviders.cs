using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using VGER_Communicator.Proxy;
using VGER_WAPI_CLASSES;


namespace VGER_Communicator.Providers
{
    public class BookingProviders
    {
        private readonly IConfiguration _configuration;
        ServiceProxy serviceProxy;

        #region Initializers

        public BookingProviders()
        {
        }

        public BookingProviders(IConfiguration configuration)
        {
            _configuration = configuration;
            serviceProxy = new ServiceProxy(_configuration);
        }

        #endregion

        #region Update Positions

        public async Task<UpdateOperationDetails_RS> UpdateOperationDetails(UpdateOperationDetails_RQ objUpdBookingRQ)
        {
            UpdateOperationDetails_RS objUpdBookingRS = new UpdateOperationDetails_RS();
            objUpdBookingRS = await serviceProxy.PostData(_configuration.GetValue<string>("ServiceUpdate:UpdateOperationDetails"), objUpdBookingRQ, typeof(UpdateOperationDetails_RS));
            return objUpdBookingRS;
        }

        public async Task<UpdatePurchaseDetails_RS> UpdatePurchaseDetails(UpdatePurchaseDetails_RQ objUpdBookingRQ)
        {
            UpdatePurchaseDetails_RS objUpdBookingRS = new UpdatePurchaseDetails_RS();
            objUpdBookingRS = await serviceProxy.PostData(_configuration.GetValue<string>("ServiceUpdate:UpdatePurchaseDetails"), objUpdBookingRQ, typeof(UpdatePurchaseDetails_RS));
            return objUpdBookingRS;
        }

        public async Task<UpdatePositionProduct_RS> UpdatePositionProduct(UpdatePositionProduct_RQ objUpdBookingRQ)
        {
            UpdatePositionProduct_RS objUpdBookingRS = new UpdatePositionProduct_RS();
            objUpdBookingRS = await serviceProxy.PostData(_configuration.GetValue<string>("ServiceUpdate:UpdatePositionProduct"), objUpdBookingRQ, typeof(UpdatePositionProduct_RS));
            return objUpdBookingRS;
        }

        #endregion
    }
}
