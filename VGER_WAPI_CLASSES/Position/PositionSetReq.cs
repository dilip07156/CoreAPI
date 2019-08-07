using System;
using System.Collections.Generic;
using System.Text;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI_CLASSES
{
    public class PositionSetReq
    {
        public string SaveType { get; set; } 
        public List<mPosition> mPosition { get; set; }
        public string Price { get; set; }
        public string FOC { get; set; }
        public bool IsClone { get; set; }
        public string QRFID { get; set; }
        public string VoyagerUserID { get; set; }
    } 
}
