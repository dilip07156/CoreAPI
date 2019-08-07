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
    [Route("api/Position")]
    public class PositionController : Controller
    {
        #region Private Variable Declaration
        private readonly IPositionRepository _positionRepository;
        private readonly IProductRepository _productRepository;
        private readonly IMSDynamicsRepository _mSDynamicsRepository;
        #endregion

        public PositionController(IPositionRepository positionRepository, IProductRepository productRepository, IMSDynamicsRepository mSDynamicsRepository)
        {
            _positionRepository = positionRepository;
            _productRepository = productRepository;
            _mSDynamicsRepository = mSDynamicsRepository;
        }

        #region Get Set Position
        /// <summary>
        /// Get Position
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetPosition")]
        public async Task<PositionGetRes> GetPosition([FromBody] PositionGetReq request)
        {
            var response = new PositionGetRes();
            try
            {
                if (request != null && !string.IsNullOrEmpty(request.QRFID))
                {
                    response = await _positionRepository.GetPosition(request);
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
        /// Set Position
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("SetPosition")]
        public async Task<PositionSetRes> SetPosition([FromBody] PositionSetReq request)
        {
            var response = new PositionSetRes();
            try
            {
                //response.QRFId = request.QRFID;
                if (request != null)
                {
                    response = await _positionRepository.SetPosition(request);

                    if (!string.IsNullOrEmpty(response.ResponseStatus.Status) && response.ResponseStatus.Status == "Success" && !string.IsNullOrEmpty(request.QRFID) && !string.IsNullOrEmpty(request.VoyagerUserID))
                    {
                       
                        Task.Run(() => _mSDynamicsRepository.CreateUpdateOpportnity(request.QRFID, request.VoyagerUserID).Result);

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
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }

            return response;
        }
        #endregion

        [Authorize]
        [HttpPost]
        [Route("GetPositionRoomAndSupplement")]
        public PositionGetRoomTypeAndSuppRes GetPositionRoomAndSupplement([FromBody] ProdCategoryRangeGetReq request)
        {
            var response = new PositionGetRoomTypeAndSuppRes();
            try
            {
                if (request != null && request.ProductId.Count>0)
                {
                    var resultRoom = _productRepository.GetProductCategoryRangeByProductID(request);
                    if (resultRoom != null && resultRoom.Count > 0)
                    {
                        response.ResponseStatus.Status = "Success";
                        response.RoomTypeList = resultRoom.ToList();
                        
                        var resultDefaultRoom = _productRepository.GetDefaultRoomsFromQuote(request);
                        if (resultDefaultRoom != null && resultDefaultRoom.Count > 0)
                        {
                            response.ResponseStatus.Status = "Success";
                            response.DefaultRoomslist = resultDefaultRoom.ToList();
                        }
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Success";
                        response.ResponseStatus.ErrorMessage = "No Records Found.";
                    }
                    
                    ProductSupplierGetReq objProductSupplierGetReq = new ProductSupplierGetReq();
                    objProductSupplierGetReq.ProductId = request.ProductId[0];
                    ProductSupplierGetRes objProductSupplierGetRes = _productRepository.GetSupplierDetails(objProductSupplierGetReq);
                    if (objProductSupplierGetRes != null)
                    {
                        response.SupplierId = objProductSupplierGetRes.SupplierId;
                        response.SupplierName = objProductSupplierGetRes.SupplierName;
                    }
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
                response.ResponseStatus.ErrorMessage = "An error occurs " + ex.Message;
            }
            return response;
        } 

        #region Prices
        [Authorize]
        [HttpPost]
        [Route("GetPositionPrice")]
        public async Task<PositionPriceGetRes> GetPositionPrice([FromBody] PositionPriceGetReq request)
        {
            var response = new PositionPriceGetRes();
            try
            {
                if (!string.IsNullOrEmpty(request.QRFID))
                {
                    PositionPriceGetRes result = await _positionRepository.GetPositionPrice(request);
                    response.ResponseStatus.Status = "Success";
                    response.ResponseStatus.ErrorMessage = (result != null && result.PositionPrice != null) ? "" : "No Records Found.";
                    response.StandardPrice = result.StandardPrice;
                    response.PositionPrice = result.PositionPrice;
                    response.IsSalesOfficeUser = result.IsSalesOfficeUser;
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
        [Route("SetPositionPrice")]
        public async Task<PositionPriceSetRes> SetPositionPrice([FromBody] PositionPriceSetReq request)
        {
            var response = new PositionPriceSetRes();
            try
            {
                if (request.PositionPrice != null && request.PositionPrice.Count > 0)
                {
                    response = await _positionRepository.SetPositionPrice(request);
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Details can not be blank.";
                }
                if (!string.IsNullOrEmpty(response.ResponseStatus.Status) && response.ResponseStatus.Status == "Success" && !string.IsNullOrEmpty(request.QRFID) && !string.IsNullOrEmpty(request.VoyagerUserID))
                {
                    //Task.Run(async () => _mSDynamicsRepository.CreateOpportunity(request, result).Result);
                    Task.Run(() => _mSDynamicsRepository.CreateUpdateOpportnity(request.QRFID, request.VoyagerUserID).Result);

                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }
            return response;
        }
        #endregion

        #region FOC
        [Authorize]
        [HttpPost]
        [Route("GetPositionFOC")]
        public async Task<PositionFOCGetRes> GetPositionFOC([FromBody] PositionFOCGetReq request)
        {
            var response = new PositionFOCGetRes();
            try
            {
                if (!string.IsNullOrEmpty(request.PositionId))
                {
                    PositionFOCGetRes result = await _positionRepository.GetPositionFOC(request);
                    response.ResponseStatus.Status = "Success";
                    response.ResponseStatus.ErrorMessage = (result != null && result.PositionFOC != null) ? "" : "No Records Found.";
                    response.StandardFOC = result.StandardFOC;
                    response.PositionFOC = result.PositionFOC;
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "PositionId can not be Null/Zero.";
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
        [Route("SetPositionFOC")]
        public async Task<PositionFOCSetRes> SetPositionFOC([FromBody] PositionFOCSetReq request)
        {
            var response = new PositionFOCSetRes();
            try
            {
                if (request.PositionFOC != null && request.PositionFOC.Count > 0)
                {
                    response = await _positionRepository.SetPositionFOC(request);
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
        #endregion

        #region Get Dynamic Tour Entities
        /// <summary>
        /// Get Dynamic TourEntity details
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetDynamicTourEntities")]
        public async Task<TourEntitiesGetRes> GetDynamicTourEntities([FromBody] TourEntitiesGetReq request)
        {
            var response = new TourEntitiesGetRes();
            try
            {
                if (request != null && !string.IsNullOrEmpty(request.QRFID))
                {
                    response = await _positionRepository.GetDynamicTourEntities(request);
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
        #endregion

        #region Get QuickPick Activities
        /// <summary>
        /// Get QuickPick Activities
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetQuickPickActivities")]
        public async Task<PosQuicePickGetRes> GetQuickPickActivities([FromBody] PositionGetReq request)
        {
            var response = new PosQuicePickGetRes();
            try
            {
                if (request != null && request.ProductType != null && !string.IsNullOrEmpty(request.QRFID))
                {
                    response = await _positionRepository.GetQuickPickActivities(request);
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
        #endregion

        #region Set DefaultMealPlan Accomodation
        [Authorize]
        [HttpPost]
        [Route("SetDefaultMealPlan")]
        public async Task<PositionDefMealSetRes> SetDefaultMealPlan([FromBody] PositionDefMealSetReq request)
        {
            var response = new PositionDefMealSetRes();
            try
            {
                if (!string.IsNullOrEmpty(request.QRFID))
                {
                    response = await _positionRepository.SetDefaultMealPlan(request);
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "QRFID can not be Zero.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }
            return response;
        }
        #endregion

        #region PositionRoomDetails
        /// <summary>
        /// Get Position Room Details
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetPositionRoomDetails")]
        public async Task<PositionRoomsGetRes> GetPositionRoomDetails([FromBody] PositionRoomsGetReq request)
        {
            var response = new PositionRoomsGetRes();
            try
            {
                if (request != null && !string.IsNullOrEmpty(request.PositionId) && !string.IsNullOrEmpty(request.QRFId))
                {
                    response = await _positionRepository.GetPositionRoomDetails(request);
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "QRF ID/Position ID can not be Null/Zero.";
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
        /// Set Position Room Details
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("SetPositionRoomDetails")]
        public async Task<PositionRoomsSetRes> SetPositionRoomDetails([FromBody] PositionRoomsSetReq request)
        {
            var response = new PositionRoomsSetRes();
            try
            {
                if (request != null && !string.IsNullOrEmpty(request.PositionId) && !string.IsNullOrEmpty(request.QRFId))
                {
                    response = await _positionRepository.SetPositionRoomDetails(request);
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "QRF ID/Position ID can not be Null/Blank.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }

            return response;
        }
        #endregion
    }
}