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
using VGER_WAPI.Providers;
using VGER_WAPI_CLASSES;


namespace VGER_WAPI.Repositories
{
    public class ProductRepository : IProductRepository
    {
        #region Private Variable Declaration 
        private readonly MongoContext _MongoContext = null;
        private readonly IConfiguration _configuration;
        private BookingProviders _bookingProviders = null;
        #endregion

        public ProductRepository(IConfiguration configuration, IOptions<MongoSettings> settings)
        {
            _MongoContext = new MongoContext(settings);
            _configuration = configuration;
            _bookingProviders = new BookingProviders(_configuration);
        }

        #region GetProductDetailsBySearchCriteria 
        public async Task<List<ProductSearchDetails>> GetProductDetailsBySearchCriteriaOldNotInUse(ProductSearchReq request)
        {
            FilterDefinition<mProducts> filter;
            filter = Builders<mProducts>.Filter.Empty;

            FilterDefinition<mProducts> filterPH;
            filterPH = Builders<mProducts>.Filter.Empty;

            FilterDefinition<mProductHotelAdditionalInfo> filterPHAI;
            filterPHAI = Builders<mProductHotelAdditionalInfo>.Filter.Empty;

            FilterDefinition<mProductHotelAdditionalInfo> filterPHPlaceHoldAPI;
            filterPHPlaceHoldAPI = Builders<mProductHotelAdditionalInfo>.Filter.Empty;

            List<ProductSearchDetails> lstCountryWise = new List<ProductSearchDetails>();
            List<ProductSearchDetails> result = new List<ProductSearchDetails>();

            try
            {
                if (request.ProdType != null && request.ProdType.Count > 0)
                {
                    // filter = filter & Builders<mProducts>.Filter.Eq(f => f.ProductType, request.ProdType.Trim());
                    filter = filter & Builders<mProducts>.Filter.Where(f => request.ProdType.Contains(f.ProductType));
                }

                if (!string.IsNullOrWhiteSpace(request.ProdName) || request.ProdName == "###")
                {
                    request.ProdName = request.ProdName.Replace("###", "");
                    filter = filter & Builders<mProducts>.Filter.Regex(x => x.ProdName, new BsonRegularExpression(new Regex(request.ProdName.Trim(), RegexOptions.IgnoreCase)));
                }

                if (!string.IsNullOrWhiteSpace(request.ProdCode))
                {
                    filter = filter & Builders<mProducts>.Filter.Regex(x => x.ProductCode, new BsonRegularExpression(new Regex(request.ProdCode.Trim(), RegexOptions.IgnoreCase)));
                }

                if (!string.IsNullOrWhiteSpace(request.Status))
                {
                    filter = filter & Builders<mProducts>.Filter.Regex(x => x.Status, new BsonRegularExpression(new Regex(request.Status.Trim(), RegexOptions.IgnoreCase)));
                }

                if (request.IsPlaceHolder != null)
                {
                    filter = filter & Builders<mProducts>.Filter.Where(x => x.Placeholder == request.IsPlaceHolder);
                }

                //if (!string.IsNullOrWhiteSpace(request.ProductCategoryID))
                //{
                //    List<string> lstprodID = _MongoContext.mProductCategory.AsQueryable().Where(p => p.DefProductCategory_Id == request.ProductCategoryID).Select(p => p.Product_Id).Distinct().ToList();
                //    if (lstprodID != null && lstprodID.Count > 0)
                //    {
                //        filter = filter & Builders<mProducts>.Filter.Where(x => lstprodID.Contains(x.VoyagerProduct_Id));
                //    }
                //}

                if (request.ProdType != null && request.ProdType.Count == 1 && (request.ProdType[0].ToLower() == "visa" || request.ProdType[0].ToLower() == "other" || request.ProdType[0].ToLower() == "fee"))
                {
                    if (!string.IsNullOrWhiteSpace(request.CountryName))
                    {
                        filter = filter & Builders<mProducts>.Filter.Regex(x => x.CountryName, new BsonRegularExpression(new Regex(request.CountryName.Trim(), RegexOptions.IgnoreCase)));
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(request.CountryName))
                    {
                        var resorts = _MongoContext.mResort.AsQueryable().Where(a => a.KeyResort == true && a.ResortType == "City" && a.ParentResortName.ToLower() == request.CountryName.ToLower()).ToList();
                        var cityids = resorts.Select(a => a.Voyager_Resort_Id).ToList();

                        filter = filter & Builders<mProducts>.Filter.Regex(x => x.CountryName, new BsonRegularExpression(new Regex(request.CountryName.Trim(), RegexOptions.IgnoreCase)));
                        filterPH = filter;
                        filterPH = filterPH & Builders<mProducts>.Filter.Where(x => x.Placeholder == true && cityids.Contains(x.Resort_Id));

                        if (!string.IsNullOrWhiteSpace(request.StarRating))
                        {
                            filterPHPlaceHoldAPI = filterPHPlaceHoldAPI & Builders<mProductHotelAdditionalInfo>.Filter.Where(x => x.StarRating == request.StarRating.Trim());
                            var resultPHAI = await _MongoContext.mProductHotelAdditionalInfo.Find(filterPHPlaceHoldAPI).ToListAsync();
                            if (resultPHAI != null && resultPHAI.Count > 0)
                            {
                                var listprodids = resultPHAI.Select(a => a.ProductId).Distinct().ToList();
                                filterPH = filterPH & Builders<mProducts>.Filter.Where(f => listprodids.Contains(f.VoyagerProduct_Id));
                            }
                        }

                        lstCountryWise = await _MongoContext.mProducts.Find(filterPH).Project(p => new ProductSearchDetails
                        {
                            ProdLocation = new ProdLocation { CountryName = p.CountryName, CountryCode = p.ParentResort_Id, CityName = p.CityName, CityCode = p.Region_Id },
                            ProdCode = p.ProductCode,
                            ProdName = p.ProdName,
                            Status = p.Status,
                            VoyagerProduct_Id = p.VoyagerProduct_Id,
                            ProdType = p.ProductType,
                            ProdTypeId = p.ProductType_Id,
                            PlaceHolder = p.Placeholder
                        }).ToListAsync();
                    }

                    if (!string.IsNullOrWhiteSpace(request.CityName))
                    {
                        filter = filter & Builders<mProducts>.Filter.Regex(x => x.CityName, new BsonRegularExpression(new Regex(request.CityName.Trim(), RegexOptions.IgnoreCase)));
                        filter = filter & Builders<mProducts>.Filter.Where(x => x.Placeholder == false || x.Placeholder == null);
                    }
                }

                if (!string.IsNullOrWhiteSpace(request.ProductAttributeName))
                {
                    if (string.IsNullOrEmpty(request.ProductAttributeName)) request.ProductAttributeName = "Y";
                    List<string> lstprodID = _MongoContext.mProductLevelAttribute.AsQueryable().Where(p => p.AttributeName == request.ProductAttributeName && p.AttributeValue == request.ProductAttributeValue).Select(p => p.Product_Id).Distinct().ToList();
                    if (lstprodID != null && lstprodID.Count > 0)
                    {
                        filter = filter & Builders<mProducts>.Filter.Where(x => lstprodID.Contains(x.VoyagerProduct_Id));
                    }
                }

                #region filter by Location, Chain, StarRating
                if (!string.IsNullOrWhiteSpace(request.Location))
                {
                    filterPHAI = filterPHAI & Builders<mProductHotelAdditionalInfo>.Filter.Eq(f => f.Location, request.Location.Trim());
                }

                if (!string.IsNullOrWhiteSpace(request.Chain))
                {
                    filterPHAI = filterPHAI & Builders<mProductHotelAdditionalInfo>.Filter.Where(x => x.HotelChain == request.Chain.Trim());
                }

                if (!string.IsNullOrWhiteSpace(request.StarRating))
                {
                    filterPHAI = filterPHAI & Builders<mProductHotelAdditionalInfo>.Filter.Where(x => x.StarRating == request.StarRating.Trim());
                }
                if (!string.IsNullOrWhiteSpace(request.ProductCategoryID))
                {
                    filterPHAI = filterPHAI & Builders<mProductHotelAdditionalInfo>.Filter.Where(x => x.BudgetCategoryId == request.ProductCategoryID);
                }
                if (!string.IsNullOrWhiteSpace(request.Location) || !string.IsNullOrWhiteSpace(request.Chain) || !string.IsNullOrWhiteSpace(request.StarRating)
                    || !string.IsNullOrWhiteSpace(request.ProductCategoryID))
                {
                    var resultPHAI = await _MongoContext.mProductHotelAdditionalInfo.Find(filterPHAI).ToListAsync();
                    if (resultPHAI != null && resultPHAI.Count > 0)
                    {
                        var listprodids = resultPHAI.Select(a => a.ProductId).Distinct().ToList();
                        filter = filter & Builders<mProducts>.Filter.Where(f => listprodids.Contains(f.VoyagerProduct_Id));
                    }
                }
                #endregion

                result = await _MongoContext.mProducts.Find(filter).Project(p => new ProductSearchDetails
                {
                    ProdLocation = new ProdLocation { CountryName = p.CountryName, CountryCode = p.ParentResort_Id, CityName = p.CityName, CityCode = p.Region_Id },
                    ProdCode = p.ProductCode,
                    ProdName = p.ProdName,
                    Status = p.Status,
                    VoyagerProduct_Id = p.VoyagerProduct_Id,
                    ProdType = p.ProductType,
                    ProdTypeId = p.ProductType_Id,
                    PlaceHolder = p.Placeholder
                }).ToListAsync();

                result.AddRange(lstCountryWise);

                var listprodid = result.Select(a => a.VoyagerProduct_Id).Distinct().ToList();
                var lstprodcatID = listprodid != null && listprodid.Count > 0 ? _MongoContext.mProductHotelAdditionalInfo.AsQueryable().Where(p => listprodid.Contains(p.ProductId)).Distinct().ToList() : new List<mProductHotelAdditionalInfo>();
                if (lstprodcatID != null && lstprodcatID.Count > 0)
                {
                    mProductHotelAdditionalInfo catid = new mProductHotelAdditionalInfo();
                    result.Where(a => string.IsNullOrEmpty(a.CategoryId)).ToList().
                   ForEach(a =>
                   {
                       catid = lstprodcatID.Where(b => b.ProductId == a.VoyagerProduct_Id).FirstOrDefault();
                       if (catid != null)
                       {
                           a.CategoryId = catid.BudgetCategoryId;
                           a.CategoryName = catid.BudgetCategory;
                           a.Chain = catid.HotelChain;
                           a.ChainId = catid.HotelChainId;
                           a.StarRatingId = catid.StarRatingId;
                           a.StarRating = catid.StarRating != null ? catid.StarRating.Trim() : catid.StarRating;
                           a.Location = catid.Location != null ? catid.Location.Trim() : catid.Location;
                           a.LocationId = catid.LocationId;
                       }
                   });
                }

                result = result.OrderBy(p => p.ProdName).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return result ?? new List<ProductSearchDetails>();
        }

        public async Task<List<ProductSearchDetails>> GetProductDetailsBySearchCriteria(ProductSearchReq request)
        {
            FilterDefinition<mProducts_Lite> filter = Builders<mProducts_Lite>.Filter.Empty;
            FilterDefinition<mProducts_Lite> filterPH = Builders<mProducts_Lite>.Filter.Empty;
            List<ProductSearchDetails> result = new List<ProductSearchDetails>();
            List<ProductSearchDetails> lstCountryWise = new List<ProductSearchDetails>();
            string StarRating = "";//, BudgetCategory = "";
            //List<string> CountriesLowerCase = new List<string>();
            //List<string> CitiesLowerCase = new List<string>();

            try
            {
                #region Preparing Start Rating, Budget Category, City & Country Lists

                if (!string.IsNullOrWhiteSpace(request.StarRating))
                {
                    if (!int.TryParse(request.StarRating, out int myNum) && request.StarRating.Split(" ")?.Length > 1)
                    {
                        StarRating = request.StarRating.Split(" ")?[0];
                        //BudgetCategory = request.StarRating.Split(" ")?[1];
                        //if (BudgetCategory == "STD") BudgetCategory = "Standard";
                        //else if (BudgetCategory == "SUP") BudgetCategory = "Superior";
                        //else if (BudgetCategory == "DLX") BudgetCategory = "Deluxe";
                    }
                }
                if (request.CountryList == null || request.CountryList.Count < 1)
                {
                    request.CountryList = new List<string>();
                }
                if (!string.IsNullOrWhiteSpace(request.CountryName) || request.CountryList?.Count > 0)
                {
                    if (!string.IsNullOrWhiteSpace(request.CountryName))
                    {
                        request.CountryList.Add(request.CountryName);
                    }
                    //CountriesLowerCase = request.CountryList.Select(b => b.ToLower()).ToList();
                }
                if (request.CityList == null || request.CityList.Count < 1)
                {
                    request.CityList = new List<string>();
                }
                if (!string.IsNullOrWhiteSpace(request.CityName) || request.CityList?.Count > 0)
                {
                    if (!string.IsNullOrWhiteSpace(request.CityName))
                    {
                        request.CityList.Add(request.CityName);
                    }
                    //CitiesLowerCase = request.CityList.Select(b => b.ToLower()).ToList();
                }

                #endregion

                #region Product level filters

                if (request.ProdType != null && request.ProdType.Count > 0)
                {
                    filter = filter & Builders<mProducts_Lite>.Filter.Where(f => request.ProdType.Contains(f.ProductType));
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
                    filter = filter & Builders<mProducts_Lite>.Filter.Regex(x => x.Status, new BsonRegularExpression(new Regex(request.Status.Trim(), RegexOptions.IgnoreCase)));
                }

                if (request.IsPlaceHolder != null)
                {
                    filter = filter & Builders<mProducts_Lite>.Filter.Where(x => x.Placeholder == request.IsPlaceHolder);
                }

                if (request.ProdType != null && request.ProdType.Count == 1 && (request.ProdType[0].ToLower() == "visa" || request.ProdType[0].ToLower() == "other" || request.ProdType[0].ToLower() == "fee"))
                {
                    if (request.CountryList?.Count > 0)
                    {
                        filter = filter & Builders<mProducts_Lite>.Filter.Where(x => request.CountryList.Contains(x.CountryName));
                    }
                }
                else
                {
                    if (request.CountryList?.Count > 0)
                    {
                        var KeyCityNames = _MongoContext.mResort.AsQueryable().Where(a => a.KeyResort == true && a.ResortType == "City" &&
                            request.CountryList.Contains(a.ParentResortName)).Select(a => a.ResortName).ToList();

                        filter = filter & Builders<mProducts_Lite>.Filter.Where(x => request.CountryList.Contains(x.CountryName));
                        filterPH = filter;
                        filterPH = filterPH & Builders<mProducts_Lite>.Filter.Where(x => x.Placeholder == true && KeyCityNames.Contains(x.CityName));

                        if (!string.IsNullOrWhiteSpace(request.StarRating))
                        {
                            if (int.TryParse(request.StarRating, out int myNum))
                            {
                                filterPH = filterPH & Builders<mProducts_Lite>.Filter.Eq(x => x.StarRating, request.StarRating.Trim());
                            }
                            else if (!string.IsNullOrWhiteSpace(StarRating))
                            {
                                filterPH = filterPH & Builders<mProducts_Lite>.Filter.Eq(x => x.StarRating, StarRating.Trim());
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(request.ProductCategoryName))
                        {
                            filterPH = filterPH & Builders<mProducts_Lite>.Filter.Where(x => x.BdgPriceCategory == request.ProductCategoryName);
                        }

                        if (request.StarRatingList?.Count > 0)
                        {
                            List<string> StarList = request.StarRatingList.Select(a => a?.Substring(0, 1)).ToList();
                            if (StarList?.Count > 0)
                            {
                                filterPH = filterPH & Builders<mProducts_Lite>.Filter.Where(x => StarList.Contains(x.StarRating));
                                //filterPH = filterPH & Builders<mProducts_Lite>.Filter.Eq(x => x.BdgPriceCategory, "Standard");
                            }
                        }

                        lstCountryWise = await _MongoContext.mProducts_Lite.Find(filterPH).Project(p => new ProductSearchDetails
                        {
                            ProdLocation = new ProdLocation { CountryName = p.CountryName, CountryCode = p.CountryId, CityName = p.CityName, CityCode = p.CityId },
                            ProdCode = p.ProductCode,
                            ProdName = p.ProdName,
                            Status = p.Status,
                            VoyagerProduct_Id = p.VoyagerProduct_Id,
                            ProdType = p.ProductType,
                            ProdTypeId = p.ProductType_Id,
                            PlaceHolder = p.Placeholder,
                            CategoryName = p.BdgPriceCategory,
                            Chain = p.Chain,
                            StarRating = p.StarRating + " " + (p.BdgPriceCategory == "Deluxe" ? "DLX" : p.BdgPriceCategory == "Superior" ? "SUP" : "STD"),
                            Location = p.Location,
                            DefaultSupplierId = p.DefaultSupplierId,
                            DefaultSupplier = p.DefaultSupplier
                        }).ToListAsync();
                    }

                    if (request.CityList?.Count > 0)
                    {
                        filter = filter & Builders<mProducts_Lite>.Filter.Where(x => request.CityList.Contains(x.CityName));
                        filter = filter & Builders<mProducts_Lite>.Filter.Where(x => x.Placeholder == false || x.Placeholder == null);
                    }
                }

                if (!string.IsNullOrWhiteSpace(request.ProductAttributeName))
                {
                    if (string.IsNullOrEmpty(request.ProductAttributeName)) request.ProductAttributeName = "Y";

                    List<string> lstprodID = _MongoContext.mProductLevelAttribute.AsQueryable()
                        .Where(p => p.AttributeName == request.ProductAttributeName && p.AttributeValue == request.ProductAttributeValue)
                        .Select(p => p.Product_Id).Distinct().ToList();

                    if (lstprodID != null && lstprodID.Count > 0)
                    {
                        filter = filter & Builders<mProducts_Lite>.Filter.Where(x => lstprodID.Contains(x.VoyagerProduct_Id));
                    }
                }

                #endregion

                #region filter by Location, Chain, StarRating
                if (!string.IsNullOrWhiteSpace(request.Location))
                {
                    filter = filter & Builders<mProducts_Lite>.Filter.Eq(f => f.Location, request.Location.Trim());
                }

                if (!string.IsNullOrWhiteSpace(request.Chain))
                {
                    filter = filter & Builders<mProducts_Lite>.Filter.Where(x => x.Chain == request.Chain.Trim());
                }

                if (!string.IsNullOrWhiteSpace(request.StarRating))
                {
                    if (int.TryParse(request.StarRating, out int myNum))
                    {
                        filter = filter & Builders<mProducts_Lite>.Filter.Eq(x => x.StarRating, request.StarRating.Trim());
                    }
                    else if (!string.IsNullOrWhiteSpace(StarRating))
                    {
                        filter = filter & Builders<mProducts_Lite>.Filter.Eq(x => x.StarRating, StarRating.Trim());
                    }
                }

                if (request.StarRatingList?.Count > 0)
                {
                    List<string> StarList = request.StarRatingList.Select(a => a?.Substring(0, 1)).ToList();
                    if (StarList?.Count > 0)
                    {
                        filter = filter & Builders<mProducts_Lite>.Filter.Where(x => StarList.Contains(x.StarRating));
                        //filter = filter & Builders<mProducts_Lite>.Filter.Eq(x => x.BdgPriceCategory, "Standard");
                    }
                }

                if (!string.IsNullOrWhiteSpace(request.ProductCategoryName))
                {
                    filter = filter & Builders<mProducts_Lite>.Filter.Where(x => x.BdgPriceCategory == request.ProductCategoryName);
                }
                #endregion

                result = await _MongoContext.mProducts_Lite.Find(filter).Project(p => new ProductSearchDetails
                {
                    ProdLocation = new ProdLocation { CountryName = p.CountryName, CountryCode = p.CountryId, CityName = p.CityName, CityCode = p.CityId },
                    ProdCode = p.ProductCode,
                    ProdName = p.ProdName,
                    Status = p.Status,
                    VoyagerProduct_Id = p.VoyagerProduct_Id,
                    ProdType = p.ProductType,
                    ProdTypeId = p.ProductType_Id,
                    PlaceHolder = p.Placeholder,
                    CategoryName = p.BdgPriceCategory,
                    Chain = p.Chain,
                    StarRating = p.StarRating + " " + (p.BdgPriceCategory == "Deluxe" ? "DLX" : p.BdgPriceCategory == "Superior" ? "SUP" : "STD"),
                    Location = p.Location,
                    DefaultSupplierId = p.DefaultSupplierId,
                    DefaultSupplier = p.DefaultSupplier
                }).ToListAsync();

                result.AddRange(lstCountryWise);
                result = result.OrderBy(p => p.ProdName).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return result ?? new List<ProductSearchDetails>();
        }

        public async Task<List<ProductWithRate>> GetProductWithRateBySearchCriteria(ProductWithRateSearchReq request)
        {
            FilterDefinition<Products> filter = Builders<Products>.Filter.Empty;
            List<ProductWithRate> result = new List<ProductWithRate>();

            try
            {
                #region Product level filters

                if (!string.IsNullOrWhiteSpace(request.ProdId))
                {
                    filter = filter & Builders<Products>.Filter.Where(f => f.VoyagerProduct_Id == request.ProdId);
                }

                if (!string.IsNullOrWhiteSpace(request.ProdType))
                {
                    filter = filter & Builders<Products>.Filter.Eq(f => f.ProductType, request.ProdType.Trim());
                }

                if (!string.IsNullOrWhiteSpace(request.CityName))
                {
                    string[] CityCountry = request.CityName.Split(',');
                    if (CityCountry.Length > 0)
                    {
                        filter = filter & Builders<Products>.Filter.Regex(x => x.CountryName, new BsonRegularExpression(new Regex(CityCountry[1].Trim(), RegexOptions.IgnoreCase)));

                        if (CityCountry.Length > 1)
                        {
                            filter = filter & Builders<Products>.Filter.Regex(x => x.CityName, new BsonRegularExpression(new Regex(CityCountry[0].Trim(), RegexOptions.IgnoreCase)));
                            filter = filter & Builders<Products>.Filter.Where(x => x.Placeholder == false || x.Placeholder == null);
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(request.ProdName))
                {
                    request.ProdName = request.ProdName.Replace("###", "");
                    filter = filter & Builders<Products>.Filter.Regex(x => x.ProductName, new BsonRegularExpression(new Regex(request.ProdName.Trim(), RegexOptions.IgnoreCase)));
                }

                if (!string.IsNullOrWhiteSpace(request.ProdCode))
                {
                    filter = filter & Builders<Products>.Filter.Regex(x => x.ProductCode, new BsonRegularExpression(new Regex(request.ProdCode.Trim(), RegexOptions.IgnoreCase)));
                }

                if (!string.IsNullOrWhiteSpace(request.Status))
                {
                    if (request.Status == "Active")
                    {
                        filter = filter & Builders<Products>.Filter.Where(f => f.Status == null || f.Status == "" || f.Status == " ");
                    }
                    else if (request.Status == "Inactive")
                    {
                        filter = filter & Builders<Products>.Filter.Where(f => f.Status == "-" || f.Status == "X");
                    }
                }

                if (!string.IsNullOrWhiteSpace(request.BudgetCategory))
                {
                    filter = filter & Builders<Products>.Filter.Eq(x => x.HotelAdditionalInfo.BdgPriceCategory, request.BudgetCategory.Trim());
                }

                if (!string.IsNullOrWhiteSpace(request.StarRating))
                {
                    filter = filter & Builders<Products>.Filter.Regex(x =>  x.HotelAdditionalInfo.StarRating,new BsonRegularExpression(new Regex(request.StarRating.Trim(), RegexOptions.IgnoreCase)));
                }

                if (!string.IsNullOrWhiteSpace(request.Location))
                {
                    filter = filter & Builders<Products>.Filter.Eq(f => f.HotelAdditionalInfo.Location, request.Location.Trim());
                }

                if (!string.IsNullOrWhiteSpace(request.Chain))
                {
                    filter = filter & Builders<Products>.Filter.Eq(x => x.HotelAdditionalInfo.Chain, request.Chain.Trim());
                }

                if (request.Facilities != null && request.Facilities.Count > 0)
                {
                    foreach (var item in request.Facilities)
                    {
                        filter = filter & Builders<Products>.Filter.Where(f => f.ProductFacilities.Select(a => a.FacilityDesc).ToList().Contains(item));
                    }
                }

                if (request.IsPlaceHolder != null)
                {
                    filter = filter & Builders<Products>.Filter.Where(x => x.Placeholder == request.IsPlaceHolder);
                }

                #endregion

                result = await _MongoContext.Products.Find(filter).Project(p => new ProductWithRate
                {
                    VoyagerProduct_Id = p.VoyagerProduct_Id,
                    ProductName = p.ProductName,
                    CountryName = p.CountryName,
                    CityName = p.CityName,
                    ProductCategories = p.ProductCategories,
                    DefaultSupplierId = p.ProductSuppliers.Where(a => a.IsDefault == true).FirstOrDefault() != null ? p.ProductSuppliers.Where(a => a.IsDefault == true).FirstOrDefault().Company_Id : "",
                    DefaultSupplier = p.ProductSuppliers.Where(a => a.IsDefault == true).FirstOrDefault() != null ? p.ProductSuppliers.Where(a => a.IsDefault == true).FirstOrDefault().CompanyName : ""
                }).ToListAsync();

                result = result.OrderBy(p => p.ProductName).ToList();

                #region Get Contract Rate
                var lstProductList = new List<string>();
                lstProductList = result.Select(a => a.VoyagerProduct_Id).ToList();

                //var resultContract1 = (from pc in _MongoContext.mProductContract.AsQueryable()
                //                      join pp in _MongoContext.mPricePeriod.AsQueryable() on pc.VoyagerProductContract_Id equals pp.ProductContract_Id
                //                      join ppr in _MongoContext.mProductPrice.AsQueryable() on pp.VoyagerPricePeriod_Id equals ppr.PricePeriod_Id
                //                      join cc in _MongoContext.mCurrency.AsQueryable() on pc.Currency_Id equals cc.VoyagerCurrency_Id
                //                      where lstProductList.Contains(pc.Product_Id)
                //                      select new ProductContractInfo
                //                      {
                //                          ProductRangeId = ppr.ProductRange_Id,
                //                          ProductId = pp.Product_Id,
                //                          DayComboPattern = pp.DayComboPattern,
                //                          Price = ppr.Price,
                //                          FromDate = pp.Datemin,
                //                          ToDate = pp.Datemax,
                //                          CurrencyId = pc.Currency_Id,
                //                          Currency = cc.Currency,
                //                          ContractId = pc.VoyagerProductContract_Id,
                //                          SupplierId = pc.Supplier_Id
                //                      }).Distinct().ToList();

                var productContract = _MongoContext.ProductContracts.AsQueryable().Where(a => lstProductList.Contains(a.Product_Id)).ToList();
                var resultContract = new List<ProductContractInfo>();
                if (productContract?.Count > 0)
                {
                    foreach (var prodCon in productContract)
                    {
                        foreach (var pricePerd in prodCon.PricePeriods)
                        {
                            foreach (var price in pricePerd.Prices)
                            {
                                resultContract.Add(new ProductContractInfo
                                {
                                    ProductRangeId = price.ProductRange_ID,
                                    ProductId = prodCon.Product_Id,
                                    DayComboPattern = pricePerd.DayCombo,
                                    Price = Convert.ToString(price.Contract_Price),
                                    FromDate = pricePerd.Period_Start_Date,
                                    ToDate = pricePerd.Period_End_Date,
                                    CurrencyId = prodCon.Contract_Currency_Id,
                                    Currency = prodCon.Contract_Currency,
                                    ContractId = prodCon.ProductContract_Id,
                                    SupplierId = prodCon.Supplier_Id
                                });
                            }
                        }
                    }
                }


                if (resultContract?.Count > 0)
                {
                    foreach (var prod in result)
                    {
                        prod.ProductContracts = resultContract.Where(a => a.ProductId == prod.VoyagerProduct_Id && a.SupplierId == prod.DefaultSupplierId).ToList();
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return result ?? new List<ProductWithRate>();
        }
        #endregion

        #region GetProductDetailsByCountryCityProdType 
        public async Task<List<ProductSearchDetails>> GetProductDetailsByCountryCityProdType(ProductSearchReq request)
        {
            try
            {
                FilterDefinition<mProducts> filter;
                FilterDefinition<mProducts> filterPH;
                filter = Builders<mProducts>.Filter.Empty;
                filterPH = Builders<mProducts>.Filter.Empty;

                List<ProductSearchDetails> lstCountryWise = new List<ProductSearchDetails>();

                if (request.ProdType != null && request.ProdType.Count > 0)
                {
                    filter = filter & Builders<mProducts>.Filter.Where(f => request.ProdType.Contains(f.ProductType));
                }

                if (!string.IsNullOrWhiteSpace(request.ProdName) || request.ProdName == "###")
                {
                    request.ProdName = request.ProdName.Replace("###", "");
                    filter = filter & Builders<mProducts>.Filter.Regex(x => x.ProdName, new BsonRegularExpression(new Regex(request.ProdName.Trim(), RegexOptions.IgnoreCase)));
                }

                if (!string.IsNullOrWhiteSpace(request.CountryName))
                {
                    filter = filter & Builders<mProducts>.Filter.Regex(x => x.CountryName, new BsonRegularExpression(new Regex(request.CountryName.Trim(), RegexOptions.IgnoreCase)));
                    //filterPH = filter;
                    //filterPH = filterPH & Builders<mProducts>.Filter.Where(x => x.Placeholder == false);

                    //lstCountryWise = await _MongoContext.mProducts.Find(filterPH).Project(p => new ProductSearchDetails
                    //{
                    //    ProdLocation = new ProdLocation { CountryName = p.CountryName, CountryCode = p.ParentResort_Id, CityName = p.CityName, CityCode = p.Region_Id },
                    //    ProdCode = p.ProductCode,
                    //    ProdName = p.ProdName,
                    //    Status = p.Status,
                    //    VoyagerProduct_Id = p.VoyagerProduct_Id,
                    //    ProdType = p.ProductType,
                    //    ProdTypeId = p.ProductType_Id
                    //}).ToListAsync();
                }

                if (request.CityList != null && request.CityList.Count > 0)
                {
                    filter = filter & Builders<mProducts>.Filter.Where(x => request.CityList.Contains(x.CityName));
                    filter = filter & Builders<mProducts>.Filter.Where(x => x.Placeholder == false || x.Placeholder == null);
                }

                if (!string.IsNullOrWhiteSpace(request.ProductAttributeName))
                {
                    if (string.IsNullOrEmpty(request.ProductAttributeName)) request.ProductAttributeValue = "Y";
                    List<string> lstprodID = _MongoContext.mProductLevelAttribute.AsQueryable().Where(p => p.AttributeName == request.ProductAttributeName && p.AttributeValue == request.ProductAttributeValue).Select(p => p.Product_Id).Distinct().ToList();
                    if (lstprodID == null) lstprodID = new List<string>();
                    filter = filter & Builders<mProducts>.Filter.Where(x => lstprodID.Contains(x.VoyagerProduct_Id));
                }

                var result = await _MongoContext.mProducts.Find(filter).Project(p => new ProductSearchDetails
                {
                    ProdLocation = new ProdLocation { CountryName = p.CountryName, CountryCode = p.ParentResort_Id, CityName = p.CityName, CityCode = p.Region_Id },
                    ProdCode = p.ProductCode,
                    ProdName = p.ProdName,
                    Status = p.Status,
                    VoyagerProduct_Id = p.VoyagerProduct_Id,
                    ProdType = p.ProductType,
                    ProdTypeId = p.ProductType_Id
                }).ToListAsync();

                //result.AddRange(lstCountryWise);
                result = result.OrderBy(p => p.ProdName).ToList();

                return result ?? new List<ProductSearchDetails>();
            }
            catch (Exception ex)
            {
                return new List<ProductSearchDetails>();
            }
        }
        #endregion

        #region GetProductCategoryRangeByProductID 
        public List<ProductRangeDetails> GetProductCategoryRangeByProductID(ProdCategoryRangeGetReq request)
        {
            List<ProductRangeDetails> result = new List<ProductRangeDetails>();
            var products = _MongoContext.Products.AsQueryable();
            var productsList = products.Where(a => request.ProductId.Contains(a.VoyagerProduct_Id) && a.ProductCategories != null && a.ProductCategories.Count > 0).Select(a => new
            {
                a.VoyagerProduct_Id,
                a.ProductCategories
            }).ToList();
            var prodRangeList = new List<ProductRangeDetails>();

            foreach (var prod in productsList)
            {
                var productCategories = prod.ProductCategories.Where(b => string.IsNullOrEmpty(b.Status)).ToList();
                foreach (var prodCat in productCategories)
                {
                    prodCat.ProductRanges = prodCat.ProductRanges.Where(a => a.Status.ToLower() == "active").ToList();
                    prodRangeList.AddRange(prodCat.ProductRanges.Select(a => new ProductRangeDetails
                    {
                        ProductId = prod.VoyagerProduct_Id,
                        ProductCategoryId = prodCat.ProductCategory_Id,
                        ProductCategoryName = prodCat.ProductCategoryName,
                        ProductRangeCode = a.ProductTemplateCode,
                        ProductRangeName = a.ProductTemplateName,
                        VoyagerProductRange_Id = a.ProductRange_Id,
                        AdditionalYN = a.AdditionalYn ?? false,
                        PersonType = a.PersonType,
                        AgeRange = (a.Agemin == null || a.Agemin == "" || a.Agemax == null || a.Agemax == "") ? null : (a.Agemin + " - " + a.Agemax)
                    }).ToList());
                }
            }
            result = prodRangeList;
            //  List<ProductRangeDetails> aa= GetProductCategoryRangeByProductIDOld(request);
            return result != null ? result.ToList() : (new List<ProductRangeDetails>());
        }

        public List<ProductRangeDetails> GetProductCategoryRangeByProductIDOld(ProdCategoryRangeGetReq request)
        {
            List<ProductRangeDetails> result = new List<ProductRangeDetails>();
            var products = _MongoContext.Products.AsQueryable();
            var productsList = products.Where(a => request.ProductId.Contains(a.VoyagerProduct_Id) && a.ProductCategories != null && a.ProductCategories.Count > 0).Select(a => new
            {
                a.VoyagerProduct_Id,
                a.ProductCategories
            }).ToList();
            var prodRangeList = new List<ProductRangeDetails>();

            foreach (var prod in productsList)
            {
                var productCategories = prod.ProductCategories.Where(b => string.IsNullOrEmpty(b.Status)).ToList();
                foreach (var prodCat in productCategories)
                {
                    prodCat.ProductRanges = prodCat.ProductRanges.Where(a => a.Status.ToLower() == "active").ToList();
                    prodRangeList.AddRange(prodCat.ProductRanges.Select(a => new ProductRangeDetails
                    {
                        ProductId = prod.VoyagerProduct_Id,
                        ProductCategoryId = prodCat.ProductCategory_Id,
                        ProductCategoryName = prodCat.ProductCategoryName,
                        ProductRangeCode = a.ProductTemplateCode,
                        ProductRangeName = a.ProductTemplateName,
                        VoyagerProductRange_Id = a.ProductRange_Id,
                        AdditionalYN = a.AdditionalYn,
                        PersonType = a.PersonType,
                        AgeRange = (a.Agemin == null || a.Agemin == "" || a.Agemax == null || a.Agemax == "") ? null : (a.Agemin + " - " + a.Agemax)
                    }).ToList()); 
                }
            }
            result = prodRangeList;
          //  List<ProductRangeDetails> aa= GetProductCategoryRangeByProductIDOld(request);
            return result != null ? result.ToList() : (new List<ProductRangeDetails>());
        }

        //public List<ProductRangeDetails> GetProductCategoryRangeByProductIDOld(ProdCategoryRangeGetReq request)
        //{
        //    List<ProductRangeDetails> result = new List<ProductRangeDetails>();
        //    FilterDefinition<mProductCategory> filterCat;
        //    filterCat = Builders<mProductCategory>.Filter.Empty;

        //    filterCat = filterCat & Builders<mProductCategory>.Filter.Where(f => request.ProductId.Contains(f.Product_Id));

        //    var resultCat = _MongoContext.mProductCategory.Find(filterCat).Project(p => new ProdCategoryRangeDetails
        //    { ProductCategoryId = p.VoyagerProductCategory_Id, ProductCategoryName = p.ProductCategoryName }).ToList();

        //    List<string> CateIds = new List<string>();

        //    CateIds = resultCat.Select(a => a.ProductCategoryId).ToList();

        //    FilterDefinition<mProductRange> filter;
        //    filter = Builders<mProductRange>.Filter.Empty;
        //    filter = filter & Builders<mProductRange>.Filter.Where(f => CateIds.Contains(f.ProductCategory_Id));

        //    var resultRange = _MongoContext.mProductRange.Find(filter).Project(r => new ProductRangeDetails
        //    {
        //        ProductId = r.Product_Id,
        //        ProductCategoryId = r.ProductCategory_Id,
        //        ProductRangeCode = r.ProductRangeCode,
        //        ProductRangeName = r.ProductRangeName,
        //        VoyagerProductRange_Id = r.VoyagerProductRange_Id,
        //        AdditionalYN = r.AdditionalYn,
        //        PersonType = r.PersonType,
        //        AgeRange = (r.Agemin == null || r.Agemin == "" || r.Agemax == null || r.Agemax == "") ? null : (r.Agemin + " - " + r.Agemax)
        //    }).ToList();

        //    for (int i = 0; i < resultCat.Count; i++)
        //    {
        //        for (int j = 0; j < resultRange.Count; j++)
        //        {
        //            if (resultCat[i].ProductCategoryId == resultRange[j].ProductCategoryId)
        //            {
        //                var obj = new ProductRangeDetails
        //                {
        //                    ProductCategoryId = resultCat[i].ProductCategoryId,
        //                    ProductCategoryName = resultCat[i].ProductCategoryName,
        //                    ProductRangeCode = resultRange[j].ProductRangeCode,
        //                    ProductRangeName = resultRange[j].ProductRangeName,
        //                    VoyagerProductRange_Id = resultRange[j].VoyagerProductRange_Id,
        //                    AdditionalYN = resultRange[j].AdditionalYN,
        //                    PersonType = resultRange[j].PersonType,
        //                    AgeRange = resultRange[j].AgeRange,
        //                    ProductId = resultRange[j].ProductId
        //                };
        //                result.Add(obj);
        //            }
        //        }
        //    }

        //    return result != null ? result.ToList() : (new List<ProductRangeDetails>());
        //}
        #endregion

        #region GetProductCategoryByParam 
        public List<ProdCategoryDetails> GetProductCategoryByParam(ProductCatGetReq request)
        {
            List<ProdCategoryDetails> lstProdCategoryDetails = new List<ProdCategoryDetails>();
            if (!string.IsNullOrEmpty(request.ProductName))
            {
                var prodCat = _MongoContext.Products.AsQueryable().Where(p => p.ProductName == request.ProductName).Select(p => new
                {
                    p.ProductCategories,
                    p.VoyagerProduct_Id
                }).ToList();

                foreach (var item in prodCat)
                {
                    var ProductCategories = item.ProductCategories.Where(a => string.IsNullOrEmpty(a.Status)).ToList();
                    if (ProductCategories?.Count > 0)
                    {
                        lstProdCategoryDetails.AddRange(ProductCategories.Select(a =>
                        new ProdCategoryDetails { ProductCategoryId = a.ProductCategory_Id, ProductCategoryName = a.ProductCategoryName, ProductId = item.VoyagerProduct_Id }).ToList());
                    }
                }
            }
            else if (!string.IsNullOrEmpty(request.ProductId))
            {
                var prodCat = _MongoContext.Products.AsQueryable().Where(p => p.VoyagerProduct_Id == request.ProductId).Select(p => new
                {
                    p.ProductCategories
                }).FirstOrDefault();

                lstProdCategoryDetails = prodCat.ProductCategories.Where(a => string.IsNullOrEmpty(a.Status)).Select(a =>
                  new ProdCategoryDetails { ProductCategoryId = a.ProductCategory_Id, ProductCategoryName = a.ProductCategoryName, ProductId = request.ProductId }).ToList();
            }
            else if (request.ProdCatIdList != null && request.ProdCatIdList.Count > 0)
            {
                var prod = _MongoContext.Products.AsQueryable().Where(p => p.ProductCategories.Any(a => request.ProdCatIdList.Contains(a.ProductCategory_Id))).
                    Select(a => new
                    {
                        a.ProductCategories,
                        a.VoyagerProduct_Id
                    }).ToList();

                var prodCat = prod.Select(p => new
                {
                    ProductCategories = p.ProductCategories.Where(a => string.IsNullOrEmpty(a.Status) && request.ProdCatIdList.Contains(a.ProductCategory_Id)).ToList(),
                    p.VoyagerProduct_Id
                }).ToList();

                foreach (var item in prodCat)
                {
                    if (item.ProductCategories?.Count > 0)
                    {
                        lstProdCategoryDetails.AddRange(item.ProductCategories.Select(a =>
                        new ProdCategoryDetails { ProductCategoryId = a.ProductCategory_Id, ProductCategoryName = a.ProductCategoryName, ProductId = item.VoyagerProduct_Id }).ToList());
                    }
                }
            }
            else if (request.ProductIdList != null && request.ProductIdList.Count > 0)
            {
                var prodCat = _MongoContext.Products.AsQueryable().Where(p => request.ProductIdList.Contains(p.VoyagerProduct_Id)).Select(p => new
                {
                    p.ProductCategories,
                    p.VoyagerProduct_Id
                }).ToList();
                foreach (var item in prodCat)
                {
                    var ProductCategories = item.ProductCategories.Where(a => string.IsNullOrEmpty(a.Status)).ToList();
                    if (ProductCategories?.Count > 0)
                    {
                        lstProdCategoryDetails.AddRange(ProductCategories.Select(a =>
                        new ProdCategoryDetails { ProductCategoryId = a.ProductCategory_Id, ProductCategoryName = a.ProductCategoryName, ProductId = item.VoyagerProduct_Id }).ToList());
                    }
                }
            }
            else
            {
                var prod = _MongoContext.Products.AsQueryable();
                var prodCat = prod.Select(p => new
                {
                    ProductCategories = p.ProductCategories.Where(b => string.IsNullOrEmpty(b.Status)).ToList(),
                    p.VoyagerProduct_Id
                }).ToList();

                foreach (var item in prodCat)
                {
                    if (item.ProductCategories?.Count > 0)
                    {
                        lstProdCategoryDetails.AddRange(item.ProductCategories.Select(a =>
                        new ProdCategoryDetails { ProductCategoryId = a.ProductCategory_Id, ProductCategoryName = a.ProductCategoryName, ProductId = item.VoyagerProduct_Id }).ToList());
                    }
                }
            }
            lstProdCategoryDetails = lstProdCategoryDetails.OrderBy(a => a.ProductCategoryName).ToList();
            return lstProdCategoryDetails ?? new List<ProdCategoryDetails>();
        }

        public List<ProdCategoryDetails> GetProductCategoryByParamOld(ProductCatGetReq request)
        {
            List<ProdCategoryDetails> lstProdCategoryDetails = new List<ProdCategoryDetails>();
            if (!string.IsNullOrEmpty(request.ProductName))
            {
                string prodID = _MongoContext.mProducts.AsQueryable().Where(p => p.ProdName == request.ProductName).Select(p => p.VoyagerProduct_Id).FirstOrDefault();
                if (!string.IsNullOrEmpty(prodID))
                {
                    lstProdCategoryDetails = _MongoContext.mProductCategory.AsQueryable().Where(p => p.Product_Id == prodID)
                .Select(m => new ProdCategoryDetails { ProductCategoryId = m.VoyagerProductCategory_Id, ProductCategoryName = m.ProductCategoryName, ProductId = m.Product_Id }).ToList();
                }
            }
            else if (!string.IsNullOrEmpty(request.ProductId))
            {
                lstProdCategoryDetails = _MongoContext.mProductCategory.AsQueryable().Where(p => p.Product_Id == request.ProductId)
                .Select(m => new ProdCategoryDetails { ProductCategoryId = m.VoyagerProductCategory_Id, ProductCategoryName = m.ProductCategoryName, ProductId = m.Product_Id }).ToList();
            }
            else if (request.ProdCatIdList != null && request.ProdCatIdList.Count > 0)
            {
                lstProdCategoryDetails = _MongoContext.mProductCategory.AsQueryable().Where(p => request.ProdCatIdList.Contains(p.VoyagerProductCategory_Id))
               .Select(m => new ProdCategoryDetails { ProductCategoryId = m.VoyagerProductCategory_Id, ProductCategoryName = m.ProductCategoryName, ProductId = m.Product_Id }).ToList();
            }
            else if (request.ProductIdList != null && request.ProductIdList.Count > 0)
            {
                lstProdCategoryDetails = _MongoContext.mProductCategory.AsQueryable().Where(p => request.ProductIdList.Contains(p.Product_Id))
                .Select(m => new ProdCategoryDetails { ProductCategoryId = m.VoyagerProductCategory_Id, ProductCategoryName = m.ProductCategoryName, ProductId = m.Product_Id }).ToList();
            }
            else
            {
                lstProdCategoryDetails = _MongoContext.mProductCategory.AsQueryable()
               .Select(m => new ProdCategoryDetails { ProductCategoryId = m.VoyagerProductCategory_Id, ProductCategoryName = m.ProductCategoryName, ProductId = m.Product_Id }).ToList();
            }
            lstProdCategoryDetails = lstProdCategoryDetails.OrderBy(a => a.ProductCategoryName).ToList();
            return lstProdCategoryDetails ?? new List<ProdCategoryDetails>();
        }
        #endregion

        #region GetProductRangeByParam 
        public ProductRangeGetRes GetProductRangeByParam(ProductRangeGetReq request)
        {
            ProductRangeGetRes ProductRangeRes = new ProductRangeGetRes();
            try
            {
                if (!string.IsNullOrEmpty(request.ProductName) && !string.IsNullOrEmpty(request.ProductCatId) && request.AdditionalYN != null)
                {
                    var prod = _MongoContext.Products.AsQueryable().Where(p => p.ProductName == request.ProductName).Select(p => new
                    {
                        p.ProductCategories,
                        p.VoyagerProduct_Id
                    }).ToList();

                    foreach (var item in prod)
                    {
                        var ProductCategories = item.ProductCategories.Where(b => string.IsNullOrEmpty(b.Status)).ToList();
                        if (ProductCategories?.Count > 0)
                        {
                            var prodCat = ProductCategories.Where(a => a.ProductCategory_Id == request.ProductCatId).FirstOrDefault();
                            var prodRange = prodCat.ProductRanges.Where(a => ((a.AdditionalYn == request.AdditionalYN) || (a.AdditionalYn == null && request.AdditionalYN == false)) && a.Status.ToLower() == "active").ToList();
                            if (prodRange?.Count > 0)
                            {
                                ProductRangeRes.ProductRangeDetails.AddRange(prodRange.Select(a =>
                                new ProductRangeDetails
                                {
                                    ProductRangeCode = a.ProductTemplateCode,
                                    ProductRangeName = a.ProductTemplateName,
                                    VoyagerProductRange_Id = a.ProductRange_Id,
                                    PersonType = a.PersonType,
                                    AgeRange = (a.Agemin == null || a.Agemin == "" || a.Agemax == null || a.Agemax == "") ? null : (a.Agemin + " - " + a.Agemax),
                                    AdditionalYN = a.AdditionalYn ?? false,
                                    ProductId = item.VoyagerProduct_Id,
                                    ProductCategoryId = prodCat.ProductCategory_Id,
                                    ProductMenu = a.ProductMenu
                                }).ToList());
                            }
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(request.ProductId) && !string.IsNullOrEmpty(request.ProductCatId) && request.AdditionalYN != null)
                {
                    var prod = _MongoContext.Products.AsQueryable().Where(p => p.VoyagerProduct_Id == request.ProductId).Select(p => new
                    {
                        p.ProductCategories,
                        p.VoyagerProduct_Id
                    }).ToList();

                    foreach (var item in prod)
                    {
                        var ProductCategories = item.ProductCategories.Where(b => string.IsNullOrEmpty(b.Status)).ToList();
                        if (ProductCategories?.Count > 0)
                        {
                            var prodCatInfo = ProductCategories.Where(a => a.ProductCategory_Id == request.ProductCatId).ToList();
                            foreach (var prodCat in prodCatInfo)
                            {
                                var prodRange = prodCat.ProductRanges.Where(a => a.Status.ToLower() == "active" && (a.AdditionalYn == request.AdditionalYN || (a.AdditionalYn == null && request.AdditionalYN == false))).ToList();
                                ProductRangeRes.ProductRangeDetails.AddRange(prodRange.Select(a =>
                                new ProductRangeDetails
                                {
                                    ProductRangeCode = a.ProductTemplateCode,
                                    ProductRangeName = a.ProductTemplateName,
                                    VoyagerProductRange_Id = a.ProductRange_Id,
                                    PersonType = a.PersonType,
                                    AgeRange = (a.Agemin == null || a.Agemin == "" || a.Agemax == null || a.Agemax == "") ? null : (a.Agemin + " - " + a.Agemax),
                                    AdditionalYN = a.AdditionalYn ?? false,
                                    ProductId = item.VoyagerProduct_Id,
                                    ProductCategoryId = prodCat.ProductCategory_Id,
                                    ProductMenu = a.ProductMenu
                                }).ToList());
                            }
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(request.ProductId) && string.IsNullOrEmpty(request.ProductCatId) && request.AdditionalYN != null)
                {
                    var prod = _MongoContext.Products.AsQueryable().Where(p => p.VoyagerProduct_Id == request.ProductId).Select(p => new
                    {
                        p.ProductCategories,
                        p.VoyagerProduct_Id
                    }).ToList();

                    foreach (var item in prod)
                    {
                        var ProductCategories = item.ProductCategories.Where(b => string.IsNullOrEmpty(b.Status)).ToList();
                        if (ProductCategories?.Count > 0)
                        {
                            foreach (var prodCat in ProductCategories)
                            {
                                var prodRange = prodCat.ProductRanges.Where(a => a.Status.ToLower() == "active" && (a.AdditionalYn == request.AdditionalYN || (a.AdditionalYn == null && request.AdditionalYN == false))).ToList();
                                if (prodRange?.Count > 0)
                                {
                                    ProductRangeRes.ProductRangeDetails.AddRange(prodRange.Select(a =>
                                       new ProductRangeDetails
                                       {
                                           ProductRangeCode = a.ProductTemplateCode,
                                           ProductRangeName = a.ProductTemplateName,
                                           VoyagerProductRange_Id = a.ProductRange_Id,
                                           PersonType = a.PersonType,
                                           AgeRange = (a.Agemin == null || a.Agemin == "" || a.Agemax == null || a.Agemax == "") ? null : (a.Agemin + " - " + a.Agemax),
                                           AdditionalYN = a.AdditionalYn ?? false,
                                           ProductId = item.VoyagerProduct_Id,
                                           ProductCategoryId = prodCat.ProductCategory_Id,
                                           ProductMenu = a.ProductMenu
                                       }).ToList());
                                }
                            }
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(request.ProductId) && !string.IsNullOrEmpty(request.ProductCatId) && request.AdditionalYN == null)
                {
                    var prod = _MongoContext.Products.AsQueryable().Where(p => p.VoyagerProduct_Id == request.ProductId).Select(p => new
                    {
                        p.ProductCategories,
                        p.VoyagerProduct_Id
                    }).ToList();

                    foreach (var item in prod)
                    {
                        var ProductCategories = item.ProductCategories.Where(b => string.IsNullOrEmpty(b.Status)).ToList();
                        if (ProductCategories?.Count > 0)
                        {
                            var prodCat = ProductCategories.Where(a => a.ProductCategory_Id == request.ProductCatId).FirstOrDefault();
                            ProductRangeRes.ProductRangeDetails.AddRange(prodCat.ProductRanges.Where(a => a.Status.ToLower() == "active").Select(a =>
                                new ProductRangeDetails
                                {
                                    ProductRangeCode = a.ProductTemplateCode,
                                    ProductRangeName = a.ProductTemplateName,
                                    VoyagerProductRange_Id = a.ProductRange_Id,
                                    PersonType = a.PersonType,
                                    AgeRange = (a.Agemin == null || a.Agemin == "" || a.Agemax == null || a.Agemax == "") ? null : (a.Agemin + " - " + a.Agemax),
                                    AdditionalYN = a.AdditionalYn ?? false,
                                    ProductId = item.VoyagerProduct_Id,
                                    ProductCategoryId = prodCat.ProductCategory_Id,
                                    ProductMenu = a.ProductMenu
                                }).ToList());
                        }
                    }
                }
                else if (request.ProductIdList != null && request.ProductIdList.Count > 0 && request.PersonTypeList != null && request.PersonTypeList.Count > 0)
                {
                    var prod = _MongoContext.Products.AsQueryable().Where(p => request.ProductIdList.Contains(p.VoyagerProduct_Id)).Select(p => new
                    {
                        p.ProductCategories,
                        p.VoyagerProduct_Id
                    }).ToList();

                    foreach (var item in prod)
                    {
                        var ProductCategories = item.ProductCategories.Where(b => string.IsNullOrEmpty(b.Status)).ToList();
                        if (ProductCategories?.Count > 0)
                        {
                            foreach (var prodCat in ProductCategories)
                            {
                                var prodRange = prodCat.ProductRanges.Where(a => a.Status.ToLower() == "active" && request.PersonTypeList.Contains(a.PersonType)).ToList();
                                if (prodRange?.Count > 0)
                                {
                                    ProductRangeRes.ProductRangeDetails.AddRange(prodRange.Select(a =>
                                    new ProductRangeDetails
                                    {
                                        ProductRangeCode = a.ProductTemplateCode,
                                        ProductRangeName = a.ProductTemplateName,
                                        VoyagerProductRange_Id = a.ProductRange_Id,
                                        PersonType = a.PersonType,
                                        AgeRange = (a.Agemin == null || a.Agemin == "" || a.Agemax == null || a.Agemax == "") ? null : (a.Agemin + " - " + a.Agemax),
                                        AdditionalYN = a.AdditionalYn ?? false,
                                        ProductId = item.VoyagerProduct_Id,
                                        ProductCategoryId = prodCat.ProductCategory_Id,
                                        ProductMenu = a.ProductMenu
                                    }).ToList());
                                }
                            }
                        }
                    }
                }
                else if (request.ProductRangeIdList != null && request.ProductRangeIdList.Count > 0)
                {
                    var prod = _MongoContext.Products.AsQueryable();
                    var prodInfo = prod.Where(a => a.ProductCategories.Any(b => b.ProductRanges.Any(c => request.ProductRangeIdList.Contains(c.ProductRange_Id)))).ToList();

                    foreach (var item in prodInfo)
                    {
                        if (item.ProductCategories?.Count > 0)
                        {
                            item.ProductCategories = item.ProductCategories.Where(b => string.IsNullOrEmpty(b.Status)).ToList();
                            foreach (var prodCat in item.ProductCategories)
                            {
                                var prodRangeInfo = prodCat.ProductRanges.Where(a => a.Status.ToLower() == "active" && request.ProductRangeIdList.Contains(a.ProductRange_Id)).ToList();
                                if (prodRangeInfo?.Count > 0)
                                {
                                    ProductRangeRes.ProductRangeDetails.AddRange(prodRangeInfo.Select(a =>
                                   new ProductRangeDetails
                                   {
                                       ProductRangeCode = a.ProductTemplateCode,
                                       ProductRangeName = a.ProductTemplateName,
                                       VoyagerProductRange_Id = a.ProductRange_Id,
                                       PersonType = a.PersonType,
                                       AgeRange = (a.Agemin == null || a.Agemin == "" || a.Agemax == null || a.Agemax == "") ? null : (a.Agemin + " - " + a.Agemax),
                                       AdditionalYN = a.AdditionalYn ?? false,
                                       ProductId = item.VoyagerProduct_Id,
                                       ProductCategoryId = prodCat.ProductCategory_Id,
                                       ProductMenu = a.ProductMenu
                                   }).ToList());
                                }
                            }
                        }
                    }
                }
                else if (request.ProductIdList != null && request.ProductIdList.Count > 0 && request.ProductCatIdList != null && request.ProductCatIdList.Count > 0)
                {
                    var prod = _MongoContext.Products.AsQueryable().Where(p => request.ProductIdList.Contains(p.VoyagerProduct_Id)).Select(p => new
                    {
                        p.ProductCategories,
                        p.VoyagerProduct_Id
                    }).ToList();

                    foreach (var item in prod)
                    {
                        var prodCatInfo = item.ProductCategories.Where(a => string.IsNullOrEmpty(a.Status) && request.ProductCatIdList.Contains(a.ProductCategory_Id)).ToList();
                        foreach (var prodCat in prodCatInfo)
                        {
                            ProductRangeRes.ProductRangeDetails.AddRange(prodCat.ProductRanges.Where(a => a.Status.ToLower() == "active").Select(a =>
                                   new ProductRangeDetails
                                   {
                                       ProductRangeCode = a.ProductTemplateCode,
                                       ProductRangeName = a.ProductTemplateName,
                                       VoyagerProductRange_Id = a.ProductRange_Id,
                                       PersonType = a.PersonType,
                                       AgeRange = (a.Agemin == null || a.Agemin == "" || a.Agemax == null || a.Agemax == "") ? null : (a.Agemin + " - " + a.Agemax),
                                       AdditionalYN = a.AdditionalYn ?? false,
                                       ProductId = item.VoyagerProduct_Id,
                                       ProductCategoryId = prodCat.ProductCategory_Id,
                                       ProductMenu = a.ProductMenu
                                   }).ToList());
                        }
                    }
                }
                else if (request.ProductIdList != null && request.ProductIdList.Count > 0 && (request.ProductCatIdList == null || request.ProductCatIdList.Count == 0))
                {
                    var prod = _MongoContext.Products.AsQueryable().Where(p => request.ProductIdList.Contains(p.VoyagerProduct_Id)).Select(p => new
                    {
                        p.ProductCategories,
                        p.VoyagerProduct_Id
                    }).ToList();

                    foreach (var item in prod)
                    {
                        var ProductCategories = item.ProductCategories.Where(b => string.IsNullOrEmpty(b.Status)).ToList();
                        if (ProductCategories?.Count > 0)
                        {
                            foreach (var prodCat in ProductCategories)
                            {
                                ProductRangeRes.ProductRangeDetails.AddRange(prodCat.ProductRanges.Where(a => a.Status.ToLower() == "active").Select(a =>
                                    new ProductRangeDetails
                                    {
                                        ProductRangeCode = a.ProductTemplateCode,
                                        ProductRangeName = a.ProductTemplateName,
                                        VoyagerProductRange_Id = a.ProductRange_Id,
                                        PersonType = a.PersonType,
                                        AgeRange = (a.Agemin == null || a.Agemin == "" || a.Agemax == null || a.Agemax == "") ? null : (a.Agemin + " - " + a.Agemax),
                                        AdditionalYN = a.AdditionalYn ?? false,
                                        ProductId = item.VoyagerProduct_Id,
                                        ProductCategoryId = prodCat.ProductCategory_Id,
                                        ProductMenu = a.ProductMenu
                                    }).ToList());
                            }
                        }
                    }
                }
                ProductRangeRes.ProductRangeDetails = ProductRangeRes?.ProductRangeDetails?.OrderBy(q => q.PersonType).ToList();
                //if (request.QRFId <= 0)
                //{
                var resultDefaultRange = GetDefaultProductRangeFromQuote(request);
                if (resultDefaultRange != null && resultDefaultRange.Count > 0)
                {
                    ProductRangeRes.DefProdRangelist = resultDefaultRange.ToList();
                }
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return ProductRangeRes ?? new ProductRangeGetRes();
        }

        public ProductRangeGetRes GetProductRangeByParamOld(ProductRangeGetReq request)
        {
            ProductRangeGetRes ProductRangeRes = new ProductRangeGetRes();
            try
            {
                if (!string.IsNullOrEmpty(request.ProductName) && !string.IsNullOrEmpty(request.ProductCatId) && request.AdditionalYN != null)
                {
                    string prodID = _MongoContext.mProducts.AsQueryable().Where(p => p.ProdName == request.ProductName).Select(p => p.VoyagerProduct_Id).FirstOrDefault();
                    if (!string.IsNullOrEmpty(prodID))
                    {
                        ProductRangeRes.ProductRangeDetails = _MongoContext.mProductRange.AsQueryable().Where(p => p.Product_Id == prodID && p.ProductCategory_Id == request.ProductCatId && ((p.AdditionalYn == request.AdditionalYN) || (p.AdditionalYn == null && request.AdditionalYN == false)))
                        .Select(p => new ProductRangeDetails { ProductRangeCode = p.ProductRangeCode, ProductRangeName = p.ProductRangeName, VoyagerProductRange_Id = p.VoyagerProductRange_Id, PersonType = p.PersonType, AgeRange = (p.Agemin == null || p.Agemin == "" || p.Agemax == null || p.Agemax == "") ? null : (p.Agemin + " - " + p.Agemax), AdditionalYN = p.AdditionalYn }).OrderBy(q => q.PersonType).ToList();
                    }
                }
                else if (!string.IsNullOrEmpty(request.ProductId) && !string.IsNullOrEmpty(request.ProductCatId) && request.AdditionalYN != null)
                {
                    ProductRangeRes.ProductRangeDetails = _MongoContext.mProductRange.AsQueryable().Where(p => p.Product_Id == request.ProductId && p.ProductCategory_Id == request.ProductCatId && ((p.AdditionalYn == request.AdditionalYN) || (p.AdditionalYn == null && request.AdditionalYN == false)))
                    .Select(p => new ProductRangeDetails { ProductRangeCode = p.ProductRangeCode, ProductRangeName = p.ProductRangeName, VoyagerProductRange_Id = p.VoyagerProductRange_Id, PersonType = p.PersonType, AgeRange = (p.Agemin == null || p.Agemin == "" || p.Agemax == null || p.Agemax == "") ? null : (p.Agemin + " - " + p.Agemax), AdditionalYN = p.AdditionalYn }).OrderBy(q => q.PersonType).ToList();
                }
                else if (!string.IsNullOrEmpty(request.ProductId) && string.IsNullOrEmpty(request.ProductCatId) && request.AdditionalYN != null)
                {
                    ProductRangeRes.ProductRangeDetails = _MongoContext.mProductRange.AsQueryable().Where(p => p.Product_Id == request.ProductId && ((p.AdditionalYn == request.AdditionalYN) || (p.AdditionalYn == null && request.AdditionalYN == false)))
                   .Select(p => new ProductRangeDetails { ProductRangeCode = p.ProductRangeCode, ProductRangeName = p.ProductRangeName, VoyagerProductRange_Id = p.VoyagerProductRange_Id, PersonType = p.PersonType, AgeRange = (p.Agemin == null || p.Agemin == "" || p.Agemax == null || p.Agemax == "") ? null : (p.Agemin + " - " + p.Agemax), AdditionalYN = p.AdditionalYn }).OrderBy(q => q.PersonType).ToList();
                }
                else if (!string.IsNullOrEmpty(request.ProductId) && !string.IsNullOrEmpty(request.ProductCatId) && request.AdditionalYN == null)
                {
                    ProductRangeRes.ProductRangeDetails = _MongoContext.mProductRange.AsQueryable().Where(p => p.Product_Id == request.ProductId && p.ProductCategory_Id == request.ProductCatId)
                   .Select(p => new ProductRangeDetails { ProductRangeCode = p.ProductRangeCode, ProductRangeName = p.ProductRangeName, VoyagerProductRange_Id = p.VoyagerProductRange_Id, PersonType = p.PersonType, AgeRange = (p.Agemin == null || p.Agemin == "" || p.Agemax == null || p.Agemax == "") ? null : (p.Agemin + " - " + p.Agemax), AdditionalYN = p.AdditionalYn }).OrderBy(q => q.PersonType).ToList();
                }
                else if (request.ProductIdList != null && request.ProductIdList.Count > 0 && request.PersonTypeList != null && request.PersonTypeList.Count > 0)
                {
                    ProductRangeRes.ProductRangeDetails = _MongoContext.mProductRange.AsQueryable().Where(p => request.ProductIdList.Contains(p.Product_Id) && request.PersonTypeList.Contains(p.PersonType))
                   .Select(p => new ProductRangeDetails { ProductRangeCode = p.ProductRangeCode, ProductRangeName = p.ProductRangeName, VoyagerProductRange_Id = p.VoyagerProductRange_Id, PersonType = p.PersonType, AgeRange = (p.Agemin == null || p.Agemin == "" || p.Agemax == null || p.Agemax == "") ? null : (p.Agemin + " - " + p.Agemax), AdditionalYN = p.AdditionalYn, ProductId = p.Product_Id, ProductCategoryId = p.ProductCategory_Id }).OrderBy(q => q.PersonType).ToList();
                }
                else if (request.ProductRangeIdList != null && request.ProductRangeIdList.Count > 0)
                {
                    ProductRangeRes.ProductRangeDetails = _MongoContext.mProductRange.AsQueryable().Where(p => request.ProductRangeIdList.Contains(p.VoyagerProductRange_Id))
                   .Select(p => new ProductRangeDetails { VoyagerProductRange_Id = p.VoyagerProductRange_Id, ProductCategoryId = p.ProductCategory_Id, ProductMenu = p.ProductMenu, ProductRangeName = p.ProductRangeName, AdditionalYN = p.AdditionalYn }).ToList();
                }
                else if (request.ProductIdList != null && request.ProductIdList.Count > 0 && request.ProductCatIdList != null && request.ProductCatIdList.Count > 0)
                {
                    ProductRangeRes.ProductRangeDetails = _MongoContext.mProductRange.AsQueryable().Where(p => request.ProductIdList.Contains(p.Product_Id) && request.ProductCatIdList.Contains(p.ProductCategory_Id))
                   .Select(p => new ProductRangeDetails { ProductRangeCode = p.ProductRangeCode, ProductRangeName = p.ProductRangeName, VoyagerProductRange_Id = p.VoyagerProductRange_Id, PersonType = p.PersonType, AgeRange = (p.Agemin == null || p.Agemin == "" || p.Agemax == null || p.Agemax == "") ? null : (p.Agemin + " - " + p.Agemax), AdditionalYN = p.AdditionalYn, ProductId = p.Product_Id, ProductCategoryId = p.ProductCategory_Id, ProductMenu = p.ProductMenu }).OrderBy(q => q.PersonType).ToList();
                }
                else if (request.ProductIdList != null && request.ProductIdList.Count > 0 && (request.ProductCatIdList == null || request.ProductCatIdList.Count == 0))
                {
                    ProductRangeRes.ProductRangeDetails = _MongoContext.mProductRange.AsQueryable().Where(p => request.ProductIdList.Contains(p.Product_Id))
                   .Select(p => new ProductRangeDetails { ProductRangeCode = p.ProductRangeCode, ProductRangeName = p.ProductRangeName, VoyagerProductRange_Id = p.VoyagerProductRange_Id, PersonType = p.PersonType, AgeRange = (p.Agemin == null || p.Agemin == "" || p.Agemax == null || p.Agemax == "") ? null : (p.Agemin + " - " + p.Agemax), AdditionalYN = p.AdditionalYn, ProductId = p.Product_Id, ProductCategoryId = p.ProductCategory_Id, ProductMenu = p.ProductMenu }).OrderBy(q => q.PersonType).ToList();
                }

                //if (request.QRFId <= 0)
                //{
                var resultDefaultRange = GetDefaultProductRangeFromQuote(request);
                if (resultDefaultRange != null && resultDefaultRange.Count > 0)
                {
                    ProductRangeRes.DefProdRangelist = resultDefaultRange.ToList();
                }
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return ProductRangeRes ?? new ProductRangeGetRes();
        }

        #endregion

        #region GetContractRatesByProductID 
        public ProdContractGetRes GetContractRatesByProductID(ProdContractGetReq request, List<string> ranges = null)
        {
            ProdContractGetRes prodContractGetRes = new ProdContractGetRes();
            try
            {
                var result = (from pc in _MongoContext.mProductContract.AsQueryable()
                              join pp in _MongoContext.mPricePeriod.AsQueryable() on pc.VoyagerProductContract_Id equals pp.ProductContract_Id
                              join ppr in _MongoContext.mProductPrice.AsQueryable() on pp.VoyagerPricePeriod_Id equals ppr.PricePeriod_Id
                              join cc in _MongoContext.mCurrency.AsQueryable() on pc.Currency_Id equals cc.VoyagerCurrency_Id
                              where request.ProductIDList.Contains(pc.Product_Id)
                              //&& (pc.Agent_Id == request.AgentId || request.AgentId == null)
                              && (pc.Supplier_Id == request.SupplierId || request.SupplierId == null)
                              select new ProductContractInfo
                              {
                                  ProductRangeId = ppr.ProductRange_Id,
                                  ProductId = pp.Product_Id,
                                  DayComboPattern = pp.DayComboPattern,
                                  Price = ppr.Price,
                                  FromDate = pp.Datemin,
                                  ToDate = pp.Datemax,
                                  CurrencyId = pc.Currency_Id,
                                  Currency = cc.Currency,
                                  ContractId = pc.VoyagerProductContract_Id,
                                  SupplierId = pc.Supplier_Id
                              }).Distinct().ToList();

                if (result != null && result.Count > 0)
                {
                    if (ranges?.Count > 0)
                        prodContractGetRes.ProductContractInfo = result.Where(a => ranges.Any(b => b == a.ProductRangeId)).ToList();
                    else
                        prodContractGetRes.ProductContractInfo = result;
                    return (prodContractGetRes.ProductContractInfo?.Count > 0) ? prodContractGetRes : (new ProdContractGetRes());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return prodContractGetRes;
        }
        #endregion

        #region GetProdCatDefByName
        public List<ProdCatDefProperties> GetProdCatDefByName(ProdCatDefGetReq request)
        {
            List<ProdCatDefProperties> result = new List<ProdCatDefProperties>();
            if (!string.IsNullOrEmpty(request.Name))
            {
                result = _MongoContext.mProdCatDef.AsQueryable()
                      .Where(c => c.Standard == true && c.Name.ToLower().Contains(request.Name.ToLower().Trim()))
                      .Select(c => new ProdCatDefProperties { Name = c.Name, VoyagerDefProductCategoryId = c.VoyagerDefProductCategoryId }).Distinct().ToList();
            }
            else
            {
                result = _MongoContext.mProdCatDef.AsQueryable()
                    .Where(c => c.Standard == true)
                    .Select(c => new ProdCatDefProperties { Name = c.Name, VoyagerDefProductCategoryId = c.VoyagerDefProductCategoryId }).Distinct().ToList();

            }
            result = result.OrderBy(a => a.Name).ToList();
            return result ?? new List<ProdCatDefProperties>();
        }
        #endregion

        #region GetProdCatDef
        public List<ProdCatDefProperties> GetProdCatDef()
        {
            List<ProdCatDefProperties> result = new List<ProdCatDefProperties>();

            result = _MongoContext.mProdCatDef.AsQueryable()
                .Where(c => c.Standard == true)
                .Select(c => new ProdCatDefProperties { Name = c.Name, VoyagerDefProductCategoryId = c.VoyagerDefProductCategoryId }).Distinct().ToList();

            result = result.OrderBy(a => a.Name).ToList();
            return result ?? new List<ProdCatDefProperties>();
        }
        #endregion

        #region GetProdCatDefById
        public List<ProdCatDefProperties> GetProdCatDefById(ProdCatDefGetReq request)
        {
            List<ProdCatDefProperties> result = new List<ProdCatDefProperties>();
            if (!string.IsNullOrEmpty(request.Name))
            {
                result = _MongoContext.mProductCategory.AsQueryable()
                      .Where(c => c.Product_Id == request.Name)
                      .Select(c => new ProdCatDefProperties { Name = c.ProductCategoryName, VoyagerDefProductCategoryId = c.VoyagerProductCategory_Id }).Distinct().ToList();
            }
            else
            {
                result = _MongoContext.mProdCatDef.AsQueryable()
                    .Where(c => c.Standard == true)
                    .Select(c => new ProdCatDefProperties { Name = c.Name, VoyagerDefProductCategoryId = c.VoyagerDefProductCategoryId }).Distinct().ToList();

            }
            return result ?? new List<ProdCatDefProperties>();
        }
        #endregion

        #region GetProdAttributeDetailsByNameOrVal
        public List<ProductAttributeDetails> GetProdAttributeDetailsByNameOrVal(ProductAttributeReq request)
        {
            List<ProductAttributeDetails> lstProductAttributeDetails = new List<ProductAttributeDetails>();
            List<mProductAttribute> lstProductAttribute = new List<mProductAttribute>();
            List<mAttributeValues> lstAttributeValues = new List<mAttributeValues>();

            lstProductAttribute = !string.IsNullOrEmpty(request.AttributeName) ? (_MongoContext.mProductAttribute.AsQueryable()
                                  .Where(pa => pa.AttributeName.ToLower() == request.AttributeName.ToLower()).ToList()) : (_MongoContext.mProductAttribute.AsQueryable().ToList());

            if (!string.IsNullOrEmpty(request.Attributevalue))
            {
                if (!string.IsNullOrEmpty(request.Status) && request.Status == "chain")
                {
                    lstAttributeValues = (_MongoContext.mAttributeValues.AsQueryable().Where(av => av.Value.ToLower().Contains(request.Attributevalue.ToLower()))).Distinct().ToList();
                }
                else
                {
                    lstAttributeValues = (_MongoContext.mAttributeValues.AsQueryable().Where(av => av.Value.ToLower() == request.Attributevalue.ToLower())).Distinct().ToList();
                }
            }
            else if (lstProductAttribute != null && lstProductAttribute.Count > 0)
            {
                var prodids = lstProductAttribute.Select(b => b.VoyagerAttribute_Id).ToList();
                lstAttributeValues = _MongoContext.mAttributeValues.AsQueryable().Where(a => prodids.Contains(a.Attribute_Id)).ToList();
            }
            else
            {
                lstAttributeValues = _MongoContext.mAttributeValues.AsQueryable().ToList();
            }

            if (lstProductAttribute != null && lstProductAttribute.Count > 0 && lstAttributeValues != null && lstAttributeValues.Count > 0)
            {
                lstProductAttributeDetails = (from pa in lstProductAttribute
                                              join av in lstAttributeValues on pa.VoyagerAttribute_Id equals av.Attribute_Id
                                              select new ProductAttributeDetails { AttributeId = av.VoyagerAttributeValues_Id, Value = av.Value }).Distinct().ToList();
            }
            lstProductAttributeDetails = lstProductAttributeDetails.OrderBy(a => a.Value).ToList();
            return lstProductAttributeDetails ?? new List<ProductAttributeDetails>();
        }
        #endregion

        #region GetDefaultRoomsFromQuote 
        public List<ProdCategoryRangeDetails> GetDefaultRoomsFromQuote(ProdCategoryRangeGetReq request)
        {
            List<ProdCategoryRangeDetails> result = new List<ProdCategoryRangeDetails>();
            List<AgentRoom> rooms = _MongoContext.mQuote.AsQueryable().Where(q => q.QRFID == request.QRFID).Select(d => d.AgentRoom).FirstOrDefault();

            List<string> roomTypeName = new List<string>();
            foreach (var data in rooms)
            {
                roomTypeName.Add(data.RoomTypeName.ToUpper());
            }

            FilterDefinition<mProductRange> filter;
            filter = Builders<mProductRange>.Filter.Empty;

            filter = filter & Builders<mProductRange>.Filter.Where(f => roomTypeName.Contains(f.ProductRangeName) && f.Product_Id == request.ProductId[0].Trim() & (f.AdditionalYn == false || f.AdditionalYn == null));

            var resultRange = _MongoContext.mProductRange.Find(filter).Project(r => new ProdCategoryRangeDetails
            { ProductCategoryId = r.ProductCategory_Id, ProductRangeCode = r.ProductRangeCode, ProductRangeName = r.ProductRangeName, VoyagerProductRange_Id = r.VoyagerProductRange_Id, PersonType = r.PersonType, AgeRange = (r.Agemin == null || r.Agemin == "" || r.Agemax == null || r.Agemax == "") ? null : (r.Agemin + " - " + r.Agemax) }).ToList();


            List<string> CateIds = new List<string>();
            foreach (var data in resultRange)
            {
                CateIds.Add(data.ProductCategoryId);
            }

            FilterDefinition<mProductCategory> filterCat;
            filterCat = Builders<mProductCategory>.Filter.Empty;

            filterCat = filterCat & Builders<mProductCategory>.Filter.Where(f => CateIds.Contains(f.VoyagerProductCategory_Id) && f.Default == true && f.Product_Id == request.ProductId[0].Trim());

            var resultCat = _MongoContext.mProductCategory.Find(filterCat).Project(p => new ProdCategoryRangeDetails
            { ProductCategoryId = p.VoyagerProductCategory_Id, ProductCategoryName = p.ProductCategoryName }).ToList();

            for (int i = 0; i < resultCat.Count; i++)
            {
                for (int j = 0; j < resultRange.Count; j++)
                {
                    if (resultCat[i].ProductCategoryId == resultRange[j].ProductCategoryId)
                    {
                        var obj = new ProdCategoryRangeDetails
                        {
                            ProductCategoryId = resultCat[i].ProductCategoryId,
                            ProductCategoryName = resultCat[i].ProductCategoryName,
                            ProductRangeCode = resultRange[j].ProductRangeCode,
                            ProductRangeName = resultRange[j].ProductRangeName,
                            VoyagerProductRange_Id = resultRange[j].VoyagerProductRange_Id,
                            PersonType = resultRange[j].PersonType,
                            AgeRange = resultRange[j].AgeRange
                        };
                        result.Add(obj);
                    }
                }
            }

            return result != null ? result.OrderBy(a => a.ProductRangeName).ToList() : (new List<ProdCategoryRangeDetails>());
        }
        #endregion

        #region GetDefaultProductRangeFromQuote 
        public List<ProductRangeDetails> GetDefaultProductRangeFromQuote(ProductRangeGetReq request)
        {
            List<ProductRangeDetails> result = new List<ProductRangeDetails>();
            try
            {
                var PaxInfoList = _MongoContext.mQuote.AsQueryable().Where(q => q.QRFID == request.QRFID).Select(d => d.AgentPassengerInfo).FirstOrDefault();
                ProductRangeDetails ProdRange = new ProductRangeDetails();

                if (PaxInfoList != null && PaxInfoList.Count > 0)
                {
                    var prod = _MongoContext.Products.AsQueryable().Where(a => a.VoyagerProduct_Id == request.ProductId).FirstOrDefault();
                    var prodCat = prod?.ProductCategories.Where(a => a.ProductCategory_Id == request.ProductCatId && string.IsNullOrEmpty(a.Status)).FirstOrDefault();
                    if (prodCat != null)
                    {
                        var ProdRangeList = prodCat.ProductRanges.Where(a => a.ProductTemplateCode == "TICKET" && a.Status.ToLower() == "active").ToList();
                        foreach (AgentPassengerInfo PaxInfo in PaxInfoList)
                        {
                            if (PaxInfo.Type == "ADULT")
                            {
                                var ProdRangeVal = ProdRangeList.Where(a => a.PersonType == "ADULT").FirstOrDefault();
                                if (ProdRangeVal != null)
                                {
                                    ProdRange = new ProductRangeDetails
                                    {
                                        VoyagerProductRange_Id = ProdRangeVal.ProductRange_Id,
                                        ProductRangeCode = ProdRangeVal.ProductTemplateCode,
                                        ProductRangeName = ProdRangeVal.ProductTemplateName,
                                        ProductCategoryId = prodCat.ProductCategory_Id,
                                        ProductCategoryName = prodCat.ProductCategoryName,
                                        PersonType = ProdRangeVal.PersonType,
                                        AgeRange = (ProdRangeVal.Agemin == null || ProdRangeVal.Agemin == "" || ProdRangeVal.Agemax == null || ProdRangeVal.Agemax == "") ? null : (ProdRangeVal.Agemin + " - " + ProdRangeVal.Agemax),
                                        AdditionalYN = ProdRangeVal.AdditionalYn ?? false
                                    };
                                    result.Add(ProdRange);
                                }
                            }
                            else if (PaxInfo.Type == "CHILDWITHBED" || PaxInfo.Type == "CHILDWITHOUTBED" || PaxInfo.Type == "INFANT")
                            {
                                foreach (int age in PaxInfo.Age)
                                {
                                    if (PaxInfo.count > 0)
                                    {
                                        foreach (var prodRange in ProdRangeList.Where(a => a.PersonType == "CHILD" || a.PersonType == "INFANT").ToList())
                                        {
                                            if (age >= Convert.ToInt32(prodRange.Agemin) && age <= Convert.ToInt32(prodRange.Agemax))
                                            {
                                                if (result.Where(a => a.VoyagerProductRange_Id == prodRange.ProductRange_Id).Count() < 1)
                                                {
                                                    ProdRange = new ProductRangeDetails
                                                    {
                                                        VoyagerProductRange_Id = prodRange.ProductRange_Id,
                                                        ProductRangeCode = prodRange.ProductTemplateCode,
                                                        ProductRangeName = prodRange.ProductTemplateName,
                                                        ProductCategoryId = prodCat.ProductCategory_Id,
                                                        ProductCategoryName = prodCat.ProductCategoryName,
                                                        PersonType = prodRange.PersonType,
                                                        AgeRange = (prodRange.Agemin == null || prodRange.Agemin == "" || prodRange.Agemax == null || prodRange.Agemax == "") ? null : (prodRange.Agemin + " - " + prodRange.Agemax),
                                                        AdditionalYN = prodRange.AdditionalYn ?? false
                                                    };
                                                    result.Add(ProdRange);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return result != null ? result.ToList() : (new List<ProductRangeDetails>());
        }
        #endregion

        #region Get product supplier  
        public ProductSupplierGetRes GetSupplierDetails(ProductSupplierGetReq request)
        {
            ProductSupplierGetRes objProductSupplierGetRes = new ProductSupplierGetRes();
            try
            {
                if (!string.IsNullOrEmpty(request.ProductId) && request.ProductId != "0")
                {
                    var SupplierDetails = _MongoContext.mProductSupplier.AsQueryable().Where(p => p.Product_Id == request.ProductId && p.DafaultSupplier == true).FirstOrDefault();
                    objProductSupplierGetRes.SupplierId = SupplierDetails.Company_Id;
                    if (!string.IsNullOrEmpty(objProductSupplierGetRes.SupplierId) && objProductSupplierGetRes.SupplierId != "0")
                    {
                        objProductSupplierGetRes.SupplierName = _MongoContext.mCompany.AsQueryable().Where(c => c.VoyagerCompany_Id == objProductSupplierGetRes.SupplierId).Select(s => s.Name).FirstOrDefault();
                    }
                }
                else if (request.ProductIdList != null && request.ProductIdList.Count > 0)
                {
                    var SupplierDetails = _MongoContext.mProductSupplier.AsQueryable().Where(p => request.ProductIdList.Contains(p.Product_Id) && p.DafaultSupplier == true).ToList();
                    if (SupplierDetails != null && SupplierDetails.Count > 0)
                    {
                        var supplierids = SupplierDetails.Select(a => a.Company_Id).ToList();
                        var rescompny = _MongoContext.mCompany.AsQueryable().Where(c => supplierids.Contains(c.VoyagerCompany_Id)).ToList();
                        if (rescompny != null && rescompny.Count > 0)
                        {
                            objProductSupplierGetRes.SupllierList = (from a in rescompny
                                                                     join b in SupplierDetails on a.VoyagerCompany_Id equals b.Company_Id
                                                                     select new SupplierData { CurrencyId = b.Currency_Id, ProdId = b.Product_Id, SupplierId = a.VoyagerCompany_Id, SupplierName = a.Name }).ToList();
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return objProductSupplierGetRes;
        }

        public ProductSupplierGetRes GetProductSupplierList(ProductSupplierGetReq request)
        {
            ProductSupplierGetRes objProductSupplierGetRes = new ProductSupplierGetRes();
            try
            {
                if (!string.IsNullOrEmpty(request.ProductId) && request.ProductId != "0")
                {
                    //var SupplierDetails = _MongoContext.mProductSupplier.AsQueryable().Where(p => p.Product_Id == request.ProductId && !string.IsNullOrEmpty(p.Company_Id) && !(p.Status == "X" || p.Status == "-")).ToList();
                    //var supplierIdList = SupplierDetails.Select(x => x.Company_Id).ToList();
                    //var supplierList = _MongoContext.mCompany.AsQueryable().Where(a => supplierIdList.Contains(a.VoyagerCompany_Id)).ToList();
                    //var curIdsList = SupplierDetails.Select(a => a.Currency_Id).ToList();
                    //var curList = _MongoContext.mCurrency.AsQueryable().Where(a => curIdsList.Contains(a.VoyagerCurrency_Id)).Select(a => new { a.VoyagerCurrency_Id, a.Currency }).ToList();

                    var SupplierDetails = _MongoContext.Products.AsQueryable().Where(p => p.VoyagerProduct_Id == request.ProductId && !(p.Status == "X" || p.Status == "-")).FirstOrDefault()?.ProductSuppliers.Where(a => a.Status != "X" && a.Status != "-").ToList();
                    var supplierIdList = SupplierDetails.Select(x => x.Company_Id).ToList();
                    var supplierList = _MongoContext.mCompany.AsQueryable().Where(a => supplierIdList.Contains(a.VoyagerCompany_Id)).ToList();
                    var curIdsList = SupplierDetails.Select(a => a.CurrencyId).ToList();
                    var curList = _MongoContext.mCurrency.AsQueryable().Where(a => curIdsList.Contains(a.VoyagerCurrency_Id)).Select(a => new { a.VoyagerCurrency_Id, a.Currency }).ToList();

                    foreach (var supp in SupplierDetails)
                    {
                        var Company = supplierList.Where(c => c.VoyagerCompany_Id == supp.Company_Id).FirstOrDefault();
                        if (Company != null)
                        {
                            var supplier = new SupplierData();
                            supplier.ProdId = request.ProductId;
                            supplier.SupplierId = supp.Company_Id;
                            supplier.SupplierName = supp.CompanyName;
                            supplier.CurrencyId = supp.CurrencyId;
                            supplier.Currency = supp.CurrencyName;
                            supplier.CityName = Company.CityName;
                            objProductSupplierGetRes.SupllierList.Add(supplier);
                        }
                    }
                    objProductSupplierGetRes.SupllierList = objProductSupplierGetRes.SupllierList.OrderBy(a => a.SupplierName).ToList();

                    if (request.IsContractRateRequired)
                    {
                        #region Get Contract Rate
                        var lstProductList = new List<string>();
                        lstProductList.Add(request.ProductId);

                        var resultContract = (from pc in _MongoContext.mProductContract.AsQueryable()
                                              join pp in _MongoContext.mPricePeriod.AsQueryable() on pc.VoyagerProductContract_Id equals pp.ProductContract_Id
                                              join ppr in _MongoContext.mProductPrice.AsQueryable() on pp.VoyagerPricePeriod_Id equals ppr.PricePeriod_Id
                                              join cc in _MongoContext.mCurrency.AsQueryable() on pc.Currency_Id equals cc.VoyagerCurrency_Id
                                              where lstProductList.Contains(pc.Product_Id)
                                              select new ProductContractInfo
                                              {
                                                  ProductRangeId = ppr.ProductRange_Id,
                                                  ProductId = pp.Product_Id,
                                                  DayComboPattern = pp.DayComboPattern,
                                                  Price = ppr.Price,
                                                  FromDate = pp.Datemin,
                                                  ToDate = pp.Datemax,
                                                  CurrencyId = pc.Currency_Id,
                                                  Currency = cc.Currency,
                                                  ContractId = pc.VoyagerProductContract_Id,
                                                  SupplierId = pc.Supplier_Id
                                              }).Distinct().ToList();
                        if (resultContract?.Count > 0)
                        {
                            objProductSupplierGetRes.ProductContracts = resultContract;
                        }
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return objProductSupplierGetRes;
        }
        #endregion

        #region GetProductTypeByName
        public List<mProductType> GetProductTypeByProdType(ProdTypeGetReq request)
        {
            List<mProductType> result = new List<mProductType>();
            if (string.IsNullOrEmpty(request.ProdType))
            {
                result = _MongoContext.mProductType.AsQueryable().ToList();
            }
            else
            {
                result = _MongoContext.mProductType.AsQueryable().Where(c => c.Prodtype == request.ProdType).Select(x => new mProductType { Prodtype = x.Prodtype, VoyagerProductType_Id = x.VoyagerProductType_Id }).ToList();
            }
            return result ?? new List<mProductType>();
        }
        #endregion

        #region GetNationalityList
        public ProdNationalityGetRes GetNationalityList(string CompanyId)
        {
            ProdNationalityGetRes result = new ProdNationalityGetRes();
            var NationalityList = _MongoContext.mResort.AsQueryable().Where(a => a.Nation != null && a.Nation != "").Select(a => a.Nation).Distinct().OrderBy(val => val);
            result.NationalityList = NationalityList.Select(a => new AttributeValues { AttributeValue_Id = a, Value = a }).ToList();

            if (!string.IsNullOrEmpty(CompanyId))
            {
                string CountryId = _MongoContext.mCompanies.AsQueryable().Where(a => a.Company_Id == CompanyId).Select(a => a.Country_Id).FirstOrDefault();
                result.CompanyNationality = _MongoContext.mResort.AsQueryable().Where(a => a.Voyager_Resort_Id == CountryId).Select(a => a.Nation).FirstOrDefault();
            }
            return result ?? new ProdNationalityGetRes();
        }
        #endregion

        #region GetProductsByNames
        public List<mProducts> GetProductsByNames(ProductGetReq request)
        {
            FilterDefinition<mProducts> filter;
            filter = Builders<mProducts>.Filter.Empty;

            List<mProducts> result = new List<mProducts>();

            if (request.ProductName != null && request.ProductName.Count > 0)
            {
                filter = filter & Builders<mProducts>.Filter.Where(f => request.ProductName.Contains(f.ProdName));
            }

            if (!string.IsNullOrEmpty(request.CityName))
            {
                filter = filter & Builders<mProducts>.Filter.Where(f => f.CityName == request.CityName);
            }
            result = _MongoContext.mProducts.Find(filter).ToList();
            return result ?? new List<mProducts>();
        }
        #endregion

        #region GetSimilarHotels
        public async Task<SimilarHotelsGetRes> GetSimilarHotels(SimilarHotelsGetReq request)
        {
            SimilarHotelsGetRes result = new SimilarHotelsGetRes();

            try
            {
                var product = _MongoContext.Products.AsQueryable().Where(a => a.VoyagerProduct_Id == request.ProductId).FirstOrDefault();
                var position = new mPosition();
                var qrfposition = new mQRFPosition();
                var AlternateHotelsParameter = new AlternateServiesParameter();
                var AlternateHotels = new List<AlternateServices>();
                var positionCity = "";
                var positionCountry = "";

                if (request.IsClone)
                {
                    qrfposition = _MongoContext.mQRFPosition.AsQueryable().Where(a => a.PositionId == request.PositionId).FirstOrDefault();
                    AlternateHotels = qrfposition.AlternateHotels;
                    AlternateHotelsParameter = qrfposition.AlternateHotelsParameter;
                    positionCity = qrfposition.CityName;
                    positionCountry = qrfposition.CountryName;
                }
                else
                {
                    position = _MongoContext.mPosition.AsQueryable().Where(a => a.PositionId == request.PositionId).FirstOrDefault();
                    AlternateHotels = position.AlternateHotels;
                    AlternateHotelsParameter = position.AlternateHotelsParameter;
                    positionCity = position.CityName;
                    positionCountry = position.CountryName;
                }

                if (positionCity == AlternateHotelsParameter.City && positionCountry == AlternateHotelsParameter.Country && product.HotelAdditionalInfo.Location == AlternateHotelsParameter.Location &&
                    product.HotelAdditionalInfo.StarRating == AlternateHotelsParameter.StarRating && product.HotelAdditionalInfo.BdgPriceCategory == AlternateHotelsParameter.BdgCategary)
                {
                    #region GetHotelsFromPosition

                    if (AlternateHotels != null)
                    {
                        result.SelectedHotelList = AlternateHotels.Where(a => a.IsBlackListed == false).Select(p => new ProductList
                        {
                            VoyagerProductId = p.Product_Id,
                            Name = p.Product_Name,
                            Location = p.Attributes.Location,
                            StarRating = p.Attributes.StarRating,
                            Category = p.Attributes.BdgPriceCategory,
                            SupplierId = p.SupplierInfo.Id,
                            Supplier = p.SupplierInfo.Name,
                            LocationInfo = new ProductLocation { CountryName = p.Country, CountryCode = p.Country_Id, CityName = p.City, CityCode = p.City_Id }
                        }).ToList();

                        result.BlackListedHotelList = AlternateHotels.Where(a => a.IsBlackListed == true).Select(p => new ProductList
                        {
                            VoyagerProductId = p.Product_Id,
                            Name = p.Product_Name,
                            Location = p.Attributes.Location,
                            StarRating = p.Attributes.StarRating,
                            Category = p.Attributes.BdgPriceCategory,
                            SupplierId = p.SupplierInfo.Id,
                            Supplier = p.SupplierInfo.Name,
                            LocationInfo = new ProductLocation { CountryName = p.Country, CountryCode = p.Country_Id, CityName = p.City, CityCode = p.City_Id }
                        }).ToList();
                    }
                    #endregion
                }
                else
                {
                    if (request.IsClone)
                    {
                        position.AlternateHotels = new List<AlternateServices>();
                        position.AlternateHotelsParameter = new AlternateServiesParameter();

                        ReplaceOneResult replaceResult = await _MongoContext.mQRFPosition.ReplaceOneAsync(Builders<mQRFPosition>.Filter.Eq("PositionId", qrfposition.PositionId), qrfposition);
                        result.ResponseStatus.Status = replaceResult.MatchedCount > 0 ? "Success" : "Failure";
                        result.ResponseStatus.ErrorMessage = replaceResult.MatchedCount > 0 ? "Saved Successfully." : "Details not updated.";
                    }
                    else
                    {
                        position.AlternateHotels = new List<AlternateServices>();
                        position.AlternateHotelsParameter = new AlternateServiesParameter();

                        ReplaceOneResult replaceResult = await _MongoContext.mPosition.ReplaceOneAsync(Builders<mPosition>.Filter.Eq("PositionId", position.PositionId), position);
                        result.ResponseStatus.Status = replaceResult.MatchedCount > 0 ? "Success" : "Failure";
                        result.ResponseStatus.ErrorMessage = replaceResult.MatchedCount > 0 ? "Saved Successfully." : "Details not updated.";
                    }
                }

                List<ProductList> ProductList = new List<ProductList>();
                FilterDefinition<Products> filter;
                filter = Builders<Products>.Filter.Empty;

                filter = filter & Builders<Products>.Filter.Eq(f => f.ProductType, "Hotel");

                if (!string.IsNullOrWhiteSpace(product.HotelAdditionalInfo.StarRating))
                {
                    filter = filter & Builders<Products>.Filter.Where(x => x.HotelAdditionalInfo.StarRating == product.HotelAdditionalInfo.StarRating);
                }

                if (!string.IsNullOrWhiteSpace(product.HotelAdditionalInfo.BdgPriceCategory))
                {
                    filter = filter & Builders<Products>.Filter.Where(x => x.HotelAdditionalInfo.BdgPriceCategory == product.HotelAdditionalInfo.BdgPriceCategory);
                }

                if (!string.IsNullOrWhiteSpace(product.HotelAdditionalInfo.Location))
                {
                    filter = filter & Builders<Products>.Filter.Eq(f => f.HotelAdditionalInfo.Location, product.HotelAdditionalInfo.Location);
                }

                if (!string.IsNullOrWhiteSpace(positionCity))
                {
                    filter = filter & Builders<Products>.Filter.Where(x => x.CityName == positionCity);
                }

                if (!string.IsNullOrWhiteSpace(positionCountry))
                {
                    filter = filter & Builders<Products>.Filter.Where(x => x.CountryName == positionCountry);
                }

                filter = filter & Builders<Products>.Filter.Where(x => x.Placeholder == false);

                ProductList = await _MongoContext.Products.Find(filter).Project(p => new ProductList
                {

                    VoyagerProductId = p.VoyagerProduct_Id,
                    Name = p.ProductName,
                    Location = p.HotelAdditionalInfo.Location,
                    StarRating = p.HotelAdditionalInfo.StarRating,
                    Category = p.HotelAdditionalInfo.BdgPriceCategory,
                    SupplierId = p.ProductSuppliers.Where(a => a.IsDefault == true).Select(b => b.Company_Id).FirstOrDefault(),
                    Supplier = p.ProductSuppliers.Where(a => a.IsDefault == true).Select(b => b.CompanyName).FirstOrDefault(),
                    LocationInfo = new ProductLocation { CountryName = p.CountryName, CountryCode = p.CountryId, CityName = p.CityName, CityCode = p.CityId }
                }).ToListAsync();


                var NearByCities = _MongoContext.Products.AsQueryable().Where(x => x.VoyagerProduct_Id == request.ProductId).Select(y => y.NearByCities).FirstOrDefault();
                List<ProductList> NBCProductList = new List<ProductList>();
                if (NearByCities != null)
                {
                    if (NearByCities.Count > 0)
                    {
                        List<string> NBCCityList = new List<string>();
                        NBCCityList = NearByCities.Select(a => a.CityName).ToList();

                        List<string> NBCCountryList = new List<string>();
                        NBCCountryList = NearByCities.Select(a => a.CountryName).ToList();

                        List<string> NBCLocationList = new List<string>();
                        NBCLocationList = NearByCities.Select(a => a.Location).ToList();

                        FilterDefinition<Products> filterNBC;
                        filterNBC = Builders<Products>.Filter.Empty;

                        filterNBC = filterNBC & Builders<Products>.Filter.Eq(f => f.ProductType, "Hotel");

                        if (!string.IsNullOrWhiteSpace(product.HotelAdditionalInfo.StarRating))
                        {
                            filterNBC = filterNBC & Builders<Products>.Filter.Where(x => x.HotelAdditionalInfo.StarRating == product.HotelAdditionalInfo.StarRating);
                        }

                        if (!string.IsNullOrWhiteSpace(product.HotelAdditionalInfo.BdgPriceCategory))
                        {
                            filterNBC = filterNBC & Builders<Products>.Filter.Where(x => x.HotelAdditionalInfo.BdgPriceCategory == product.HotelAdditionalInfo.BdgPriceCategory);
                        }

                        if (NBCCityList.Count > 0)
                        {
                            filterNBC = filterNBC & Builders<Products>.Filter.Where(f => NBCCityList.Contains(f.CityName));
                        }

                        if (NBCCountryList.Count > 0)
                        {
                            filterNBC = filterNBC & Builders<Products>.Filter.Where(f => NBCCountryList.Contains(f.CountryName));
                        }

                        if (NBCLocationList.Count > 0)
                        {
                            filterNBC = filterNBC & Builders<Products>.Filter.Where(f => NBCLocationList.Contains(f.HotelAdditionalInfo.Location));
                        }
                        filter = filter & Builders<Products>.Filter.Where(x => x.Placeholder == false);

                        NBCProductList = await _MongoContext.Products.Find(filterNBC).Project(p => new ProductList
                        {
                            VoyagerProductId = p.VoyagerProduct_Id,
                            Name = p.ProductName,
                            Location = p.HotelAdditionalInfo.Location,
                            StarRating = p.HotelAdditionalInfo.StarRating,
                            Category = p.HotelAdditionalInfo.BdgPriceCategory,
                            SupplierId = p.ProductSuppliers.Where(a => a.IsDefault == true).Select(b => b.Company_Id).FirstOrDefault(),
                            Supplier = p.ProductSuppliers.Where(a => a.IsDefault == true).Select(b => b.CompanyName).FirstOrDefault(),
                            LocationInfo = new ProductLocation { CountryName = p.CountryName, CountryCode = p.CountryId, CityName = p.CityName, CityCode = p.CityId }
                        }).ToListAsync();

                        if (NBCProductList.Count > 0)
                        {
                            ProductList.AddRange(NBCProductList);
                        }
                    }

                }

                ProductList.Distinct();
                ProductList = ProductList.OrderBy(a => a.Name).ToList();

                if (ProductList.Count > 0)
                {
                    if (AlternateHotels != null)
                        ProductList = ProductList.FindAll(a => !AlternateHotels.Select(b => b.Product_Id).ToList().Contains(a.VoyagerProductId));
                    result.SelectedHotelList.AddRange(ProductList);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                result.ResponseStatus.Status = "Failure";
                result.ResponseStatus.ErrorMessage = ex.Message;
            }
            return result;
        }

        #endregion

        #region SetSimilarHotels
        public async Task<SimilarHotelsSetRes> SetSimilarHotels(SimilarHotelsSetReq request)
        {
            SimilarHotelsSetRes result = new SimilarHotelsSetRes();
            try
            {
                var savedAlternateServices = new List<AlternateServices>();
                var position = new mPosition();
                var qrfposition = new mQRFPosition();
                var booking = new Bookings();
                var bookingPosition = new Positions();

                if (!string.IsNullOrEmpty(request.Caller) && request.Caller.ToLower() == "bookings")
                {
                    booking = _MongoContext.Bookings.AsQueryable().Where(a => a.BookingNumber == request.BookingNumber).FirstOrDefault();
                    savedAlternateServices = booking.Positions.Where(a => a.Position_Id == request.PositionId).FirstOrDefault().AlternateServices;
                    bookingPosition = booking.Positions.Where(a => a.Position_Id == request.PositionId).FirstOrDefault();
                }
                else
                {
                    if (request.IsClone)
                    {
                        qrfposition = _MongoContext.mQRFPosition.AsQueryable().Where(a => a.PositionId == request.PositionId).FirstOrDefault();
                        savedAlternateServices = qrfposition.AlternateHotels;
                    }
                    else
                    {
                        position = _MongoContext.mPosition.AsQueryable().Where(a => a.PositionId == request.PositionId).FirstOrDefault();
                        savedAlternateServices = position.AlternateHotels;
                    }
                }

                var productIdsList = request.SelectedHotelList.Select(a => a.VoyagerProductId).ToList();
                productIdsList.AddRange(request.BlacklistedHotelList.Select(a => a.VoyagerProductId).ToList());

                var productList = _MongoContext.Products.AsQueryable().Where(a => productIdsList.Contains(a.VoyagerProduct_Id)).ToList();
                if (productList == null)
                {
                    result.ResponseStatus.Status = "Failure";
                    result.ResponseStatus.ErrorMessage = "No Products found!!!";
                    return result;
                }

                var CompanyIdsList = new List<string>();
                productList.ForEach(a =>
                {
                    if (a.ProductSuppliers != null)
                        CompanyIdsList.AddRange(a.ProductSuppliers.Where(c => c.IsDefault == true).Select(b => b.Company_Id).ToList());
                });
                var companyList = _MongoContext.mCompanies.AsQueryable().Where(a => CompanyIdsList.Contains(a.Company_Id)).ToList();


                if ((position != null && request.IsClone != true) || (qrfposition != null && request.IsClone == true))
                {
                    var HotelList = new List<AlternateServices>();
                    for (int i = 0; i < request.SelectedHotelList.Count; i++)
                    {
                        var savedHotel = savedAlternateServices.Where(a => a.Product_Id == request.SelectedHotelList[i].VoyagerProductId && a.SupplierInfo.Id == request.SelectedHotelList[i].SupplierId).FirstOrDefault();
                        var product = productList.Where(a => a.VoyagerProduct_Id == request.SelectedHotelList[i].VoyagerProductId).FirstOrDefault();
                        var company = companyList.Where(a => a.Company_Id == product.ProductSuppliers.Where(b => b.IsDefault == true).Select(c => c.Company_Id).FirstOrDefault()).FirstOrDefault();

                        if (product != null)
                        {
                            var newHotel = new AlternateServices();

                            if (savedHotel != null)
                            {
                                savedHotel.SortOrder = i + 1;
                                savedHotel.IsBlackListed = false;
                                savedHotel.AuditTrail.MODI_DT = DateTime.Now;
                                savedHotel.AuditTrail.MODI_US = request.EditUser;
                                HotelList.Add(savedHotel);
                            }
                            else
                            {
                                newHotel.AlternateServies_Id = Guid.NewGuid().ToString();
                                newHotel.SortOrder = i + 1;
                                newHotel.IsBlackListed = false;

                                newHotel.Product_Id = request.SelectedHotelList[i].VoyagerProductId;
                                newHotel.Product_Name = product.ProductName;
                                newHotel.Country_Id = product.CountryId;
                                newHotel.Country = product.CountryName;
                                newHotel.City_Id = product.CityId;
                                newHotel.City = product.CityName;

                                newHotel.Attributes = product.HotelAdditionalInfo;

                                if (product.ProductSuppliers != null)
                                {
                                    var supplier = product.ProductSuppliers.Where(a => a.IsDefault == true).FirstOrDefault();
                                    if (supplier != null)
                                    {
                                        newHotel.SupplierInfo.Id = supplier.Company_Id;
                                        newHotel.SupplierInfo.Name = supplier.CompanyName;
                                        newHotel.SupplierInfo.Code = "";
                                        newHotel.SupplierInfo.Contact_Id = supplier.Contact_Group_Id;
                                        newHotel.SupplierInfo.Contact_Name = supplier.Contact_Group_Name;
                                        newHotel.SupplierInfo.Contact_Email = supplier.Contact_Group_Email;
                                        newHotel.SupplierInfo.Contact_Tel = "";
                                        newHotel.SupplierInfo.Contact_SendType = supplier.ContactVia;
                                        newHotel.SupplierInfo.ISSUBAGENT = null;
                                        if (company != null && company.ContactDetails != null && company.ContactDetails.Count > 0)
                                        {
                                            var Contact = company.ContactDetails.Where(a => a.Contact_Id == supplier.Contact_Group_Id).FirstOrDefault();
                                            if (Contact != null)
                                            {
                                                newHotel.SupplierInfo.Contact_Tel = Contact.TEL;
                                                newHotel.SupplierInfo.Contact_Name = Contact.CommonTitle + " " + supplier.Contact_Group_Name;
                                            }
                                        }
                                        if (company != null)
                                        {
                                            newHotel.SupplierInfo.ParentCompany_Id = company.ParentAgent_Id;
                                            newHotel.SupplierInfo.ParentCompany_Name = company.ParentAgent_Name;
                                        }
                                    }
                                }

                                newHotel.Request_Status = "Pending";
                                newHotel.Availability_Status = "PENDING";
                                newHotel.Requested_On = null;
                                newHotel.PPTwin_Rate = null;

                                newHotel.AuditTrail.CREA_DT = DateTime.Now;
                                newHotel.AuditTrail.CREA_US = request.EditUser;
                                newHotel.AuditTrail.MODI_DT = null;
                                newHotel.AuditTrail.MODI_US = null;
                                newHotel.AuditTrail.STATUS_DT = null;
                                newHotel.AuditTrail.STATUS_US = null;

                                if (!string.IsNullOrEmpty(request.Caller) && request.Caller.ToLower() == "bookings")
                                {
                                    newHotel.Request_RoomsAndPrices = new List<BookingRoomsAndPrices>();

                                    foreach (var objbookingRoomsAndPrices in bookingPosition.BookingRoomsAndPrices)
                                    {
                                        var newBookingRoomsAndPrices = new BookingRoomsAndPrices();

                                        var productCategory = product.ProductCategories.Where(a => a.IsDefault == true).FirstOrDefault();

                                        if (productCategory != null)
                                        {
                                            var productRange = productCategory.ProductRanges.Where(a => a.ProductTemplateCode == objbookingRoomsAndPrices.RoomShortCode && a.PersonType == objbookingRoomsAndPrices.PersonType).FirstOrDefault();

                                            newBookingRoomsAndPrices.BookingRooms_Id = Guid.NewGuid().ToString();
                                            newBookingRoomsAndPrices.PositionPricing_Id = Guid.NewGuid().ToString();
                                            newBookingRoomsAndPrices.Req_Count = objbookingRoomsAndPrices.Req_Count;
                                            newBookingRoomsAndPrices.ProductRange_Id = productRange?.ProductRange_Id;
                                            newBookingRoomsAndPrices.Category_Id = productCategory.ProductCategory_Id;
                                            newBookingRoomsAndPrices.CategoryName = productCategory.ParentCategoryName;
                                            newBookingRoomsAndPrices.ProductTemplate_Id = objbookingRoomsAndPrices.ProductTemplate_Id;
                                            newBookingRoomsAndPrices.RoomShortCode = objbookingRoomsAndPrices.RoomShortCode;
                                            newBookingRoomsAndPrices.RoomName = objbookingRoomsAndPrices.RoomName;
                                            newBookingRoomsAndPrices.Capacity = productRange?.Quantity;
                                            newBookingRoomsAndPrices.PersonType_Id = objbookingRoomsAndPrices.PersonType_Id;
                                            newBookingRoomsAndPrices.PersonType = objbookingRoomsAndPrices.PersonType;
                                            newBookingRoomsAndPrices.Age = objbookingRoomsAndPrices.Age;
                                            newBookingRoomsAndPrices.ApplyMarkup = true;
                                            newBookingRoomsAndPrices.AllocationUsed = "none";
                                            newBookingRoomsAndPrices.CrossPosition_Id = objbookingRoomsAndPrices.CrossPosition_Id;
                                            newBookingRoomsAndPrices.StartDate = bookingPosition.STARTDATE;
                                            newBookingRoomsAndPrices.EndDate = bookingPosition.ENDDATE;

                                            if (product.ProductSuppliers != null)
                                            {
                                                var supplier = product.ProductSuppliers.Where(a => a.Company_Id == newHotel.SupplierInfo.Id).FirstOrDefault();
                                                if (supplier != null)
                                                {
                                                    newBookingRoomsAndPrices.BuyCurrency_Id = supplier.CurrencyId;
                                                    newBookingRoomsAndPrices.BuyCurrency_Name = supplier.CurrencyName;
                                                }
                                            }

                                            newBookingRoomsAndPrices.Action = "R";
                                            newBookingRoomsAndPrices.BudgetPrice = objbookingRoomsAndPrices.BudgetPrice;
                                            newBookingRoomsAndPrices.RequestedPrice = objbookingRoomsAndPrices.RequestedPrice;
                                            newBookingRoomsAndPrices.BuyPrice = objbookingRoomsAndPrices.BuyPrice;
                                            newBookingRoomsAndPrices.ContractedBuyPrice = objbookingRoomsAndPrices.ContractedBuyPrice;

                                            newHotel.Request_RoomsAndPrices.Add(newBookingRoomsAndPrices);
                                        }
                                    }
                                }

                                HotelList.Add(newHotel);
                            }
                        }
                    }

                    for (int i = 0; i < request.BlacklistedHotelList.Count; i++)
                    {
                        var savedHotel = savedAlternateServices.Where(a => a.Product_Id == request.BlacklistedHotelList[i].VoyagerProductId && a.SupplierInfo?.Id == request.BlacklistedHotelList[i].SupplierId).FirstOrDefault();

                        var product = productList.Where(a => a.VoyagerProduct_Id == request.BlacklistedHotelList[i].VoyagerProductId).FirstOrDefault();
                        var company = companyList.Where(a => a.Company_Id == product.ProductSuppliers.Where(b => b.IsDefault == true).Select(c => c.Company_Id).FirstOrDefault()).FirstOrDefault();

                        if (product != null)
                        {
                            if (savedHotel != null)
                            {
                                savedHotel.SortOrder = i + 1;
                                savedHotel.IsBlackListed = true;
                                savedHotel.AuditTrail.MODI_DT = DateTime.Now;
                                savedHotel.AuditTrail.MODI_US = request.EditUser;
                                HotelList.Add(savedHotel);
                            }
                            else
                            {
                                var newHotel = new AlternateServices();

                                newHotel.AlternateServies_Id = Guid.NewGuid().ToString();
                                newHotel.SortOrder = i + 1;
                                newHotel.IsBlackListed = true;

                                newHotel.Product_Id = request.BlacklistedHotelList[i].VoyagerProductId;
                                newHotel.Product_Name = product.ProductName;
                                newHotel.Country_Id = product.CountryId;
                                newHotel.Country = product.CountryName;
                                newHotel.City_Id = product.CityId;
                                newHotel.City = product.CityName;

                                newHotel.Attributes = product.HotelAdditionalInfo;

                                if (product.ProductSuppliers != null)
                                {
                                    var supplier = product.ProductSuppliers.Where(a => a.IsDefault == true).FirstOrDefault();
                                    if (supplier != null)
                                    {
                                        newHotel.SupplierInfo.Id = supplier.Company_Id;
                                        newHotel.SupplierInfo.Name = supplier.CompanyName;
                                        newHotel.SupplierInfo.Code = "";
                                        newHotel.SupplierInfo.Contact_Id = supplier.Contact_Group_Id;
                                        newHotel.SupplierInfo.Contact_Name = supplier.Contact_Group_Name;
                                        newHotel.SupplierInfo.Contact_Email = supplier.Contact_Group_Email;
                                        newHotel.SupplierInfo.Contact_Tel = "";
                                        newHotel.SupplierInfo.Contact_SendType = supplier.ContactVia;
                                        newHotel.SupplierInfo.ISSUBAGENT = null;
                                        if (company != null && company.ContactDetails != null && company.ContactDetails.Count > 0)
                                        {
                                            var Contact = company.ContactDetails.Where(a => a.Contact_Id == supplier.Contact_Group_Id).FirstOrDefault();
                                            if (Contact != null)
                                            {
                                                newHotel.SupplierInfo.Contact_Tel = Contact.TEL;
                                                newHotel.SupplierInfo.Contact_Name = Contact.CommonTitle + " " + supplier.Contact_Group_Name;
                                            }
                                        }
                                        if (company != null)
                                        {
                                            newHotel.SupplierInfo.ParentCompany_Id = company.ParentAgent_Id;
                                            newHotel.SupplierInfo.ParentCompany_Name = company.ParentAgent_Name;
                                        }
                                    }
                                }

                                newHotel.Request_Status = "Pending";
                                newHotel.Availability_Status = "PENDING";
                                newHotel.Requested_On = null;
                                newHotel.PPTwin_Rate = null;

                                newHotel.AuditTrail.CREA_DT = DateTime.Now;
                                newHotel.AuditTrail.CREA_US = request.EditUser;
                                newHotel.AuditTrail.MODI_DT = null;
                                newHotel.AuditTrail.MODI_US = null;
                                newHotel.AuditTrail.STATUS_DT = null;
                                newHotel.AuditTrail.STATUS_US = null;

                                if (!string.IsNullOrEmpty(request.Caller) && request.Caller.ToLower() == "bookings")
                                {
                                    newHotel.Request_RoomsAndPrices = new List<BookingRoomsAndPrices>();

                                    foreach (var objbookingRoomsAndPrices in bookingPosition.BookingRoomsAndPrices)
                                    {
                                        var newBookingRoomsAndPrices = new BookingRoomsAndPrices();

                                        var productCategory = product.ProductCategories.Where(a => a.IsDefault == true).FirstOrDefault();

                                        if (productCategory != null)
                                        {
                                            var productRange = productCategory.ProductRanges.Where(a => a.ProductTemplateCode == objbookingRoomsAndPrices.RoomShortCode && a.PersonType == objbookingRoomsAndPrices.PersonType).FirstOrDefault();

                                            newBookingRoomsAndPrices.BookingRooms_Id = Guid.NewGuid().ToString();
                                            newBookingRoomsAndPrices.PositionPricing_Id = Guid.NewGuid().ToString();
                                            newBookingRoomsAndPrices.Req_Count = objbookingRoomsAndPrices.Req_Count;
                                            newBookingRoomsAndPrices.ProductRange_Id = productRange?.ProductRange_Id;
                                            newBookingRoomsAndPrices.Category_Id = productCategory.ProductCategory_Id;
                                            newBookingRoomsAndPrices.CategoryName = productCategory.ParentCategoryName;
                                            newBookingRoomsAndPrices.ProductTemplate_Id = objbookingRoomsAndPrices.ProductTemplate_Id;
                                            newBookingRoomsAndPrices.RoomShortCode = objbookingRoomsAndPrices.RoomShortCode;
                                            newBookingRoomsAndPrices.RoomName = objbookingRoomsAndPrices.RoomName;
                                            newBookingRoomsAndPrices.Capacity = productRange?.Quantity;
                                            newBookingRoomsAndPrices.PersonType_Id = objbookingRoomsAndPrices.PersonType_Id;
                                            newBookingRoomsAndPrices.PersonType = objbookingRoomsAndPrices.PersonType;
                                            newBookingRoomsAndPrices.Age = objbookingRoomsAndPrices.Age;
                                            newBookingRoomsAndPrices.ApplyMarkup = true;
                                            newBookingRoomsAndPrices.AllocationUsed = "none";
                                            newBookingRoomsAndPrices.CrossPosition_Id = objbookingRoomsAndPrices.CrossPosition_Id;
                                            newBookingRoomsAndPrices.StartDate = bookingPosition.STARTDATE;
                                            newBookingRoomsAndPrices.EndDate = bookingPosition.ENDDATE;

                                            if (product.ProductSuppliers != null)
                                            {
                                                var supplier = product.ProductSuppliers.Where(a => a.Company_Id == newHotel.SupplierInfo.Id).FirstOrDefault();
                                                if (supplier != null)
                                                {
                                                    newBookingRoomsAndPrices.BuyCurrency_Id = supplier.CurrencyId;
                                                    newBookingRoomsAndPrices.BuyCurrency_Name = supplier.CurrencyName;
                                                }
                                            }

                                            newBookingRoomsAndPrices.Action = "R";
                                            newBookingRoomsAndPrices.BudgetPrice = objbookingRoomsAndPrices.BudgetPrice;
                                            newBookingRoomsAndPrices.RequestedPrice = objbookingRoomsAndPrices.RequestedPrice;
                                            newBookingRoomsAndPrices.BuyPrice = objbookingRoomsAndPrices.BuyPrice;
                                            newBookingRoomsAndPrices.ContractedBuyPrice = objbookingRoomsAndPrices.ContractedBuyPrice;

                                            newHotel.Request_RoomsAndPrices.Add(newBookingRoomsAndPrices);
                                        }
                                    }
                                }

                                HotelList.Add(newHotel);
                            }
                        }
                    }

                    var productMain = _MongoContext.Products.AsQueryable().Where(a => a.VoyagerProduct_Id == request.ProductId).FirstOrDefault();

                    if (!string.IsNullOrEmpty(request.Caller) && request.Caller.ToLower() == "bookings")
                    {
                        // var bookingPosition = booking.Positions.Where(a => a.Position_Id == request.PositionId).FirstOrDefault();

                        var finallist = HotelList.Where(a => a.Product_Id != bookingPosition.Product_Id && a.SupplierInfo != null && a.SupplierInfo.Id != bookingPosition.SupplierInfo?.Id).ToList();
                        //if (finallist.Count > 0)
                        //{
                        bookingPosition.AlternateServices = HotelList;
                        bookingPosition.City = productMain.CityName;
                        bookingPosition.Country = productMain.CountryName;
                        if (bookingPosition.Attributes == null) bookingPosition.Attributes = new HotelAdditionalInfo();
                        bookingPosition.Attributes.Location = productMain.HotelAdditionalInfo.Location;
                        bookingPosition.Attributes.StarRating = productMain.HotelAdditionalInfo.StarRating;
                        bookingPosition.Attributes.BdgPriceCategory = productMain.HotelAdditionalInfo.BdgPriceCategory;

                        var resultBooking = await _MongoContext.Bookings.FindOneAndUpdateAsync(a => a.BookingNumber == booking.BookingNumber
                            && a.Positions.Any(b => b.Position_Id == bookingPosition.Position_Id), Builders<Bookings>.Update.Set(a => a.Positions[-1], bookingPosition));

                        //ReplaceOneResult replaceResult = await _MongoContext.Bookings.ReplaceOneAsync(Builders<Bookings>.Filter.Eq("BookingNumber", booking.BookingNumber), booking);
                        if (resultBooking != null)
                        {
                            //The below Bridge service will UPSERT the AlternateServices in PositionRequests table in SQL
                            ResponseStatus responseStatus = await _bookingProviders.SetBookingAlternateServices(new BookingPosAltSetReq()
                            {
                                BookingNumber = request.BookingNumber,
                                PositionId = request.PositionId,
                                User = request.EditUser
                            });
                            if (responseStatus != null && responseStatus.Status != null && responseStatus.Status == "Success")
                            {
                                result.ResponseStatus.Status = "Success";
                                result.ResponseStatus.ErrorMessage = "Saved Successfully.";
                            }
                            else
                            {
                                result.ResponseStatus.Status = "Failure";
                                result.ResponseStatus.ErrorMessage = "Details not updated.";
                            }
                        }
                        else
                        {
                            result.ResponseStatus.Status = "Failure";
                            result.ResponseStatus.ErrorMessage = "Details not updated.";
                        }
                    }
                    else
                    {
                        if (request.IsClone)
                        {
                            qrfposition.AlternateHotels = HotelList;

                            qrfposition.AlternateHotelsParameter.City = productMain.CityName;
                            qrfposition.AlternateHotelsParameter.Country = productMain.CountryName;
                            qrfposition.AlternateHotelsParameter.Location = productMain.HotelAdditionalInfo.Location;
                            qrfposition.AlternateHotelsParameter.StarRating = productMain.HotelAdditionalInfo.StarRating;
                            qrfposition.AlternateHotelsParameter.BdgCategary = productMain.HotelAdditionalInfo.BdgPriceCategory;

                            ReplaceOneResult replaceResult = await _MongoContext.mQRFPosition.ReplaceOneAsync(Builders<mQRFPosition>.Filter.Eq("PositionId", qrfposition.PositionId), qrfposition);
                            result.ResponseStatus.Status = replaceResult.MatchedCount > 0 ? "Success" : "Failure";
                            result.ResponseStatus.ErrorMessage = replaceResult.MatchedCount > 0 ? "Saved Successfully." : "Details not updated.";
                        }
                        else
                        {
                            position.AlternateHotels = HotelList;

                            position.AlternateHotelsParameter.City = productMain.CityName;
                            position.AlternateHotelsParameter.Country = productMain.CountryName;
                            position.AlternateHotelsParameter.Location = productMain.HotelAdditionalInfo.Location;
                            position.AlternateHotelsParameter.StarRating = productMain.HotelAdditionalInfo.StarRating;
                            position.AlternateHotelsParameter.BdgCategary = productMain.HotelAdditionalInfo.BdgPriceCategory;

                            ReplaceOneResult replaceResult = await _MongoContext.mPosition.ReplaceOneAsync(Builders<mPosition>.Filter.Eq("PositionId", position.PositionId), position);
                            result.ResponseStatus.Status = replaceResult.MatchedCount > 0 ? "Success" : "Failure";
                            result.ResponseStatus.ErrorMessage = replaceResult.MatchedCount > 0 ? "Saved Successfully." : "Details not updated.";
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                result.ResponseStatus.Status = "Failure";
                result.ResponseStatus.ErrorMessage = ex.Message;
            }
            return result;
        }

        #endregion

        #region Save Default Similar Hotels
        public async Task<bool> SaveSimilarHotels(string PositionId, string ProductID, string EditUser, bool IsClone)
        {
            SimilarHotelsGetReq getrequest = new SimilarHotelsGetReq();
            SimilarHotelsGetRes getresponse = new SimilarHotelsGetRes();
            getrequest.PositionId = PositionId;
            getrequest.ProductId = ProductID;
            getrequest.IsClone = IsClone;

            getresponse = await GetSimilarHotels(getrequest);

            SimilarHotelsSetReq setrequest = new SimilarHotelsSetReq();
            SimilarHotelsSetRes setresponse = new SimilarHotelsSetRes();

            setrequest.PositionId = PositionId;
            setrequest.ProductId = ProductID;
            setrequest.SelectedHotelList = getresponse.SelectedHotelList;
            setrequest.BlacklistedHotelList = getresponse.BlackListedHotelList;
            setrequest.EditUser = EditUser;
            setrequest.IsClone = IsClone;

            setresponse = await SetSimilarHotels(setrequest);
            return true;
        }
        #endregion

        #region Get Product Markups
        public async Task<MarkupDetails> GetProdMarkups(ProdMarkupsGetReq request)
        {
            var response = new MarkupDetails();
            try
            {

                var markups = _MongoContext.mMarkups.AsQueryable().Where(a => a.Markups_Id == request.MarkupsId).FirstOrDefault();

                if (markups != null)
                {
                    if (string.IsNullOrEmpty(request.ProductType))
                    {
                        response = markups.MarkupDetails.Where(a => string.IsNullOrEmpty(a.ProductType) && a.StartDate <= DateTime.Now && a.EndDate >= DateTime.Now).FirstOrDefault();
                    }
                    else
                    {
                        response = markups.MarkupDetails.Where(a => a.ProductType.ToUpper() == request.ProductType.ToUpper() && a.StartDate <= DateTime.Now && a.EndDate >= DateTime.Now).FirstOrDefault();
                        if (response == null)
                        {
                            response = markups.MarkupDetails.Where(a => string.IsNullOrEmpty(a.ProductType) && a.StartDate <= DateTime.Now && a.EndDate >= DateTime.Now).FirstOrDefault();
                        }
                    }
                    return response;
                }

                return null;

            }
            catch (Exception)
            {
                return null;
            }
        }
        #endregion

        #region Product Contracts
        public async Task<ProductContractsGetRes> GetProductContracts(ProductContractsGetReq request)
        {
            FilterDefinition<Contracts> filter = Builders<Contracts>.Filter.Empty;
            ProductContractsGetRes response = new ProductContractsGetRes();
            try
            {
                if (!string.IsNullOrEmpty(request.SupplierId))
                {
                    filter = filter & Builders<Contracts>.Filter.Eq(x => x.Supplier_Id, request.SupplierId);
                }
                if (!string.IsNullOrEmpty(request.ProductId))
                {
                    filter = filter & Builders<Contracts>.Filter.Eq(x => x.Product_Id, request.ProductId);
                }
                if (!string.IsNullOrEmpty(request.BuySellType))
                {
                    filter = filter & Builders<Contracts>.Filter.Eq(x => x.BuySellType, request.BuySellType);
                }
                if (!string.IsNullOrEmpty(request.AgentId))
                {
                    filter = filter & Builders<Contracts>.Filter.Eq(x => x.ForAgent_Id, request.AgentId);
                }
                response.ProductContract = await _MongoContext.ProductContracts.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }
            return response;
        }
        #endregion

    }
}