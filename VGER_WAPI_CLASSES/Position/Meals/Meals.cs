using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{ 
    public class MealDetails
    {
        public int DayNo { get; set; }
        public int PositionSequence { get; set; }
        public string DayID { get; set; }
        public string DefaultPlan { get; set; }
        public string RoutingDaysID { get; set; }
        public string RoutingCity { get; set; }

        public string EarlyMorningTeaId { get; set; }
        public string BreakfastId { get; set; }
        public string BrunchId { get; set; }
        public string LunchId { get; set; }
        public string TeaId { get; set; }
        public string DinnerId { get; set; }

        public bool IsEarlyMorningTea { get; set; }
        public bool IsBreakfast { get; set; }
        public bool IsBrunch { get; set; }
        public bool IsLunch { get; set; }
        public bool IsTea { get; set; }
        public bool IsDinner { get; set; }
        public bool IsDeleted { get; set; }

        public string EarlyMorningTea { get; set; }
        public string Breakfast { get; set; }
        public string Brunch { get; set; }
        public string Lunch { get; set; }
        public string Tea { get; set; }
        public string Dinner { get; set; }
    }
}
