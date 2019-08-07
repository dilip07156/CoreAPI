using System.Collections.Generic;
using System.Threading.Tasks;
using VGER_WAPI_CLASSES;


namespace VGER_WAPI.Repositories
{
    public interface IProductRepository
    {
        #region Product
        //Task<List<ProductSearchDetails>> GetProductDetailsBySearchCriteriaOldNotInUse(ProductSearchReq request);
        Task<List<ProductSearchDetails>> GetProductDetailsBySearchCriteria(ProductSearchReq request);

        Task<List<ProductWithRate>> GetProductWithRateBySearchCriteria(ProductWithRateSearchReq request);

        Task<List<ProductSearchDetails>> GetProductDetailsByCountryCityProdType(ProductSearchReq request);

        List<ProductRangeDetails> GetProductCategoryRangeByProductID(ProdCategoryRangeGetReq request);

        List<ProdCategoryDetails> GetProductCategoryByParam(ProductCatGetReq request);

        ProductRangeGetRes GetProductRangeByParam(ProductRangeGetReq request);
        
		ProdContractGetRes GetContractRatesByProductID(ProdContractGetReq request, List<string> ranges = null);

        List<ProdCatDefProperties> GetProdCatDefByName(ProdCatDefGetReq request);

        List<ProdCatDefProperties> GetProdCatDef();

        List<ProductAttributeDetails> GetProdAttributeDetailsByNameOrVal(ProductAttributeReq request);

        List<ProdCatDefProperties> GetProdCatDefById(ProdCatDefGetReq request);

        List<ProdCategoryRangeDetails> GetDefaultRoomsFromQuote(ProdCategoryRangeGetReq request);

        ProductSupplierGetRes GetSupplierDetails(ProductSupplierGetReq request);

        ProductSupplierGetRes GetProductSupplierList(ProductSupplierGetReq request);

        List<mProductType> GetProductTypeByProdType(ProdTypeGetReq request);

        ProdNationalityGetRes GetNationalityList(string CompanyId);

        List<mProducts> GetProductsByNames(ProductGetReq request);

        Task<SimilarHotelsGetRes> GetSimilarHotels(SimilarHotelsGetReq request);

        Task<SimilarHotelsSetRes> SetSimilarHotels(SimilarHotelsSetReq request);

        Task<bool> SaveSimilarHotels(string PositionId, string ProductID, string EditUser, bool IsClone);

        Task<MarkupDetails> GetProdMarkups(ProdMarkupsGetReq request);

        Task<ProductContractsGetRes> GetProductContracts(ProductContractsGetReq request);
        #endregion
        
    }
}
