using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VGER_WAPI.Helpers;
using VGER_WAPI.Models;
using VGER_WAPI_CLASSES;


namespace VGER_WAPI.Repositories
{
    public class GuesstimateRepository : IGuesstimateRepository
    {
        #region Private Variable Declaration
        private readonly MongoContext _MongoContext = null;
        private readonly IProductRepository _productRepository;
        private readonly IGenericRepository _genericRepository;
        #endregion

        public GuesstimateRepository(IOptions<MongoSettings> settings, IProductRepository productRepository, IGenericRepository genericRepository)
        {
            _MongoContext = new MongoContext(settings);
            _productRepository = productRepository;
            _genericRepository = genericRepository;
        }

        public GuesstimateGetRes GetGuesstimate(GuesstimateGetReq request)
        {
            GuesstimateGetRes response = new GuesstimateGetRes();
            bool IsStandardPrice = true;

            var ProductType = _MongoContext.mProductType.AsQueryable().ToList();
            List<string> SeriveProduct = GetServiceProduct();
            List<string> UnitProduct = GetUnitProduct();

            response.Guesstimate = _MongoContext.mGuesstimate.AsQueryable().Where(a => a.QRFID == request.QRFID && a.IsCurrentVersion == true).OrderByDescending(b => b.VersionId).FirstOrDefault();

            if (string.IsNullOrEmpty(request.CalculateFor))
            {
                response.Guesstimate.GuesstimatePosition.ForEach(a => a.GuesstimatePrice.RemoveAll(d => !(d.DepartureId == request.DepartureId && d.PaxSlabId == request.PaxSlabId && d.SupplierId == a.ActiveSupplierId)));
            }
            else
            {
                string ChangeRule = response.Guesstimate.ChangeRule;

                if (ChangeRule == "LP")
                {
                    response.Guesstimate.GuesstimatePosition.ForEach(a => a.GuesstimatePrice.RemoveAll(d => !(d.DepartureId == request.DepartureId && d.PaxSlabId == request.PaxSlabId && d.SupplierId == a.ActiveSupplierId)));
                    response.Guesstimate.ChangeRule = ChangeRule;
                    response.Guesstimate.CalculateFor = request.CalculateFor;

                    List<string> RangeId = new List<string>();
                    response.Guesstimate.GuesstimatePosition.ForEach(a => a.GuesstimatePrice.ForEach(b => RangeId.Add(b.ProductRangeId)));
                    var RangeList = _MongoContext.mProductRange.AsQueryable().Where(x => RangeId.Contains(x.VoyagerProductRange_Id)).ToList();

                    foreach (var guessPos in response.Guesstimate.GuesstimatePosition.Where(a => !a.KeepZero))
                    {
                        var guessPrice = 0.0;
                        var contractId = "";
                        var rangeId = "";
                        if (guessPos.ProductType.ToLower() == "hotel")
                        {
                            rangeId = guessPos.GuesstimatePrice.Where(x => x.Type == "ADULT" && x.ProductRangeCode == "TWIN").Select(y => y.ProductRangeId).FirstOrDefault();
                        }
                        else if (SeriveProduct.Contains(guessPos.ProductType.ToLower()))
                        {
                            rangeId = guessPos.GuesstimatePrice.Where(x => x.Type == "ADULT").Select(y => y.ProductRangeId).FirstOrDefault();
                        }
                        else if (UnitProduct.Contains(guessPos.ProductType.ToLower()))
                        {
                            int quantity = 0;
                            rangeId = guessPos.GuesstimatePrice.Where(x => x.Type == "UNIT").Select(y => y.ProductRangeId).FirstOrDefault();
                            foreach (var priceUnit in guessPos.GuesstimatePrice.Where(x => x.Type == "UNIT"))
                            {
                                int quantPrice = Convert.ToInt32(RangeList.Where(x => x.VoyagerProductRange_Id == priceUnit.ProductRangeId).Select(y => y.Quantity).FirstOrDefault());
                                if (quantPrice > quantity)
                                {
                                    rangeId = priceUnit.ProductRangeId;
                                    quantity = quantPrice;
                                }
                            }
                        }

                        GuesstimateGetRes responseSuppPrice1 = new GuesstimateGetRes();
                        request.SupplierId = guessPos.ActiveSupplierId;
                        request.PositionId = guessPos.PositionId;
                        responseSuppPrice1 = GetSupplierPrice(request);

                        guessPos.GuesstimatePrice = responseSuppPrice1.Guesstimate.GuesstimatePosition[0].GuesstimatePrice;

                        guessPrice = guessPos.GuesstimatePrice.Where(x => x.ProductRangeId == rangeId).Select(y => y.BudgetPrice).FirstOrDefault();
                        contractId = guessPos.GuesstimatePrice.Where(x => x.ProductRangeId == rangeId).Select(y => y.ContractId).FirstOrDefault();

                        if (request.CalculateFor == "ALL" || (request.CalculateFor == "B" && guessPrice == 0) || (request.CalculateFor == "BG" && (guessPrice == 0 || string.IsNullOrEmpty(contractId))))
                        {
                            ProductSupplierGetRes objProductSupplierGetRes = new ProductSupplierGetRes();
                            ProductSupplierGetReq requestSupp = new ProductSupplierGetReq();
                            requestSupp.ProductId = guessPos.ProductId;

                            objProductSupplierGetRes = _productRepository.GetProductSupplierList(requestSupp);

                            foreach (var supplier in objProductSupplierGetRes.SupllierList.Where(x => x.SupplierId != guessPos.ActiveSupplierId))
                            {
                                GuesstimateGetRes responseSuppPrice = new GuesstimateGetRes();
                                request.SupplierId = supplier.SupplierId;
                                request.PositionId = guessPos.PositionId;
                                responseSuppPrice = GetSupplierPrice(request);

                                var suppPrice = 0.0;
                                suppPrice = responseSuppPrice.Guesstimate.GuesstimatePosition[0].GuesstimatePrice.Where(x => x.ProductRangeId == rangeId).Select(y => y.BudgetPrice).FirstOrDefault();

                                if (suppPrice > 0 && (suppPrice < guessPrice || guessPrice == 0))
                                {
                                    guessPos.ActiveSupplierId = supplier.SupplierId;
                                    guessPos.ActiveSupplier = supplier.SupplierName;

                                    guessPos.GuesstimatePrice = responseSuppPrice.Guesstimate.GuesstimatePosition[0].GuesstimatePrice;
                                    guessPrice = suppPrice;
                                }
                            }
                        }

                    }
                }

                else if (ChangeRule == "LPP")
                {
                    double ChangeRulePercent = Convert.ToDouble(response.Guesstimate.ChangeRulePercent);

                    response.Guesstimate.GuesstimatePosition.ForEach(a => a.GuesstimatePrice.RemoveAll(d => !(d.DepartureId == request.DepartureId && d.PaxSlabId == request.PaxSlabId)));
                    response.Guesstimate.ChangeRule = ChangeRule;
                    response.Guesstimate.ChangeRulePercent = ChangeRulePercent;
                    response.Guesstimate.CalculateFor = request.CalculateFor;

                    List<string> RangeId = new List<string>();
                    response.Guesstimate.GuesstimatePosition.ForEach(a => a.GuesstimatePrice.ForEach(b => RangeId.Add(b.ProductRangeId)));
                    var RangeList = _MongoContext.mProductRange.AsQueryable().Where(x => RangeId.Contains(x.VoyagerProductRange_Id)).ToList();

                    foreach (var guessPos in response.Guesstimate.GuesstimatePosition.Where(a => !a.KeepZero))
                    {
                        double guessPriceActSup = 0;
                        var contractIdActSup = "";
                        var rangeIdActSup = "";
                        if (request.CalculateFor != "ALL")
                        {
                            if (guessPos.ProductType.ToLower() == "hotel")
                            {
                                rangeIdActSup = guessPos.GuesstimatePrice.Where(x => x.Type == "ADULT" && x.ProductRangeCode == "TWIN" && x.SupplierId == guessPos.ActiveSupplierId).Select(y => y.ProductRangeId).FirstOrDefault();
                            }
                            else if (SeriveProduct.Contains(guessPos.ProductType.ToLower()))
                            {
                                rangeIdActSup = guessPos.GuesstimatePrice.Where(x => x.Type == "ADULT" && x.SupplierId == guessPos.ActiveSupplierId).Select(y => y.ProductRangeId).FirstOrDefault();
                            }
                            else if (UnitProduct.Contains(guessPos.ProductType.ToLower()))
                            {
                                int quantity = 0;
                                rangeIdActSup = guessPos.GuesstimatePrice.Where(x => x.Type == "UNIT" && x.SupplierId == guessPos.ActiveSupplierId).Select(y => y.ProductRangeId).FirstOrDefault();
                                foreach (var priceUnit in guessPos.GuesstimatePrice.Where(x => x.Type == "UNIT"))
                                {
                                    int quantPrice = Convert.ToInt32(RangeList.Where(x => x.VoyagerProductRange_Id == priceUnit.ProductRangeId).Select(y => y.Quantity).FirstOrDefault());
                                    if (quantPrice > quantity)
                                    {
                                        rangeIdActSup = priceUnit.ProductRangeId;
                                        quantity = quantPrice;
                                    }
                                }
                            }
                            guessPriceActSup = guessPos.GuesstimatePrice.Where(x => x.ProductRangeId == rangeIdActSup && x.SupplierId == guessPos.ActiveSupplierId).Select(y => y.BudgetPrice).FirstOrDefault();
                            contractIdActSup = guessPos.GuesstimatePrice.Where(x => x.ProductRangeId == rangeIdActSup && x.SupplierId == guessPos.ActiveSupplierId).Select(y => y.ContractId).FirstOrDefault();
                        }


                        if (request.CalculateFor == "ALL" || (request.CalculateFor == "B" && guessPriceActSup == 0) || (request.CalculateFor == "BG" && (guessPriceActSup == 0 || string.IsNullOrEmpty(contractIdActSup))))
                        {
                            var guessPrice = 0.0;
                            var rangeId = "";
                            if (guessPos.ProductType.ToLower() == "hotel")
                            {
                                rangeId = guessPos.GuesstimatePrice.Where(x => x.Type == "ADULT" && x.ProductRangeCode == "TWIN" && x.SupplierId == guessPos.DefaultSupplierId).Select(y => y.ProductRangeId).FirstOrDefault();
                            }
                            else if (SeriveProduct.Contains(guessPos.ProductType.ToLower()))
                            {
                                rangeId = guessPos.GuesstimatePrice.Where(x => x.Type == "ADULT" && x.SupplierId == guessPos.DefaultSupplierId).Select(y => y.ProductRangeId).FirstOrDefault();
                            }
                            else if (UnitProduct.Contains(guessPos.ProductType.ToLower()))
                            {
                                int quantity = 0;
                                rangeId = guessPos.GuesstimatePrice.Where(x => x.Type == "UNIT" && x.SupplierId == guessPos.DefaultSupplierId).Select(y => y.ProductRangeId).FirstOrDefault();
                                foreach (var priceUnit in guessPos.GuesstimatePrice.Where(x => x.Type == "UNIT" && x.SupplierId == guessPos.DefaultSupplierId))
                                {
                                    int quantPrice = Convert.ToInt32(RangeList.Where(x => x.VoyagerProductRange_Id == priceUnit.ProductRangeId).Select(y => y.Quantity).FirstOrDefault());
                                    if (quantPrice > quantity)
                                    {
                                        rangeId = priceUnit.ProductRangeId;
                                        quantity = quantPrice;
                                    }
                                }
                            }

                            GuesstimateGetRes responseSuppPrice1 = new GuesstimateGetRes();
                            request.SupplierId = guessPos.DefaultSupplierId;
                            request.PositionId = guessPos.PositionId;
                            responseSuppPrice1 = GetSupplierPrice(request);

                            guessPrice = responseSuppPrice1.Guesstimate.GuesstimatePosition[0].GuesstimatePrice.Where(x => x.ProductRangeId == rangeId).Select(y => y.BudgetPrice).FirstOrDefault();

                            //guessPrice = guessPos.GuesstimatePrice.Where(x => x.ProductRangeId == rangeId && x.SupplierId == guessPos.DefaultSupplierId).Select(y => y.BudgetPrice).FirstOrDefault();

                            guessPrice = guessPrice - (guessPrice * ChangeRulePercent / 100);

                            ProductSupplierGetRes objProductSupplierGetRes = new ProductSupplierGetRes();
                            ProductSupplierGetReq requestSupp = new ProductSupplierGetReq();
                            requestSupp.ProductId = guessPos.ProductId;

                            objProductSupplierGetRes = _productRepository.GetProductSupplierList(requestSupp);

                            guessPos.GuesstimatePrice.RemoveAll(d => !(d.SupplierId == guessPos.ActiveSupplierId));

                            foreach (var supplier in objProductSupplierGetRes.SupllierList.Where(x => x.SupplierId != guessPos.DefaultSupplierId))
                            {
                                GuesstimateGetRes responseSuppPrice = new GuesstimateGetRes();
                                request.SupplierId = supplier.SupplierId;
                                request.PositionId = guessPos.PositionId;
                                responseSuppPrice = GetSupplierPrice(request);

                                var suppPrice = 0.0;
                                suppPrice = responseSuppPrice.Guesstimate.GuesstimatePosition[0].GuesstimatePrice.Where(x => x.ProductRangeId == rangeId).Select(y => y.BudgetPrice).FirstOrDefault();

                                if (suppPrice > 0 && (suppPrice < guessPrice || guessPrice == 0))
                                {
                                    guessPos.ActiveSupplierId = supplier.SupplierId;
                                    guessPos.ActiveSupplier = supplier.SupplierName;

                                    guessPos.GuesstimatePrice = responseSuppPrice.Guesstimate.GuesstimatePosition[0].GuesstimatePrice;
                                    guessPrice = suppPrice;
                                }
                            }
                        }
                        else
                        {
                            guessPos.GuesstimatePrice.RemoveAll(d => !(d.SupplierId == guessPos.ActiveSupplierId));
                        }
                    }
                }

                else if (ChangeRule == "PS")
                {
                    response.Guesstimate.GuesstimatePosition.ForEach(a => a.GuesstimatePrice.RemoveAll(d => !(d.DepartureId == request.DepartureId && d.PaxSlabId == request.PaxSlabId)));
                    response.Guesstimate.ChangeRule = ChangeRule;
                    response.Guesstimate.CalculateFor = request.CalculateFor;

                    if (request.CalculateFor == "ALL")
                    {
                        foreach (var guessPos in response.Guesstimate.GuesstimatePosition.Where(a => !a.KeepZero))
                        {
                            guessPos.ActiveSupplierId = guessPos.DefaultSupplierId;
                            guessPos.ActiveSupplier = guessPos.DefaultSupplier;

                            guessPos.GuesstimatePrice.RemoveAll(x => x.SupplierId != guessPos.DefaultSupplierId);

                            GuesstimateGetRes responseSuppPrice = new GuesstimateGetRes();
                            request.SupplierId = guessPos.ActiveSupplierId;
                            request.PositionId = guessPos.PositionId;
                            responseSuppPrice = GetSupplierPrice(request);

                            guessPos.GuesstimatePrice = responseSuppPrice.Guesstimate.GuesstimatePosition[0].GuesstimatePrice;
                        }
                    }
                    else
                    {
                        List<string> RangeId = new List<string>();
                        response.Guesstimate.GuesstimatePosition.ForEach(a => a.GuesstimatePrice.ForEach(b => RangeId.Add(b.ProductRangeId)));
                        var RangeList = _MongoContext.mProductRange.AsQueryable().Where(x => RangeId.Contains(x.VoyagerProductRange_Id)).ToList();

                        foreach (var guessPos in response.Guesstimate.GuesstimatePosition.Where(a => !a.KeepZero))
                        {
                            double guessPrice = 0;
                            var contractId = "";
                            var rangeId = "";
                            if (guessPos.ProductType.ToLower() == "hotel")
                            {
                                rangeId = guessPos.GuesstimatePrice.Where(x => x.Type == "ADULT" && x.ProductRangeCode == "TWIN" && x.SupplierId == guessPos.ActiveSupplierId).Select(y => y.ProductRangeId).FirstOrDefault();
                            }
                            else if (SeriveProduct.Contains(guessPos.ProductType.ToLower()))
                            {
                                rangeId = guessPos.GuesstimatePrice.Where(x => x.Type == "ADULT" && x.SupplierId == guessPos.ActiveSupplierId).Select(y => y.ProductRangeId).FirstOrDefault();
                            }
                            else if (UnitProduct.Contains(guessPos.ProductType.ToLower()))
                            {
                                int quantity = 0;
                                rangeId = guessPos.GuesstimatePrice.Where(x => x.Type == "UNIT" && x.SupplierId == guessPos.ActiveSupplierId).Select(y => y.ProductRangeId).FirstOrDefault();
                                foreach (var priceUnit in guessPos.GuesstimatePrice.Where(x => x.Type == "UNIT"))
                                {
                                    int quantPrice = Convert.ToInt32(RangeList.Where(x => x.VoyagerProductRange_Id == priceUnit.ProductRangeId).Select(y => y.Quantity).FirstOrDefault());
                                    if (quantPrice > quantity)
                                    {
                                        rangeId = priceUnit.ProductRangeId;
                                        quantity = quantPrice;
                                    }
                                }
                            }
                            guessPrice = guessPos.GuesstimatePrice.Where(x => x.ProductRangeId == rangeId && x.SupplierId == guessPos.ActiveSupplierId).Select(y => y.BudgetPrice).FirstOrDefault();
                            contractId = guessPos.GuesstimatePrice.Where(x => x.ProductRangeId == rangeId && x.SupplierId == guessPos.ActiveSupplierId).Select(y => y.ContractId).FirstOrDefault();

                            if (request.CalculateFor == "B")
                            {
                                if (guessPrice == 0)
                                {
                                    guessPos.ActiveSupplierId = guessPos.DefaultSupplierId;
                                    guessPos.ActiveSupplier = guessPos.DefaultSupplier;

                                    guessPos.GuesstimatePrice.RemoveAll(x => x.SupplierId != guessPos.DefaultSupplierId);

                                    GuesstimateGetRes responseSuppPrice = new GuesstimateGetRes();
                                    request.SupplierId = guessPos.ActiveSupplierId;
                                    request.PositionId = guessPos.PositionId;
                                    responseSuppPrice = GetSupplierPrice(request);

                                    guessPos.GuesstimatePrice = responseSuppPrice.Guesstimate.GuesstimatePosition[0].GuesstimatePrice;
                                }
                                else
                                {
                                    guessPos.GuesstimatePrice.RemoveAll(x => x.SupplierId != guessPos.ActiveSupplierId);
                                }
                            }
                            else if (request.CalculateFor == "BG")
                            {
                                if (guessPrice == 0 || string.IsNullOrEmpty(contractId))
                                {
                                    guessPos.ActiveSupplierId = guessPos.DefaultSupplierId;
                                    guessPos.ActiveSupplier = guessPos.DefaultSupplier;

                                    guessPos.GuesstimatePrice.RemoveAll(x => x.SupplierId != guessPos.DefaultSupplierId);

                                    GuesstimateGetRes responseSuppPrice = new GuesstimateGetRes();
                                    request.SupplierId = guessPos.ActiveSupplierId;
                                    request.PositionId = guessPos.PositionId;
                                    responseSuppPrice = GetSupplierPrice(request);

                                    guessPos.GuesstimatePrice = responseSuppPrice.Guesstimate.GuesstimatePosition[0].GuesstimatePrice;
                                }
                                else
                                {
                                    guessPos.GuesstimatePrice.RemoveAll(x => x.SupplierId != guessPos.ActiveSupplierId);
                                }
                            }
                        }
                    }
                }

                #region Save Guesstimate for new prices
                GuesstimateSetReq setRequest = new GuesstimateSetReq();
                setRequest.IsNewVersion = false;
                setRequest.Guesstimate = response.Guesstimate;

                SetGuesstimate(setRequest, true);
                #endregion

            }

            response.LastVersionId = _MongoContext.mGuesstimate.AsQueryable().Where(a => a.QRFID == request.QRFID).OrderByDescending(b => b.VersionId).Select(x => x.VersionId).FirstOrDefault();
            foreach (var guessPos in response.Guesstimate.GuesstimatePosition)
            {
                if (guessPos.StandardPrice == false)
                    IsStandardPrice = false;

                string ProductTypeInit = "";

                ProductTypeInit = ProductType.Where(x => x.Prodtype == guessPos.ProductType).Select(y => y.ProductTypeInitial).FirstOrDefault();

                foreach (var guessPosPrice in guessPos.GuesstimatePrice)
                {
                    if (guessPosPrice.Type == "UNIT")
                    {
                        guessPosPrice.ProductRangeCode = guessPosPrice.ProductRangeCode + "(" + ProductTypeInit + ")";
                        guessPosPrice.Type = guessPosPrice.Type + "(" + ProductTypeInit + ")";
                    }
                }
            }
            response.IsStandardPrice = IsStandardPrice;
            return response;
        }

        public async Task<GuesstimateSetRes> SetGuesstimate(GuesstimateSetReq request, bool fromGet = false)
        {
            GuesstimateSetRes response = new GuesstimateSetRes();
            try
            {
                mGuesstimate guesstimate;
                var curList = request.Guesstimate.GuesstimatePosition.Select(a => a.BuyCurrency).ToList();
                var curDetails = _MongoContext.mCurrency.AsQueryable().Where(a => curList.Contains(a.Currency)).Select(a => new { a.Currency, a.VoyagerCurrency_Id }).ToList();

                if (request.IsNewVersion)
                {
                    guesstimate = _MongoContext.mGuesstimate.AsQueryable().Where(a => a.GuesstimateId == request.Guesstimate.GuesstimateId).FirstOrDefault();

                    var guesstimateOld = guesstimate;
                    guesstimateOld.IsCurrentVersion = false;
                    guesstimateOld.EditUser = request.Guesstimate.EditUser;
                    guesstimateOld.EditDate = DateTime.Now;
                    ReplaceOneResult replaceResult = await _MongoContext.mGuesstimate.ReplaceOneAsync(Builders<mGuesstimate>.Filter.Eq("GuesstimateId", guesstimateOld.GuesstimateId), guesstimateOld);

                    guesstimate._Id = ObjectId.Empty;
                    guesstimate.GuesstimateId = Guid.NewGuid().ToString();
                    guesstimate.ChangeRule = request.Guesstimate.ChangeRule;
                    guesstimate.ChangeRulePercent = request.Guesstimate.ChangeRulePercent;
                    guesstimate.CalculateFor = request.Guesstimate.CalculateFor;
                    guesstimate.VersionId = request.Guesstimate.VersionId;
                    guesstimate.VersionName = request.Guesstimate.VersionName;
                    guesstimate.VersionDescription = request.Guesstimate.VersionDescription;
                    guesstimate.IsCurrentVersion = true;
                    guesstimate.CreateUser = request.Guesstimate.CreateUser;
                    guesstimate.CreateDate = DateTime.Now;

                    foreach (var guessPos in guesstimate.GuesstimatePosition)
                    {
                        foreach (var reqGuessPos in request.Guesstimate.GuesstimatePosition)
                        {
                            if (guessPos.GuesstimatePositionId == reqGuessPos.GuesstimatePositionId)
                            {
                                guessPos.ActiveSupplierId = reqGuessPos.ActiveSupplierId;
                                guessPos.ActiveSupplier = reqGuessPos.ActiveSupplier;
                                guessPos.KeepAs = reqGuessPos.KeepAs;
                                guessPos.KeepZero = reqGuessPos.KeepZero;
                                guessPos.BuyCurrency = reqGuessPos.BuyCurrency;
                                var curid = curDetails.Where(a => a.Currency.ToLower() == guessPos.BuyCurrency.ToLower()).FirstOrDefault().VoyagerCurrency_Id;

                                var priceData = guessPos.GuesstimatePrice.Where(x => x.SupplierId == reqGuessPos.ActiveSupplierId).ToList();
                                if (priceData.Count < 1)
                                {
                                    var priceDataDefault = guessPos.GuesstimatePrice.Where(x => x.SupplierId == guessPos.DefaultSupplierId).ToList();
                                    var priceDataDefaultnew = priceDataDefault;
                                    var objPrice = new GuesstimatePrice();

                                    foreach (var objPositionPrices in priceDataDefault)
                                    {
                                        var item = new GuesstimatePrice();
                                        item.GuesstimatePriceId = Guid.NewGuid().ToString();
                                        item.PositionId = objPositionPrices.PositionId;
                                        item.PositionPriceId = objPositionPrices.PositionPriceId;
                                        item.DepartureId = objPositionPrices.DepartureId;
                                        item.Period = objPositionPrices.Period;
                                        item.PaxSlabId = objPositionPrices.PaxSlabId;
                                        item.PaxSlab = objPositionPrices.PaxSlab;
                                        item.Type = objPositionPrices.Type;
                                        item.RoomId = objPositionPrices.RoomId;
                                        item.SupplierId = reqGuessPos.ActiveSupplierId;
                                        item.Supplier = reqGuessPos.ActiveSupplier;
                                        item.ProductCategoryId = objPositionPrices.ProductCategoryId;
                                        item.ProductCategory = objPositionPrices.ProductCategory;
                                        item.ProductRangeId = objPositionPrices.ProductRangeId;
                                        item.ProductRange = objPositionPrices.ProductRange;
                                        item.ProductRangeCode = objPositionPrices.ProductRangeCode;
                                        item.ProductType = objPositionPrices.ProductType;
                                        item.KeepAs = objPositionPrices.KeepAs;
                                        item.BuyCurrencyId = curid;
                                        item.BuyCurrency = guessPos.BuyCurrency;
                                        item.ContractId = objPositionPrices.ContractId;
                                        item.ContractPrice = objPositionPrices.ContractPrice;
                                        item.BudgetPrice = objPositionPrices.BudgetPrice;
                                        item.BuyPrice = objPositionPrices.BuyPrice;
                                        item.MarkupAmount = objPositionPrices.MarkupAmount;
                                        item.BuyNetPrice = objPositionPrices.BuyNetPrice;
                                        item.SellCurrencyId = objPositionPrices.SellCurrencyId;
                                        item.SellCurrency = objPositionPrices.SellCurrency;
                                        item.SellNetPrice = objPositionPrices.SellNetPrice;
                                        item.TaxAmount = objPositionPrices.TaxAmount;
                                        item.SellPrice = objPositionPrices.SellPrice;
                                        item.ExchangeRateId = objPositionPrices.ExchangeRateId;
                                        item.ExchangeRatio = objPositionPrices.ExchangeRatio;

                                        item.CreateDate = objPositionPrices.CreateDate;
                                        item.CreateUser = objPositionPrices.CreateUser;
                                        item.EditUser = objPositionPrices.EditUser;
                                        item.EditDate = objPositionPrices.EditDate;
                                        item.IsDeleted = objPositionPrices.IsDeleted;

                                        guessPos.GuesstimatePrice.Add(item);
                                    }
                                }

                                if (fromGet)
                                {
                                    foreach (var guessPrice in guessPos.GuesstimatePrice)
                                    { 
                                        foreach (var reqGuessPrice in reqGuessPos.GuesstimatePrice)
                                        {
                                            if (guessPrice.RoomId == reqGuessPrice.RoomId
                                           && guessPrice.SupplierId == reqGuessPos.ActiveSupplierId
                                           && (guessPrice.DepartureId == reqGuessPrice.DepartureId)
                                           && (guessPrice.PaxSlabId == reqGuessPrice.PaxSlabId))
                                            {
                                                guessPrice.BuyCurrencyId = curid;
                                                guessPrice.BuyCurrency = guessPos.BuyCurrency;
                                                guessPrice.BudgetPrice = reqGuessPrice.BudgetPrice;
                                                guessPrice.ContractPrice = reqGuessPrice.ContractPrice;
                                                break;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (var guessPrice in guessPos.GuesstimatePrice)
                                    { 
                                        foreach (var reqGuessPrice in reqGuessPos.GuesstimatePrice)
                                        {
                                            if (guessPrice.RoomId == reqGuessPrice.RoomId
                                           && guessPrice.SupplierId == reqGuessPos.ActiveSupplierId
                                           && (guessPrice.DepartureId == request.DepartureId || request.DepartureId == 0)
                                           && (guessPrice.PaxSlabId == request.PaxSlabId || request.PaxSlabId == 0))
                                            {
                                                guessPrice.BuyCurrencyId = curid;
                                                guessPrice.BuyCurrency = guessPos.BuyCurrency;
                                                guessPrice.BudgetPrice = reqGuessPrice.BudgetPrice;
                                                guessPrice.ContractPrice = reqGuessPrice.ContractPrice;
                                                break;
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }
                    await _MongoContext.mGuesstimate.InsertOneAsync(guesstimate);
                    response.ResponseStatus.Status = "Success";
                    response.ResponseStatus.ErrorMessage = "Saved Successfully.";
                }
                else
                {
                    guesstimate = _MongoContext.mGuesstimate.AsQueryable().Where(a => a.GuesstimateId == request.Guesstimate.GuesstimateId).FirstOrDefault();
                    guesstimate.ChangeRule = request.Guesstimate.ChangeRule;
                    guesstimate.ChangeRule = request.Guesstimate.ChangeRule;
                    guesstimate.ChangeRulePercent = request.Guesstimate.ChangeRulePercent;
                    guesstimate.CalculateFor = request.Guesstimate.CalculateFor;
                    guesstimate.EditUser = request.Guesstimate.EditUser;
                    guesstimate.EditDate = DateTime.Now;

                    foreach (var guessPos in guesstimate.GuesstimatePosition)
                    {
                        foreach (var reqGuessPos in request.Guesstimate.GuesstimatePosition.Where(x => x.GuesstimatePositionId == guessPos.GuesstimatePositionId))
                        {
                            if (guessPos.GuesstimatePositionId == reqGuessPos.GuesstimatePositionId)
                            {
                                guessPos.ActiveSupplierId = reqGuessPos.ActiveSupplierId;
                                guessPos.ActiveSupplier = reqGuessPos.ActiveSupplier;
                                guessPos.KeepAs = reqGuessPos.KeepAs;
                                guessPos.KeepZero = reqGuessPos.KeepZero;
                                guessPos.BuyCurrency = reqGuessPos.BuyCurrency;
                                var curid = curDetails.Where(a => a.Currency.ToLower() == guessPos.BuyCurrency.ToLower()).FirstOrDefault().VoyagerCurrency_Id;

                                var priceData = guessPos.GuesstimatePrice.Where(x => x.SupplierId == reqGuessPos.ActiveSupplierId).ToList();
                                if (priceData.Count < 1)
                                {
                                    var priceDataDefault = guessPos.GuesstimatePrice.Where(x => x.SupplierId == guessPos.DefaultSupplierId).ToList();
                                    var priceDataDefaultnew = priceDataDefault;
                                    var objPrice = new GuesstimatePrice();

                                    foreach (var objPositionPrices in priceDataDefault)
                                    {
                                        var item = new GuesstimatePrice();
                                        item.GuesstimatePriceId = Guid.NewGuid().ToString();
                                        item.PositionId = objPositionPrices.PositionId;
                                        item.PositionPriceId = objPositionPrices.PositionPriceId;
                                        item.DepartureId = objPositionPrices.DepartureId;
                                        item.Period = objPositionPrices.Period;
                                        item.PaxSlabId = objPositionPrices.PaxSlabId;
                                        item.PaxSlab = objPositionPrices.PaxSlab;
                                        item.Type = objPositionPrices.Type;
                                        item.RoomId = objPositionPrices.RoomId;
                                        item.SupplierId = reqGuessPos.ActiveSupplierId;
                                        item.Supplier = reqGuessPos.ActiveSupplier;
                                        item.ProductCategoryId = objPositionPrices.ProductCategoryId;
                                        item.ProductCategory = objPositionPrices.ProductCategory;
                                        item.ProductRangeId = objPositionPrices.ProductRangeId;
                                        item.ProductRange = objPositionPrices.ProductRange;
                                        item.ProductRangeCode = objPositionPrices.ProductRangeCode;
                                        item.ProductType = objPositionPrices.ProductType;
                                        item.KeepAs = objPositionPrices.KeepAs;
                                        item.BuyCurrencyId = curid;
                                        item.BuyCurrency = guessPos.BuyCurrency;
                                        item.ContractId = objPositionPrices.ContractId;
                                        item.ContractPrice = objPositionPrices.ContractPrice;
                                        item.BudgetPrice = objPositionPrices.BudgetPrice;
                                        item.BuyPrice = objPositionPrices.BuyPrice;
                                        item.MarkupAmount = objPositionPrices.MarkupAmount;
                                        item.BuyNetPrice = objPositionPrices.BuyNetPrice;
                                        item.SellCurrencyId = objPositionPrices.SellCurrencyId;
                                        item.SellCurrency = objPositionPrices.SellCurrency;
                                        item.SellNetPrice = objPositionPrices.SellNetPrice;
                                        item.TaxAmount = objPositionPrices.TaxAmount;
                                        item.SellPrice = objPositionPrices.SellPrice;
                                        item.ExchangeRateId = objPositionPrices.ExchangeRateId;
                                        item.ExchangeRatio = objPositionPrices.ExchangeRatio;

                                        item.CreateDate = objPositionPrices.CreateDate;
                                        item.CreateUser = objPositionPrices.CreateUser;
                                        item.EditUser = objPositionPrices.EditUser;
                                        item.EditDate = objPositionPrices.EditDate;
                                        item.IsDeleted = objPositionPrices.IsDeleted;

                                        guessPos.GuesstimatePrice.Add(item);
                                    }
                                }

                                if (fromGet)
                                {
                                    foreach (var guessPrice in guessPos.GuesstimatePrice)
                                    { 
                                        foreach (var reqGuessPrice in reqGuessPos.GuesstimatePrice)
                                        {
                                            guessPrice.BuyCurrencyId = curid;
                                            guessPrice.BuyCurrency = guessPos.BuyCurrency;
                                            if (guessPrice.RoomId == reqGuessPrice.RoomId
                                                && guessPrice.SupplierId == reqGuessPos.ActiveSupplierId
                                                && (guessPrice.DepartureId == reqGuessPrice.DepartureId)
                                                && (guessPrice.PaxSlabId == reqGuessPrice.PaxSlabId))
                                            {
                                                guessPrice.BudgetPrice = reqGuessPrice.BudgetPrice;
                                                guessPrice.ContractId = reqGuessPrice.ContractId;
                                                guessPrice.ContractPrice = reqGuessPrice.ContractPrice;
                                                break;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (var guessPrice in guessPos.GuesstimatePrice)
                                    { 
                                        foreach (var reqGuessPrice in reqGuessPos.GuesstimatePrice)
                                        {
                                            if (guessPrice.RoomId == reqGuessPrice.RoomId
                                                && guessPrice.SupplierId == reqGuessPos.ActiveSupplierId
                                                && (guessPrice.DepartureId == request.DepartureId || request.DepartureId == 0)
                                                && (guessPrice.PaxSlabId == request.PaxSlabId || request.PaxSlabId == 0))
                                            {
                                                guessPrice.BuyCurrencyId = curid;
                                                guessPrice.BuyCurrency = guessPos.BuyCurrency;
                                                guessPrice.BudgetPrice = reqGuessPrice.BudgetPrice;
                                                guessPrice.ContractId = reqGuessPrice.ContractId;
                                                guessPrice.ContractPrice = reqGuessPrice.ContractPrice;
                                                break;
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }
                    ReplaceOneResult replaceResult = await _MongoContext.mGuesstimate.ReplaceOneAsync(Builders<mGuesstimate>.Filter.Eq("GuesstimateId", guesstimate.GuesstimateId), guesstimate);
                    response.ResponseStatus.Status = replaceResult.MatchedCount > 0 ? "Success" : "Failure";
                    response.ResponseStatus.ErrorMessage = replaceResult.MatchedCount > 0 ? "Saved Successfully." : "Details not updated.";
                }

                //if (string.IsNullOrEmpty(item.GuesstimateId) || item.GuesstimateId == Guid.Empty.ToString())
                //{
                //    objPositionPrices = _MongoContext.mPositionPrice.AsQueryable().Where(a => a.PositionPriceId == item.PositionPriceId).FirstOrDefault();

                //    item.GuesstimateId = Guid.NewGuid().ToString();
                //    item.QRFID = objPositionPrices.QRFID;
                //    item.PositionId = objPositionPrices.PositionId;
                //    item.PositionPriceId = objPositionPrices.PositionPriceId;
                //    item.DepartureId = objPositionPrices.DepartureId;
                //    item.Period = objPositionPrices.Period;
                //    item.PaxSlabId = objPositionPrices.PaxSlabId;
                //    item.PaxSlab = objPositionPrices.PaxSlab;
                //    item.Type = objPositionPrices.Type;
                //    item.RoomId = objPositionPrices.RoomId;
                //    //item.SupplierId = objPositionPrices.SupplierId;
                //    //item.Supplier = objPositionPrices.Supplier;
                //    item.ProductCategoryId = objPositionPrices.ProductCategoryId;
                //    item.ProductRangeId = objPositionPrices.ProductRangeId;
                //    item.ProductRange = objPositionPrices.ProductRange;
                //    item.ProductRangeCode = objPositionPrices.ProductRangeCode;
                //    item.ProductType = objPositionPrices.ProductType;
                //    //item.KeepAs = objPositionPrices.KeepAs;
                //    item.BuyCurrencyId = objPositionPrices.BuyCurrencyId;
                //    item.BuyCurrency = objPositionPrices.BuyCurrency;
                //    item.ContractId = objPositionPrices.ContractId;
                //    item.ContractPrice = objPositionPrices.ContractPrice;
                //    //item.BudgetPrice = objPositionPrices.BudgetPrice;
                //    item.BuyPrice = objPositionPrices.BuyPrice;
                //    item.MarkupAmount = objPositionPrices.MarkupAmount;
                //    item.BuyNetPrice = objPositionPrices.BuyNetPrice;
                //    item.SellCurrencyId = objPositionPrices.SellCurrencyId;
                //    item.SellCurrency = objPositionPrices.SellCurrency;
                //    item.SellNetPrice = objPositionPrices.SellNetPrice;
                //    item.TaxAmount = objPositionPrices.TaxAmount;
                //    item.SellPrice = objPositionPrices.SellPrice;
                //    item.ExchangeRateId = objPositionPrices.ExchangeRateId;
                //    item.ExchangeRatio = objPositionPrices.ExchangeRatio;

                //    item.CreateDate = DateTime.Now;
                //    item.EditUser = "";
                //    item.EditDate = null;
                //    item.IsDeleted = false;

                //    await _MongoContext.mGuesstimate.InsertOneAsync(item);
                //    response.ResponseStatus.Status = "Success";
                //    response.ResponseStatus.ErrorMessage = "Saved Successfully.";
                //}
                //else
                //{
                //    objPositionPrices = _MongoContext.mGuesstimate.AsQueryable().Where(a => a.PositionPriceId == item.PositionPriceId).FirstOrDefault();
                //    item._Id = objPositionPrices._Id;
                //    item.CreateDate = objPositionPrices.CreateDate;
                //    item.CreateUser = objPositionPrices.CreateUser;
                //    item.EditDate = DateTime.Now;
                //    item.IsDeleted = objPositionPrices.IsDeleted;

                //    ReplaceOneResult replaceResult = await _MongoContext.mGuesstimate.ReplaceOneAsync(Builders<mGuesstimate>.Filter.Eq("PositionPriceId", item.PositionPriceId), item);
                //    response.ResponseStatus.Status = replaceResult.MatchedCount > 0 ? "Success" : "Failure";
                //    response.ResponseStatus.ErrorMessage = replaceResult.MatchedCount > 0 ? "Saved Successfully." : "Details not updated.";

                //}

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }
            return response;
        }

        public List<GuesstimateVersion> GetGuesstimateVersions(GuesstimateGetReq request)
        {
            List<GuesstimateVersion> response = new List<GuesstimateVersion>();

            var guesstimateList = _MongoContext.mGuesstimate.AsQueryable().Where(a => a.QRFID == request.QRFID).OrderByDescending(b => b.VersionId).ToList();

            foreach (var guess in guesstimateList.Where(x => !x.IsDeleted))
            {
                response.Add(new GuesstimateVersion
                {
                    QRFID = guess.QRFID,
                    GuesstimateId = guess.GuesstimateId,
                    VersionId = guess.VersionId,
                    VersionName = guess.VersionName,
                    VersionDescription = guess.VersionDescription,
                    VersionCreateDate = guess.CreateDate,
                    IsCurrentVersion = guess.IsCurrentVersion
                });
            }
            return response;
        }

        public async Task<GuesstimateSetRes> UpdateGuesstimateVersion(GuesstimateVersionSetReq request)
        {
            GuesstimateSetRes response = new GuesstimateSetRes();
            try
            {
                List<mGuesstimate> guesstimate;
                guesstimate = _MongoContext.mGuesstimate.AsQueryable().Where(a => a.QRFID == request.QRFID && a.IsCurrentVersion == true).ToList();

                foreach (var guess in guesstimate)
                {
                    guess.IsCurrentVersion = false;
                    guess.EditUser = request.EditUser;
                    guess.EditDate = DateTime.Now;
                    ReplaceOneResult replaceResult = await _MongoContext.mGuesstimate.ReplaceOneAsync(Builders<mGuesstimate>.Filter.Eq("GuesstimateId", guess.GuesstimateId), guess);
                    response.ResponseStatus.Status = replaceResult.MatchedCount > 0 ? "Success" : "Failure";
                    response.ResponseStatus.ErrorMessage = replaceResult.MatchedCount > 0 ? "Saved Successfully." : "Details not updated.";
                }

                mGuesstimate guesstimateNew;
                guesstimateNew = _MongoContext.mGuesstimate.AsQueryable().Where(a => a.QRFID == request.QRFID && a.GuesstimateId == request.GuesstimateId).FirstOrDefault();

                guesstimateNew.IsCurrentVersion = true;
                guesstimateNew.EditUser = request.EditUser;
                guesstimateNew.EditDate = DateTime.Now;
                ReplaceOneResult replaceResultNew = await _MongoContext.mGuesstimate.ReplaceOneAsync(Builders<mGuesstimate>.Filter.Eq("GuesstimateId", guesstimateNew.GuesstimateId), guesstimateNew);
                response.ResponseStatus.Status = replaceResultNew.MatchedCount > 0 ? "Success" : "Failure";
                response.ResponseStatus.ErrorMessage = replaceResultNew.MatchedCount > 0 ? "Version changed Successfully." : "Version not changed.";

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }
            return response;
        }

        public GuesstimateGetRes GetSupplierPrice(GuesstimateGetReq request)
        {
            GuesstimateGetRes response = new GuesstimateGetRes();

            var ProductType = _MongoContext.mProductType.AsQueryable().ToList();
            response.Guesstimate = _MongoContext.mGuesstimate.AsQueryable().Where(a => a.QRFID == request.QRFID && a.IsCurrentVersion == true).OrderByDescending(b => b.VersionId).FirstOrDefault();
            response.Guesstimate.GuesstimatePosition.RemoveAll(x => x.PositionId != request.PositionId);
            //var activeSupplierPrice = response.Guesstimate.GuesstimatePosition[0].GuesstimatePrice.Where(d => (d.DepartureId == request.DepartureId && d.PaxSlabId == request.PaxSlabId && d.SupplierId == request.SupplierId)).ToList();
            var activeSupplierPrice = response.Guesstimate.GuesstimatePosition[0].GuesstimatePrice.Where(d => (d.SupplierId == request.SupplierId)).ToList();
            //response.Guesstimate.GuesstimatePosition.ForEach(a => a.GuesstimatePrice.RemoveAll(d => !(d.DepartureId == request.DepartureId && d.PaxSlabId == request.PaxSlabId && d.SupplierId == a.DefaultSupplierId)));
            response.Guesstimate.GuesstimatePosition.ForEach(a => a.GuesstimatePrice.RemoveAll(d => !(d.SupplierId == a.DefaultSupplierId)));

            //if (response.Guesstimate.GuesstimatePosition[0].DefaultSupplierId != request.SupplierId)
            //{
            response.Guesstimate.GuesstimatePosition[0].GuesstimatePrice.ForEach(a => a.BudgetPrice = 0);
            response.Guesstimate.GuesstimatePosition[0].GuesstimatePrice.ForEach(a => a.ContractId = "");
            response.Guesstimate.GuesstimatePosition[0].GuesstimatePrice.ForEach(a => a.ContractPrice = 0);

            var lstProductList = new List<string>();
            lstProductList.Add(response.Guesstimate.GuesstimatePosition[0].ProductId);
            ProdContractGetRes prodContractGetRes = new ProdContractGetRes();
            ProdContractGetReq prodContractGetReq = new ProdContractGetReq
            {
                QRFID = request.QRFID,
                ProductIDList = lstProductList
            };
            var rangelist = response.Guesstimate.GuesstimatePosition[0].GuesstimatePrice.Select(c => c.ProductRangeId).ToList();
            prodContractGetRes = _productRepository.GetContractRatesByProductID(prodContractGetReq, rangelist);

            foreach (var resPrice in response.Guesstimate.GuesstimatePosition[0].GuesstimatePrice)
            {
                foreach (var actPrice in activeSupplierPrice)
                {
                    if (resPrice.PositionPriceId == actPrice.PositionPriceId)
                    {
                        resPrice.SupplierId = actPrice.SupplierId;
                        resPrice.Supplier = actPrice.Supplier;
                        resPrice.BudgetPrice = actPrice.BudgetPrice;
                        resPrice.ContractPrice = actPrice.ContractPrice;
                    }
                }
            }

            List<ProductContractInfo> lstProductContractInfo = new List<ProductContractInfo>();
            if (prodContractGetRes != null && prodContractGetRes.ProductContractInfo.Count > 0)
            {
                for (int i = 0; i < response.Guesstimate.GuesstimatePosition[0].GuesstimatePrice.Count; i++)
                {
                    lstProductContractInfo = prodContractGetRes.ProductContractInfo.Where(a => a.SupplierId == request.SupplierId && a.ProductId == response.Guesstimate.GuesstimatePosition[0].ProductId).ToList();
                    if (lstProductContractInfo != null && lstProductContractInfo.Count > 0)
                    {
                        for (int j = 0; j < lstProductContractInfo.Count; j++)
                        {
                            if (response.Guesstimate.GuesstimatePosition[0].GuesstimatePrice[i].ProductRangeId == lstProductContractInfo[j].ProductRangeId)
                            {
                                if (lstProductContractInfo[j].FromDate <= response.Guesstimate.GuesstimatePosition[0].GuesstimatePrice[i].Period && response.Guesstimate.GuesstimatePosition[0].GuesstimatePrice[i].Period <= lstProductContractInfo[j].ToDate)
                                {
                                    response.Guesstimate.GuesstimatePosition[0].GuesstimatePrice[i].BudgetPrice = Convert.ToDouble(lstProductContractInfo[j].Price);
                                    response.Guesstimate.GuesstimatePosition[0].GuesstimatePrice[i].ContractId = lstProductContractInfo[j].ContractId;
                                    response.Guesstimate.GuesstimatePosition[0].GuesstimatePrice[i].ContractPrice = Convert.ToDouble(lstProductContractInfo[j].Price);
                                    break;
                                }
                            }
                        }
                    }
                }
            }


            #region GetMarkupValue and Add in Contract price

            bool IsSalesOfficeUser = _genericRepository.IsSalesOfficeUser(request.LoginUserId);
            var curId1 = _MongoContext.Products.AsQueryable().Where(x => x.VoyagerProduct_Id == response.Guesstimate.GuesstimatePosition[0].ProductId)?.Select(a => a.ProductSuppliers)?.FirstOrDefault()?.
                                    Where(a => a.Company_Id == request.SupplierId).Select(a => a.CurrencyId).FirstOrDefault();
            if (IsSalesOfficeUser == true)
            {                 
                var curId = _MongoContext.Products.AsQueryable().Where(x => x.VoyagerProduct_Id == response.Guesstimate.GuesstimatePosition[0].ProductId).Select(a => a.ProductSuppliers)?.FirstOrDefault()?.
                                    Where(a => a.Company_Id == request.SupplierId).Select(a => a.CurrencyId).FirstOrDefault();

                var UserCompany_Id = _MongoContext.mUsers.AsQueryable().Where(x => !string.IsNullOrEmpty(x.UserName) && x.UserName.ToLower() == request.LoginUserId.ToLower().Trim()).Select(y => y.Company_Id).FirstOrDefault();
                var Markup_Id = _MongoContext.mCompanies.AsQueryable().Where(x => x.Company_Id == UserCompany_Id && x.Markups.Any(y => y.Markup_For == "Groups")).FirstOrDefault().Markups.FirstOrDefault().Markup_Id;

                if (!string.IsNullOrEmpty(Markup_Id))
                {
                    for (int i = 0; i < response.Guesstimate.GuesstimatePosition[0].GuesstimatePrice.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(response.Guesstimate.GuesstimatePosition[0].GuesstimatePrice[i].ContractId))
                        {
                            ProdMarkupsGetReq prodMarkupsGetReq = new ProdMarkupsGetReq();

                            prodMarkupsGetReq.MarkupsId = Markup_Id;
                            prodMarkupsGetReq.ProductType = response.Guesstimate.GuesstimatePosition[0].GuesstimatePrice[i].ProductType;
                            var MarkupDetails = _productRepository.GetProdMarkups(prodMarkupsGetReq).Result;

                            if (MarkupDetails != null)
                            {
                                double MarkupValue = Convert.ToDouble(MarkupDetails.PercMarkUp) <= 0 ? Convert.ToDouble(MarkupDetails.FixedMarkUp) : Convert.ToDouble(MarkupDetails.PercMarkUp);

                                if (MarkupDetails.MARKUPTYPE == "Fixed")
                                {
                                    double markup = MarkupValue;
                                    if (MarkupDetails.CURRENCY_ID != curId)
                                    {
                                        var rate = _genericRepository.getExchangeRate(MarkupDetails.CURRENCY_ID, curId, request.QRFID);
                                        if (rate != null)
                                            markup = MarkupValue * Convert.ToDouble(rate.Value);
                                    }
                                    if (markup > 0)
                                        response.Guesstimate.GuesstimatePosition[0].GuesstimatePrice[i].BudgetPrice = response.Guesstimate.GuesstimatePosition[0].GuesstimatePrice[i].BudgetPrice + Math.Round(markup, 2);
                                }
                                else
                                {
                                    response.Guesstimate.GuesstimatePosition[0].GuesstimatePrice[i].BudgetPrice = response.Guesstimate.GuesstimatePosition[0].GuesstimatePrice[i].BudgetPrice + (response.Guesstimate.GuesstimatePosition[0].GuesstimatePrice[i].BudgetPrice * MarkupValue / 100);
                                }
                            }
                        }
                    }
                }
            }

            #endregion
            // }

            return response;
        }

        public async Task<GuesstimateSetRes> SetGuesstimateChangeRule(GuesstimateChangeRuleSetReq request)
        {
            GuesstimateSetRes response = new GuesstimateSetRes();
            try
            {
                mGuesstimate guesstimate;
                guesstimate = _MongoContext.mGuesstimate.AsQueryable().Where(a => a.GuesstimateId == request.GuesstimateId).FirstOrDefault();

                guesstimate.ChangeRule = request.ChangeRule;
                guesstimate.ChangeRulePercent = request.ChangeRulePercent;
                guesstimate.EditUser = request.EditUser;
                guesstimate.EditDate = DateTime.Now;
                ReplaceOneResult replaceResult = await _MongoContext.mGuesstimate.ReplaceOneAsync(Builders<mGuesstimate>.Filter.Eq("GuesstimateId", guesstimate.GuesstimateId), guesstimate);
                response.GuesstimateId = guesstimate.GuesstimateId;
                response.ResponseStatus.Status = replaceResult.MatchedCount > 0 ? "Success" : "Failure";
                response.ResponseStatus.ErrorMessage = replaceResult.MatchedCount > 0 ? "Saved Successfully." : "Details not updated.";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                response.ResponseStatus.Status = "Failure";
                response.ResponseStatus.ErrorMessage = ex.Message;
            }
            return response;
        }

        public List<string> GetServiceProduct()
        {
            List<string> list = new List<string>();

            list.Add("meal");
            list.Add("attractions");
            list.Add("sightseeing - citytour");
            list.Add("visa");
            list.Add("insurance");
            list.Add("ferry passenger");
            list.Add("scheduled transfer");
            list.Add("train");
            list.Add("domestic flight");
            list.Add("overnight ferry");

            return list;
        }

        public List<string> GetUnitProduct()
        {
            List<string> list = new List<string>();

            list.Add("coach");
            list.Add("ferry transfer");
            list.Add("guide");
            list.Add("private transfer");
            list.Add("ldc");

            return list;
        }
    }
}
