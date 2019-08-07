using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VGER_WAPI.Helpers;
using Microsoft.Extensions.Options;
using VGER_WAPI.Models;
using Microsoft.AspNetCore.Authorization;
using VGER_WAPI.Repositories;
using VGER_WAPI_CLASSES;


namespace VGER_WAPI.Controllers
{
	[Produces("application/json")]
	[Route("api/QRFSummary")]
	public class QRFSummaryController : Controller
	{
		#region Private Variable Declaration

		private readonly IQRFSummaryRepository _qrfSummarRepository;
		private readonly MongoContext _MongoContext = null;
        private readonly IMSDynamicsRepository _mSDynamicsRepository;

        #endregion

        public QRFSummaryController(IQRFSummaryRepository qrfSummarRepository, IOptions<MongoSettings> settings, IMSDynamicsRepository mSDynamicsRepository)
        {
            _qrfSummarRepository = qrfSummarRepository;
            _MongoContext = new MongoContext(settings);
            _mSDynamicsRepository = mSDynamicsRepository;
        }

        /// <summary>
        /// Get QRF Summary
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
		[HttpPost]
		[Route("GetQRFSummary")]
		public async Task<QRFSummaryGetRes> GetQRFSummary([FromBody] QRFSummaryGetReq request)
		{
			var response = new QRFSummaryGetRes();
			try
			{
				if (request != null && !string.IsNullOrEmpty(request.QRFID))
				{
					response = await _qrfSummarRepository.GetQRFSummary(request);
				}
				else
				{
					response.ResponseStatus.Status = "Failure";
					response.ResponseStatus.ErrorMessage = "QRF ID can not be Null/Zero.";
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
		/// Update Quote details
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		[Authorize]
		[HttpPost]
		[Route("SubmitQuote")]
		public async Task<QuoteSetRes> SubmitQuote([FromBody] QuoteSetReq request)
		{
			var response = new QuoteSetRes();
			try
			{
				if (request != null)
				{
					response = await _qrfSummarRepository.SubmitQuote(request);
					if (response == null)
						response = new QuoteSetRes() { ResponseStatus = new ResponseStatus() { Status = "Error", ErrorMessage = "Error while Submitting Quote" } };
				}
				else
				{
					response.ResponseStatus.Status = "Failure";
					response.ResponseStatus.ErrorMessage = "Details can not be blank.";
				}
                if (!string.IsNullOrEmpty(response.ResponseStatus.Status) && response.ResponseStatus.Status == "Success" && !string.IsNullOrEmpty(request.QRFID) && !string.IsNullOrEmpty(request.VoyagerUserID))
                {
                    Task.Run(() => _mSDynamicsRepository.CreateUpdateOpportnity(request.QRFID, request.VoyagerUserID).Result);
                }
            }
			catch (Exception ex)
			{
				response.ResponseStatus.Status = "Failure";
				response.ResponseStatus.ErrorMessage = "An Error Occurs " + ex.Message;
			}
			return response;
		}

	}
}