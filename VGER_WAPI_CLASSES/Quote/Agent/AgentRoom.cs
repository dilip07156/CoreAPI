using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class AgentRoom
    {
        public string RoomTypeID { get; set; }
        public string RoomTypeName { get; set; }
        public int? RoomCount { get; set; } 
    }
}
