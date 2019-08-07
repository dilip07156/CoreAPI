using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using VGER_WAPI.Repositories;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/ProductPDP")]
    public class ProductPDPController : Controller
    {
        private readonly IProductPDPRepository _productRepository;

        public ProductPDPController(IProductPDPRepository productRepository)
        {
            _productRepository = productRepository;
        }

        #region Product
        /// <summary>
        /// Get Product Full Details By Product Id
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetProductFullDetailsById")]
        public async Task<ProductPDPSearchRes> GetProductFullDetailsById([FromBody] List<string> request)
        {
            var response = new ProductPDPSearchRes();
            try
            {
                if (request != null)
                {
                    var result = await _productRepository.GetProductFullDetailsById(request);
                    if (result != null)
                    {
                        response.ResponseStatus.Status = "Success";
                        response.ProductDetails = result;
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Success";
                        response.ResponseStatus.ErrorMessage = "No Records Found.";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Product details can not be blank.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An error occurs " + ex.Message;
            }
            return response;
        }

        /// <summary>
        /// Get Product SRP Hotel Details
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetProductSRPHotelDetails")]//GetProductSRPHotelDetails
        public async Task<ProductSRPHotelGetRes> GetProductSRPHotelDetails([FromBody] ProductSRPHotelGetReq request)
        {
            var response = new ProductSRPHotelGetRes();
            try
            {
                if (request != null)
                {
                    response = await _productRepository.GetProductSRPHotelDetails(request);
                    if (response == null)
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "An error.";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Product details can not be blank.";
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An error occurs " + ex.Message;
            }
            return response;
        }

        #endregion

    }
}