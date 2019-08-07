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
	public class ProposalRepository : IProposalRepository
	{
		#region Private Variable Declaration
		private readonly MongoContext _MongoContext = null;
		#endregion

		public ProposalRepository(IOptions<MongoSettings> settings)
		{
			_MongoContext = new MongoContext(settings);
		}

		public ProposalGetRes GetProposal(ProposalGetReq request)
		{
			try
			{
				if (!string.IsNullOrEmpty(request.QRFID))
				{
					ProposalGetRes response = new ProposalGetRes();
					response.Proposal = _MongoContext.mProposal.AsQueryable().Where(a => a.QRFID == request.QRFID).OrderByDescending(x => x.Version).FirstOrDefault();
					if (!string.IsNullOrWhiteSpace(request.DocType) && request.DocType.ToUpper() == "QUOTATION-NEW")
						response.TermsAndConditions = _MongoContext.mTermsAndConditions.AsQueryable().Where(a => a.DocumentType.ToLower() == request.DocType.ToLower() && (a.Section == "" || a.Section == null || a.Section.ToLower() == request.Section.ToLower())).OrderBy(x => x.OrderNr).ToList();
					else
						response.TermsAndConditions = _MongoContext.mTermsAndConditions.AsQueryable().Where(a => a.Section == request.Section).OrderBy(x => x.OrderNr).ToList();

					if (request.Section == "Exclusions" && (response.Proposal == null || string.IsNullOrEmpty(response.Proposal.Exclusions)))
					{
						var res = _MongoContext.mQRFPosition.AsQueryable().Where(a => a.QRFID == request.QRFID && (a.ProductType.ToUpper() == "LDC" || a.ProductType.ToUpper() == "COACH")).ToList();
						if (res != null && res.Count > 0)
						{
							List<string> lstExclusions = new List<string>();
							foreach (var item in res)
							{
								if (item.IsCityPermit)
								{
									lstExclusions.Add("City Permit (" + item.ProductType + " starting in " + item.ProductName + " on " + item.StartingFrom + ": " + item.FromPickUpLoc + ")");
								}
								if (item.IsParkingCharges)
								{
									lstExclusions.Add("Parking Charges (" + item.ProductType + " starting in " + item.ProductName + " on " + item.StartingFrom + ": " + item.FromPickUpLoc + ")");
								}
								if (item.IsRoadTolls)
								{
									lstExclusions.Add("Road Tolls (" + item.ProductType + " starting in " + item.ProductName + " on " + item.StartingFrom + ": " + item.FromPickUpLoc + ")");
								}
							}
							response.TermsAndConditions.AddRange(lstExclusions.Select(a => new mTermsAndConditions { OrderNr = response.TermsAndConditions.Count + 1, Section = "Exclusions", Tcs = a }));
						}
					}

					return response;
				}
				else
				{
					return null;
				}

			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
			return null;
		}

		public async Task<ProposalSetRes> SetProposal(ProposalSetReq request)
		{
			ProposalSetRes response = new ProposalSetRes();
			try
			{
				mProposal proposal = new mProposal();
				if (!string.IsNullOrEmpty(request.Proposal.ProposalId))
				{
					proposal = _MongoContext.mProposal.AsQueryable().Where(x => x.ProposalId == request.Proposal.ProposalId).FirstOrDefault();
				}
				else if (!string.IsNullOrEmpty(request.Proposal.QRFID))
				{
					proposal = _MongoContext.mProposal.AsQueryable().Where(x => x.QRFID == request.Proposal.QRFID).FirstOrDefault();
				}

				if (request.IsNewVersion == true)
				{
					//Add
					if (!string.IsNullOrEmpty(request.Proposal.QRFID) && string.IsNullOrEmpty(request.Proposal.ItineraryId))
					{
						proposal._Id = ObjectId.Empty;
						proposal.QRFID = proposal.QRFID;
						proposal.ProposalId = Guid.NewGuid().ToString();
						proposal.ItineraryId = proposal.ItineraryId;
						proposal.Version = proposal.Version + 1;
						proposal.CreateUser = proposal.CreateUser;
						proposal.CreateDate = DateTime.Now;

						await _MongoContext.mProposal.InsertOneAsync(proposal);
						response.ResponseStatus.Status = "Success";
						response.ResponseStatus.ErrorMessage = "Saved Successfully.";
					}
					else
					{
						response.ResponseStatus.Status = "Error";
						response.ResponseStatus.ErrorMessage = "No records to insert.";
					}
				}
				else
				{
					//Update     
					proposal.PriceBreakup = request.Proposal.PriceBreakup ?? proposal.PriceBreakup;
					proposal.Inclusions = request.Proposal.Inclusions ?? proposal.Inclusions;
					proposal.Exclusions = request.Proposal.Exclusions ?? proposal.Exclusions;
					proposal.Terms = request.Proposal.Terms ?? proposal.Terms;
					proposal.CoveringNote = request.Proposal.CoveringNote ?? proposal.CoveringNote;
					proposal.Review = request.Proposal.Review ?? proposal.Review;
					proposal.ProposalIncludeRegions = request.Proposal.ProposalIncludeRegions ?? proposal.ProposalIncludeRegions;
					proposal.EditDate = DateTime.Now;
					proposal.EditUser = request.Proposal.EditUser;

					ReplaceOneResult replaceResult = await _MongoContext.mProposal.ReplaceOneAsync(Builders<mProposal>.Filter.Eq("ProposalId", proposal.ProposalId), proposal);
					response.ResponseStatus.Status = replaceResult.MatchedCount > 0 ? "Success" : "Failure";
					response.ResponseStatus.ErrorMessage = replaceResult.MatchedCount > 0 ? "Saved Successfully." : "Details not updated.";
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				response.ResponseStatus.Status = "Failure";
				response.ResponseStatus.ErrorMessage = ex.Message;
			}
			return response;
		}

		public ProposalDocumentGetRes GetProposalDocumentDetailsByQRFID(QuoteAgentGetReq request)
		{
			ProposalDocumentGetRes proposalDoc = new ProposalDocumentGetRes
			{
				QRFQuote = _MongoContext.mQRFPrice.AsQueryable().Where(q => q.QRFID == request.QRFID).FirstOrDefault(),
				Proposal = _MongoContext.mProposal.AsQueryable().Where(q => q.QRFID == request.QRFID && q.IsDeleted == false).FirstOrDefault(),
				Itinerary = _MongoContext.mItinerary.AsQueryable().Where(a => a.QRFID == request.QRFID).FirstOrDefault(),
				GenericImages = _MongoContext.mGenericImages.AsQueryable().Where(a => a.ImageType == "GENERIC_PRODUCT").ToList(),
			};
			proposalDoc.Itinerary.ItineraryDays.ForEach(a => a.ItineraryDescription = a.ItineraryDescription.Where(b => b.IsDeleted == false).ToList());

			if (proposalDoc.QRFQuote.RoutingInfo != null && proposalDoc.QRFQuote.RoutingInfo.Count > 0 && !string.IsNullOrEmpty(proposalDoc.QRFQuote.RoutingInfo[0].FromCityName))
			{
				var resort = _MongoContext.mResort.AsQueryable().Where(q => q.ResortType == "Country" && q.ResortName == proposalDoc.QRFQuote.RoutingInfo[0].FromCityName.Split(',', StringSplitOptions.None)[1].Trim()).ToList();
				proposalDoc.ProductImages = resort.Select(a => new Images { ImageIdentifier = "country", ImageURL = a.ImageURL }).ToList();
			}

			if (proposalDoc.Itinerary != null)
			{
				proposalDoc.Itinerary.ItineraryDays.ForEach(b => b.ItineraryDescription = b.ItineraryDescription.OrderBy(c => c.StartTime).ToList());

			}

			#region Commented do not delete (may be used later)
			//List<ItineraryDescriptionInfo> list = new List<ItineraryDescriptionInfo>();
			//foreach (var item in proposalDocument.Itinerary.ItineraryDays)
			//{
			//    list = new List<ItineraryDescriptionInfo>();
			//    list.AddRange(item.ItineraryDescription.Where(a => a.ProductType == "Domestic Flight").OrderBy(c => c.StartTime).ToList());
			//    list.AddRange(item.ItineraryDescription.Where(a => a.ProductType == "VISA").OrderBy(c => c.StartTime).ToList());
			//    list.AddRange(item.ItineraryDescription.Where(a => a.ProductType == "LDC").OrderBy(c => c.StartTime).ToList());
			//    list.AddRange(item.ItineraryDescription.Where(a => a.ProductType == "Coach").OrderBy(c => c.StartTime).ToList());
			//    list.AddRange(item.ItineraryDescription.Where(a => a.ProductType == "Private Transfer").OrderBy(c => c.StartTime).ToList());
			//    list.AddRange(item.ItineraryDescription.Where(a => a.ProductType == "Scheduled Transfer").OrderBy(c => c.StartTime).ToList());
			//    list.AddRange(item.ItineraryDescription.Where(a => a.ProductType == "Ferry Transfer").OrderBy(c => c.StartTime).ToList());
			//    list.AddRange(item.ItineraryDescription.Where(a => a.ProductType == "Ferry Passenger").OrderBy(c => c.StartTime).ToList());
			//    list.AddRange(item.ItineraryDescription.Where(a => a.ProductType == "Train").OrderBy(c => c.StartTime).ToList());
			//    list.AddRange(item.ItineraryDescription.Where(a => a.ProductType == "Hotel").OrderBy(c => c.StartTime).ToList());
			//    list.AddRange(item.ItineraryDescription.Where(a => a.ProductType == "Overnight Ferry").OrderBy(c => c.StartTime).ToList());
			//    list.AddRange(item.ItineraryDescription.Where(a => a.ProductType == "Attractions").OrderBy(c => c.StartTime).ToList());
			//    list.AddRange(item.ItineraryDescription.Where(a => a.ProductType == "Sightseeing - CityTour").OrderBy(c => c.StartTime).ToList());
			//    list.AddRange(item.ItineraryDescription.Where(a => a.ProductType == "Guide").OrderBy(c => c.StartTime).ToList());
			//    list.AddRange(item.ItineraryDescription.Where(a => a.ProductType == "Meal").OrderBy(c => c.StartTime).ToList());
			//    list.AddRange(item.ItineraryDescription.Where(a => !list.Select(b => b.ProductType).ToList().Contains(a.ProductType)).OrderBy(c => c.ProductType).ThenBy(c => c.StartTime).ToList());

			//    //proposalDocument.Itinerary.ItineraryDays.Find(a => a.ItineraryDescription == item.ItineraryDescription).ItineraryDescription = list;
			//    item.ItineraryDescription = list;
			//}
			#endregion

			return proposalDoc;
		}

		public ProposalDocumentGetRes GetProposalDocumentHeaderDetails(QuoteAgentGetReq request)
		{
			var System = _MongoContext.mSystem.AsQueryable().FirstOrDefault();
			var company = _MongoContext.mContacts.AsQueryable().Where(c => c.Company_Id == System.CoreCompany_Id && c.MAIL.ToLower() == "paochin.hu@coxandkings.ae")
							.Select(c => new CompanyDetailsRes { SystemEmail = c.MAIL, SystemPhone = c.TEL, SystemWebsite = c.WEB }).FirstOrDefault();

			ProposalDocumentGetRes proposalDocument = new ProposalDocumentGetRes
			{
				SystemEmail = company.SystemEmail ?? "",
				SystemPhone = company.SystemPhone ?? "",
				SystemWebsite = company.SystemWebsite ?? ""
			};

			return proposalDocument;
		}

		public ProposalGetRes GetHotelSummaryByQrfId(ProposalGetReq request)
		{
			ProposalGetRes response = new ProposalGetRes();
			try
			{
				var itineraryId = _MongoContext.mProposal.AsQueryable().Where(x => x.QRFID == request.QRFID).Select(x => x.ItineraryId).FirstOrDefault();
				if (!string.IsNullOrEmpty(itineraryId))
				{
					var itinerary = _MongoContext.mItinerary.AsQueryable().Where(x => x.ItineraryID == itineraryId).FirstOrDefault();
					response.Hotels = itinerary.ItineraryDays.OrderBy(x => x.Date).SelectMany(x => x.Hotel).Where(x => x.IsDeleted == false).ToList();
				}
				return response;
			}
			catch (Exception ex)
			{
				throw;
			}
		}
	}
}
