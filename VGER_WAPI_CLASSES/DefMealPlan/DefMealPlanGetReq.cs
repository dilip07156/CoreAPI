using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    /// <summary>
    /// DefMealPlanGetReq defined taking the status as true/false for fetching the DefMealPlan details
    /// </summary>
    public class DefMealPlanGetReq
    {
        /// <summary>
        /// Status is either true/false
        /// </summary>
        public string Status { get; set; }
    }

    public class DefMealTypeGetReq
    {
        /// <summary>
        /// Status is either true/false
        /// </summary>
        public bool ForBreakfast { get; set; }
    }
}
