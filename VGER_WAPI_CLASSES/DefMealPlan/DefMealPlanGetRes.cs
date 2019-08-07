using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    /// <summary>
    /// DefMealPlanGetRes defined for sending the details of mDefMealPlan
    /// </summary>
    public class DefMealPlanGetRes
    {
        /// <summary>
        /// conatins the name of MealPlan and its status
        /// </summary>
        public List<mDefMealPlan> mDefMealPlan { get; set; } = new List<mDefMealPlan>();

        /// <summary>
        /// ResponseStatus maintains the status and ErroMessage of Set Response
        /// </summary>
        public ResponseStatus ResponseStatus { get; set; } = new ResponseStatus();
    }
}
