using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class ProductSupplierGetRes
    {
        public string ProdId { get; set; }
        public string SupplierName { get; set; }
        public string SupplierId { get; set; }
        public List<SupplierData> SupllierList { get; set; } = new List<SupplierData>();
        public List<ProductContractInfo> ProductContracts { get; set; } = new List<ProductContractInfo>();
        public ResponseStatus ResponseStatus { get; set; } = new ResponseStatus();
    }

    public class SupplierData
    {
        public string ProdId { get; set; }
        public string SupplierName { get; set; }
        public string SupplierId { get; set; }
        public string CurrencyId { get; set; }
        public string Currency { get; set; }
        public string CityName { get; set; }
    } 

    public class ProductSupplierInfo
    {
        public List<ProductSupplier> ProductSupplier = new List<ProductSupplier>();
        public string Product_Id { get; set; }
    }
}