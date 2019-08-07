using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VGER_DISTRIBUTION.Helpers;
using VGER_DISTRIBUTION.Models;
using VGER_DISTRIBUTION.Repositories;
using VGER_WAPI_CLASSES;



namespace VGER_DISTRIBUTION.Repositories
{
    public class ProductRepository : IProductRepository
    {
        #region Private Variable Declaration
        private readonly MongoContext _MongoContext = null;
        #endregion

        public ProductRepository(IOptions<MongoSettings> settings)
        {
            _MongoContext = new MongoContext(settings);
        }

        #region GetProductDetailsBySearchCriteria 
        public async Task<ProductListRes> GetProductDetailsBySearchCriteria(ProductListReq request)
        {
            FilterDefinition<mProducts> filter;
            filter = Builders<mProducts>.Filter.Empty;
            FilterDefinition<mProductHotelAdditionalInfo> filterHAI;


            if (!string.IsNullOrWhiteSpace(request.ProdType))
            {
                //filter = filter & Builders<mProducts>.Filter.Where(f => request.ProdType == f.ProductType);
                filter = filter & Builders<mProducts>.Filter.Regex(x => x.ProductType, new BsonRegularExpression(new Regex(request.ProdType, RegexOptions.IgnoreCase)));
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

            if (request.IsPlaceHolder != null)
            {
                filter = filter & Builders<mProducts>.Filter.Where(x => x.Placeholder == request.IsPlaceHolder);
            }


            if (!string.IsNullOrWhiteSpace(request.City_Id))
            {
                string lCity_Id = request.City_Id.ToLower();
                filter = filter & Builders<mProducts>.Filter.Regex(x => x.Resort_Id, new BsonRegularExpression(new Regex(lCity_Id.Trim(), RegexOptions.IgnoreCase)));
            }

            if (!string.IsNullOrWhiteSpace(request.Country_Id))
            {
                string lCountry_Id = request.Country_Id.ToLower();
                filter = filter & Builders<mProducts>.Filter.Regex(x => x.ParentResort_Id, new BsonRegularExpression(new Regex(lCountry_Id.Trim(), RegexOptions.IgnoreCase)));
            }

            filter = filter & Builders<mProducts>.Filter.Regex(x => x.Status, new BsonRegularExpression(new Regex("", RegexOptions.IgnorePatternWhitespace)));



            var result = await _MongoContext.mProducts.Find(filter).Project(p => new ProductList
            {
                LocationInfo = new ProductLocation
                {
                    CountryName = p.CountryName,
                    CountryCode = p.ParentResort_Id,
                    CityName = p.CityName,
                    CityCode = p.Resort_Id,
                    Lat = p.Lat,
                    Long = p.Long,
                    Address = p.FullAddress,
                    PostCode = p.PostCode
                },
                Code = p.ProductCode,
                Name = p.ProdName,
                VoyagerProductId = p.VoyagerProduct_Id,
                Type = p.ProductType,
            }).ToListAsync();

            result = result.OrderBy(p => p.Name).ToList();

            var res = new ProductListRes();

            if (result.Count() > 0)
            {
                foreach(ProductList p in result)
                {
                    if (p.Type.Trim().ToLower() == "hotel")
                    {
                        filterHAI = Builders<mProductHotelAdditionalInfo>.Filter.Empty;
                        var Product_Id = p.VoyagerProductId;
                        filterHAI = filterHAI & Builders<mProductHotelAdditionalInfo>.Filter.Regex(x => x.ProductId, new BsonRegularExpression(new Regex(Product_Id, RegexOptions.IgnorePatternWhitespace)));

                        var resultHAI = await _MongoContext.mProductHotelAdditionalInfo.Find(filterHAI).Project(x => new
                        {
                            BudgetCategory = x.BudgetCategory,
                            StarRating = x.StarRating,
                            Location = x.Location,
                            HotelChain = x.HotelChain
                        }).FirstOrDefaultAsync();

                        if(resultHAI != null)
                        {
                            p.Location = resultHAI.Location;
                            p.StarRating = resultHAI.StarRating;
                            p.Category = resultHAI.BudgetCategory;
                            p.Chain = resultHAI.HotelChain;
                        }
                    }

                }
                res.Products = result;
            }
            return res;
        }
        #endregion

        public async Task<ProductDetails> GetProductDetail(ProductDetailReq request)
        {
            request.Product_Id = request.Product_Id.ToLower();
            ProductDetailRes response = new ProductDetailRes();
            FilterDefinition<mProducts> filter;
            filter = Builders<mProducts>.Filter.Empty;
            filter = filter & Builders<mProducts>.Filter.Regex(x => x.VoyagerProduct_Id, new BsonRegularExpression(new Regex(request.Product_Id, RegexOptions.IgnorePatternWhitespace)));

            var result = await _MongoContext.mProducts.Find(filter).Project(p => new ProductDetails
            {
                Location = new ProductLocation
                {
                    CountryName = p.CountryName,
                    CountryCode = p.ParentResort_Id,
                    CityName = p.CityName,
                    CityCode = p.Resort_Id,
                    Lat = (p.Lat == "0.00") ? "" : p.Lat ,
                    Long = (p.Long == "0.00") ? "" : p.Long,
                    Address = p.FullAddress,
                    PostCode = p.PostCode
                },
                Code = p.ProductCode ?? "",
                Description = p.ProdDesc ?? "",
                Email = p.Suppmail ?? "",
                Fax = p.Suppfax ?? "",
                IsPlaceHolder = p.Placeholder ?? false,
                Name = p.ProdName ?? "",
                Product_Id = p.VoyagerProduct_Id,
                Telephone = p.Supptel ?? "",
                Type = p.ProductType ?? "",
                Website = p.Suppweb ?? ""
            }).FirstOrDefaultAsync();

            if (result != null)
            {
                if (result.Type.ToLower() == "hotel")
                {
                    var resHotelInfo = (from h in _MongoContext.mProductHotelAdditionalInfo.AsQueryable()
                                        where h.ProductId.ToLower() == request.Product_Id.ToLower()
                                        select new HotelInfo
                                        {
                                            Category = h.BudgetCategory ?? "",
                                            Chain = h.HotelChain ?? "",
                                            HotelType = h.HotelType ?? "",
                                            Location = h.Location ?? "",
                                            StarRating = h.StarRating ?? "",
                                            Corner = h.Corner
                                        }).FirstOrDefault();
                    if (resHotelInfo != null)
                    {
                        result.Metro = resHotelInfo.Corner;
                        resHotelInfo.Corner = null;
                        result.HotelInfo = resHotelInfo;
                    }
                }
                var catInfo = (from c in _MongoContext.mProductCategory.AsQueryable()
                               join p in _MongoContext.mProdCatDef.AsQueryable() on c.ParentCategory_Id equals p.VoyagerDefProductCategoryId //into jp
                               //from jdp in jp.DefaultIfEmpty()
                               where c.Product_Id.ToLower() == request.Product_Id.ToLower()
                               orderby c.Default, c.ProductCategoryName
                               select new ProductCategoryInfo
                               {
                                   Category_Id = c.VoyagerProductCategory_Id,
                                   Name = c.ProductCategoryName ?? "",
                                   IsDefault = c.Default,
                                   ParentCategory = p.Name ?? ""

                               }).ToList();

                if(catInfo.Count > 0)
                {
                    foreach(ProductCategoryInfo cat in catInfo)
                    {
                        var rngInfo = (from r in _MongoContext.mProductRange.AsQueryable()
                                       where r.ProductCategory_Id.ToLower() == cat.Category_Id.ToLower()
                                       orderby r.AdditionalYn, r.PersonType, r.ProductRangeCode
                                       select new ProductRangesInfo
                                       {
                                           Range_Id = r.VoyagerProductRange_Id,
                                           //Capacity = ((r.Quantity ?? 0) == 0) ? "" : r.Quantity.ToString(),
                                           Capacity = r.Quantity,
                                           ChargeBasis = r.PersonType ?? "",
                                           Description = r.ProductRangeName ?? "",
                                           isSupplement = r.AdditionalYn ?? false,
                                           MaxAge = r.Agemax ?? "",
                                           MinAge = r.Agemin ?? "",
                                           Name = r.ProductRangeCode ?? "",
                                           Menu = (result.Type.ToLower() == "meal") ? r.ProductMenu_Id : null
                                       }).ToList();

                        if(rngInfo.Count > 0)
                        {
                            if(result.Type.ToLower() == "meal")
                            {
                                foreach(ProductRangesInfo pr in rngInfo)
                                {
                                    if(pr.Menu != null)
                                    {
                                        var vrMenu = (from m in _MongoContext.mDefProductMenu.AsQueryable()
                                                      where m.ProductMenu_Id.ToLower() == pr.Menu.ToLower()
                                                      select new { Menu = m.ProductMenuDesc}
                                                      ).FirstOrDefault();

                                        if(vrMenu != null && vrMenu.Menu != null)
                                        {
                                            pr.Menu = vrMenu.Menu.Trim();
                                        }
                                    }
                                }
                            }
                            cat.Ranges = rngInfo;
                        }

                    }
                    result.Cateogry = catInfo;
                }


                //response.Product = result;
            }

            return result;

        }
    }
}
