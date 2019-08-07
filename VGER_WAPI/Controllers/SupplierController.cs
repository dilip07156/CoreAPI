using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VGER_WAPI.Repositories;
using VGER_WAPI.Helpers;
using Microsoft.Extensions.Options;
using VGER_WAPI.Models;
using Microsoft.AspNetCore.Authorization;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Controllers
{
    [Produces("application/json")]
    [Route("api/Supplier")]
    public class SupplierController : Controller
    {
        #region Private Variable Declaration
        private readonly ISupplierRepository _supplierRepository;
        private readonly IUserRepository _userRepository;
        private readonly MongoContext _MongoContext = null;
        #endregion

        public SupplierController(ISupplierRepository supplierRepository, IUserRepository userRepository, IOptions<MongoSettings> settings)
        {
            _supplierRepository = supplierRepository;
            _userRepository = userRepository;
            _MongoContext = new MongoContext(settings);
        }

        [Authorize]
        [HttpPost]
        [Route("GetSupplierData")]
        public async Task<SupplierGetRes> GetSupplierData([FromBody] SupplierGetReq request)
        {
            var response = new SupplierGetRes();
            try
            {
                response = await _supplierRepository.GetSupplierData(request);
                response.ResponseStatus.Status = "Success";
                response.ResponseStatus.ErrorMessage = response != null ? "" : "No Records Found.";
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
        [Route("GetNoOfBookingsForSupplier")]
        public async Task<SupplierGetRes> GetNoOfBookingsForSupplier([FromBody] SupplierGetReq request)
        {
            var response = new SupplierGetRes();
            try
            {
                response = await _supplierRepository.GetNoOfBookingsForSupplier(request);
                response.ResponseStatus.Status = "Success";
                response.ResponseStatus.ErrorMessage = response != null ? "" : "No Records Found.";
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
        [Route("EnableDisableSupplierProduct")]
        public async Task<SupplierGetRes> EnableDisableSupplierProduct([FromBody] SupplierGetReq request)
        {
            var response = new SupplierGetRes();
            try
            {
                response = await _supplierRepository.EnableDisableSupplierProduct(request);
                response.ResponseStatus.Status = response.ResponseStatus.Status == null ? "Success" : response.ResponseStatus.Status;
                response.ResponseStatus.ErrorMessage = response.ResponseStatus.ErrorMessage == null ? "" : response.ResponseStatus.ErrorMessage;
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Method to get all product supplier sales markets from mProductSupplierSalesMkt collection
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("GetProductSupplierSalesMkt")]
        public List<ProductSupplierSalesMarket> GetProductSupplierSalesMkt()
        {
            var response = new List<ProductSupplierSalesMarket>();
            response = _supplierRepository.GetProductSupplierSalesMkt();
            return response;
        }

        /// <summary>
        /// Method to get all product supplier sales markets from mProductSupplierSalesMkt collection
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("GetProductSupplierOperatingMkt")]
        public List<ProductSupplierOperatingMarket> GetProductSupplierOperatingMkt()
        {
            var response = new List<ProductSupplierOperatingMarket>();
            response = _supplierRepository.GetProductSupplierOperatingMkt();
            return response;
        }

        [Authorize]
        [HttpPost]
        [Route("SetSupplierProduct")]
        public async Task<SupplierSetRes> SetSupplierProduct([FromBody] SupplierSetReq request)
        {
            var response = new SupplierSetRes();
            try
            {
                response = await _supplierRepository.SetSupplierProduct(request);
                response.ResponseStatus.Status = "Success";
                response.ResponseStatus.ErrorMessage = response != null ? "" : "No Records Found.";
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Method to get business regions
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [Route("GetBusinessRegions")]
        public List<mBusinessRegions> GetBusinessRegions()
        {
            var response = new List<mBusinessRegions>();
            response = _supplierRepository.GetBusinessRegions();
            return response;
        }

		/// <summary>
		/// Method to get application list
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		[Authorize]
		[Route("GetApplications")]
		public List<mApplications> GetApplications()
		{
			var response = new List<mApplications>();
			response = _supplierRepository.GetApplications();
			return response;
		}

		/// <summary>
		/// Method to get supplier mapping list
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		[Authorize]
		[Route("GetSupplierMappings")]
		public List<Mappings> GetSupplierMappings([FromBody] SupplierGetReq request)
		{
			var response = new List<Mappings>();
			response = _supplierRepository.GetSupplierMappings(request);
			return response;
		}

		/// <summary>
		/// Method to set supplier mapping details
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		[Authorize]
		[HttpPost]
		[Route("SetSupplierMapping")]
		public async Task<SupplierSetRes> SetSupplierMapping([FromBody] SupplierSetReq request)
		{
			var response = new SupplierSetRes();
			try
			{
				response = await _supplierRepository.SetSupplierMapping(request);
				response.ResponseStatus.Status = "Success";
				response.ResponseStatus.ErrorMessage = response != null ? "" : "No Records Found.";
			}
			catch (Exception ex)
			{
				response.ResponseStatus.Status = "Failure";
				response.ResponseStatus.ErrorMessage = "An Error Occurs :- " + ex.Message;
			}

			return response;
		}

		/// <summary>
		/// Method to get Tax Reg Details
		/// </summary>
		/// <returns></returns>
		[HttpPost]
        [Authorize]
        [Route("GetTaxRegestrationDetails")]
        public async Task<TaxRegestrationDetails_Res> GetTaxRegestrationDetails([FromBody]TaxRegestrationDetails_Req request)
        {
            var response = new TaxRegestrationDetails_Res();
            response = await _supplierRepository.GetTaxRegestrationDetails(request);
            return response;
        }
    }
}