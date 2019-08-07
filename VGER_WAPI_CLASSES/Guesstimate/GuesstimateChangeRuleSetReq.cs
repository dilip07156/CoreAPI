using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class GuesstimateChangeRuleSetReq
    {
        public string GuesstimateId { get; set; }
        public string ChangeRule { get; set; }
        public double? ChangeRulePercent { get; set; }
        public string EditUser { get; set; }
    }
}
