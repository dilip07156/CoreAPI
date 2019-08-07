using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class Package
    {
        public Package()
        {
            PackageProperties = new List<PackageProperties>();
            MarginComputed = new MarginComputed();
        }
        public List<PackageProperties> PackageProperties { get; set; }
        public MarginComputed MarginComputed { get; set; }
    }

    public class PackageProperties
    {
        public string PackageID { get; set; }
        public string ComponentName { get; set; }
        public double SellingPrice { get; set; }
        public string MarginUnit { get; set; }
    }
}
