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
	public class DocumentStoreRepository : IDocumentStoreRepository
	{
		#region Private Variable Declaration
		private readonly MongoContext _MongoContext = null;
        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
        #endregion

        public DocumentStoreRepository(IOptions<MongoSettings> settings)
		{
			_MongoContext = new MongoContext(settings);
		}

		public async Task<DocumentStoreGetRes> GetDocumentStore(DocumentStoreGetReq request)
		{
			var res = new DocumentStoreGetRes() { DocumentStoreList = new List<mDocumentStore>(), ResponseStatus = new ResponseStatus() };
			try
			{
				if (request != null)
				{
					FilterDefinition<mDocumentStore> filter;
					filter = Builders<mDocumentStore>.Filter.Empty;

					if (!string.IsNullOrEmpty(request.DocumentId))
					{
						filter = filter & Builders<mDocumentStore>.Filter.Where(f => f.Document_Id == request.DocumentId);
					}
					if (!string.IsNullOrEmpty(request.DocumentType))
					{
						filter = filter & Builders<mDocumentStore>.Filter.Where(f => f.DocumentType.ToLower() == request.DocumentType.ToLower());
					}
					if (!string.IsNullOrEmpty(request.QRFID))
					{
						filter = filter & Builders<mDocumentStore>.Filter.Where(f => f.QRFID == request.QRFID);
					}
					if (!string.IsNullOrEmpty(request.BookingNumber))
					{
						filter = filter & Builders<mDocumentStore>.Filter.Where(f => f.BookingNumber == request.BookingNumber);
					}
					if (!string.IsNullOrEmpty(request.Position_Id))
					{
						filter = filter & Builders<mDocumentStore>.Filter.Where(f => f.PositionId == request.Position_Id);
					}
					if (!string.IsNullOrEmpty(request.Supplier_Id))
					{
						filter = filter & Builders<mDocumentStore>.Filter.Where(f => f.SupplierId == request.Supplier_Id);
					}
					if (!string.IsNullOrEmpty(request.Client_Id))
					{
						filter = filter & Builders<mDocumentStore>.Filter.Where(f => f.ClientId == request.Client_Id);
					}
					if (!string.IsNullOrEmpty(request.AlternateService_Id))
					{
						filter = filter & Builders<mDocumentStore>.Filter.Where(f => f.AlternateServiceId == request.AlternateService_Id);
					}
					if (!string.IsNullOrEmpty(request.SendStatus))
					{
						filter = filter & Builders<mDocumentStore>.Filter.Where(f => f.SendStatus.ToLower() == request.SendStatus.ToLower());
					}

					var mDocumentStoreList = await _MongoContext.mDocumentStore.Find(filter).ToListAsync();
					if (mDocumentStoreList != null && mDocumentStoreList.Count > 0)
					{
						res.DocumentStoreList = mDocumentStoreList;
						res.ResponseStatus.Status = "Success";
					}
					else
					{
						res.ResponseStatus.Status = "Failure";
						res.ResponseStatus.ErrorMessage = "Records not found.";
					}
				}
				else
				{
					res.ResponseStatus.Status = "Failure";
					res.ResponseStatus.ErrorMessage = "Request can not be null/blank.";
				}
			}
			catch (Exception ex)
			{
				res.ResponseStatus.Status = "Failure";
				res.ResponseStatus.ErrorMessage = !string.IsNullOrEmpty(ex.Message) ? ex.Message : (ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message)) ? ex.InnerException.Message : "";
			}
			return res;
		}

        public async Task<DocumentStoreInfoGetRes> GetCommunicationTrail(DocumentStoreGetReq request)
        {
            var res = new DocumentStoreInfoGetRes()
            {
                DocumentStoreList = new List<DocumentStoreList>(),
                DocumentStoreInfo = new DocumentStoreInfo(),
                ResponseStatus = new ResponseStatus(),
                BookingNumber = request.BookingNumber,
                QRFID = request.QRFID
            };
            try
            {
                if (request != null)
                {
                    if (!string.IsNullOrEmpty(request.QRFID))
                    {
                        var QRFPrice = _MongoContext.mQRFPrice.AsQueryable().Where(x => x.QRFID == request.QRFID && x.IsCurrentVersion == true && x.IsDeleted == false).OrderByDescending(y => y.VersionId).FirstOrDefault();
                        res.AgentTourName = QRFPrice.AgentProductInfo.TourName;
                    }
                    DocumentStoreGetRes objDocumentStoreGetRes = await GetDocumentStore(request);
                    if (objDocumentStoreGetRes?.ResponseStatus?.Status.ToLower() == "success" && objDocumentStoreGetRes?.DocumentStoreList?.Count > 0)
                    {
                        res.DocumentStoreList = objDocumentStoreGetRes.DocumentStoreList.Select(a => new DocumentStoreList
                        {
                            DocumentId = a.Document_Id,
                            From = a.From,
                            Subject = a.Subject,
                            SendDate = a.SendDate != null ? TimeZoneInfo.ConvertTimeFromUtc(a.SendDate.Value, INDIAN_ZONE) : DateTime.MinValue,
                            SendStatus = a.SendStatus,
                            To = a.To != null ? string.Join(',', a.To) : ""
                        }).ToList();

                        var firstemail = objDocumentStoreGetRes.DocumentStoreList.FirstOrDefault();
                        res.DocumentStoreInfo = new DocumentStoreInfo()
                        {
                            To = firstemail.To != null ? string.Join(',', firstemail.To) : "",
                            Body = firstemail.Body,
                            DocumentPath = firstemail.DocumentPath != null ? string.Join(',', firstemail.DocumentPath) : "",
                            From = firstemail.From,
                            SendDate = firstemail.SendDate != null ? TimeZoneInfo.ConvertTimeFromUtc(firstemail.SendDate.Value, INDIAN_ZONE) : DateTime.MinValue,
                            SendStatus = firstemail.SendStatus,
                            Subject = firstemail.Subject
                        };
                        res.ResponseStatus.Status = "Success";
                    }
                    else
                    {
                        if (objDocumentStoreGetRes?.ResponseStatus?.Status.ToLower() != "success")
                        {
                            res.ResponseStatus.Status = "Failure";
                            res.ResponseStatus.ErrorMessage = objDocumentStoreGetRes?.ResponseStatus?.ErrorMessage;
                        }
                        else
                        {
                            res.ResponseStatus.Status = "Failure";
                            res.ResponseStatus.ErrorMessage = "Communication Trail details not found.";
                        }
                    }
                }
                else
                {
                    res.ResponseStatus.Status = "Failure";
                    res.ResponseStatus.ErrorMessage = "Request can not be null/blank.";
                }
            }
            catch (Exception ex)
            {
                res.ResponseStatus.Status = "Failure";
                res.ResponseStatus.ErrorMessage = !string.IsNullOrEmpty(ex.Message) ? ex.Message : (ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message)) ? ex.InnerException.Message : "";
            }
            return res;
        }

        public async Task<DocumentStoreInfo> GetCommunicationTrailById(DocumentStoreGetReq request)
        {
            var res = new DocumentStoreInfo()
            {
                ResponseStatus = new ResponseStatus()
            };
            try
            {
                if (!string.IsNullOrEmpty(request.DocumentId))
                {
                    var mDocumentStore = await _MongoContext.mDocumentStore.Find(a => a.Document_Id == request.DocumentId).FirstOrDefaultAsync();
                    if (mDocumentStore != null)
                    {
                        res = new DocumentStoreInfo()
                        {
                            To = mDocumentStore.To != null ? string.Join(',', mDocumentStore.To) : "",
                            Body = mDocumentStore.Body,
                            DocumentPath = mDocumentStore.DocumentPath != null ? string.Join(',', mDocumentStore.DocumentPath) : "",
                            From = mDocumentStore.From,
                            SendDate = mDocumentStore.SendDate != null ? TimeZoneInfo.ConvertTimeFromUtc(mDocumentStore.SendDate.Value, INDIAN_ZONE)  : DateTime.MinValue,
                            SendStatus = mDocumentStore.SendStatus,
                            Subject = mDocumentStore.Subject,
                            ResponseStatus = new ResponseStatus()
                        };
                        res.ResponseStatus.Status = "Success";
                    }
                    else
                    {
                        res.ResponseStatus.Status = "Failure";
                        res.ResponseStatus.ErrorMessage = "Communication Trail details not found.";
                    }
                }
                else
                {
                    res.ResponseStatus.Status = "Failure";
                    res.ResponseStatus.ErrorMessage = "DocumentId can not be null/blank.";
                }
            }
            catch (Exception ex)
            {
                res.ResponseStatus.Status = "Failure";
                res.ResponseStatus.ErrorMessage = !string.IsNullOrEmpty(ex.Message) ? ex.Message : (ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message)) ? ex.InnerException.Message : "";
            }
            return res;
        }

		public async Task<DocumentStoreSetRes> SetDocumentStore(DocumentStoreSetReq request)
		{
			var res = new DocumentStoreSetRes() { ResponseStatus = new ResponseStatus() };
			try
			{
				if (request != null && request.mDocumentStore != null)
				{
					FilterDefinition<mDocumentStore> filter;
					filter = Builders<mDocumentStore>.Filter.Empty;

					int c = 0;
					if (!string.IsNullOrEmpty(request.mDocumentStore.BookingNumber) && !string.IsNullOrEmpty(request.mDocumentStore.PositionId))
					{
						FilterDefinition<mDocumentStore> filterCnt;
						filterCnt = Builders<mDocumentStore>.Filter.Empty;
						filterCnt = filterCnt & Builders<mDocumentStore>.Filter.Where(f => f.PositionId == request.mDocumentStore.PositionId && f.BookingNumber == request.mDocumentStore.BookingNumber);
						var mDocumentStore = await _MongoContext.mDocumentStore.Find(filterCnt).ToListAsync();
						if (string.IsNullOrEmpty(request.mDocumentStore.Document_Id))
						{
							c = mDocumentStore.Count + 1;
							var booking = _MongoContext.Bookings.AsQueryable().Where(a => a.BookingNumber == request.mDocumentStore.BookingNumber).FirstOrDefault();
							var pos = booking.Positions.Where(b => b.Position_Id == request.mDocumentStore.PositionId).FirstOrDefault();
							request.mDocumentStore.DocumentReference = request.mDocumentStore.BookingNumber.ToString() + "-" + pos.GRIDINFO.Split(' ')[0] + "-" + c.ToString();
						}
						else
						{
							var docres = mDocumentStore.Where(a => a.Document_Id == request.mDocumentStore.Document_Id).FirstOrDefault();
							if (docres != null)
							{
								request.mDocumentStore.DocumentReference = docres.DocumentReference;
							}
						}
					}

					if (!string.IsNullOrEmpty(request.mDocumentStore.Document_Id))
					{
						filter = filter & Builders<mDocumentStore>.Filter.Where(f => f.Document_Id == request.mDocumentStore.Document_Id);
						var mDocumentStore = await _MongoContext.mDocumentStore.Find(filter).FirstOrDefaultAsync();

						//Set System Company Id
						string SystemCompanyId = string.Empty;
						if (!string.IsNullOrWhiteSpace(request.mDocumentStore.BookingNumber))
						{
							SystemCompanyId = _MongoContext.Bookings.AsQueryable().Where(x => x.BookingNumber == request.mDocumentStore.BookingNumber).FirstOrDefault()?.SystemCompany_Id;
						}
						else if (!string.IsNullOrWhiteSpace(request.mDocumentStore.QRFID))
						{
                            SystemCompanyId = _MongoContext.mQRFPrice.AsQueryable().Where(x => x.QRFID == request.mDocumentStore.QRFID && x.IsCurrentVersion == true).OrderByDescending(y => y.VersionId).FirstOrDefault()?.SystemCompany_Id;
                        }

						request.mDocumentStore.SystemCompany_Id = SystemCompanyId;

						if (mDocumentStore != null)
						{
							request.mDocumentStore.Edit_Date = DateTime.Now;

                            var docstore = await _MongoContext.mDocumentStore.FindOneAndUpdateAsync(a => a.Document_Id == request.mDocumentStore.Document_Id,
                                             Builders<mDocumentStore>.Update.
                                             Set("QRFID", request.mDocumentStore.QRFID).
                                             Set("PositionId", request.mDocumentStore.PositionId).
                                             Set("BookingNumber", request.mDocumentStore.BookingNumber).
                                             Set("AlternateServiceId", request.mDocumentStore.AlternateServiceId).
                                             Set("SupplierId", request.mDocumentStore.SupplierId).
                                             Set("ClientId", request.mDocumentStore.ClientId).
                                             Set("QRFPriceId", request.mDocumentStore.QRFPriceId).
                                             Set("To", request.mDocumentStore.To).
                                             Set("From", request.mDocumentStore.From).
                                             Set("CC", request.mDocumentStore.CC).
                                             Set("BCC", request.mDocumentStore.BCC).
                                             Set("Subject", request.mDocumentStore.Subject).
                                             Set("Body", request.mDocumentStore.Body).
                                             Set("DocumentPath", request.mDocumentStore.DocumentPath).
                                             Set("DocumentType", request.mDocumentStore.DocumentType).
                                             Set("Importance", request.mDocumentStore.Importance).
                                             Set("SendDate", request.mDocumentStore.SendDate).
                                             Set("SendStatus", request.mDocumentStore.SendStatus).
                                             Set("ErrorMessage", request.mDocumentStore.ErrorMessage).
                                             Set("Edit_Date", request.mDocumentStore.Edit_Date).
                                             Set("Edit_User", request.mDocumentStore.Edit_User).
                                             Set("RetryDate", request.mDocumentStore.RetryDate).
                                             Set("Send_Via", request.mDocumentStore.Send_Via).
                                             Set("DocumentReference", request.mDocumentStore.DocumentReference).
                                             Set("DocumentSigned", request.mDocumentStore.DocumentSigned).
                                             Set("SupplierConfNum", request.mDocumentStore.SupplierConfNum).
                                             Set("DocumentSignInUser", request.mDocumentStore.DocumentSignInUser).
                                             Set("MailStatus", request.mDocumentStore.MailStatus).
											  Set("SystemCompany_Id", SystemCompanyId));

                            if (docstore != null)
                            {
                                res.ResponseStatus.Status = "Success";
                                res.ResponseStatus.ErrorMessage = "Document Store details upadted successfully.";
                            }
                            else
                            {
                                res.ResponseStatus.Status = "Failure";
                                res.ResponseStatus.ErrorMessage = "Document Store details not upadted.";
                            }
                        }
                        else
                        {
                            res.ResponseStatus.Status = "Failure";
                            res.ResponseStatus.ErrorMessage = "Document Id not exists in mDocumentStore collection.";
                        }
                    }
                    else
                    {
                        request.mDocumentStore.Edit_Date = null;
                        request.mDocumentStore.Edit_User = "";
                        request.mDocumentStore.Document_Id = string.IsNullOrEmpty(request.Document_Id) ? Guid.NewGuid().ToString() : request.Document_Id;
                        await _MongoContext.mDocumentStore.InsertOneAsync(request.mDocumentStore);

						res.ResponseStatus.Status = "Success";
						res.ResponseStatus.ErrorMessage = "Document Store details inserted successfully.";
					}
					res.DocumentId = request.mDocumentStore.Document_Id;
				}
				else
				{
					res.ResponseStatus.Status = "Failure";
					res.ResponseStatus.ErrorMessage = "Request can not be null/blank.";
				}
			}
			catch (Exception ex)
			{
				res.ResponseStatus.Status = "Failure";
				res.ResponseStatus.ErrorMessage = !string.IsNullOrEmpty(ex.Message) ? ex.Message : (ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message)) ? ex.InnerException.Message : "";
			}
			return res;
		}
	}
}
