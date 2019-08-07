using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class ProdCategoryRangeGetReq
    {
        public string QRFID { get; set; }
        public List<string> ProductId { get; set; }
        public string ProductCategoryId { get; set; }
        public bool AdditionalYn { get; set; }
    }
}
