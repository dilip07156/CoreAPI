using Microsoft.Extensions.Options;
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
    public class CostsheetRepository : ICostsheetRepository
    {
        #region Private Variable Declaration
        private readonly MongoContext _MongoContext = null;
        #endregion

        public CostsheetRepository(IOptions<MongoSettings> settings)
        {
            _MongoContext = new MongoContext(settings);
        }

        public CostsheetGetRes GetCostsheetData(CostsheetGetReq request)
        {
            CostsheetGetRes response = new CostsheetGetRes();
            var QRFPrice = _MongoContext.mQRFPrice.AsQueryable().Where(a => a.QRFID == request.QRFID && a.IsCurrentVersion == true).OrderByDescending(b => b.VersionId).FirstOrDefault();

            response.CostsheetVersion.QRFID = QRFPrice.QRFID;
            response.CostsheetVersion.QRFPriceId = QRFPrice.QRFPrice_Id;
            response.CostsheetVersion.VersionId = QRFPrice.VersionId;
            response.CostsheetVersion.VersionName = QRFPrice.VersionName;
            response.CostsheetVersion.VersionDescription = QRFPrice.VersionDescription;
            response.CostsheetVersion.IsCurrentVersion = QRFPrice.IsCurrentVersion;
            response.CostsheetVersion.VersionCreateDate = QRFPrice.CreateDate;

            response.QRFSalesFOC = QRFPrice.QRFSalesFOC.AsQueryable().Where(a => a.DateRangeId == request.DepartureId).ToList();

            if (request.DepartureId > 0)
            {
                response.QrfPackagePrice = _MongoContext.mQRFPackagePrice.AsQueryable().Where(a => a.QRFPrice_Id == QRFPrice.QRFPrice_Id && a.Departure_Id == request.DepartureId).ToList();
                response.QrfNonPackagePrice = _MongoContext.mQRFNonPackagedPrice.AsQueryable().Where(a => a.QRFPrice_Id == QRFPrice.QRFPrice_Id && a.Departure_Id == request.DepartureId).ToList();
                response.QRFPositionTotalCost = _MongoContext.mQRFPositionTotalCost.AsQueryable().Where(a => a.QRFPrice_Id == QRFPrice.QRFPrice_Id && a.Departure_Id == request.DepartureId).ToList();
            }
            else
            {
                response.QrfPackagePrice = _MongoContext.mQRFPackagePrice.AsQueryable().Where(a => a.QRFPrice_Id == QRFPrice.QRFPrice_Id).ToList();
                response.QrfNonPackagePrice = _MongoContext.mQRFNonPackagedPrice.AsQueryable().Where(a => a.QRFPrice_Id == QRFPrice.QRFPrice_Id).ToList();
                response.QRFPositionTotalCost = _MongoContext.mQRFPositionTotalCost.AsQueryable().Where(a => a.QRFPrice_Id == QRFPrice.QRFPrice_Id).ToList();
            }

            if (request.PaxSlabId > 0)
            {
                response.QrfPackagePrice = response.QrfPackagePrice.Where(x => x.PaxSlab_Id == request.PaxSlabId).ToList();
                response.QrfNonPackagePrice = response.QrfNonPackagePrice.Where(x => x.PaxSlab_Id == request.PaxSlabId).ToList();
            }

            //response.QRFPositionTotalCost = _MongoContext.mQRFPositionTotalCost.AsQueryable().Where(a => a.QRFPrice_Id == QRFPrice.QRFPrice_Id && a.Departure_Id == request.DepartureId).ToList();

            return response;
        }

        public List<CostsheetVersion> GetCostsheetVersions(CostsheetGetReq request)
        {
            List<CostsheetVersion> response = new List<CostsheetVersion>();

            var costsheetList = _MongoContext.mQRFPrice.AsQueryable().Where(a => a.QRFID == request.QRFID).OrderByDescending(b => b.VersionId).ToList();

            foreach (var costsheet in costsheetList.Where(x => !x.IsDeleted))
            {
                response.Add(new CostsheetVersion
                {
                    QRFID = costsheet.QRFID,
                    QRFPriceId = costsheet.QRFPrice_Id,
                    VersionId = costsheet.VersionId,
                    VersionName = costsheet.VersionName,
                    VersionDescription = costsheet.VersionDescription,
                    VersionCreateDate = costsheet.CreateDate,
                    IsCurrentVersion = costsheet.IsCurrentVersion
                });
            }
            return response;
        }

        public async Task<CostsheetVerSetRes> UpdateCostsheetVersion(CostsheetVerSetReq request)
        {
            CostsheetVerSetRes response = new CostsheetVerSetRes();
            try
            {
                List<mQRFPrice> qrfPriceList;
                qrfPriceList = _MongoContext.mQRFPrice.AsQueryable().Where(a => a.QRFID == request.QRFID && a.IsCurrentVersion == true).ToList();

                foreach (var objqrfprice in qrfPriceList)
                {
                    objqrfprice.IsCurrentVersion = false;
                    objqrfprice.EditUser = request.Create_User;
                    objqrfprice.EditDate = DateTime.Now;
                    ReplaceOneResult replaceResult = await _MongoContext.mQRFPrice.ReplaceOneAsync(Builders<mQRFPrice>.Filter.Eq("QRFPrice_Id", objqrfprice.QRFPrice_Id), objqrfprice);
                    response.ResponseStatus.Status = replaceResult.MatchedCount > 0 ? "Success" : "Failure";
                    response.ResponseStatus.ErrorMessage = replaceResult.MatchedCount > 0 ? "Saved Successfully." : "Details not updated.";
                }

                mQRFPrice qrfPriceNew;
                qrfPriceNew = _MongoContext.mQRFPrice.AsQueryable().Where(a => a.QRFID == request.QRFID && a.QRFPrice_Id == request.QRFPriceId).FirstOrDefault();

                qrfPriceNew.IsCurrentVersion = true;
                qrfPriceNew.EditUser = request.Create_User;
                qrfPriceNew.EditDate = DateTime.Now;
                ReplaceOneResult replaceResultNew = await _MongoContext.mQRFPrice.ReplaceOneAsync(Builders<mQRFPrice>.Filter.Eq("QRFPrice_Id", qrfPriceNew.QRFPrice_Id), qrfPriceNew);
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

        //below function will check if Active Costheet version has zero price then it will return error msg
        public async Task<ResponseStatus> CheckActiveCostsheetPrice(CostsheetGetReq request)
        {
            ResponseStatus response = new ResponseStatus();
            try
            {
                var QRFPrice = await _MongoContext.mQRFPrice.FindAsync(m => m.QRFID == request.QRFID && m.IsCurrentVersion == true).Result.FirstOrDefaultAsync();

                if (QRFPrice != null)
                {
                    bool flag = false;
                    var lstQrfPackagePrice = _MongoContext.mQRFPackagePrice.AsQueryable().Where(a => a.QRFPrice_Id == QRFPrice.QRFPrice_Id).ToList();

                    if (lstQrfPackagePrice.Count == 0)
                    {
                        flag = true;
                    }
                    else
                    {
                        var strRoomType = new List<string> { "CHILDWITHOUTBED", "CHILDWITHBED", "INFANT" };
                        var objQrfPackagePrice = lstQrfPackagePrice.Where(a => a.SellPrice == 0 && !strRoomType.Contains(a.RoomName)).FirstOrDefault();
                        if (objQrfPackagePrice != null)
                        {
                            flag = true;
                        }
                    }
                    if (flag)
                    {
                        response.Status = "Failure";
                        if (request.EnquiryPipeline.ToLower() == "costing approval pipeline")
                        {
                            response.ErrorMessage = "Can not submit costsheet with 0.00 value. Please Reject costsheet to enter rates into Guesstimate page and re-generate the costsheet.";
                        }
                        else
                        {
                            response.ErrorMessage = "Can not submit costsheet with 0.00 value. Please enter cost into Guesstimate page and re-generate the costsheet.";
                        }
                    }
                    else
                    {
                        response.Status = "Success";
                    }
                }
                else
                {
                    response.Status = "Failure";
                    response.ErrorMessage = "QRFID not found in mQRFPrice.";
                }
            }
            catch (Exception ex)
            {
                response.Status = "Failure";
                response.ErrorMessage = "An Error Occurs :- " + ex.Message;
            }
            return response;
        }

        //public async Task<MailSetRes> SetEmailDetails(MailSetReq request)
        //{
        //    MailSetRes response = new MailSetRes();
        //    response.ResponseStatus = new ResponseStatus();
        //    if (request != null && !string.IsNullOrEmpty(request.QRFID) && !string.IsNullOrEmpty(request.QRFPriceID))
        //    {
        //        var resQuote = await _MongoContext.mQuote.FindAsync(a => a.QRFID == request.QRFID).Result.FirstOrDefaultAsync();
        //        if (resQuote != null)
        //        {
        //            var QRFPrice = _MongoContext.mQRFPrice.AsQueryable().Where(x => x.QRFID == request.QRFID && x.QRFPrice_Id == request.QRFPriceID).FirstOrDefault();
        //            if (QRFPrice != null)
        //            {
        //                request.EmailDetails.EmailDetailsId = string.IsNullOrEmpty(request.EmailDetails.EmailDetailsId) ? Guid.NewGuid().ToString() : request.EmailDetails.EmailDetailsId;
        //                UpdateResult resultFlag = await _MongoContext.mQRFPrice.UpdateOneAsync(Builders<mQRFPrice>.Filter.Eq("QRFPrice_Id", request.QRFPriceID),
        //                                 Builders<mQRFPrice>.Update.Push<EmailDetails>("EmailDetails", request.EmailDetails));

        //                response.ResponseStatus.Status = resultFlag.MatchedCount > 0 ? "Success" : "Failure";
        //                response.ResponseStatus.ErrorMessage = resultFlag.MatchedCount > 0 ? "Saved Successfully." : "Details not updated.";
        //            }
        //            else
        //            {
        //                response.ResponseStatus.ErrorMessage = "QRFPrice ID not exists.";
        //                response.ResponseStatus.Status = "Error";
        //            }
        //        }
        //        else
        //        {
        //            response.ResponseStatus.ErrorMessage = "QRFID not exists.";
        //            response.ResponseStatus.Status = "Error";
        //        }
        //    }
        //    else
        //    {
        //        response.ResponseStatus.ErrorMessage = "Request details can not be blank/null.";
        //        response.ResponseStatus.Status = "Error";
        //    }

        //    return response;
        //}

        //public async Task<List<mQRFPackagePrice>> GetlstQrfPackagePrice(CostsheetGetReq request)
        //{
        //    string QRFPriceId = _MongoContext.mQRFPrice.AsQueryable().Where(a => a.QRFID == request.QRFId && a.IsCurrentVersion == true).OrderByDescending(b => b.VersionId).Select(b=>b.QRFPrice_Id).FirstOrDefault();
        //    List<mQRFPackagePrice> response = new List<mQRFPackagePrice>();
        //    response = _MongoContext.mQRFPackagePrice.AsQueryable().Where(a => a.QRFPrice_Id == QRFPriceId && a.Departure_Id == request.DepartureId).ToList();
        //    return response;            
        //}

        //public async Task<List<mQRFNonPackagedPrice>> GetlstQrfNonPackagePrice(CostsheetGetReq request)
        //{
        //    string QRFPriceId = _MongoContext.mQRFPrice.AsQueryable().Where(a => a.QRFID == request.QRFId && a.IsCurrentVersion == true).OrderByDescending(b => b.VersionId).Select(b => b.QRFPrice_Id).FirstOrDefault();
        //    List<mQRFNonPackagedPrice> response = new List<mQRFNonPackagedPrice>();
        //    response = _MongoContext.mQRFNonPackagedPrice.AsQueryable().Where(a => a.QRFPrice_Id == QRFPriceId && a.Departure_Id == request.DepartureId).ToList();
        //    return response;
        //}

        //public async Task<List<mQRFPositionTotalCost>> GetlstQrfPositionTotalCost(CostsheetGetReq request)
        //{
        //    string QRFPriceId = _MongoContext.mQRFPrice.AsQueryable().Where(a => a.QRFID == request.QRFId && a.IsCurrentVersion == true).OrderByDescending(b => b.VersionId).Select(b => b.QRFPrice_Id).FirstOrDefault();
        //    List<mQRFPositionTotalCost> response = new List<mQRFPositionTotalCost>();
        //    response = _MongoContext.mQRFPositionTotalCost.AsQueryable().Where(a => a.QRFPrice_Id == QRFPriceId && a.Departure_Id == request.DepartureId).ToList();
        //    return response;
        //}
    }
}