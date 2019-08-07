using System.Collections.Generic;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI_CLASSES
{
    public class ProdTypeGetRes
    {
        public ProdTypeGetRes()
        {
            ProductTypeList = new List<mProductType>();
            ResponseStatus = new ResponseStatus();
        }
        public List<mProductType> ProductTypeList { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
    }
}
