using System.Collections.Generic;
using System.Threading.Tasks;
using VGER_WAPI_CLASSES;


namespace VGER_WAPI.Repositories
{
    public interface IProductPDPRepository
    {
        #region Product PDP
        Task<List<Products>> GetProductFullDetailsById(List<string> request);

        Task<ProductSRPHotelGetRes> GetProductSRPHotelDetails(ProductSRPHotelGetReq request);
        #endregion
    }
}
