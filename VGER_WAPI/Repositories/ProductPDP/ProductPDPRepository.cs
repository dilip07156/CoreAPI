using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VGER_WAPI.Helpers;
using VGER_WAPI.Models;
using VGER_WAPI_CLASSES;


namespace VGER_WAPI.Repositories
{
    public class ProductPDPRepository : IProductPDPRepository
    {
        #region Private Variable Declaration
        private readonly MongoContext _MongoContext = null;
        #endregion

        public ProductPDPRepository(IOptions<MongoSettings> settings)
        {
            _MongoContext = new MongoContext(settings);
        }

        #region GetProductFullDetailsById 
        public async Task<List<Products>> GetProductFullDetailsById(List<string> request)
        {
            List<Products> result = new List<Products>();
            try
            {
                if (request?.Count > 0)
                {
                    result = await _MongoContext.Products.AsQueryable().Where(a => request.Contains(a.VoyagerProduct_Id)).ToAsyncEnumerable().ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return result ?? null;
        }

        public async Task<ProductSRPHotelGetRes> GetProductSRPHotelDetails(ProductSRPHotelGetReq request)
        {
            ProductSRPHotelGetRes response = new ProductSRPHotelGetRes() { ProductSRPRouteInfo = new List<ProductSRPRouteInfo>(), QRFID = request.QRFID, ResponseStatus = new ResponseStatus() };
            try
            {
                var resQuote = await _MongoContext.mQuote.FindAsync(a => a.QRFID == request.QRFID).Result.FirstOrDefaultAsync();
                if (resQuote != null)
                {
                    var accomres = new mPosition();
                    var position = _MongoContext.mPosition.AsQueryable().Where(x => x.QRFID == request.QRFID && x.ProductType == "Hotel" && x.IsDeleted == false).ToList();
                    if (position != null && position.Count > 0)
                    {
                        if (resQuote.RoutingInfo != null && resQuote.RoutingInfo.Count > 0)
                        {
                            var prodidlist = position.Select(a => a.ProductID).ToList();
                            FilterDefinition<mProducts_Lite> filter;
                            filter = Builders<mProducts_Lite>.Filter.Empty;
                            filter = filter & Builders<mProducts_Lite>.Filter.Where(a => prodidlist.Contains(a.VoyagerProduct_Id));
                            var resultProdSRP = await _MongoContext.mProducts_Lite.Find(filter).ToListAsync();

                            //changes this logic from routing wise list to position wise list
                            int day = 0;
                            int prevnight = 0;
                            foreach (var Route in resQuote.RoutingInfo.Where(a => a.Nights > 0))
                            {
                                if (day == 0) { day = 1; }
                                else { day = prevnight + day; }

                                response.ProductSRPRouteInfo.Add(new ProductSRPRouteInfo
                                {
                                    DayNo = day,
                                    Day = "Day " + day.ToString(),
                                    Duration = Route.Nights,
                                    RoutingDaysID = resQuote.RoutingDays.Where(a => a.Days == "Day " + day.ToString()).FirstOrDefault().RoutingDaysID,
                                    FromCityId = Route.FromCityID,
                                    FromCity = Route.FromCityName,
                                    ToCityId = Route.ToCityID,
                                    ToCity = Route.ToCityName,
                                });
                                prevnight = Route.Nights;
                            }

                            foreach (var item in response.ProductSRPRouteInfo)
                            {
                                accomres = position.Where(a => a.RoutingDaysID == item.RoutingDaysID).FirstOrDefault();
                                item.ProductSRPDetails = resultProdSRP.Where(a => a.VoyagerProduct_Id == accomres.ProductID).Select(a => new ProductSRPDetails
                                {
                                    Address = a.Address,
                                    BdgPriceCategory = a.BdgPriceCategory,
                                    Chain = a.Chain,
                                    CityName = a.CityName,
                                    CountryName = a.CountryName,
                                    DefaultSupplier = a.DefaultSupplier,
                                    HotelImageURL = a.HotelImageURL,
                                    HotelType = a.HotelType,
                                    Location = a.Location,
                                    PostCode = a.PostCode,
                                    ProdDesc = a.ProdDesc,
                                    ProdName = a.ProdName,
                                    ProductCode = a.ProductCode,
                                    ProductType = a.ProductType,
                                    StarRating = a.StarRating,
                                    Street = a.Street,
                                    VoyagerProduct_Id = a.VoyagerProduct_Id,
                                    ProductType_Id = a.ProductType_Id,
                                    Rooms = a.Rooms
                                }).FirstOrDefault();

                                if (accomres != null)
                                {
                                    item.ProdId = accomres.ProductID;
                                }
                            }

                            response.ResponseStatus.Status = "Success";
                        }
                        else
                        {
                            response.ResponseStatus.ErrorMessage = "RoutingInfo not exists in mQuote.";
                            response.ResponseStatus.Status = "Error";
                        }
                    }
                    else
                    {
                        response.ResponseStatus.ErrorMessage = "QRFID not exists in mPosition.";
                        response.ResponseStatus.Status = "Error";
                    }
                }
                else
                {
                    response.ResponseStatus.ErrorMessage = "QRFID not exists in mQuote.";
                    response.ResponseStatus.Status = "Error";
                }
            }
            catch (Exception)
            {

                throw;
            }
            return response;
        }
        #endregion

    }
}
