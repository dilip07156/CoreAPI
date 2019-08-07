using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using VGER_WAPI.Repositories;
using VGER_WAPI_CLASSES;
using VGER_WAPI.Helpers;
using Microsoft.Extensions.Options;
using VGER_WAPI.Models;
using MongoDB.Driver;

namespace VGER_WAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/Accomodation")]
    public class AccomodationController : Controller
    {
        #region Private Variable Declaration
        private readonly IAccomodationRepository _accomodationRepository;
        private readonly IConfiguration _configuration;
        private readonly IProductRepository _productRepository;
        private readonly MongoContext _MongoContext = null;
        #endregion

        public AccomodationController(IConfiguration configuration, IAccomodationRepository accomodationRepository,IProductRepository productRepository, IOptions<MongoSettings> settings)
        {
            _configuration = configuration;
            _accomodationRepository = accomodationRepository;
            _productRepository = productRepository;
            _MongoContext = new MongoContext(settings);
        }

        [Authorize]
        [HttpPost]
        [Route("GetAccomodationByQRFID")]
        public async Task<AccomodationGetRes> GetAccomodationByQRFID([FromBody] AccomodationGetReq request)
        {
            var response = new AccomodationGetRes();
            try
            {
                response.QRFID = request.QRFId;
                if (request != null && request.QRFId > 0)
                {
                    List<AccomodationInfo> result = await _accomodationRepository.GetAccomodationByQRFID(request);
                    response.ResponseStatus.Status = "Success";
                    response.ResponseStatus.ErrorMessage = result != null ? "" : "No Records Found.";

                    response.AccomodationInfo = result;
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
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message.ToString();
            }

            return response;
        }

        /// <summary>
        /// Insert /Update Quote Route details
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("InsertUpdateAccomodation")]
        public async Task<AccomodationSetRes> InsertUpdateAccomodation([FromBody] AccomodationSetReq request)
        {
            var response = new AccomodationSetRes();
            try
            {
                response.QRFId = request.QRFID;
                if (request != null)
                {
                    string result = await _accomodationRepository.InsertUpdateAccomodation(request);
                    if (request.SaveType == "full")
                    {
                        response.ResponseStatus.Status = result == "1" ? "Success" : "Failure";
                        response.ResponseStatus.ErrorMessage = result != "1" ? result : "";
                    }
                    else
                    {
                        response.ResponseStatus.Status = result != "" ? "Success" : "Failure";
                        response.ResponseStatus.ErrorMessage = result != "1" ? result : "";
                        response.AccomodationId = result;
                    }
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
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message.ToString();
            }
            return response;
        }


        [Authorize]
        [HttpPost]
        [Route("GetAccomodationRoomAndSupplement")]
        public AccomodationGetRoomTypeAndSuppRes GetAccomodationRoomAndSupplement([FromBody] ProdCategoryRangeGetReq request)
        {
            var response = new AccomodationGetRoomTypeAndSuppRes();
            try
            {
                if (request != null && !string.IsNullOrEmpty(request.ProductId))
                {
                    request.AdditionalYn = false;
                    var resultRoom = _productRepository.GetProductCategoryRangeByProductID(request);
                    if (resultRoom != null && resultRoom.Count > 0)
                    {
                        response.ResponseStatus.Status = "Success";
                        response.RoomTypeList = resultRoom.ToList();
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Success";
                        response.ResponseStatus.ErrorMessage = "No Records Found.";
                    }

                    request.AdditionalYn = true;
                    var resultSupp = _productRepository.GetProductCategoryRangeByProductID(request);
                    if (resultSupp != null && resultSupp.Count > 0)
                    {
                        response.ResponseStatus.Status = "Success";
                        response.SupplementList = resultSupp.ToList();
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Success";
                        response.ResponseStatus.ErrorMessage = "No Records Found.";
                    }

                   response.SupplierId = _MongoContext.mProductSupplier.AsQueryable().Where(p => p.Product_Id == request.ProductId && p.DafaultSupplier == true ).Select(s => s.VoyagerProductSupplier_Id).FirstOrDefault();
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Product Id can not be null/blank.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An error occurs " + ex.Message.ToString();
            }
            return response;
        }
    }
}