using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_DISTRIBUTION.Models;
using VGER_WAPI_CLASSES;

namespace VGER_DISTRIBUTION.Repositories
{
    public interface IBookingRepository
    {
        Task<BookingSearchRes> GetBookings(BookingSearchReq request, UserCookieDetail userdetails);
        Task<BookingDetails> GetBookingDetail(BookingDetailReq request, UserCookieDetail userdetails);
        Task<UpdateOperationDetails_RS> UpdateOperationDetails(UpdateOperationDetails_RQ request, IConfiguration _configuration, UserCookieDetail userdetails);
        Task<UpdatePurchaseDetails_RS> UpdatePurchaseDetails(UpdatePurchaseDetails_RQ request, IConfiguration _configuration, UserCookieDetail userdetails);
        Task<UpdatePositionProduct_RS> UpdatePositionProduct(UpdatePositionProduct_RQ request, IConfiguration _configuration, UserCookieDetail userdetails);
    }
}
