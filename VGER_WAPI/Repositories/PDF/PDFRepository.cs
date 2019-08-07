using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VGER_WAPI.Helpers;
using VGER_WAPI.Models;
using VGER_WAPI_CLASSES;

namespace VGER_WAPI.Repositories
{
	public class PDFRepository : IPDFRepository
	{
		#region Variable Declaration
		private readonly MongoContext _MongoContext = null;
		private readonly IConfiguration _configuration;
		private readonly IHostingEnvironment _env;
		private readonly IDocumentStoreRepository _documentStoreRepository;
		private readonly IEmailRepository _emailRepository;
		#endregion

		public PDFRepository(IOptions<MongoSettings> settings, IConfiguration configuration, IHostingEnvironment env, IDocumentStoreRepository documentStoreRepository, IEmailRepository emailRepository)
		{
			_MongoContext = new MongoContext(settings);
			_configuration = configuration;
			_env = env;
			_documentStoreRepository = documentStoreRepository;
			_emailRepository = emailRepository;
		}

		/// <summary>
		/// Create PDF Template and Send PDF through Email function
		/// </summary>
		/// <param name="request">Required params for template creation and send PDF</param>
		/// <returns>response</returns>
		public async Task<PDFGetRes> GeneratePDF(PDFGetReq request)
		{
			PDFGetRes response = new PDFGetRes() { ResponseStatusMessage = new ResponseStatusMessage() { ErrorMessage = new List<string>() }, PDFTemplateGetRes = new List<PDFTemplateGetRes>() };
			try
			{
				List<PDFTemplateGetRes> pdfContent = new List<PDFTemplateGetRes>();

				if (!string.IsNullOrEmpty(request.DocumentType))
				{
					string docType = request.DocumentType.ToUpper();
					string pathToFile = "";
					pathToFile = GetPath(docType);
					if (string.IsNullOrEmpty(pathToFile))
					{
						response.ResponseStatusMessage.Status = "Error";
						response.ResponseStatusMessage.ErrorMessage.Add("File path not found");
						return response;
					}

					request.DocumentPath = pathToFile;
					switch (docType)
					{
						case DocType.OPSVOUCHER: //OPS Hotel Booking Confirmation
							{
								pdfContent = await CreateOPSPositionVoucherDetails(request);
								break;
							}
						case DocType.OPSROOMING: //OPS Position Rooming List
							{
								pdfContent = await CreateOPSPositionRoomingList(request);
								break;
							}
						case DocType.OPSFULLITINERARY: //OPS Booking Full Itinerary
							{
								pdfContent = await CreateOPSFullItinerary(request);
								break;
							}
						default:
							return response;
					}

					if (request.IsSendEmail == true && pdfContent?.Count > 0)
					{
						try
						{
							for (int i = 0; i < pdfContent.Count; i++)
							{
								if (pdfContent[i].ResponseStatusMessage.Status?.ToLower() == "success")
								{
									var sendPDFRes = await SendPDF(pdfContent[i], request);
									if (sendPDFRes.ResponseStatusMessage.ErrorMessage?.Count > 0)
									{
										response.ResponseStatusMessage.Status = "Error";
										response.ResponseStatusMessage.ErrorMessage.AddRange(sendPDFRes.ResponseStatusMessage.ErrorMessage);
									}
									else
									{
										response.ResponseStatusMessage.Status = "Success";
									}
								}
								else
								{
									response.ResponseStatusMessage.Status = "Error";
									response.ResponseStatusMessage.ErrorMessage.AddRange(pdfContent[i].ResponseStatusMessage.ErrorMessage);
								}
								response.PDFTemplateGetRes.Add(pdfContent[i]);
							}
						}
						catch (Exception ex)
						{
							response.ResponseStatusMessage.Status = "Error";
							response.ResponseStatusMessage.ErrorMessage.Add(ex.Message);
						}
					}
					else if (request.IsSendEmail == false && pdfContent?.Count > 0)
					{
						response.PDFTemplateGetRes = pdfContent;
						var errorMsg = pdfContent.Where(a => a.ResponseStatusMessage.Status.ToLower() != "success").SelectMany(a => a.ResponseStatusMessage.ErrorMessage).ToList();
						if (errorMsg.Count() > 0)
						{
							response.ResponseStatusMessage.Status = "Error";
							response.ResponseStatusMessage.ErrorMessage.AddRange(pdfContent.SelectMany(a => a.ResponseStatusMessage.ErrorMessage));
						}
						else
						{
							response.ResponseStatusMessage.Status = "Success";
						}
					}
					else
					{
						response.ResponseStatusMessage.Status = "Error";
						response.ResponseStatusMessage.ErrorMessage.Add("PDF content not found");
					}
				}
				else
				{
					response.ResponseStatusMessage.Status = "Error";
					response.ResponseStatusMessage.ErrorMessage.Add("Document Type not can not be Null/Empty.");
				}
			}
			catch (Exception ex)
			{
				response.ResponseStatusMessage.Status = "Error";
				response.ResponseStatusMessage.ErrorMessage.Add(ex.Message);
			}
			return response;
		}

		#region PDF Template Common Methods 
		/// <summary>
		/// To get template path from config file
		/// </summary>
		/// <param name="documentType"></param>
		/// <returns></returns>
		public string GetPath(string documentType)
		{
			string templatePath, pathToFile;
			try
			{
				string filePath = _env.ContentRootPath;
				string doctype = documentType.ToUpper();

				switch (doctype)
				{
					case DocType.OPSVOUCHER:
						{
							templatePath = _configuration.GetValue<string>("PDFTemplates:OPSVOUCHER");
							break;
						}
					case DocType.OPSROOMING:
						{
							templatePath = _configuration.GetValue<string>("PDFTemplates:OPSROOMINGPDF");
							break;
						}
					case DocType.OPSFULLITINERARY:
						{
							templatePath = _configuration.GetValue<string>("PDFTemplates:OPSFULLITINERARY");
							break;
						}
					default:
						return string.Empty;
				}

				pathToFile = filePath + templatePath;

			}
			catch (Exception ex)
			{
				pathToFile = ex.Message;
			}
			return pathToFile;
		}

		public PDFGenerateGetRes GenerateAndSavePDF(string fileName, PdfDocument pdfDocument)
		{
			PDFGenerateGetRes response = new PDFGenerateGetRes();

			string PDFPath = _configuration.GetValue<string>("SystemSettings:ProposalDocumentFilePath");
			string FullPDFPath = Path.Combine(PDFPath, fileName);
			if (!Directory.Exists(PDFPath)) Directory.CreateDirectory(PDFPath);

			response.FullPDFPath = FullPDFPath;
			response.PDFPath = PDFPath;
			PdfConvert._configuration = _configuration;
			response.ResponseStatusMessage = PdfConvert.GenerateDocument(pdfDocument, new PdfOutput() { OutputFilePath = FullPDFPath });
			return response;
		}

		/// <summary>
		/// Send pdf through Mail function
		/// </summary>
		/// <param name="pdfContent">pdf details generated for pdf template creation</param>
		/// /// <param name="pDFGetReq">Req param of PDF</param>
		/// <returns>Response Status i.e. pdf sent or Not sent</returns>
		public async Task<PDFGetRes> SendPDF(PDFTemplateGetRes pdfContent, PDFGetReq pDFGetReq)
		{
			PDFGetRes response = new PDFGetRes() { PDFTemplateGetRes = new List<PDFTemplateGetRes>(), ResponseStatusMessage = new ResponseStatusMessage() { ErrorMessage = new List<string>() } };

			try
			{
				EmailGetRes emailGetRes = await _emailRepository.SendEmail(new EmailTemplateGetRes()
				{
					UserEmail = pDFGetReq.UserEmail,
					AlternateServiceId = pdfContent.AlternateServiceId,
					Attachment = new List<string>() { pdfContent.DocumentDetails.FullDocumentPath },
					BCC = pdfContent.BCC,
					Body = pdfContent.Body,
					CC = pdfContent.CC,
					Client = pdfContent.Client,
					DocumentPath = new List<string>() { pdfContent.DocumentDetails.DocumentPath },
					DocumentReference = pdfContent.DocumentDetails.DocumentReference,
					Document_Id = pdfContent.Document_Id,
					From = pdfContent.From,
					Importance = pdfContent.Importance,
					SendVia = pdfContent.SendVia,
					Subject = pdfContent.Subject,
					ResponseStatusMessage = pdfContent.ResponseStatusMessage,
					SupplierId = pdfContent.SupplierId,
					To = pdfContent.To,
					PathType = pdfContent.PathType,
					EmailGetReq = new EmailGetReq()
					{
						IsSaveDocStore = true,
						AlternateServiceId = pdfContent.AlternateServiceId,
						BookingNo = pDFGetReq.BookingNo,
						DocumentType = pDFGetReq.DocumentType,
						PositionId = pdfContent.PositionId,
						SupplierId = pdfContent.SupplierId,
						QrfId = pDFGetReq.QRFID,
						QRFPriceId = pdfContent.QRFPriceId,
						MailStatus = pDFGetReq.MailStatus,
						SystemCompany_Id = pDFGetReq.SystemCompany_Id,
						PlacerUserId = pDFGetReq.UserId,
						Importance = pdfContent.Importance
					}
				});

				response.ResponseStatusMessage.Status = emailGetRes.ResponseStatus.Status;
				if (!string.IsNullOrWhiteSpace(emailGetRes.ResponseStatus.ErrorMessage))
				{
					response.ResponseStatusMessage.ErrorMessage.AddRange(emailGetRes.ResponseStatus.ErrorMessage.Trim().TrimEnd('|').Split('|'));
				}
			}
			catch (Exception ex)
			{
				response.ResponseStatusMessage.ErrorMessage.Add(ex.Message);
				response.ResponseStatusMessage.Status = "Error";
			}
			return response;
		}
		#endregion

		#region PDF Template Methods
		#region OPS 
		public async Task<List<PDFTemplateGetRes>> CreateOPSPositionVoucherDetails(PDFGetReq request)
		{
			List<PDFTemplateGetRes> lstResponse = new List<PDFTemplateGetRes>() { };
			PDFTemplateGetRes response = new PDFTemplateGetRes() { ResponseStatusMessage = new ResponseStatusMessage() { ErrorMessage = new List<string>() }, DocumentDetails = new DocumentDetails() };

			try
			{
				if (!string.IsNullOrWhiteSpace(request.Module))
				{
					request.Module = request.Module.ToLower();
					if (request.PositionIds?.Count > 0)
					{
						var booking = await _MongoContext.Bookings.FindAsync(x => x.BookingNumber == request.BookingNo).Result.FirstOrDefaultAsync();
						if (booking != null)
						{
							var positions = booking.Positions.Where(a => request.PositionIds.Contains(a.Position_Id)).ToList();
							if (positions.Count > 0)
							{
								var PositionIds = positions.Select(a => a.Position_Id).ToList();
								var HotelPLacerIDs = positions.Select(a => a.HotelPLacer_ID).ToList();

								List<string> companyidList = new List<string>() { booking.SystemCompany_Id };
								var posSupplierIds = positions.Select(a => a.SupplierInfo.Id).ToList();
								companyidList.AddRange(posSupplierIds);
								var companies = _MongoContext.mCompanies.AsQueryable().Where(x => companyidList.Contains(x.Company_Id)).ToList();

								if (companies?.Count > 0)
								{
									var sysCompany = companies.Where(a => a.Company_Id == booking.SystemCompany_Id).FirstOrDefault();
									var posSuppliers = companies.Where(a => posSupplierIds.Contains(a.Company_Id)).ToList();

									if (sysCompany != null && posSuppliers?.Count > 0)
									{
										var contacts = _MongoContext.mContacts.AsQueryable().Where(x => x.VoyagerContact_Id == booking.StaffDetails.Staff_OpsUser_Id).FirstOrDefault();

										if (contacts != null)
										{
											string fileName = "";
											string pdfFileName = "";
											bool IshotelPlacer = true;
											string URLinitial = _configuration.GetValue<string>("SystemSettings:URLinitial");
											string ProposalDocumentFilePath = _configuration.GetValue<string>("SystemSettings:ProposalDocumentFilePath");
											string FileHandlerName = "";
											string FileHandlerContactNo = "";
											string FileHandlerEmail = "";
											string filepath = "";
											string maxDocumentStore = "";
											string documentNo = "";
											string VoucherDate = DateTime.Now.ToString("dd/MM/yyyy");
											var builder = new StringBuilder();
											var posSupplier = new mCompanies();
											var compcontacts = new List<CompanyContacts>();
											var hotelplacer = new CompanyContacts();
											List<ServiceDetailsOption> lstServiceDetailsOption = new List<ServiceDetailsOption>();
											string blankRow = "<table width='950' border='0' align='center' cellpadding='0' cellspacing='0' class='tbl-center'><tr><td colspan='2'>&nbsp;</td></tr></table>";

											var posCountryids = positions.Select(a => a.Country_Id).ToList();
											var posProductIds = positions.Select(a => a.Product_Id).ToList();
											var companyids = new List<string>() { booking.SystemCompany_Id, booking.AgentInfo.Id };
											companyids.AddRange(posSupplierIds);

											var emergencyContacts = _MongoContext.mEmergencyContacts.AsQueryable().Where(x => posCountryids.Contains(x.Country_Id) && x.Company_Id == booking.SystemCompany_Id).ToList();
											var termsAndConditions = _MongoContext.mTermsAndConditions.AsQueryable().Where(x => x.DocumentType.ToLower() == "voucher").ToList();

											var docStoreList = _MongoContext.mDocumentStore.AsQueryable().Where(x => x.BookingNumber == booking.BookingNumber && PositionIds.Contains(x.PositionId)).ToList();
											var contactList = _MongoContext.mCompanies.AsQueryable().Where(x => x.ContactDetails != null && x.ContactDetails.Count > 0 && x.ContactDetails.Any(a => HotelPLacerIDs.Contains(a.Contact_Id)));
											var mSystem = _MongoContext.mSystem.AsQueryable().Where(x => x.CoreCompany_Id == booking.SystemCompany_Id).FirstOrDefault();

											foreach (var item in positions)
											{
												response = new PDFTemplateGetRes() { ResponseStatusMessage = new ResponseStatusMessage() { ErrorMessage = new List<string>() }, DocumentDetails = new DocumentDetails() };
												FileHandlerName = "";
												FileHandlerContactNo = "";
												FileHandlerEmail = "";
												filepath = "";

												lstServiceDetailsOption = new List<ServiceDetailsOption>();
												var serviceBuilder = new StringBuilder();

												docStoreList = docStoreList.Where(a => a.PositionId == item.Position_Id).ToList();
												maxDocumentStore = docStoreList?.Count > 0 ? docStoreList.Count.ToString() : "0";
												int newdocNo = Convert.ToInt32(maxDocumentStore) + 1;
												documentNo = newdocNo > 9 ? newdocNo.ToString() : "0" + newdocNo.ToString();
												fileName = booking.BookingNumber + "-" + item.OrderNr + "-" + documentNo;
												pdfFileName = fileName + ".pdf";
												filepath = Path.Combine(ProposalDocumentFilePath, pdfFileName);

												bool isFileExists = File.Exists(filepath);
												isFileExists = request.Module == "position" ? false : (request.Module == "booking" && isFileExists) ? true : false;

												if (!isFileExists)
												{
													posSupplier = posSuppliers.Where(a => a.Company_Id == item.SupplierInfo.Id).FirstOrDefault();
													builder = new StringBuilder();
													using (StreamReader SourceReader = File.OpenText(request.DocumentPath))
													{
														builder.Append(SourceReader.ReadToEnd());
													}

													if (!string.IsNullOrWhiteSpace(item.HotelPLacer_ID))
													{
														compcontacts = contactList.Where(a => a.HeadOffice_Id == item.HotelPLacer_ID).FirstOrDefault()?.ContactDetails;
														hotelplacer = compcontacts?.Where(a => a.Contact_Id == item.HotelPLacer_ID).FirstOrDefault();
														if (!string.IsNullOrWhiteSpace(hotelplacer?.MAIL))
														{
															FileHandlerName = hotelplacer.FIRSTNAME + hotelplacer.LastNAME;
															FileHandlerContactNo = hotelplacer.TEL;
															FileHandlerEmail = hotelplacer.MAIL;
														}
														else
															IshotelPlacer = false;
													}
													else
														IshotelPlacer = false;

													if (IshotelPlacer == false)
													{
														FileHandlerName = booking.StaffDetails.Staff_OpsUser_Name;
														FileHandlerContactNo = contacts.TEL;
														FileHandlerEmail = booking.StaffDetails.Staff_OpsUser_Email;
													}

													var builderPassenger = new StringBuilder();
													booking.BookingPax = booking.BookingPax.Where(a => !string.IsNullOrWhiteSpace(a.PERSTYPE) && a.PERSTYPE.ToLower() == "adult").ToList();
													foreach (var itemBookingPax in booking.BookingPax)
													{
														builderPassenger.Append(itemBookingPax.PERSTYPE + (itemBookingPax.PERSONS > 0 ? " X " + itemBookingPax.PERSONS.ToString() : ""));
														builderPassenger.Append(",");
													}

                                                    builder.Replace("{{URLinitial}}", URLinitial);
                                                    builder.Replace("{{SYS_COMPANY_NAME}}", sysCompany.Name);
                                                    builder.Replace("{{SYS_COMPANY_ADDR1}}", sysCompany.Street);
                                                    builder.Replace("{{SYS_COMPANY_ADDR2}}", (!string.IsNullOrWhiteSpace(sysCompany.Street2) ? sysCompany.Street2 : "") +" "+ (!string.IsNullOrWhiteSpace(sysCompany.Street3) ? sysCompany.Street3 : ""));
                                                    builder.Replace("{{SYS_COMPANY_CITY}}", sysCompany.CityName);
                                                    builder.Replace("{{SYS_COMPANY_COUNTRY}}", sysCompany.CountryName);
                                                    builder.Replace("{{SYS_COMPANY_POSTALCODE}}", sysCompany.Zipcode);
                                                    builder.Replace("{{FileHandler_CONTACT_NAME}}", FileHandlerName);
                                                    builder.Replace("{{FileHandler_CONTACT_TEL_No}}", FileHandlerContactNo);

                                                    builder.Replace("{{FileHandler_CONTACT_EMAIL}}", FileHandlerEmail);
                                                    builder.Replace("{{FileHandler_DATE}}", VoucherDate);
                                                    builder.Replace("{{SUPPLIER_NAME}}", item.SupplierInfo.Name);
                                                    builder.Replace("{{Supplier_Address1}}", posSupplier.Street);
                                                    builder.Replace("{{Supplier_Address2}}", (!string.IsNullOrWhiteSpace(posSupplier.Street2) ? posSupplier.Street2 : "") +" "+ (!string.IsNullOrWhiteSpace(posSupplier.Street3) ? posSupplier.Street3 : ""));
                                                    builder.Replace("{{Supplier_City}}", posSupplier.CityName);
                                                    builder.Replace("{{Supplier_PostalCode}}", posSupplier.Zipcode);
                                                    builder.Replace("{{SUPPLIER_COUNTRY}}", posSupplier.CountryName);

													builder.Replace("{{POS_Supplier_Confirmation}}", item.Supplier_Confirmation);
													builder.Replace("{{POS_SUPPLIER_CONTACT_NAME}}", item.SupplierInfo.Contact_Name);
													builder.Replace("{{POS_SUPPLIER_TELEPHONE}}", item.SupplierInfo.Contact_Tel);
													builder.Replace("{{POS_SUPPLIER_EMAIL}}", item.SupplierInfo.Contact_Email);
													builder.Replace("{{PRODUCT_NAME}}", item.Product_Name);
													builder.Replace("{{BOOKING_REF_NO}}", booking.BookingNumber);
													builder.Replace("{{POS_ORDERNO}}", item.OrderNr);
													builder.Replace("{{POS_PASSENGER_BREAKDOWN}}", Convert.ToString(builderPassenger).TrimEnd(','));

													builder.Replace("{{Nationality}}", booking.GuestDetails.Nationality_Name);
													builder.Replace("{{POS_START_DATE}}", (item.STARTDATE != null ? item.STARTDATE.Value.ToString("dd/MM/yyyy") : ""));
													builder.Replace("{{POS_END_Date}}", (item.ENDDATE != null ? item.ENDDATE.Value.ToString("dd/MM/yyyy") : ""));
													builder.Replace("{{POS_START_TIME}}", item.STARTTIME);
													builder.Replace("{{POS_END_TIME}}", item.ENDTIME);
													builder.Replace("{{POS_START_LOC}}", item.STARTLOC);
													builder.Replace("{{POS_END_LOC}}", item.ENDLOC);

													builder.Replace("{{VOUCHER_NOTE}}", item.VoucherNote);

													var emergencyContactsBuilder = new StringBuilder();
													emergencyContactsBuilder.Append(blankRow);
													emergencyContactsBuilder.Append("<table width='950' border='0' align='center' cellpadding='0' cellspacing='0' class='tbl-center'>");
													emergencyContactsBuilder.Append("<tr><td valign='top'><h2>Emergency Contact Details</h2></td></tr><tr><td colspan='2'>&nbsp;</td></tr><tr><td valign='top'>");
													emergencyContactsBuilder.Append("<p>In case of emergency out of office hours, please use the below number(s)</p></td></tr><tr><td colspan='2'>&nbsp;</td></tr></table>");

													emergencyContactsBuilder.Append("<table width='700' border='0' align ='center' cellpadding ='0' cellspacing ='0' class='plain-table'>");
													emergencyContactsBuilder.Append("<thead><tr class='bck-col'><td align='center' valign='middle' height='40'><b>Contact Name</b></td><td align='center' valign='middle' height='40'><b>Contact Number</b></td></tr></thead>");
													emergencyContactsBuilder.Append("<tbody>");

													if (emergencyContacts?.Count > 0)
													{
														foreach (var itemEmergencyContacts in emergencyContacts)
														{
															emergencyContactsBuilder.Append("<tr>");
															emergencyContactsBuilder.Append("<td class='bck-white padl10'>" + itemEmergencyContacts.ContactName + "</td>");
															emergencyContactsBuilder.Append("<td class='bck-white padl10'>" + itemEmergencyContacts.EmergencyNo + "</td>");
															emergencyContactsBuilder.Append("</tr>");
														}
													}
													else
													{
														emergencyContactsBuilder.Append("<tr><td class='bck-white padl15'>&nbsp;</td><td class='bck-white padl15'>" + mSystem.EmergencyPhoneGroups + "</td></tr>");
													}
													emergencyContactsBuilder.Append("</tbody>");
													emergencyContactsBuilder.Append("</table>");
													builder.Replace("{{emergencydetails}}", emergencyContactsBuilder.ToString());

													var termsandcond = termsAndConditions.Where(a => !string.IsNullOrWhiteSpace(a.Tcs)
																		&& (companyidList.Contains(a.CompanyId) || (a.ProductId == item.Product_Id))).Select(a => a.Tcs).ToList();

													if (termsandcond.Count > 0)
													{
														var termsBuilder = new StringBuilder();
														termsBuilder.Append(blankRow);
														termsBuilder.Append("<table width='950' border='0' align ='center' cellpadding ='0' cellspacing ='0' class='tbl-center'>");
														termsBuilder.Append("<thead><tr><td><h2>Terms and Conditions</h2></td></tr><tr><td>&nbsp;</td></tr></thead>");
														termsBuilder.Append("<tbody><tr><td valign='top'>");
														termsBuilder.Append("<ul>");

														foreach (var itemTermsandcond in termsandcond)
														{
															termsBuilder.Append("<li>" + itemTermsandcond + "</li>");
														}
														termsBuilder.Append("</ul>");
														termsBuilder.Append("</td></tr></tbody></table>");
														builder.Replace("{{POS_TERMS_AND_CONDITIONS}}", termsBuilder.ToString());
													}
													else
													{
														builder.Replace("{{POS_TERMS_AND_CONDITIONS}}", "");
													}
													item.BookingRoomsAndPrices = item.BookingRoomsAndPrices.OrderBy(a => a.RoomName.ToLower() == "single" ? "A" : a.RoomName.ToLower() == "double" ? "B" :
																					a.RoomName.ToLower() == "twin" ? "C" : a.RoomName.ToLower() == "triple" ? "D" : a.RoomName.ToLower() == "quad" ? "E" :
																					a.RoomName.ToLower() == "tsu" ? "F" : "G").ToList();

													foreach (var itemBookingRoomsAndPrices in item.BookingRoomsAndPrices)
													{
														serviceBuilder.Append("<tr>");
														serviceBuilder.Append("<td class='bck-white padl10'>" + itemBookingRoomsAndPrices.CategoryName + "</td>");
														serviceBuilder.Append("<td class='bck-white padl10'>" + itemBookingRoomsAndPrices.RoomName + "</td>");
														serviceBuilder.Append("<td class='bck-white padl10'>" + Convert.ToString(itemBookingRoomsAndPrices.Req_Count) + "</td>");
														serviceBuilder.Append("<td class='bck-white padl10'>" + (item.ProductType.ToLower().Trim() == "hotel" ? item.HOTELMEALPLAN : item.Menu) + "</td>");
														serviceBuilder.Append("</tr>");
													}
													builder.Replace("{{servicedetails}}", serviceBuilder.ToString());

													var optionBuilder = new StringBuilder();
													if (item.ProductType.ToLower().Trim() == "hotel")
													{
														lstServiceDetailsOption.Add(new ServiceDetailsOption() { OptionName = "Breakfast Type", OptionValue = item.BreakFastType });
														lstServiceDetailsOption.Add(new ServiceDetailsOption() { OptionName = "Board Basis", OptionValue = item.HOTELMEALPLAN });
													}
													else if (item.ProductType.ToLower().Trim() == "meal")
													{
														lstServiceDetailsOption.Add(new ServiceDetailsOption() { OptionName = "Includes Water", OptionValue = item.Water ? "Yes" : "No" });
														lstServiceDetailsOption.Add(new ServiceDetailsOption() { OptionName = "Includes Bread", OptionValue = item.Bread ? "Yes" : "No" });
														lstServiceDetailsOption.Add(new ServiceDetailsOption() { OptionName = "Includes Dessert", OptionValue = item.Dessert ? "Yes" : "No" });
														lstServiceDetailsOption.Add(new ServiceDetailsOption() { OptionName = "Includes Tea / Coffee", OptionValue = item.Tea ? "Yes" : "No" });
													}
													else if (item.ProductType.ToLower().Trim() == "attractions")
													{
														lstServiceDetailsOption.Add(new ServiceDetailsOption() { OptionName = "Ticket Pickup", OptionValue = item.TicketLocation });
														lstServiceDetailsOption.Add(new ServiceDetailsOption() { OptionName = "Guide Purchase", OptionValue = item.GuidePurchaseTicket ? "Yes" : "No" });
													}
													else if (item.ProductType.ToLower().Trim() == "coach" || item.ProductType.ToLower().Trim() == "ldc")
													{
														lstServiceDetailsOption.Add(new ServiceDetailsOption() { OptionName = "Includes Parking", OptionValue = item.Parking ? "Yes" : "No" });
														lstServiceDetailsOption.Add(new ServiceDetailsOption() { OptionName = "Includes Road Tolls", OptionValue = item.RoadTolls ? "Yes" : "No" });
														lstServiceDetailsOption.Add(new ServiceDetailsOption() { OptionName = "Includes City permits", OptionValue = item.CityPermits ? "Yes" : "No" });
														lstServiceDetailsOption.Add(new ServiceDetailsOption() { OptionName = "Driver Name", OptionValue = item.DriverName });
														lstServiceDetailsOption.Add(new ServiceDetailsOption() { OptionName = "Driver Contact Number", OptionValue = item.DriverContactNumber });
														lstServiceDetailsOption.Add(new ServiceDetailsOption() { OptionName = "Vehicle Registration Number", OptionValue = item.VehicleRegistration });
													}
													if (lstServiceDetailsOption.Count > 0)
													{
														foreach (var itemServiceDetailsOption in lstServiceDetailsOption)
														{
															optionBuilder.Append("<tr>");
															optionBuilder.Append("<td class='bck-white padl10'>" + itemServiceDetailsOption.OptionName + "</td>");
															optionBuilder.Append("<td class='bck-white padl10'>" + itemServiceDetailsOption.OptionValue + "</td>");
															optionBuilder.Append("</tr>");
														}
													}
													else
													{
														optionBuilder.Append("<tr>");
														optionBuilder.Append("<td colspan='2' class='bck-white padl10'>-</td>");
														optionBuilder.Append("</tr>");
													}
													builder.Replace("{{optiondetails}}", optionBuilder.ToString());

													response.DocumentDetails.Html = builder.ToString();

													PDFGenerateGetRes res = GenerateAndSavePDF(pdfFileName, new PdfDocument() { Html = response.DocumentDetails.Html });
													response.ResponseStatusMessage = res.ResponseStatusMessage;
													response.DocumentDetails.DocumentName = pdfFileName;
													response.DocumentDetails.DocumentPath = filepath;
													response.DocumentDetails.FullDocumentPath = filepath;
													lstResponse.Add(response);
												}
												else
												{
													response.ResponseStatusMessage.Status = "Success";
													response.DocumentDetails.DocumentName = pdfFileName;
													response.DocumentDetails.DocumentPath = filepath;
													response.DocumentDetails.FullDocumentPath = filepath;
													lstResponse.Add(response);
												}
											}

											if (request.Module == "booking")
											{
												ResponseStatus resCreateZipFile = CommonFunction.CreateZipFile(new ZipDetails()
												{
													DocumentDetails = lstResponse.Select(a => a.DocumentDetails).ToList(),
													ZipFileName = booking.BookingNumber + "_Voucher.zip",
													ZipFilePath = ProposalDocumentFilePath
												});

												lstResponse = new List<PDFTemplateGetRes>();
												response.ResponseStatusMessage.Status = resCreateZipFile.Status;
												if (resCreateZipFile.ErrorMessage?.Count() > 0)
												{
													response.ResponseStatusMessage.ErrorMessage.Add(resCreateZipFile.ErrorMessage);
												}
												response.DocumentDetails.DocumentName = booking.BookingNumber + "_Voucher.zip";
												response.DocumentDetails.DocumentPath = ProposalDocumentFilePath;
												response.DocumentDetails.FullDocumentPath = ProposalDocumentFilePath;
												lstResponse.Add(response);
											}
										}
										else
										{
											response.ResponseStatusMessage.Status = "Failure";
											response.ResponseStatusMessage.ErrorMessage.Add("Contact details not found in monogodb for Staff_OpsUser_Id:-" + booking.StaffDetails.Staff_OpsUser_Id);
											lstResponse.Add(response);
										}
									}
									else
									{
										response.ResponseStatusMessage.Status = "Failure";
										response.ResponseStatusMessage.ErrorMessage.Add("SystemCompany_Id/Position SupplierId not found in monogodb.");
										lstResponse.Add(response);
									}
								}
								else
								{
									response.ResponseStatusMessage.Status = "Failure";
									response.ResponseStatusMessage.ErrorMessage.Add("SystemCompany_Id/Position SupplierId not found in monogodb.");
									lstResponse.Add(response);
								}
							}
							else
							{
								response.ResponseStatusMessage.Status = "Failure";
								response.ResponseStatusMessage.ErrorMessage.Add("Position details not found in monogodb.");
								lstResponse.Add(response);
							}
						}
						else
						{
							response.ResponseStatusMessage.Status = "Failure";
							response.ResponseStatusMessage.ErrorMessage.Add("Booking Number " + request.BookingNo + " not found in monogodb.");
							lstResponse.Add(response);
						}
					}
					else
					{
						response.ResponseStatusMessage.Status = "Failure";
						response.ResponseStatusMessage.ErrorMessage.Add("Position Id can not be null.");
						lstResponse.Add(response);
					}
				}
				else
				{
					response.ResponseStatusMessage.Status = "Failure";
					response.ResponseStatusMessage.ErrorMessage.Add("Module Name can not be null/empty.");
					lstResponse.Add(response);
				}
			}
			catch (Exception ex)
			{
				response.ResponseStatusMessage.Status = "Failure";
				response.ResponseStatusMessage.ErrorMessage.Add(ex.Message);
				lstResponse.Add(response);
			}

			return lstResponse;
		}

		public async Task<List<PDFTemplateGetRes>> CreateOPSPositionRoomingList(PDFGetReq request)
		{
			List<PDFTemplateGetRes> lstResponse = new List<PDFTemplateGetRes>();
			PDFTemplateGetRes response = new PDFTemplateGetRes() { ResponseStatusMessage = new ResponseStatusMessage() { ErrorMessage = new List<string>() }, DocumentDetails = new DocumentDetails() };

			try
			{
				string mailPath = _emailRepository.GetPath(request.DocumentType);
				if (!string.IsNullOrWhiteSpace(mailPath))
				{
					request.MailPath = mailPath;
					var booking = await _MongoContext.Bookings.FindAsync(x => x.BookingNumber == request.BookingNo).Result.FirstOrDefaultAsync();
					if (booking != null)
					{
						var positions = booking.Positions.Where(a => request.PositionIds.Contains(a.Position_Id)).ToList();
						if (positions.Count > 0)
						{
							var PositionIds = positions.Select(a => a.Position_Id).ToList();
							var HotelPLacerIDs = positions.Select(a => a.HotelPLacer_ID).ToList();
							var users = _MongoContext.mUsers.AsQueryable().Where(a => a.VoyagerUser_Id == request.UserId).FirstOrDefault();

							List<string> companyidList = new List<string>() { booking.SystemCompany_Id, users.Company_Id };
							var companies = _MongoContext.mCompanies.AsQueryable().Where(x => companyidList.Contains(x.Company_Id)).ToList();

							if (companies?.Count > 0)
							{
								var sysCompany = companies.Where(a => a.Company_Id == booking.SystemCompany_Id).FirstOrDefault();
								if (sysCompany != null)
								{
									var contacts = _MongoContext.mContacts.AsQueryable().Where(x => x.VoyagerContact_Id == booking.StaffDetails.Staff_OpsUser_Id).FirstOrDefault();
									if (contacts != null)
									{
										string blankRow = "<table width='950' border='0' align='center' cellpadding='0' cellspacing='0' class='tbl-center'><tr><td colspan='2'>&nbsp;</td></tr></table>";
										string fileName = "";
										string pdfFileName = "";
										bool IshotelPlacer = true;
										string URLinitial = _configuration.GetValue<string>("SystemSettings:URLinitial");
										string ProposalDocumentFilePath = _configuration.GetValue<string>("SystemSettings:ProposalDocumentFilePath");
										string FileHandlerName = "";
										string FileHandlerContactNo = "";
										string FileHandlerEmail = "";
										string FileHandlerDate = DateTime.Now.ToString("dd/MM/yyyy");
										string filepath = "";
										string maxDocumentStore = "";
										string documentNo = "";
										var builder = new StringBuilder();
										var compcontacts = new List<CompanyContacts>();
										var hotelplacer = new CompanyContacts();
										string posSTDT = "";
										string posEDDT = "";

										var contactList = _MongoContext.mCompanies.AsQueryable().Where(x => x.ContactDetails != null && x.ContactDetails.Count > 0 && x.ContactDetails.Any(a => HotelPLacerIDs.Contains(a.Contact_Id)));
										var docStoreList = _MongoContext.mDocumentStore.AsQueryable().Where(x => x.BookingNumber == booking.BookingNumber && PositionIds.Contains(x.PositionId)).ToList();

										string centralMailBox = "";

										var contactMails = companies.Where(a => a.Company_Id == users.Company_Id).FirstOrDefault().ContactDetails?.Where(a => a.IsCentralEmail == true
															&& !string.IsNullOrWhiteSpace(a.MAIL)).Select(a => a.MAIL).ToList();
										if (contactMails?.Count > 0)
										{
											centralMailBox = string.Join(";", contactMails);
										}

										foreach (var item in positions)
										{
											response = new PDFTemplateGetRes() { ResponseStatusMessage = new ResponseStatusMessage() { ErrorMessage = new List<string>() }, DocumentDetails = new DocumentDetails() };
											if (!string.IsNullOrWhiteSpace(item.SupplierInfo.Contact_Email))
											{
												if (!string.IsNullOrWhiteSpace(booking.StaffDetails?.Staff_OpsUser_Email))
												{
													var mailRes = await CreateMailOPSRoomingList(booking, item, request);
													if (mailRes?.ResponseStatusMessage?.Status.ToLower() == "success")
													{
														posSTDT = (item.STARTDATE != null ? item.STARTDATE.Value.ToString("dd/MM/yyyy") : "");
														posEDDT = (item.ENDDATE != null ? item.ENDDATE.Value.ToString("dd/MM/yyyy") : "");

														FileHandlerName = "";
														FileHandlerContactNo = "";
														FileHandlerEmail = "";
														filepath = "";

														docStoreList = docStoreList.Where(a => a.PositionId == item.Position_Id).ToList();
														maxDocumentStore = docStoreList?.Count > 0 ? docStoreList.Count.ToString() : "0";
														int newdocNo = Convert.ToInt32(maxDocumentStore) + 1;
														documentNo = newdocNo > 9 ? newdocNo.ToString() : "0" + newdocNo.ToString();
														fileName = booking.BookingNumber + "-" + item.OrderNr + "-" + documentNo;
														pdfFileName = fileName + ".pdf";
														filepath = Path.Combine(ProposalDocumentFilePath, pdfFileName);

														//bool isFileExists = File.Exists(filepath);
														var roomigBuilder = new StringBuilder();
														builder = new StringBuilder();
														using (StreamReader SourceReader = File.OpenText(request.DocumentPath))
														{
															builder.Append(SourceReader.ReadToEnd());
														}

														if (!string.IsNullOrWhiteSpace(item.HotelPLacer_ID))
														{
															compcontacts = contactList.Where(a => a.HeadOffice_Id == item.HotelPLacer_ID).FirstOrDefault()?.ContactDetails;
															hotelplacer = compcontacts?.Where(a => a.Contact_Id == item.HotelPLacer_ID).FirstOrDefault();
															if (!string.IsNullOrWhiteSpace(hotelplacer?.MAIL))
															{
																FileHandlerName = hotelplacer.FIRSTNAME + hotelplacer.LastNAME;
																FileHandlerContactNo = hotelplacer.TEL;
																FileHandlerEmail = hotelplacer.MAIL;
															}
															else
																IshotelPlacer = false;
														}
														else
															IshotelPlacer = false;

														if (IshotelPlacer == false)
														{
															FileHandlerName = booking.StaffDetails.Staff_OpsUser_Name;
															FileHandlerContactNo = contacts.TEL;
															FileHandlerEmail = booking.StaffDetails.Staff_OpsUser_Email;
														}

														var builderPassenger = new StringBuilder();
														booking.BookingPax = booking.BookingPax.Where(a => !string.IsNullOrWhiteSpace(a.PERSTYPE) && a.PERSTYPE.ToLower() == "adult").ToList();
														foreach (var itemBookingPax in booking.BookingPax)
														{
															builderPassenger.Append(itemBookingPax.PERSTYPE + (itemBookingPax.PERSONS > 0 ? " X " + itemBookingPax.PERSONS.ToString() : ""));
															builderPassenger.Append(",");
														}

                                                        builder.Replace("{{URLinitial}}", URLinitial);
                                                        builder.Replace("{{SYS_COMPANY_NAME}}", sysCompany.Name);
                                                        builder.Replace("{{SYS_COMPANY_ADDR1}}", sysCompany.Street);
                                                        builder.Replace("{{SYS_COMPANY_ADDR2}}", (!string.IsNullOrWhiteSpace(sysCompany.Street2) ? sysCompany.Street2 : "") + " "+(!string.IsNullOrWhiteSpace(sysCompany.Street3) ? sysCompany.Street3 : ""));
                                                        builder.Replace("{{SYS_COMPANY_CITY}}", sysCompany.CityName);
                                                        builder.Replace("{{SYS_COMPANY_COUNTRY}}", sysCompany.CountryName);
                                                        builder.Replace("{{SYS_COMPANY_POSTALCODE}}", sysCompany.Zipcode);
                                                        builder.Replace("{{FileHandler_CONTACT_NAME}}", FileHandlerName);
                                                        builder.Replace("{{FileHandler_CONTACT_TEL_No}}", FileHandlerContactNo);

														builder.Replace("{{FileHandler_CONTACT_EMAIL}}", FileHandlerEmail);
														builder.Replace("{{FileHandler_DATE}}", FileHandlerDate);

														builder.Replace("{{PRODUCT_NAME}}", item.Product_Name);
														builder.Replace("{{BOOKING_REF_NO}}", booking.BookingNumber);
														builder.Replace("{{POS_ORDERNO}}", item.OrderNr);
														builder.Replace("{{POS_PASSENGER_BREAKDOWN}}", Convert.ToString(builderPassenger).TrimEnd(','));

														builder.Replace("{{Nationality}}", booking.GuestDetails.Nationality_Name);
														builder.Replace("{{POS_START_DATE}}", posSTDT);
														builder.Replace("{{POS_END_Date}}", posEDDT);
														builder.Replace("{{POS_START_TIME}}", item.STARTTIME);
														builder.Replace("{{POS_END_TIME}}", item.ENDTIME);

														builder.Replace("{{Book_START_DATE}}", (booking.STARTDATE != null ? booking.STARTDATE.Value.ToString("dd/MM/yyyy") : ""));
														builder.Replace("{{Book_END_Date}}", (booking.ENDDATE != null ? booking.ENDDATE.Value.ToString("dd/MM/yyyy") : ""));
														builder.Replace("{{Tour_Leader_Name}}", booking.TourLeader_Name);
														builder.Replace("{{Tour_Leader_Contact_Det}}", booking.TourLeader_Contact);

														builder.Replace("{{POS_Board_Basis}}", item.HOTELMEALPLAN);
														builder.Replace("{{Porterage}}", (item.Porterage == true ? "Yes" : "No"));

														if (booking.RoomingList?.Count > 0)
														{
															string name = "";
															roomigBuilder.Append(blankRow);
															roomigBuilder.Append("<table width='950' border='0' cellpadding='0' cellspacing='0'><tr><td valign='top'><h2>Rooming List</h2></td></tr></table>");
															roomigBuilder.Append(blankRow);
															roomigBuilder.Append("<table width='950' border='0' cellpadding='0' cellspacing='0' class='plain-table'>");
															roomigBuilder.Append("<tr><td align='center' valign='middle' height='40'><b>Passenger Number</b></td>");
															roomigBuilder.Append("<td align='center' valign='middle' height='40'><b>Passenger Name</b></td>");
															roomigBuilder.Append("<td align='center' valign='middle' height='40'><b>Date of Birth</b></td>");
															roomigBuilder.Append("<td align='center' valign='middle' height='40'><b>Sex</b></td>");
															roomigBuilder.Append("<td align='center' valign='middle' height='40'><b>Passport</b></td>");
															roomigBuilder.Append("<td align='center' valign='middle' height='40'><b>Passport Expiry</b></td>");
															roomigBuilder.Append("<td align='center' valign='middle' height='40'><b>Room Type</b></td>");
															roomigBuilder.Append("<td align='center' valign='middle' height='40'><b>Room Number</b></td>");
															roomigBuilder.Append("<td align='center' valign='middle' height='40'><b>Is Tour Leader</b></td>");
															roomigBuilder.Append("<td align='center' valign='middle' height='40'><b>SRS</b></td>");
															roomigBuilder.Append("<td align='center' valign='middle' height='40'><b>Comment</b></td>");
															roomigBuilder.Append("</tr>");

															foreach (var itemRooming in booking.RoomingList)
															{
																name = itemRooming.FirstName + (string.IsNullOrWhiteSpace(itemRooming.LastName) ? "" : (" " + itemRooming.LastName));
																roomigBuilder.Append("<tr>");
																roomigBuilder.Append("<td class='bck-white padl10'>" + itemRooming.PassengerNumber.ToString() + "</td>");
																roomigBuilder.Append("<td class='bck-white padl10'>" + name + "</td>");
																roomigBuilder.Append("<td class='bck-white padl10'>" + (itemRooming.DateOfBirth != null ? itemRooming.DateOfBirth.Value.ToString("dd-MMM-yyyy") : "") + "</td>");
																roomigBuilder.Append("<td class='bck-white padl10'>" + itemRooming.Sex + "</td>");
																roomigBuilder.Append("<td class='bck-white padl10'>" + itemRooming.PassportNumber + "</td>");
																roomigBuilder.Append("<td class='bck-white padl10'>" + (itemRooming.PassportExpiry != null ? itemRooming.PassportExpiry.Value.ToString("dd-MMM-yyyy") : "") + "</td>");
																roomigBuilder.Append("<td class='bck-white padl10'>" + itemRooming.RoomType + "</td>");
																roomigBuilder.Append("<td class='bck-white padl10'>" + Convert.ToString(itemRooming.RoomAssignment) + "</td>");
																roomigBuilder.Append("<td class='bck-white padl10'>" + (itemRooming.ISTourLeader == true ? "Yes" : "No") + "</td>");
																roomigBuilder.Append("<td class='bck-white padl10'><ul>");
																foreach (var itemSRS in itemRooming.SpecialAssistanceRequirements)
																{
																	roomigBuilder.Append("<li>" + itemSRS + "</li>");
																}
																roomigBuilder.Append("</ul></td>");
																roomigBuilder.Append("<td class='bck-white padl10'><ul>");
																foreach (var itemDR in itemRooming.DietaryRequirements)
																{
																	roomigBuilder.Append("<li>" + itemDR + "</li>");
																}
																roomigBuilder.Append("</ul></td>");
																roomigBuilder.Append("</tr>");
															}
															builder.Replace("{{Rooming_List}}", roomigBuilder.ToString());
														}
														else
														{
															builder.Replace("{{Rooming_List}}", "");
														}

														response.DocumentDetails.Html = builder.ToString();
														PDFGenerateGetRes res = GenerateAndSavePDF(pdfFileName, new PdfDocument() { Html = response.DocumentDetails.Html });
														response.ResponseStatusMessage = res.ResponseStatusMessage;
														response.DocumentDetails.DocumentName = pdfFileName;
														response.DocumentDetails.DocumentPath = filepath;
														response.DocumentDetails.FullDocumentPath = filepath;

														response.To = item.SupplierInfo.Contact_Email;
														response.Subject = "Rooming List - " + booking.BookingNumber + " - " + booking.CustRef + " - Check In:" + posSTDT + " - Check Out:" + posEDDT;
														response.SupplierId = item.SupplierInfo.Id;
														response.UserEmail = request.UserEmail;
														response.SystemCompany_Id = booking.SystemCompany_Id;
														response.QRFID = booking.QRFID;
														response.PositionId = item.Position_Id;
														response.DocumentDetails.DocumentReference = fileName;
														response.Body = mailRes.Body;
														response.PathType = "proposalpdfpath";
														var email = _emailRepository.GetSmtpCredentials(booking.StaffDetails.Staff_OpsUser_Email)?.UserName;
														response.From = Encrypt.DecryptData("", email);
														response.CC = centralMailBox;
													}
													else
													{
														response.ResponseStatusMessage = mailRes.ResponseStatusMessage;
													}
												}
												else
												{
													response.ResponseStatusMessage.Status = "Failure";
													response.ResponseStatusMessage.ErrorMessage.Add("File handler Email can not be null/blank for PositionId " + item.Position_Id);
												}
											}
											else
											{
												response.ResponseStatusMessage.Status = "Failure";
												response.ResponseStatusMessage.ErrorMessage.Add("Supplier Email ID can not be null/blank for PositionId " + item.Position_Id);
											}
											lstResponse.Add(response);
										}
									}
									else
									{
										response.ResponseStatusMessage.Status = "Failure";
										response.ResponseStatusMessage.ErrorMessage.Add("Contact details not found in monogodb for Staff_OpsUser_Id:-" + booking.StaffDetails.Staff_OpsUser_Id);
										lstResponse.Add(response);
									}
								}
								else
								{
									response.ResponseStatusMessage.Status = "Failure";
									response.ResponseStatusMessage.ErrorMessage.Add("SystemCompany_Id not found in monogodb.");
									lstResponse.Add(response);
								}
							}
							else
							{
								response.ResponseStatusMessage.Status = "Failure";
								response.ResponseStatusMessage.ErrorMessage.Add("SystemCompany_Id/Position SupplierId not found in monogodb.");
								lstResponse.Add(response);
							}
						}
						else
						{
							response.ResponseStatusMessage.Status = "Failure";
							response.ResponseStatusMessage.ErrorMessage.Add("Position details not found in monogodb.");
							lstResponse.Add(response);
						}
					}
					else
					{
						response.ResponseStatusMessage.Status = "Failure";
						response.ResponseStatusMessage.ErrorMessage.Add("Booking Number " + request.BookingNo + " not found in monogodb.");
						lstResponse.Add(response);
					}
				}
				else
				{
					response.ResponseStatusMessage.Status = "Success";
					response.ResponseStatusMessage.ErrorMessage.Add("Mail Path can not be null/empty for DocType " + request.DocumentType);
				}
			}
			catch (Exception ex)
			{
				response.ResponseStatusMessage.Status = "Failure";
				response.ResponseStatusMessage.ErrorMessage.Add(ex.Message);
				lstResponse.Add(response);
			}
			return lstResponse;
		}

		//OPS Rooming List Mail Creation Template
		public async Task<MailGenerateRes> CreateMailOPSRoomingList(Bookings resBooking, Positions position, PDFGetReq request)
		{
			MailGenerateRes response = new MailGenerateRes() { ResponseStatusMessage = new ResponseStatusMessage() { ErrorMessage = new List<string>() } };
			StringBuilder builder = new StringBuilder();

			try
			{
				using (StreamReader SourceReader = File.OpenText(request.MailPath))
				{
					builder.Append(SourceReader.ReadToEnd());
				}

				var ProductSrp = _MongoContext.mProducts_Lite.AsQueryable().Where(a => a.VoyagerProduct_Id == position.Product_Id).FirstOrDefault();
				var mUser = await _MongoContext.mUsers.FindAsync(a => a.VoyagerUser_Id == request.UserId).Result.FirstOrDefaultAsync();
				var mCompany = await _MongoContext.mCompanies.FindAsync(a => a.Company_Id == mUser.Company_Id).Result.FirstOrDefaultAsync();
				var resContact = mCompany.ContactDetails.Where(a => a.Contact_Id == mUser.Contact_Id).FirstOrDefault();
				var supplierEmail = position.SupplierInfo.Contact_Email;

				int days = Convert.ToInt32(position.DURATION);
				days = days + 1;
				#region replace email content
				builder.Replace("{{Product_Supplier_Contact_Details_for_Groups}}", position.SupplierInfo.Contact_Name);
				builder.Replace("{{Hotel_Name}}", ProductSrp.ProdName);
				builder.Replace("{{Hotel_Address_Line_1}}", string.IsNullOrWhiteSpace(ProductSrp.Address) ? "" : ",</br>");
				builder.Replace("{{Hotel_City}}", ProductSrp.CityName);
				builder.Replace("{{Hotel_Country}}", ProductSrp.CountryName);
				builder.Replace("{{Our_Booking_Reference_Number}}", resBooking.BookingNumber);
				builder.Replace("{{Client_Tour_Name}}", resBooking.CustRef);
				builder.Replace("{{Check_In_Date}}", Convert.ToDateTime(position.STARTDATE).ToString("dd-MMM-yyyy"));
				builder.Replace("{{Check_Out_Date}}", Convert.ToDateTime(position.ENDDATE).ToString("dd-MMM-yyyy"));
				builder.Replace("{{Position_Duration}}", position.DURATION);
				builder.Replace("{{Placer_Name}}", resContact.FIRSTNAME + " " + resContact.LastNAME);
				builder.Replace("{{Placer_Contact_Number}}", resContact.TEL);
				builder.Replace("{{Placer_Email}}", resContact.MAIL);

				var rooms = new StringBuilder();
				if (position.BookingRoomsAndPrices != null && position.BookingRoomsAndPrices.Count > 0)
				{
					for (int a = 0; a < position.BookingRoomsAndPrices.Count; a++)
					{
						var persontype = position.BookingRoomsAndPrices[a].PersonType; if (!string.IsNullOrWhiteSpace(persontype)) { persontype = "(" + persontype + ")"; } else { persontype = ""; }
						var categoryname = (position.BookingRoomsAndPrices[a].CategoryName == null ? "" : position.BookingRoomsAndPrices[a].CategoryName.ToUpper()) + " " + position.BookingRoomsAndPrices[a].RoomName + " " + persontype;
						rooms.Append("<tr>");
						rooms.Append("<td>" + categoryname + "</td>");
						rooms.Append("<td>" + position.BookingRoomsAndPrices[a].Req_Count + "</td>");
						rooms.Append("<td>" + position.BookingRoomsAndPrices[a].BuyPrice + "(" + position.BookingRoomsAndPrices[a].BuyCurrency_Name + ")" + "</td>");
						rooms.Append("<td>" + position.HOTELMEALPLAN + "</td>");
						if (a == 0)
						{
							var interconnectrooms = position.InterConnectingRooms != null ? "InterConnecting Rooms : " + position.InterConnectingRooms + "<br>" : "";
							var washchngeroom = (position.WashChangeRoom != null && position.WashChangeRoom > 0) ? "Wash and Change Rooms : " + position.WashChangeRoom + "<br>" : "";
							var latecheckout = position.LateCheckout != null ? "<br> Late Check out : " + position.LateCheckout : "";
							rooms.Append("<td rowspan=" + position.BookingRoomsAndPrices.Count + ">" + interconnectrooms + washchngeroom + latecheckout + "<br></td>");
							rooms.Append("<td rowspan=" + position.BookingRoomsAndPrices.Count + ">" + position.Special_Requests + "</td>");
						}
						rooms.Append("</tr>");
					}
				}
				else
				{
					rooms.Append("<tr>");
					rooms.Append("<td></td>");
					rooms.Append("<td></td>");
					rooms.Append("<td></td>");
					rooms.Append("<td></td>");
					rooms.Append("<td></td>");
					rooms.Append("<td></td>");
					rooms.Append("</tr>");
				}
				builder.Replace("{{BookingRooms}}", rooms.ToString());
				response.ResponseStatusMessage.Status = "Success";
				response.ResponseStatusMessage.ErrorMessage.Add("Mail Template Created Successfully.");
				response.DocPath = request.MailPath;
				response.Body = builder.ToString();
				#endregion

			}
			catch (Exception ex)
			{
				response.ResponseStatusMessage.Status = "Error";
				response.ResponseStatusMessage.ErrorMessage.Add(ex.Message);
			}
			return response;
		}

		public async Task<List<PDFTemplateGetRes>> CreateOPSFullItinerary(PDFGetReq request)
		{
			List<PDFTemplateGetRes> lstResponse = new List<PDFTemplateGetRes>() { };
			PDFTemplateGetRes response = new PDFTemplateGetRes() { ResponseStatusMessage = new ResponseStatusMessage() { ErrorMessage = new List<string>() }, DocumentDetails = new DocumentDetails() };

			try
			{
				if (!string.IsNullOrWhiteSpace(request.Module))
				{
					request.Module = request.Module.ToLower();
					//if (request.PositionIds?.Count > 0)
					//{
						//var booking = await _MongoContext.Bookings.FindAsync(x => x.BookingNumber == request.BookingNo).Result.FirstOrDefaultAsync();
						//if (booking != null)
						//{
						//	var positions = booking.Positions.Where(a => request.PositionIds.Contains(a.Position_Id)).ToList();
						//	if (positions.Count > 0)
						//	{
						//		var PositionIds = positions.Select(a => a.Position_Id).ToList();
						//		var HotelPLacerIDs = positions.Select(a => a.HotelPLacer_ID).ToList();

						//		List<string> companyidList = new List<string>() { booking.SystemCompany_Id };
						//		var posSupplierIds = positions.Select(a => a.SupplierInfo.Id).ToList();
						//		companyidList.AddRange(posSupplierIds);
						//		var companies = _MongoContext.mCompanies.AsQueryable().Where(x => companyidList.Contains(x.Company_Id)).ToList();

						//		if (companies?.Count > 0)
						//		{
						//			var sysCompany = companies.Where(a => a.Company_Id == booking.SystemCompany_Id).FirstOrDefault();
						//			var posSuppliers = companies.Where(a => posSupplierIds.Contains(a.Company_Id)).ToList();

						//			if (sysCompany != null && posSuppliers?.Count > 0)
						//			{
						//				var contacts = _MongoContext.mContacts.AsQueryable().Where(x => x.VoyagerContact_Id == booking.StaffDetails.Staff_OpsUser_Id).FirstOrDefault();

						//				if (contacts != null)
						//				{
						//					string fileName = "";
						//					string pdfFileName = "";
						//					bool IshotelPlacer = true;
						//					string URLinitial = _configuration.GetValue<string>("SystemSettings:URLinitial");
						//					string ProposalDocumentFilePath = _configuration.GetValue<string>("SystemSettings:ProposalDocumentFilePath");
						//					string FileHandlerName = "";
						//					string FileHandlerContactNo = "";
						//					string FileHandlerEmail = "";
						//					string filepath = "";
						//					string maxDocumentStore = "";
						//					string documentNo = "";
						//					string VoucherDate = DateTime.Now.ToString("dd/MM/yyyy");
						//					var builder = new StringBuilder();
						//					var posSupplier = new mCompanies();
						//					var compcontacts = new List<CompanyContacts>();
						//					var hotelplacer = new CompanyContacts();
						//					List<ServiceDetailsOption> lstServiceDetailsOption = new List<ServiceDetailsOption>();
						//					string blankRow = "<table width='950' border='0' align='center' cellpadding='0' cellspacing='0' class='tbl-center'><tr><td colspan='2'>&nbsp;</td></tr></table>";

						//					var posCountryids = positions.Select(a => a.Country_Id).ToList();
						//					var posProductIds = positions.Select(a => a.Product_Id).ToList();
						//					var companyids = new List<string>() { booking.SystemCompany_Id, booking.AgentInfo.Id };
						//					companyids.AddRange(posSupplierIds);

						//					var emergencyContacts = _MongoContext.mEmergencyContacts.AsQueryable().Where(x => posCountryids.Contains(x.Country_Id) && x.Company_Id == booking.SystemCompany_Id).ToList();
						//					var termsAndConditions = _MongoContext.mTermsAndConditions.AsQueryable().Where(x => x.DocumentType.ToLower() == "voucher").ToList();

						//					var docStoreList = _MongoContext.mDocumentStore.AsQueryable().Where(x => x.BookingNumber == booking.BookingNumber && PositionIds.Contains(x.PositionId)).ToList();
						//					var contactList = _MongoContext.mCompanies.AsQueryable().Where(x => x.ContactDetails != null && x.ContactDetails.Count > 0 && x.ContactDetails.Any(a => HotelPLacerIDs.Contains(a.Contact_Id)));
						//					var mSystem = _MongoContext.mSystem.AsQueryable().Where(x => x.CoreCompany_Id == booking.SystemCompany_Id).FirstOrDefault();

						//					foreach (var item in positions)
						//					{
						//						response = new PDFTemplateGetRes() { ResponseStatusMessage = new ResponseStatusMessage() { ErrorMessage = new List<string>() }, DocumentDetails = new DocumentDetails() };
						//						FileHandlerName = "";
						//						FileHandlerContactNo = "";
						//						FileHandlerEmail = "";
						//						filepath = "";

						//						lstServiceDetailsOption = new List<ServiceDetailsOption>();
						//						var serviceBuilder = new StringBuilder();

						//						docStoreList = docStoreList.Where(a => a.PositionId == item.Position_Id).ToList();
						//						maxDocumentStore = docStoreList?.Count > 0 ? docStoreList.Count.ToString() : "0";
						//						int newdocNo = Convert.ToInt32(maxDocumentStore) + 1;
						//						documentNo = newdocNo > 9 ? newdocNo.ToString() : "0" + newdocNo.ToString();
						//						fileName = booking.BookingNumber + "-" + item.OrderNr + "-" + documentNo;
						//						pdfFileName = fileName + ".pdf";
						//						filepath = Path.Combine(ProposalDocumentFilePath, pdfFileName);

						//						bool isFileExists = File.Exists(filepath);
						//						isFileExists = request.Module == "position" ? false : (request.Module == "booking" && isFileExists) ? true : false;

						//						if (!isFileExists)
						//						{
						//							posSupplier = posSuppliers.Where(a => a.Company_Id == item.SupplierInfo.Id).FirstOrDefault();
						//							builder = new StringBuilder();
						//							using (StreamReader SourceReader = File.OpenText(request.DocumentPath))
						//							{
						//								builder.Append(SourceReader.ReadToEnd());
						//							}

						//							if (!string.IsNullOrWhiteSpace(item.HotelPLacer_ID))
						//							{
						//								compcontacts = contactList.Where(a => a.HeadOffice_Id == item.HotelPLacer_ID).FirstOrDefault()?.ContactDetails;
						//								hotelplacer = compcontacts?.Where(a => a.Contact_Id == item.HotelPLacer_ID).FirstOrDefault();
						//								if (!string.IsNullOrWhiteSpace(hotelplacer?.MAIL))
						//								{
						//									FileHandlerName = hotelplacer.FIRSTNAME + hotelplacer.LastNAME;
						//									FileHandlerContactNo = hotelplacer.TEL;
						//									FileHandlerEmail = hotelplacer.MAIL;
						//								}
						//								else
						//									IshotelPlacer = false;
						//							}
						//							else
						//								IshotelPlacer = false;

						//							if (IshotelPlacer == false)
						//							{
						//								FileHandlerName = booking.StaffDetails.Staff_OpsUser_Name;
						//								FileHandlerContactNo = contacts.TEL;
						//								FileHandlerEmail = booking.StaffDetails.Staff_OpsUser_Email;
						//							}

						//							var builderPassenger = new StringBuilder();
						//							booking.BookingPax = booking.BookingPax.Where(a => !string.IsNullOrWhiteSpace(a.PERSTYPE) && a.PERSTYPE.ToLower() == "adult").ToList();
						//							foreach (var itemBookingPax in booking.BookingPax)
						//							{
						//								builderPassenger.Append(itemBookingPax.PERSTYPE + (itemBookingPax.PERSONS > 0 ? " X " + itemBookingPax.PERSONS.ToString() : ""));
						//								builderPassenger.Append(",");
						//							}

						//							builder.Replace("{{URLinitial}}", URLinitial);
						//							builder.Replace("{{SYS_COMPANY_NAME}}", sysCompany.Name);
						//							builder.Replace("{{SYS_COMPANY_ADDR1}}", sysCompany.Street);
						//							builder.Replace("{{SYS_COMPANY_ADDR2}}", (!string.IsNullOrWhiteSpace(sysCompany.Street2) ? sysCompany.Street2 : "") + (!string.IsNullOrWhiteSpace(sysCompany.Street3) ? sysCompany.Street3 : ""));
						//							builder.Replace("{{SYS_COMPANY_CITY}}", sysCompany.CityName);
						//							builder.Replace("{{SYS_COMPANY_COUNTRY}}", sysCompany.CountryName);
						//							builder.Replace("{{SYS_COMPANY_POSTALCODE}}", sysCompany.Zipcode);
						//							builder.Replace("{{FileHandler_CONTACT_NAME}}", FileHandlerName);
						//							builder.Replace("{{FileHandler_CONTACT_TEL_No}}", FileHandlerContactNo);

						//							builder.Replace("{{FileHandler_CONTACT_EMAIL}}", FileHandlerEmail);
						//							builder.Replace("{{FileHandler_DATE}}", VoucherDate);
						//							builder.Replace("{{SUPPLIER_NAME}}", item.SupplierInfo.Name);
						//							builder.Replace("{{Supplier_Address1}}", posSupplier.Street);
						//							builder.Replace("{{Supplier_Address2}}", (!string.IsNullOrWhiteSpace(posSupplier.Street2) ? sysCompany.Street2 : "") + (!string.IsNullOrWhiteSpace(sysCompany.Street3) ? sysCompany.Street3 : ""));
						//							builder.Replace("{{Supplier_City}}", posSupplier.CityName);
						//							builder.Replace("{{Supplier_PostalCode}}", posSupplier.Zipcode);
						//							builder.Replace("{{SUPPLIER_COUNTRY}}", posSupplier.CountryName);

						//							builder.Replace("{{POS_Supplier_Confirmation}}", item.Supplier_Confirmation);
						//							builder.Replace("{{POS_SUPPLIER_CONTACT_NAME}}", item.SupplierInfo.Contact_Name);
						//							builder.Replace("{{POS_SUPPLIER_TELEPHONE}}", item.SupplierInfo.Contact_Tel);
						//							builder.Replace("{{POS_SUPPLIER_EMAIL}}", item.SupplierInfo.Contact_Email);
						//							builder.Replace("{{PRODUCT_NAME}}", item.Product_Name);
						//							builder.Replace("{{BOOKING_REF_NO}}", booking.BookingNumber);
						//							builder.Replace("{{POS_ORDERNO}}", item.OrderNr);
						//							builder.Replace("{{POS_PASSENGER_BREAKDOWN}}", Convert.ToString(builderPassenger).TrimEnd(','));

						//							builder.Replace("{{Nationality}}", booking.GuestDetails.Nationality_Name);
						//							builder.Replace("{{POS_START_DATE}}", (item.STARTDATE != null ? item.STARTDATE.Value.ToString("dd/MM/yyyy") : ""));
						//							builder.Replace("{{POS_END_Date}}", (item.ENDDATE != null ? item.ENDDATE.Value.ToString("dd/MM/yyyy") : ""));
						//							builder.Replace("{{POS_START_TIME}}", item.STARTTIME);
						//							builder.Replace("{{POS_END_TIME}}", item.ENDTIME);
						//							builder.Replace("{{POS_START_LOC}}", item.STARTLOC);
						//							builder.Replace("{{POS_END_LOC}}", item.ENDLOC);

						//							builder.Replace("{{VOUCHER_NOTE}}", item.VoucherNote);

						//							var emergencyContactsBuilder = new StringBuilder();
						//							emergencyContactsBuilder.Append(blankRow);
						//							emergencyContactsBuilder.Append("<table width='950' border='0' align='center' cellpadding='0' cellspacing='0' class='tbl-center'>");
						//							emergencyContactsBuilder.Append("<tr><td valign='top'><h2>Emergency Contact Details</h2></td></tr><tr><td colspan='2'>&nbsp;</td></tr><tr><td valign='top'>");
						//							emergencyContactsBuilder.Append("<p>In case of emergency out of office hours, please use the below number(s)</p></td></tr><tr><td colspan='2'>&nbsp;</td></tr></table>");

						//							emergencyContactsBuilder.Append("<table width='700' border='0' align ='center' cellpadding ='0' cellspacing ='0' class='plain-table'>");
						//							emergencyContactsBuilder.Append("<thead><tr class='bck-col'><td align='center' valign='middle' height='40'><b>Contact Name</b></td><td align='center' valign='middle' height='40'><b>Contact Number</b></td></tr></thead>");
						//							emergencyContactsBuilder.Append("<tbody>");

						//							if (emergencyContacts?.Count > 0)
						//							{
						//								foreach (var itemEmergencyContacts in emergencyContacts)
						//								{
						//									emergencyContactsBuilder.Append("<tr>");
						//									emergencyContactsBuilder.Append("<td class='bck-white padl10'>" + itemEmergencyContacts.ContactName + "</td>");
						//									emergencyContactsBuilder.Append("<td class='bck-white padl10'>" + itemEmergencyContacts.EmergencyNo + "</td>");
						//									emergencyContactsBuilder.Append("</tr>");
						//								}
						//							}
						//							else
						//							{
						//								emergencyContactsBuilder.Append("<tr><td class='bck-white padl15'>&nbsp;</td><td class='bck-white padl15'>" + mSystem.EmergencyPhoneGroups + "</td></tr>");
						//							}
						//							emergencyContactsBuilder.Append("</tbody>");
						//							emergencyContactsBuilder.Append("</table>");
						//							builder.Replace("{{emergencydetails}}", emergencyContactsBuilder.ToString());

						//							var termsandcond = termsAndConditions.Where(a => !string.IsNullOrWhiteSpace(a.Tcs)
						//												&& (companyidList.Contains(a.CompanyId) || (a.ProductId == item.Product_Id))).Select(a => a.Tcs).ToList();

						//							if (termsandcond.Count > 0)
						//							{
						//								var termsBuilder = new StringBuilder();
						//								termsBuilder.Append(blankRow);
						//								termsBuilder.Append("<table width='950' border='0' align ='center' cellpadding ='0' cellspacing ='0' class='tbl-center'>");
						//								termsBuilder.Append("<thead><tr><td><h2>Terms and Conditions</h2></td></tr><tr><td>&nbsp;</td></tr></thead>");
						//								termsBuilder.Append("<tbody><tr><td valign='top'>");
						//								termsBuilder.Append("<ul>");

						//								foreach (var itemTermsandcond in termsandcond)
						//								{
						//									termsBuilder.Append("<li>" + itemTermsandcond + "</li>");
						//								}
						//								termsBuilder.Append("</ul>");
						//								termsBuilder.Append("</td></tr></tbody></table>");
						//								builder.Replace("{{POS_TERMS_AND_CONDITIONS}}", termsBuilder.ToString());
						//							}
						//							else
						//							{
						//								builder.Replace("{{POS_TERMS_AND_CONDITIONS}}", "");
						//							}
						//							item.BookingRoomsAndPrices = item.BookingRoomsAndPrices.OrderBy(a => a.RoomName.ToLower() == "single" ? "A" : a.RoomName.ToLower() == "double" ? "B" :
						//															a.RoomName.ToLower() == "twin" ? "C" : a.RoomName.ToLower() == "triple" ? "D" : a.RoomName.ToLower() == "quad" ? "E" :
						//															a.RoomName.ToLower() == "tsu" ? "F" : "G").ToList();

						//							foreach (var itemBookingRoomsAndPrices in item.BookingRoomsAndPrices)
						//							{
						//								serviceBuilder.Append("<tr>");
						//								serviceBuilder.Append("<td class='bck-white padl10'>" + itemBookingRoomsAndPrices.CategoryName + "</td>");
						//								serviceBuilder.Append("<td class='bck-white padl10'>" + itemBookingRoomsAndPrices.RoomName + "</td>");
						//								serviceBuilder.Append("<td class='bck-white padl10'>" + Convert.ToString(itemBookingRoomsAndPrices.Req_Count) + "</td>");
						//								serviceBuilder.Append("<td class='bck-white padl10'>" + (item.ProductType.ToLower().Trim() == "hotel" ? item.HOTELMEALPLAN : item.Menu) + "</td>");
						//								serviceBuilder.Append("</tr>");
						//							}
						//							builder.Replace("{{servicedetails}}", serviceBuilder.ToString());

						//							var optionBuilder = new StringBuilder();
						//							if (item.ProductType.ToLower().Trim() == "hotel")
						//							{
						//								lstServiceDetailsOption.Add(new ServiceDetailsOption() { OptionName = "Breakfast Type", OptionValue = item.BreakFastType });
						//								lstServiceDetailsOption.Add(new ServiceDetailsOption() { OptionName = "Board Basis", OptionValue = item.HOTELMEALPLAN });
						//							}
						//							else if (item.ProductType.ToLower().Trim() == "meal")
						//							{
						//								lstServiceDetailsOption.Add(new ServiceDetailsOption() { OptionName = "Includes Water", OptionValue = item.Water ? "Yes" : "No" });
						//								lstServiceDetailsOption.Add(new ServiceDetailsOption() { OptionName = "Includes Bread", OptionValue = item.Bread ? "Yes" : "No" });
						//								lstServiceDetailsOption.Add(new ServiceDetailsOption() { OptionName = "Includes Dessert", OptionValue = item.Dessert ? "Yes" : "No" });
						//								lstServiceDetailsOption.Add(new ServiceDetailsOption() { OptionName = "Includes Tea / Coffee", OptionValue = item.Tea ? "Yes" : "No" });
						//							}
						//							else if (item.ProductType.ToLower().Trim() == "attractions")
						//							{
						//								lstServiceDetailsOption.Add(new ServiceDetailsOption() { OptionName = "Ticket Pickup", OptionValue = item.TicketLocation });
						//								lstServiceDetailsOption.Add(new ServiceDetailsOption() { OptionName = "Guide Purchase", OptionValue = item.GuidePurchaseTicket ? "Yes" : "No" });
						//							}
						//							else if (item.ProductType.ToLower().Trim() == "coach" || item.ProductType.ToLower().Trim() == "ldc")
						//							{
						//								lstServiceDetailsOption.Add(new ServiceDetailsOption() { OptionName = "Includes Parking", OptionValue = item.Parking ? "Yes" : "No" });
						//								lstServiceDetailsOption.Add(new ServiceDetailsOption() { OptionName = "Includes Road Tolls", OptionValue = item.RoadTolls ? "Yes" : "No" });
						//								lstServiceDetailsOption.Add(new ServiceDetailsOption() { OptionName = "Includes City permits", OptionValue = item.CityPermits ? "Yes" : "No" });
						//								lstServiceDetailsOption.Add(new ServiceDetailsOption() { OptionName = "Driver Name", OptionValue = item.DriverName });
						//								lstServiceDetailsOption.Add(new ServiceDetailsOption() { OptionName = "Driver Contact Number", OptionValue = item.DriverContactNumber });
						//								lstServiceDetailsOption.Add(new ServiceDetailsOption() { OptionName = "Vehicle Registration Number", OptionValue = item.VehicleRegistration });
						//							}
						//							if (lstServiceDetailsOption.Count > 0)
						//							{
						//								foreach (var itemServiceDetailsOption in lstServiceDetailsOption)
						//								{
						//									optionBuilder.Append("<tr>");
						//									optionBuilder.Append("<td class='bck-white padl10'>" + itemServiceDetailsOption.OptionName + "</td>");
						//									optionBuilder.Append("<td class='bck-white padl10'>" + itemServiceDetailsOption.OptionValue + "</td>");
						//									optionBuilder.Append("</tr>");
						//								}
						//							}
						//							else
						//							{
						//								optionBuilder.Append("<tr>");
						//								optionBuilder.Append("<td colspan='2' class='bck-white padl10'>-</td>");
						//								optionBuilder.Append("</tr>");
						//							}
						//							builder.Replace("{{optiondetails}}", optionBuilder.ToString());

						//							response.DocumentDetails.Html = builder.ToString();

						//							PDFGenerateGetRes res = GenerateAndSavePDF(pdfFileName, new PdfDocument() { Html = response.DocumentDetails.Html });
						//							response.ResponseStatusMessage = res.ResponseStatusMessage;
						//							response.DocumentDetails.DocumentName = pdfFileName;
						//							response.DocumentDetails.DocumentPath = filepath;
						//							response.DocumentDetails.FullDocumentPath = filepath;
						//							lstResponse.Add(response);
						//						}
						//						else
						//						{
						//							response.ResponseStatusMessage.Status = "Success";
						//							response.DocumentDetails.DocumentName = pdfFileName;
						//							response.DocumentDetails.DocumentPath = filepath;
						//							response.DocumentDetails.FullDocumentPath = filepath;
						//							lstResponse.Add(response);
						//						}
						//					}

						//					if (request.Module == "booking")
						//					{
						//						ResponseStatus resCreateZipFile = CommonFunction.CreateZipFile(new ZipDetails()
						//						{
						//							DocumentDetails = lstResponse.Select(a => a.DocumentDetails).ToList(),
						//							ZipFileName = booking.BookingNumber + "_Voucher.zip",
						//							ZipFilePath = ProposalDocumentFilePath
						//						});

						//						lstResponse = new List<PDFTemplateGetRes>();
						//						response.ResponseStatusMessage.Status = resCreateZipFile.Status;
						//						if (resCreateZipFile.ErrorMessage?.Count() > 0)
						//						{
						//							response.ResponseStatusMessage.ErrorMessage.Add(resCreateZipFile.ErrorMessage);
						//						}
						//						response.DocumentDetails.DocumentName = booking.BookingNumber + "_Voucher.zip";
						//						response.DocumentDetails.DocumentPath = ProposalDocumentFilePath;
						//						response.DocumentDetails.FullDocumentPath = ProposalDocumentFilePath;
						//						lstResponse.Add(response);
						//					}
						//				}
						//				else
						//				{
						//					response.ResponseStatusMessage.Status = "Failure";
						//					response.ResponseStatusMessage.ErrorMessage.Add("Contact details not found in monogodb for Staff_OpsUser_Id:-" + booking.StaffDetails.Staff_OpsUser_Id);
						//					lstResponse.Add(response);
						//				}
						//			}
						//			else
						//			{
						//				response.ResponseStatusMessage.Status = "Failure";
						//				response.ResponseStatusMessage.ErrorMessage.Add("SystemCompany_Id/Position SupplierId not found in monogodb.");
						//				lstResponse.Add(response);
						//			}
						//		}
						//		else
						//		{
						//			response.ResponseStatusMessage.Status = "Failure";
						//			response.ResponseStatusMessage.ErrorMessage.Add("SystemCompany_Id/Position SupplierId not found in monogodb.");
						//			lstResponse.Add(response);
						//		}
						//	}
						//	else
						//	{
						//		response.ResponseStatusMessage.Status = "Failure";
						//		response.ResponseStatusMessage.ErrorMessage.Add("Position details not found in monogodb.");
						//		lstResponse.Add(response);
						//	}
						//}
						//else
						//{
						//	response.ResponseStatusMessage.Status = "Failure";
						//	response.ResponseStatusMessage.ErrorMessage.Add("Booking Number " + request.BookingNo + " not found in monogodb.");
						//	lstResponse.Add(response);
						//}
					//}
					//else
					//{
					//	response.ResponseStatusMessage.Status = "Failure";
					//	response.ResponseStatusMessage.ErrorMessage.Add("Position Id can not be null.");
					//	lstResponse.Add(response);
					//}
				}
				else
				{
					response.ResponseStatusMessage.Status = "Failure";
					response.ResponseStatusMessage.ErrorMessage.Add("Module Name can not be null/empty.");
					lstResponse.Add(response);
				}
			}
			catch (Exception ex)
			{
				response.ResponseStatusMessage.Status = "Failure";
				response.ResponseStatusMessage.ErrorMessage.Add(ex.Message);
				lstResponse.Add(response);
			}

			return lstResponse;
		}
		#endregion
		#endregion
	}
}
