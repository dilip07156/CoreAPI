using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class QRFPaxGetResponse
    {
        public QRFPaxSlabDetails PaxSlabDetails { get; set; } = new QRFPaxSlabDetails();
        public string Status { get; set; }
        public string QRFID { get; set; }
    }

    public class QRFPaxSlabDetails
    {
        public List<QRFPaxSlabs> QRFPaxSlabs { get; set; } = new List<QRFPaxSlabs>();
        public List<QRFHotelCategories> HotelCategories { get; set; } = new List<QRFHotelCategories>();
        public List<QRFHotelChain> HotelChain { get; set; } = new List<QRFHotelChain>();
        public string HotelFlag { get; set; }
        public string CreateUser { get; set; } = "";
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public string EditUser { get; set; } = "";
        public DateTime? EditDate { get; set; } = null;
    }

    public class QRFPaxSlabs
    {
        public long PaxSlab_Id { get; set; }
        public int From { get; set; }
        public int To { get; set; }
        public int DivideByCost { get; set; }
        public string Category { get; set; }
        public string Category_Id { get; set; }
        public string CoachType { get; set; }
        public string CoachType_Id { get; set; }
        public string Brand { get; set; }
        public string Brand_Id { get; set; }
        public int HowMany { get; set; }
        public double? BudgetAmount { get; set; }
        public bool IsDeleted { get; set; } = false;
        public string DeleteUser { get; set; } = "";
        public DateTime? DeleteDate { get; set; } = null;
        public DateTime CreateDate { get; set; } = DateTime.Now;
        public DateTime? EditDate { get; set; } = null;
    }

    public class QRFHotelCategories
    {
        public string VoyagerDefProductCategoryId { get; set; }
        public string Name { get; set; }
    }

    public class QRFHotelChain
    {
        public string AttributeId { get; set; }
        public string Name { get; set; }
    }

}
