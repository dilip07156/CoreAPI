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
    [Route("api/ProductSRP")]
    public class ProductSRPController : Controller
    {
        private readonly IProductSRPRepository _productRepository;

        public ProductSRPController(IProductSRPRepository productRepository)
        {
            _productRepository = productRepository;
        }

        #region Product SRP
        /// <summary>
        /// Get Product details by Search Param
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetProductDetailsBySearchCriteria")]
        public async Task<ProductSRPSearchRes> GetProductDetailsBySearchCriteria([FromBody] ProductSRPSearchReq request)
        {
            var response = new ProductSRPSearchRes();
            try
            {
                if (request != null)
                {
                    var result = await _productRepository.GetProductDetailsBySearchCriteria(request);
                    if (result != null && result.Count > 0)
                    {
                        response.ResponseStatus.Status = "Success";
                        response.ProductSearchDetails = result.ToList();
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

        #endregion
    }
}