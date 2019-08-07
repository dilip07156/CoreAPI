using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI.Repositories;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Controllers
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

        #region Product
        /// <summary>
        /// Get Product details by Search Param
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetProductDetailsBySearchCriteria")]
        public async Task<ProductSearchRes> GetProductDetailsBySearchCriteria([FromBody] ProductSearchReq request)
        {
            var response = new ProductSearchRes();
            try
            {
                if (request != null)
                {
                    //var result1 = await _productRepository.GetProductDetailsBySearchCriteriaOldNotInUse(request);
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

        /// <summary>
        /// Get Product details with contract rate by Search Param
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetProductWithRateBySearchCriteria")]
        public async Task<ProductWithRateSearchRes> GetProductWithRateBySearchCriteria([FromBody] ProductWithRateSearchReq request)
        {
            var response = new ProductWithRateSearchRes();
            try
            {
                if (request != null)
                {
                    //var result1 = await _productRepository.GetProductDetailsBySearchCriteriaOldNotInUse(request);
                    var result = await _productRepository.GetProductWithRateBySearchCriteria(request);
                    if (result != null && result.Count > 0)
                    {
                        response.ResponseStatus.Status = "Success";
                        response.ProductWithRate = result.ToList();
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
        ///Get Product Category Range By ProductID
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetProductCategoryRangeByProductID")]
        public ProdCategoryRangeGetRes GetProductCategoryRangeByProductID([FromBody] ProdCategoryRangeGetReq request)
        {
            var response = new ProdCategoryRangeGetRes();
            try
            {
                if (request != null && request.ProductId.Count > 0)
                {
                    var result = _productRepository.GetProductCategoryRangeByProductID(request);
                    if (result != null && result.Count > 0)
                    {
                        response.ResponseStatus.Status = "Success";
                        response.ProdCategoryRangeDetails = result.ToList();
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

        /// <summary>
        ///Get Product Category By Param
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetProductCategoryByParam")]
        public ProductCatGetRes GetProductCategoryByParam([FromBody] ProductCatGetReq request)
        {
            var response = new ProductCatGetRes();
            try
            {
                if (request != null)
                {
                    var result = _productRepository.GetProductCategoryByParam(request);
                    if (result != null && result.Count > 0)
                    {
                        response.ResponseStatus.Status = "Success";
                        response.ProdCategoryDetails = result.ToList();
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
                    response.ResponseStatus.ErrorMessage = "Request Details can not be null/blank.";
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
        ///Get Product Range By Param
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetProductRangeByParam")]
        public ProductRangeGetRes GetProductRangeByParam([FromBody] ProductRangeGetReq request)
        {
            var response = new ProductRangeGetRes();
            try
            {
                if (request != null)
                {
                    var result = _productRepository.GetProductRangeByParam(request);
                    if (result != null && result.ProductRangeDetails != null && result.ProductRangeDetails.Count > 0)
                    {
                        response.ResponseStatus.Status = "Success";
                        response.ProductRangeDetails = result.ProductRangeDetails.ToList();

                        if (result.DefProdRangelist != null && result.DefProdRangelist.Count > 0) response.DefProdRangelist = result.DefProdRangelist.ToList();

                        if (!string.IsNullOrEmpty(request.ProductId))
                        {
                            ProductSupplierGetReq objProductSupplierGetReq = new ProductSupplierGetReq { ProductId = request.ProductId };
                            ProductSupplierGetRes objProductSupplierGetRes = _productRepository.GetSupplierDetails(objProductSupplierGetReq);
                            if (objProductSupplierGetRes != null)
                            {
                                response.SupplierId = objProductSupplierGetRes.SupplierId;
                                response.SupplierName = objProductSupplierGetRes.SupplierName;
                            }
                        }
                    }
                    else
                    {
                        response.ResponseStatus.Status = "Success";
                        response.ResponseStatus.ErrorMessage = "No Records Found.";
                        response.ProductRangeDetails = new List<ProductRangeDetails>();
                        response.DefProdRangelist = new List<ProductRangeDetails>();
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Request details can not be null/blank.";
                    response.ProductRangeDetails = new List<ProductRangeDetails>();
                }
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An error occurs " + ex.Message;
                response.ProductRangeDetails = new List<ProductRangeDetails>();
            }
            response.ProductCatId = request.ProductCatId;
            response.ProductId = request.ProductId;
            return response;
        }

        /// <summary>
        ///Get Contract Rates By ProductID
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetContractRatesByProductID")]
        public ProdContractGetRes GetContractRatesByProductID([FromBody] ProdContractGetReq request)
        {
            var response = new ProdContractGetRes();
            try
            {
                if (request != null && request.ProductIDList != null && request.ProductIDList.Count > 0)
                {
                    response = _productRepository.GetContractRatesByProductID(request);
                    if (response != null)
                    {
                        response.ResponseStatus.Status = "Success";
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

        /// <summary>
        ///Get Product Cat Def By Name
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetProdCatDefByName")]
        public ProdCatDefGetRes GetProdCatDefByName([FromBody] ProdCatDefGetReq request)
        {
            var response = new ProdCatDefGetRes();
            try
            {
                if (request != null)
                {
                    var result = _productRepository.GetProdCatDefByName(request);
                    if (result != null && result.Count > 0)
                    {
                        response.ResponseStatus.Status = "Success";
                        response.ProdCatDefProperties = result;
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
                    response.ResponseStatus.ErrorMessage = "Request can not be null.";
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
        ///Get Product Cat Def
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetProdCatDef")]
        public ProdCatDefGetRes GetProdCatDef()
        {
            var response = new ProdCatDefGetRes();
            try
            {
                var result = _productRepository.GetProdCatDef();
                if (result != null && result.Count > 0)
                {
                    response.ResponseStatus.Status = "Success";
                    response.ProdCatDefProperties = result;
                }
                else
                {
                    response.ResponseStatus.Status = "Success";
                    response.ResponseStatus.ErrorMessage = "No Records Found.";
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
        ///Get Product Attribute Details By Name or Val
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetProdAttributeDetailsByNameOrVal")]
        public ProductAttributeRes GetProdAttributeDetailsByNameOrVal([FromBody] ProductAttributeReq request)
        {
            var response = new ProductAttributeRes();
            try
            {
                if (request != null)
                {
                    var result = _productRepository.GetProdAttributeDetailsByNameOrVal(request);
                    if (result != null && result.Count > 0)
                    {
                        response.ResponseStatus.Status = "Success";
                        response.ProductAttributeDetails = result;
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
                    response.ResponseStatus.ErrorMessage = "Request can not be null.";
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
        /// Get Product Types By Name
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetProductTypeByProdType")]
        public ProdTypeGetRes GetProductTypeByProdType([FromBody] ProdTypeGetReq request)
        {
            var response = new ProdTypeGetRes();
            try
            {
                if (request != null)
                {
                    var result = _productRepository.GetProductTypeByProdType(request);
                    if (result != null && result.Count > 0)
                    {
                        response.ResponseStatus.Status = "Success";
                        response.ProductTypeList = result;
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
                    response.ResponseStatus.ErrorMessage = "Request can not be null.";
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
        /// Get Nationality List
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetNationalityList")]
        public ProdNationalityGetRes GetNationalityList([FromBody]string CompanyId)
        {
            var response = new ProdNationalityGetRes();
            try
            {
                var result = _productRepository.GetNationalityList(CompanyId);
                if (result?.NationalityList?.Count > 0)
                {
                    response.ResponseStatus.Status = "Success";
                    response.NationalityList = result.NationalityList;
                    response.CompanyNationality = result.CompanyNationality;
                }
                else
                {
                    response.ResponseStatus.Status = "Success";
                    response.ResponseStatus.ErrorMessage = "No Records Found.";
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
        /// Get Product Supplier List
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetProductSupplierList")]
        public ProductSupplierGetRes GetProductSupplierList([FromBody] ProductSupplierGetReq request)
        {
            var response = new ProductSupplierGetRes();
            try
            {
                var result = _productRepository.GetProductSupplierList(request);
                if (result != null && result.SupllierList.Count > 0)
                {
                    response.SupllierList = result.SupllierList;
                    response.ResponseStatus.Status = "Success";
                }
                else
                {
                    response.ResponseStatus.Status = "Success";
                    response.ResponseStatus.ErrorMessage = "No Records Found.";
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
        /// Get Similar Hotels details by Search Param
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetSimilarHotels")]
        public async Task<SimilarHotelsGetRes> GetSimilarHotels([FromBody] SimilarHotelsGetReq request)
        {
            var response = new SimilarHotelsGetRes();
            try
            {
                if (request != null)
                {
                    response = await _productRepository.GetSimilarHotels(request);
                    if (response.SelectedHotelList.Count > 0 || response.BlackListedHotelList.Count > 0)
                    {
                        response.ResponseStatus.Status = "Success";
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
                    response.ResponseStatus.ErrorMessage = "Request details can not be blank.";
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
        /// Set Alternate Hotels in Position
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("SetSimilarHotels")]
        public async Task<SimilarHotelsSetRes> SetSimilarHotels([FromBody] SimilarHotelsSetReq request)
        {
            var response = new SimilarHotelsSetRes();
            try
            {
                if (request != null)
                {
                    if (!string.IsNullOrEmpty(request.PositionId))
                    {
                        response = await _productRepository.SetSimilarHotels(request);

                    }
                    else
                    {
                        response.ResponseStatus.Status = "Failure";
                        response.ResponseStatus.ErrorMessage = "Request details can not be blank.";
                    }
                }
                else
                {
                    response.ResponseStatus.Status = "Failure";
                    response.ResponseStatus.ErrorMessage = "Request details can not be blank.";
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
        /// Get Markups by Product Type
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetProdMarkups")]
        public async Task<ProdMarkupsGetRes> GetProdMarkups([FromBody] ProdMarkupsGetReq request)
        {
            var response = new ProdMarkupsGetRes();
            try
            {
                if (request != null)
                {
                    response.MarkupDetails = await _productRepository.GetProdMarkups(request);
                    if (response.MarkupDetails != null)
                    {
                        if (!string.IsNullOrEmpty(response.MarkupDetails.MarkUpDetail_Id))
                        {
                            response.ResponseStatus.Status = "Success";
                        }
                        else
                        {
                            response.ResponseStatus.Status = "Success";
                            response.ResponseStatus.ErrorMessage = "No Records Found.";
                        }
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
                    response.ResponseStatus.ErrorMessage = "Request details can not be blank.";
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
        /// Get Markups by Product Type
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        [Route("GetProductContracts")]
        public async Task<ProductContractsGetRes> GetProductContracts([FromBody] ProductContractsGetReq request)
        {
            var response = new ProductContractsGetRes();
            try
            {
                if (request != null)
                {
                    response = await _productRepository.GetProductContracts(request);
                    if (response?.ProductContract != null)
                    {
                        response.ResponseStatus.Status = "Success";
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
                    response.ResponseStatus.ErrorMessage = "Request details can not be blank.";
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