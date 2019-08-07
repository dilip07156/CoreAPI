using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class TravelogiCountryCityRes
    {
        public string SourceSupplierCode { get; set; }
        public string SourceSupplierCountryCode { get; set; }
        public string SourceSupplierCityCode { get; set; }
        public string TargetSupplierCode { get; set; }
        public string TargetSupplierCityCode { get; set; }
        public string TargetSupplierCityName { get; set; }
        public string TargetSupplierCountryCode { get; set; }
        public string TargetSupplierCountryName { get; set; }
        public string Status { get; set; }
    }
}
