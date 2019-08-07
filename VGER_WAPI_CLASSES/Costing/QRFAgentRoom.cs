using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class QRFAgentRoom
    {
        public string RoomTypeID { get; set; }
        public string RoomTypeName { get; set; }
        public int? RoomCount { get; set; }
    }

    public class QRFAgentProductInfo
    {
        public string Type { get; set; }
        public string TypeID { get; set; }

        public string Division { get; set; }
        public string DivisionID { get; set; }

        public string Product { get; set; }
        public string ProductID { get; set; }

        public string PurposeofTravel { get; set; }
        public string PurposeofTravelID { get; set; }

        public string Destination { get; set; }
        public string DestinationID { get; set; }

        public string Priority { get; set; }
        public string TourName { get; set; }
        public string TourCode { get; set; }

        public int? Duration { get; set; }
        public string BudgetCurrency { get; set; }
        public string BudgetCurrencyID { get; set; }
        public string BudgetCurrencyCode { get; set; }

        public double? BudgetAmount { get; set; }
        public string CostingType { get; set; }
    }

    public class QRFAgentPassengerInfo
    {
        public string Type { get; set; }
        public int count { get; set; }
        public List<int> Age { get; set; }
    }
}
