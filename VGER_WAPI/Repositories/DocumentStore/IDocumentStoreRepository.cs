using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Repositories
{
    public interface IDocumentStoreRepository
    { 
        Task<DocumentStoreGetRes> GetDocumentStore(DocumentStoreGetReq request);

        Task<DocumentStoreSetRes> SetDocumentStore(DocumentStoreSetReq request);

        Task<DocumentStoreInfoGetRes> GetCommunicationTrail(DocumentStoreGetReq request);

        Task<DocumentStoreInfo> GetCommunicationTrailById(DocumentStoreGetReq request);
    }
}
