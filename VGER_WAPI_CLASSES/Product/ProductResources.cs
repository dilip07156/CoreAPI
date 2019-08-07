using System;
using System.Collections.Generic;
using System.Text;

namespace VGER_WAPI_CLASSES
{
    public class ProductResources
    {
        public Guid? ProductResource_Id { get; set; }
        public Guid? ResourceType_Id { get; set; }
        public string DDRESO { get; set; }
        public string Name { get; set; }
        public Guid? Product_Id { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public int? Resize { get; set; }
        public int? OrderNr { get; set; }
        public string Filepath { get; set; }
        public string ImageSRC { get; set; }
        public Guid? ProductContract_ID { get; set; }
        public string PhysicalLocation { get; set; }
        public bool? IsVisibleToAgent { get; set; }
    }

    public class def_ResourceType
    {
        public Guid? ResourceType_ID { get; set; }
        public int? DDRESTYPID { get; set; }
        public string DDRESTYP { get; set; }
        public string NAME { get; set; }
        public string STATUS { get; set; }
    }

    public class ArrProductResources
    {
        public string ProductResource_Id { get; set; }
        public string ResourceType_Id { get; set; }
        public string ResourceType { get; set; }
        public string DDRESO { get; set; }
        public string Name { get; set; }
        public string Product_Id { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public int? Resize { get; set; }
        public int? OrderNr { get; set; }
        public string Filepath { get; set; }
        public string ImageSRC { get; set; }
        public string ProductContract_ID { get; set; }
        public string PhysicalLocation { get; set; }
        public bool? IsVisibleToAgent { get; set; }
    }
}
