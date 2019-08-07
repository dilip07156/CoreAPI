using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI.Proxy;
using VGER_WAPI_CLASSES;
namespace VGER_WAPI.Providers
{
    public class DocumentProviders
    {
        private readonly IConfiguration _configuration;
        ServiceProxy serviceProxy;

        #region Initializers 
        public DocumentProviders(IConfiguration configuration)
        {
            _configuration = configuration;
            serviceProxy = new ServiceProxy(_configuration);
        }
        #endregion

        #region Bridge Services
        /// <summary>
        /// Insert/Update mDocumentStore collection in SQL Documents and communicationslog table
        /// </summary>
        /// <param name="objDocumentStoreGetReq">Takes DocumentId ,DocumentType , QRF,Booking , Position, ARH,Supplier , Client ,SendStatus</param>
        /// <returns></returns>
        public async Task<ResponseStatus> SetDocumentsAndCommuncationsLogDetails(DocumentStoreGetReq objDocumentStoreGetReq)
        {
            ResponseStatus objResponseStatus = new ResponseStatus();
            objResponseStatus = await serviceProxy.PostData(_configuration.GetValue<string>("Bridge:Bookings:SetDocumentsAndCommuncationsLogDetails"), objDocumentStoreGetReq, typeof(ResponseStatus), "Bridge");
            return objResponseStatus;
        } 
        #endregion
    }
}
