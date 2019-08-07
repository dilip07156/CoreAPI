using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VGER_WAPI.Helpers;
using VGER_WAPI.Repositories;
using Microsoft.Extensions.Options;
using VGER_WAPI.Models;
using Microsoft.AspNetCore.Authorization;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/Costsheet")]
    public class CostsheetController : Controller
    {
        #region Private Variable Declaration
        private readonly ICostsheetRepository _costSheetRepository;
        private readonly IQRFSummaryRepository _qRFSummaryRepository;
        private readonly MongoContext _MongoContext = null;
        #endregion

        public CostsheetController(ICostsheetRepository costSheetRepository, IQRFSummaryRepository qRFSummaryRepository, IOptions<MongoSettings> settings)
        {
            _costSheetRepository = costSheetRepository;
            _qRFSummaryRepository = qRFSummaryRepository;
            _MongoContext = new MongoContext(settings);
        }

        [Authorize]
        [HttpPost]
        [Route("GetCostsheetData")]
        public async Task<CostsheetGetRes> GetCostsheetData([FromBody] CostsheetGetReq request)
        {
            var response = new CostsheetGetRes();
            try
            {
                if (!string.IsNullOrEmpty(request.QRFID) && request != null)
                {
                    response = _costSheetRepository.GetCostsheetData(request);

                    //List<mQRFPackagePrice> pkgprice = await _costSheetRepository.GetlstQrfPackagePrice(request);
                    //List<mQRFNonPackagedPrice> nonpkgprice = await _costSheetRepository.GetlstQrfNonPackagePrice(request);
                    //List<mQRFPositionTotalCost> positionTotalCost = await _costSheetRepository.GetlstQrfPositionTotalCost(request);

                    //response.QrfPackagePrice = pkgprice;
                    //response.QrfNonPackagePrice = nonpkgprice;
                    //response.QRFPositionTotalCost = positionTotalCost;

                    response.ResponseStatus.Status = "Success";
                    response.ResponseStatus.ErrorMessage = response != null ? "" : "No Records Found.";
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "QRFId can not be Null/Zero.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }

            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("SetCostsheetNewVersion")]
        public async Task<CostsheetVerSetRes> SetCostsheetNewVersion([FromBody] CostsheetVerSetReq request)
        {
            var response = new CostsheetVerSetRes();
            try
            {
                if (!string.IsNullOrEmpty(request.QRFID) && request != null)
                {
                    if (request.Pipeline == "Guesstimate")
                    {
                        response.QRFPrice_Id = await _qRFSummaryRepository.SaveQRFPrice("Guesstimate", "Regenerated from Guesstimate", request.QRFID, request.Create_User); 

                        response.ResponseStatus.Status = !String.IsNullOrEmpty(response.QRFPrice_Id) ? "Success" : "Failure";
                        response.ResponseStatus.ErrorMessage = !String.IsNullOrEmpty(response.QRFPrice_Id) ? "Costsheet Data Saved Successfully." : "Details not updated.";
                    }
                    else if (request.Pipeline == "Amendment")
                    {
                        await _qRFSummaryRepository.SaveDefaultGuesstimate(request.QRFID, "Amendment", "Regenerated from Amendment", request.Create_User);
                        response.QRFPrice_Id = await _qRFSummaryRepository.SaveQRFPrice("Amendment", "Regenerated from Amendment", request.QRFID, request.Create_User);
                        await _qRFSummaryRepository.SaveDefaultItinerary(request.Create_User, request.QRFID,"",true);
                        response.ResponseStatus.Status = !String.IsNullOrEmpty(response.QRFPrice_Id) ? "Success" : "Failure";
                        response.ResponseStatus.ErrorMessage = !String.IsNullOrEmpty(response.QRFPrice_Id) ? "Costsheet Data Saved Successfully." : "Details not updated.";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "QRFId can not be Null/Zero.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }

            return response;
        }


        [Authorize]
        [HttpPost]
        [Route("GetCostsheetVersions")]
        public CostsheetVersionGetRes GetCostsheetVersions([FromBody] CostsheetGetReq request)
        {
            var response = new CostsheetVersionGetRes();
            try
            {
                if (!string.IsNullOrEmpty(request.QRFID))
                {
                    List<CostsheetVersion> result = _costSheetRepository.GetCostsheetVersions(request);
                    response.ResponseStatus.Status = "Success";
                    response.ResponseStatus.ErrorMessage = result != null ? "" : "No Records Found.";

                    response.CostsheetVersions = result;
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "QRFId can not be Null/Zero.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }

            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("UpdateCostsheetVersion")]
        public async Task<CostsheetVerSetRes> UpdateCostsheetVersion([FromBody] CostsheetVerSetReq request)
        {
            var response = new CostsheetVerSetRes();
            try
            {
                if (!string.IsNullOrEmpty(request.QRFID))
                {
                    response = await _costSheetRepository.UpdateCostsheetVersion(request);
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

        [Authorize]
        [HttpPost]
        [Route("CheckActiveCostsheetPrice")]
        public async Task<ResponseStatus> CheckActiveCostsheetPrice([FromBody] CostsheetGetReq request)
        {
            var response = new ResponseStatus();
            try
            {
                if (!string.IsNullOrEmpty(request.QRFID))
                {
                    response = await _costSheetRepository.CheckActiveCostsheetPrice(request);
                }
                else
                {
                    response.Status = "Failure";
                    response.ErrorMessage = "QRFID can not be blank/Null.";
                }
            }
            catch (Exception ex)
            {
                response.Status = "Failure";
                response.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }
            return response;
        }

        ///// <summary>
        ///// Set Email Details and save into mQRFPrice collection
        ///// </summary>
        ///// <param name="request"></param>
        ///// <returns></returns>
        //[Authorize]
        //[HttpPost]
        //[Route("SetEmailDetails")]
        //public async Task<MailSetRes> SetEmailDetails([FromBody] MailSetReq request)
        //{
        //    var response = new MailSetRes();
        //    try
        //    {
        //        if (request != null && !string.IsNullOrEmpty(request.QRFID))
        //        {
        //            response = await _costSheetRepository.SetEmailDetails(request);
        //        }
        //        else
        //        {
        //            response.ResponseStatus.Status = "Failure";
        //            response.ResponseStatus.ErrorMessage = "QRF ID can not be Null/Zero.";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        response.ResponseStatus.Status = "Failure";
        //        response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
        //    }

        //    return response;
        //}
    }
}