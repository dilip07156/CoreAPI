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
    public class ProductSRPRepository : IProductSRPRepository
    {
        #region Private Variable Declaration
        private readonly MongoContext _MongoContext = null;
        #endregion

        public ProductSRPRepository(IOptions<MongoSettings> settings)
        {
            _MongoContext = new MongoContext(settings);
        }

        #region Product SRP 
        public async Task<List<mProducts_Lite>> GetProductDetailsBySearchCriteria(ProductSRPSearchReq request)
        {
            FilterDefinition<mProducts_Lite> filter;
            filter = Builders<mProducts_Lite>.Filter.Empty;
            List<mProducts_Lite> result = new List<mProducts_Lite>();

            try
            {                
                if (!string.IsNullOrWhiteSpace(request.ProdId))
                {
                    filter = filter & Builders<mProducts_Lite>.Filter.Where(f => f.VoyagerProduct_Id == request.ProdId);
                }

                if (!string.IsNullOrWhiteSpace(request.ProdType))
                {
                    filter = filter & Builders<mProducts_Lite>.Filter.Eq(f => f.ProductType, request.ProdType.Trim());
                }

                if (!string.IsNullOrWhiteSpace(request.CityName))
                {
                    string[] CityCountry = request.CityName.Split(',');
                    if (CityCountry.Length > 0)
                    {
                        filter = filter & Builders<mProducts_Lite>.Filter.Regex(x => x.CountryName, new BsonRegularExpression(new Regex(CityCountry[1].Trim(), RegexOptions.IgnoreCase)));

                        if (CityCountry.Length > 1)
                        {
                            filter = filter & Builders<mProducts_Lite>.Filter.Regex(x => x.CityName, new BsonRegularExpression(new Regex(CityCountry[0].Trim(), RegexOptions.IgnoreCase)));
                            filter = filter & Builders<mProducts_Lite>.Filter.Where(x => x.Placeholder == false || x.Placeholder == null);
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(request.ProdName))
                {
                    request.ProdName = request.ProdName.Replace("###", "");
                    filter = filter & Builders<mProducts_Lite>.Filter.Regex(x => x.ProdName, new BsonRegularExpression(new Regex(request.ProdName.Trim(), RegexOptions.IgnoreCase)));
                }

                if (!string.IsNullOrWhiteSpace(request.ProdCode))
                {
                    filter = filter & Builders<mProducts_Lite>.Filter.Regex(x => x.ProductCode, new BsonRegularExpression(new Regex(request.ProdCode.Trim(), RegexOptions.IgnoreCase)));
                }

                if (!string.IsNullOrWhiteSpace(request.Status))
                {
                    if (request.Status == "Active")
                    {
                        filter = filter & Builders<mProducts_Lite>.Filter.Where(f => f.Status == null || f.Status == "" || f.Status == " ");
                    }
                    else if (request.Status == "Inactive")
                    {
                        filter = filter & Builders<mProducts_Lite>.Filter.Where(f => f.Status == "-" || f.Status == "X");
                    }
                }

                if (!string.IsNullOrWhiteSpace(request.BudgetCategory))
                {
                    filter = filter & Builders<mProducts_Lite>.Filter.Eq(x => x.BdgPriceCategory, request.BudgetCategory.Trim());
                }

                if (!string.IsNullOrWhiteSpace(request.StarRating))
                {
                    filter = filter & Builders<mProducts_Lite>.Filter.Eq(x => x.StarRating, request.StarRating.Trim());
                }

                if (!string.IsNullOrWhiteSpace(request.Location))
                {
                    filter = filter & Builders<mProducts_Lite>.Filter.Eq(f => f.Location, request.Location.Trim());
                }

                if (!string.IsNullOrWhiteSpace(request.Chain))
                {
                    filter = filter & Builders<mProducts_Lite>.Filter.Eq(x => x.Chain, request.Chain.Trim());
                }

                if (request.Facilities != null && request.Facilities.Count > 0)
                {
                    foreach (var item in request.Facilities)
                    {
                        filter = filter & Builders<mProducts_Lite>.Filter.Where(f => f.ProductFacilities.Select(a => a.FacilityDescription).ToList().Contains(item));
                    }
                }

                result = await _MongoContext.mProducts_Lite.Find(filter).ToListAsync();
                result = result.OrderBy(p => p.ProdName).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return result ?? new List<mProducts_Lite>();
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