using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class PositionDefMealSetRes
    {
        public PositionDefMealSetRes()
        {
            ResponseStatus = new ResponseStatus();
        }
        public ResponseStatus ResponseStatus { get; set; }
        public string QRFId { get; set; }
        public string ProductType { get; set; }
        public string MealType { get; set; }
    }
}
