using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Repositories
{
    public interface ISupplierRepository
    {
		/// <summary>
		/// To get all suppliers for search screen based on filters
		/// </summary>
		/// <param name="request">SupplierGetReq</param>
		/// <returns>List of Suppliers</returns>
		Task<SupplierGetRes> GetSupplierData(SupplierGetReq request);

		/// <summary>
		/// To calculate number of bookings for each supplier
		/// </summary>
		/// <param name="request"></param>
		/// <returns>Booking Count</returns>
		Task<SupplierGetRes> GetNoOfBookingsForSupplier(SupplierGetReq request);

		/// <summary>
		/// Insert/Update method to set Products array in mCompanies
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		Task<SupplierSetRes> SetSupplierProduct(SupplierSetReq request);

		/// <summary>
		/// To make Supplier product active or inactive 
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
        Task<SupplierGetRes> EnableDisableSupplierProduct(SupplierGetReq request);

		/// <summary>
		/// To get all Sales Market list
		/// </summary>
		/// <returns></returns>
        List<ProductSupplierSalesMarket> GetProductSupplierSalesMkt();

		/// <summary>
		/// To get all Operationg Market list
		/// </summary>
		/// <returns></returns>
        List<ProductSupplierOperatingMarket> GetProductSupplierOperatingMkt();

		/// <summary>
		/// To get all Business regions
		/// </summary>
		/// <returns>List of Business Regions</returns>
        List<mBusinessRegions> GetBusinessRegions();

		/// <summary>
		/// To get all Applications list from mApplications collection
		/// </summary>
		/// <returns>List of Applications</returns>
		List<mApplications> GetApplications();

		/// <summary>
		/// To get all Mappings from company using CompanyId
		/// </summary>
		/// <param name="request">Supplier Id</param>
		/// <returns>List of Mappings</returns>
		List<Mappings> GetSupplierMappings(SupplierGetReq request);

		/// <summary>
		/// To insert/update Mappings in mCompanies using CompanyId
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		Task<SupplierSetRes> SetSupplierMapping(SupplierSetReq request);

		Task<TaxRegestrationDetails_Res> GetTaxRegestrationDetails(TaxRegestrationDetails_Req request);
    }
}
