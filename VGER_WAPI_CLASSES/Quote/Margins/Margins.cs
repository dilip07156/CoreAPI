using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class Margins
    {
        public string SelectedMargin { get; set; } = "Package";
        public Package Package { get; set; }
        public Product Product { get; set; }
        public Itemwise Itemwise { get; set; }
        public string CreateUser { get; set; }
        //[BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? CreateDate { get; set; } = new DateTime();
        public string EditUser { get; set; }
        // [BsonDateTimeOptions(Kind = DateTimeKind.Local)]
        public DateTime? EditDate { get; set; }
    }
}
