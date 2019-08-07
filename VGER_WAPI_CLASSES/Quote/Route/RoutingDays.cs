using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
	public class RoutingDays
	{
		public string RoutingDaysID { get; set; }
		public long RouteID { get; set; } = 0;
		public string FromCityID { get; set; } = "";
		public string FromCityName { get; set; } = "";
		public string ToCityID { get; set; } = "";
		public string ToCityName { get; set; } = "";
		public string GridLabel { get; set; } = "";
		public string GridLabelIds { get; set; } = "";
		public string Days { get; set; } = "";
		public int DayNo { get; set; } = 0;
		public int RouteSequence { get; set; } = 0;
		public bool IsDeleted { get; set; } = false;
		public string CreateUser { get; set; } = "";
		public DateTime CreateDate { get; set; } = DateTime.Now;
		public string EditUser { get; set; } = "";
		public DateTime? EditDate { get; set; } = null;
	}

	public class RoutingDaysInfo
	{
		public List<RoutingDays> RoutingDays { get; set; }
		public List<RoutingInfo> RoutingInfo { get; set; }
	}

	public class RoutingInfoCity
	{
		public string DayId { get; set; }
		public int DayNo { get; set; }
		public string DayName { get; set; }
		public string CityID { get; set; }
		public string CityName { get; set; }
		public string StartTime { get; set; }
		public int Duration { get; set; }
		public string PositionId { get; set; }
	}
}
