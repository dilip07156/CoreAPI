using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class DefProductType
    {
        public string VoyagerProductTypeId { get; set; }
        public string ProductTypeId { get; set; }
        public string ProductType { get; set; }
        public string ChargeBy { get; set; }
        public string ChargeByDesc { get; set; }
    }

    public class DefProductTypeRes
    {

        public List<DefProductType> DefProductType { get; set; }
        public ResponseStatus ResponseStatus { get; set; }

        public DefProductTypeRes()
        {
            DefProductType = new List<DefProductType>();
            ResponseStatus = new ResponseStatus();
        }

    }

    public class DefChargeBasis
    {
        public string ChargeBy { get; set; }
        public string ChargeByDesc { get; set; }
    }

    public class DefChargeBasisRes
    {
        public List<DefChargeBasis> DefChargeBasis { get; set; }
        public ResponseStatus ResponseStatus { get; set; }

        public DefChargeBasisRes()
        {
            DefChargeBasis = new List<DefChargeBasis>();
            ResponseStatus = new ResponseStatus();
        }
    }
}
