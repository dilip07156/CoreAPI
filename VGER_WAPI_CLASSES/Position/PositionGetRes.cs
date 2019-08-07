using System;
using System.Collections.Generic;
using System.Text;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI_CLASSES
{
    public class PositionGetRes
    {
        public PositionGetRes()
        {
            //AccomodationDetails = new List<mPosition>();
            mPosition = new List<mPosition>();
            ResponseStatus = new ResponseStatus();
            MealDetails = new List<MealDetails>();
            TransferDetails = new List<TransferDetails>();
        }
        public List<mPosition> mPosition { get; set; }
        // public List<mQRFPosition> mQrfPosition { get; set; }
        //public List<mProductLevelAttribute> ProductLevelAttribute { get; set; }
        //public List<mPosition> AccomodationDetails { get; set; } 
        public ResponseStatus ResponseStatus { get; set; }

        public string QRFID { get; set; }
        public bool? IsPlaceHolder { get; set; }
        public List<AttributeValues> DaysList { get; set; }
        public List<RoutingDays> RoutingDays { get; set; }
        public List<RoutingInfo> RoutingInfo { get; set; }
        public List<ProductType> ProductType { get; set; }
        public List<AgentPassengerInfo> AgentPassengerInfo { get; set; }
        public List<MealDetails> MealDetails { get; set; }
        public List<TransferDetails> TransferDetails { get; set; }
    }
}
