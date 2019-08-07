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
    [Route("api/PDF")]
    public class PDFController : Controller
    {
        #region Private Variable Declaration
        private readonly IPDFRepository _pdfRepository; 
        #endregion

        public PDFController(IPDFRepository pdfRepository)
        {
            _pdfRepository = pdfRepository; 
        }

        #region PDF
        /// <summary>
        /// Generate PDF and if IsSendMail=true then send mail
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GeneratePDF")]
        public async Task<PDFGetRes> GeneratePDF([FromBody] PDFGetReq request)
        {
            var response = new PDFGetRes();
            try
            {
                if (request != null)
                {
                    response = await _pdfRepository.GeneratePDF(request);
                }
                else
                {
                    response.ResponseStatusMessage.Status = "Failure";
                    response.ResponseStatusMessage.ErrorMessage.Add("Details can not be blank.");
                } 
            }
            catch (Exception ex)
            {
                response.ResponseStatusMessage.Status = "Failure";
                response.ResponseStatusMessage.ErrorMessage.Add("An Error Occurred :- " + ex.Message);
            }
            return response;
        } 
        #endregion 
    }
}