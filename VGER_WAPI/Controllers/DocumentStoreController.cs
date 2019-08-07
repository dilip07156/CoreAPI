using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VGER_WAPI.Repositories;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/DocumentStore")]
    public class DocumentStoreController : Controller
    {
        #region Private Variable Declaration
        private readonly IDocumentStoreRepository _documentStoreRepository;
        #endregion

        public DocumentStoreController(IDocumentStoreRepository documentStoreRepository)
        {
            _documentStoreRepository = documentStoreRepository;
        }

        /// <summary>
        /// Get Document Store
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetDocumentStore")]
        public async Task<DocumentStoreGetRes> GetDocumentStore([FromBody] DocumentStoreGetReq request)
        {
            var response = new DocumentStoreGetRes();
            try
            {
                if (request != null)
                {
                    response = await _documentStoreRepository.GetDocumentStore(request);
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Details can not be blank.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }
            return response;
        }       

        /// <summary>
        /// Set Document Store
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("SetDocumentStore")]
        public async Task<DocumentStoreSetRes> SetDocumentStore([FromBody] DocumentStoreSetReq request)
        {
            var response = new DocumentStoreSetRes();
            try
            {
                if (request != null)
                {
                    response = await _documentStoreRepository.SetDocumentStore(request);
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Details can not be blank.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }
            return response;
        }

        /// <summary>
        ///Get Communication Trail
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetCommunicationTrail")]
        public async Task<DocumentStoreInfoGetRes> GetCommunicationTrail([FromBody] DocumentStoreGetReq request)
        {
            var response = new DocumentStoreInfoGetRes();
            try
            {
                if (request != null)
                {
                    response = await _documentStoreRepository.GetCommunicationTrail(request);
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Details can not be blank.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }
            return response;
        }

        /// <summary>
        ///Get Communication Trail By DocumentId
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetCommunicationTrailById")]
        public async Task<DocumentStoreInfo> GetCommunicationTrailById([FromBody] DocumentStoreGetReq request)
        {
            var response = new DocumentStoreInfo();
            try
            {
                if (request != null)
                {
                    response = await _documentStoreRepository.GetCommunicationTrailById(request);
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Details can not be blank.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }
            return response;
        }
    }
}