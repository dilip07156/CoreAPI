using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class TravelogiCountryCityReq
    {
        public string SourceSupplierCode { get; set; }
        public string SourceSupplierCountryCode { get; set; }
        public string SourceSupplierCityCode { get; set; }
        public string TargetSupplierCode { get; set; }
    }
}
