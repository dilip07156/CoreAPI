using System.Collections.Generic;

namespace VGER_WAPI_CLASSES
{
    public class ProductWithRateSearchReq
    {
        public string ProdId { get; set; }
        public string ProdCode { get; set; }
        public string ProdType { get; set; }
        public string ProdName { get; set; }
        public bool? IsPlaceHolder { get; set; }
        public string CityName { get; set; }
        public bool ContractPeriod { get; set; }
        public string Status { get; set; }
        public string StarRating { get; set; }
        public string Chain { get; set; }
        public string Location { get; set; }
        public string BudgetCategory { get; set; }
        public List<string> Facilities { get; set; }
    }

    public class ProductWithRateSearchRes
    {
        public ProductWithRateSearchRes()
        {
            ProductWithRate = new List<ProductWithRate>();
            ResponseStatus = new ResponseStatus();
        }
        public List<ProductWithRate> ProductWithRate { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
    }

    public class ProductWithRate
    {
        public string VoyagerProduct_Id { get; set; }
        public string ProductName { get; set; }
        public string CountryName { get; set; }
        public string CityName { get; set; }
        public string DefaultSupplierId { get; set; }
        public string DefaultSupplier { get; set; }
        public List<ProductCategory> ProductCategories { get; set; }
        public List<ProductContractInfo> ProductContracts { get; set; }
    }
}
