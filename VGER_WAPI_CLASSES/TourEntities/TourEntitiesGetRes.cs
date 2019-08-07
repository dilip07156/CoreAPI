using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class TourEntitiesGetRes
    {
        public TourEntitiesGetRes()
        {
            TourEntities = new List<TourEntities>();
            ResponseStatus = new ResponseStatus();
            PaxSlabDetails = new PaxSlabDetails(); 
            PaxSlabDetails.PaxSlabs = new List<PaxSlabs>();
        }

        public string QRFID { get; set; }
        public List<TourEntities> TourEntities { get; set; }
        public ResponseStatus ResponseStatus { get; set; }
        public List<DynamicTourEntity> DynamicTourEntity { get; set; } = new List<DynamicTourEntity>();
        public PaxSlabDetails PaxSlabDetails { get; set; }
    }
    
}
