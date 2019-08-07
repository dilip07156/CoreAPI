using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class PositionPriceSetRes
    {
        public PositionPriceSetRes()
        {
            ResponseStatus = new ResponseStatus();
        }
        public ResponseStatus ResponseStatus { get; set; }
        public long PositionPriceId { get; set; }
        public string PositionId { get; set; }
        public string ProductId { get; set; }
        public string PositionName { get; set; }

    }
}
