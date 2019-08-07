using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using VGER_DISTRIBUTION.Repositories;
using VGER_WAPI_CLASSES;

namespace VGER_DISTRIBUTION.Controllers
{
    [Produces("application/json")]
    [Route("api/Product")]
    public class ProductController : Controller
    {
        #region Private Variable Declaration
        private readonly IProductRepository _productRepository;
        #endregion

        public ProductController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        /// <summary>
        /// Get Product listing by Search Param
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetProductList")]
        [ProducesResponseType(typeof(ProductListRes), 200)]
        [ProducesResponseType(typeof(void), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetProductList([FromBody] ProductListReq request)
        {
            var response = new ProductListRes();
            try
            {
                if (!ModelState.IsValid)
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Request is not valid";
                    return BadRequest(response.ResponseStatus);
                }
                else
                {
                    if (request != null)
                    {
                        if (request.Country_Id == null)
                        {
                            response.ResponseStatus.Status = "Failure";
                            response.ResponseStatus.ErrorMessage = "Country Id can not be blank.";
                            return BadRequest(response.ResponseStatus);
                        }
                        else if (request.City_Id == null)
                        {

                            Guid Country_Id = Guid.Empty;
                            if ((!Guid.TryParse(request.Country_Id, out Country_Id)))
                            {
                                response.ResponseStatus.Status = "Failure";
                                response.ResponseStatus.ErrorMessage = "Country Id is not valid.";
                                return BadRequest(response.ResponseStatus);
                            }
                            response.ResponseStatus.Status = "Failure";
                            response.ResponseStatus.ErrorMessage = "City Id can not be blank.";
                            return BadRequest(response.ResponseStatus);
                        }
                        else
                        {
                            Guid City_Id = Guid.Empty;
                            if (!Guid.TryParse(request.City_Id, out City_Id))
                            {
                                response.ResponseStatus.Status = "Failure";
                                response.ResponseStatus.ErrorMessage = "City Id is not valid.";
                                return BadRequest(response.ResponseStatus);
                            }
                            else
                            {

                                var result = await _productRepository.GetProductDetailsBySearchCriteria(request);
                                if (result != null && result.Products.Count > 0)
                                {
                                    response.ResponseStatus.Status = "Success";
                                    response.Products = result.Products;
                                }
                                else
                                {
                                    response.ResponseStatus.Status = "Success";
                                    response.ResponseStatus.ErrorMessage = "No Records Found.";
                                    return NotFound(response.ResponseStatus);
                                }
                            }
                        }
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "Product details can not be blank.";
                        return BadRequest(response.ResponseStatus);
                    }
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An error occurs " + ex.Message.ToString();
                return BadRequest(response.ResponseStatus);
            }
            return Ok(response);
        }

        /// <summary>
        /// Get Product Details by Id
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetProductDetail")]
        [ProducesResponseType(typeof(ProductDetailRes), 200)]
        [ProducesResponseType(typeof(void), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(403)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetProductDetail([FromBody] ProductDetailReq request)
        {
            var response = new ProductDetailRes();
            try
            {
                if (!ModelState.IsValid)
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Request is not valid";
                    return BadRequest(response.ResponseStatus);
                }
                else
                {
                    if (request != null)
                    {
                        if (!string.IsNullOrWhiteSpace(request.Product_Id))
                        {
                            Guid Product_Id = Guid.Empty;
                            if (Guid.TryParse(request.Product_Id, out Product_Id))
                            {
                                if (!(Product_Id == Guid.Empty))
                                {
                                    var result = await _productRepository.GetProductDetail(request);
                                    if (result != null)
                                    {
                                        response.ResponseStatus.Status = "Success";
                                        response.Product = result;
                                    }
                                    else
                                    {
                                        response.ResponseStatus.Status = "Failure";
                                        response.ResponseStatus.ErrorMessage = "No Records Found.";
                                        return NotFound(response.ResponseStatus);
                                    }
                                }
                                else
                                {
                                    response.ResponseStatus.Status = "Failure";
                                    response.ResponseStatus.ErrorMessage = "InValid Product Id";
                                    return BadRequest(response.ResponseStatus);
                                }
                            }
                            else
                            {
                                response.ResponseStatus.Status = "Failure";
                                response.ResponseStatus.ErrorMessage = "InValid Product Id";
                                return BadRequest(response.ResponseStatus);
                            }
                        }
                        else
                        {
                            response.ResponseStatus.Status = "Failure";
                            response.ResponseStatus.ErrorMessage = "Product Id cannot be blank";
                            return BadRequest(response.ResponseStatus);
                        }
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "Product Id cannot be blank.";
                        return BadRequest(response.ResponseStatus);
                    }
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An error occurs " + ex.Message.ToString();
                return BadRequest(response.ResponseStatus);
            }
            return Ok(response);
        }
        
    }
}
