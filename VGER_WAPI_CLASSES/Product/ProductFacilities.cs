using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class ProductFacilities
    {
        public Guid? ProductFacility_Id { get; set; }
        public Guid? Facility_Id { get; set; }
        public Guid? Product_Id { get; set; }
    }

    public class Def_Facilities
    {
        public Guid? Facility_Id { get; set; }
        public string FacilityDescription { get; set; }
        public bool? ForProduct { get; set; }
        public bool? ForProductRange { get; set; }
    }

    public class ArrProductFacilities
    {
        public string ProductFacility_Id { get; set; }
        public string Product_Id { get; set; }
        public string Facility_Id { get; set; }
        public string FacilityDescription { get; set; }

    }

    public class ProductRoomTypes
    {
        public string Category { get; set; }
        public string RoomType { get; set; }
        public bool DefaultCategory { get; set; }
    }
}
