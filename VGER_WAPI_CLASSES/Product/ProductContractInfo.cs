using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class ProductContractInfo
    {
        public string ProductId { get; set; }
        public string SupplierId { get; set; }
        public string ProductCategoryId { get; set; }
        public string ProductRangeId { get; set; }
        public string DayComboPattern { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Price { get; set; }
        public string CurrencyId { get; set; }
        public string Currency { get; set; }
        public string ContractId { get; set; }
    }
}
