using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES.MIS
{
    public class MisSearchGetResList
    {
        public MisSearchGetResList()
        {
            MisResults = new List<MisSearchGetRes>();
            ResponseStatus = new ResponseStatus();

        }
        public List<MisSearchGetRes> MisResults { get; set; }
        public ResponseStatus ResponseStatus { get; set; } = new ResponseStatus();


    }
  
}
