using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class MealSetReq
    {
        public MealSetReq()
        {
            MealDays = new List<MealDays>();
        }
        public string QRFID { get; set; }
        public string UserName { get; set; }
        public string Flag { get; set; }
        public List<MealDays> MealDays { get; set; }
        public string VoyagerUserId { get; set; }
    }
}
