using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES.MIS
{
   public class MisSaveResponse
    {
        public MisSaveResponse()
        {
            Response = new ResponseStatus();
        }
        public ObjectId Id { get; set; }
        public string TypeName { get; set; }
        public string Item { get; set; }
        public string ItemUrl { get; set; }
        public string RoleName { get; set; }
        public int ItemSeq { get; set; }
        public ResponseStatus Response { get; set; }
        public List<string> Users { get; set; }
    }
}
