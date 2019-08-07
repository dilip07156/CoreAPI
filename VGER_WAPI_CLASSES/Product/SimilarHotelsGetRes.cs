using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
  public class SimilarHotelsGetRes
    {
        public List<ProductList> SelectedHotelList { get; set; } = new List<ProductList>();
        public List<ProductList> BlackListedHotelList { get; set; } = new List<ProductList>();
        public ResponseStatus ResponseStatus { get; set; } = new ResponseStatus();
    } 
}
