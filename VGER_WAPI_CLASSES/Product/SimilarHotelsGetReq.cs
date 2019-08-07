using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
  public class SimilarHotelsGetReq
    {
        public string PositionId { get; set; }
        public string ProductId { get; set; }
        public bool IsClone { get; set; }
    } 
}
