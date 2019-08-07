using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_DISTRIBUTION.Models;
using VGER_WAPI_CLASSES;


namespace VGER_DISTRIBUTION.Repositories
{
    public interface IProductRepository
    {
        Task<ProductListRes> GetProductDetailsBySearchCriteria(ProductListReq request);
        Task<ProductDetails> GetProductDetail(ProductDetailReq request);
    }
}
