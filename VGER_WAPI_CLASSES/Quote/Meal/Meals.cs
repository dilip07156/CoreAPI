using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class Meals
    {
        public Meals()
        {
            MealDays = new List<MealDays>();
        }
        public List<MealDays> MealDays { get; set; }
        public string CreateUser { get; set; } 
        public DateTime? CreateDate { get; set; } = new DateTime();
        public string EditUser { get; set; } 
        public DateTime? EditDate { get; set; }
    }

    public class MealDays
    {
        public MealDays()
        {
            MealDayInfo = new List<MealDayInfo>();
        }
        public string DayName { get; set; }
        public string RoutingDaysID { get; set; }
        public List<MealDayInfo> MealDayInfo { get; set; }
    }

    public class MealDayInfo
    {
        public string MealType { get; set; }
        public string StartTime { get; set; }
        public string PositionID { get; set; }
        public string ProductID { get; set; }
        public string Address { get; set; }
        public string FullAddress { get; set; }
        public string Telephone { get; set; }
        public string Mail { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}
