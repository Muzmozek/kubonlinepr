using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RazorEngine;
using RazorEngine.Templating;
using KUBOnlinePRPM.Models;
using System.IO;
using KUBOnlinePRPM.Controllers;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Core.Objects;
using System.DirectoryServices;
using System.DirectoryServices.Protocols;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices.AccountManagement;
using System.Net;
using System.Globalization;
using KUBOnlinePRPM.Service;
using System.Diagnostics;

namespace KUBOnlinePRPM.Controllers
{
    public class DecimalModelBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext,
            ModelBindingContext bindingContext)
        {
            ValueProviderResult valueResult = bindingContext.ValueProvider
                .GetValue(bindingContext.ModelName);
            ModelState modelState = new ModelState { Value = valueResult };
            object actualValue = null;
            try
            {
                actualValue = Convert.ToDecimal(valueResult.AttemptedValue,
                    CultureInfo.CurrentCulture);
            }
            catch (FormatException e)
            {
                modelState.Errors.Add(e);
            }

            bindingContext.ModelState.Add(bindingContext.ModelName, modelState);
            return actualValue;
        }
    }
    public class PRController : Controller
    {
        private KUBOnlinePREntities db = new KUBOnlinePREntities();
        //private KUBMEntities KUBMdb = new KUBMEntities();
        private KUBHelper KUBHelper = new KUBHelper();
        private PRService PRService = new PRService();

        //private KUB_TelEntities db1 = new KUB_TelEntities();
        public ActionResult Index()
        {
            //var testList = db1.KUB_Telekomunikasi_Sdn_Bhd_Contact.ToList();
            //ActiveDirectoryContext context = new ActiveDirectoryContext();
            //context.Configuration.UseDatabaseNullSemantics = true;
            //var query = from line in context.User select line;
            return View();
        }
        public JsonResult GetProjectInfo(int projectId)
        {
            var projectInfo = db.Projects.Select(m => new NewPRModel()
            {
                ProjectId = m.projectId,
                PaperVerified = m.paperVerified,
                BudgetedAmount = m.budgetedAmount,
                UtilizedToDate = m.utilizedToDate,
                BudgetBalance = m.budgetBalance
            }).Where(m => m.ProjectId == projectId).FirstOrDefault();

            return Json(new { projectInfo }, JsonRequestBehavior.AllowGet);

        }
        public JsonResult GetItemCodeInfo(int CodeId)
        {
            var ItemCodeInfo = db.ItemCodes.Select(m => new PRItemsTable()
            {
                UOM = m.UoM,
                CodeId = m.codeId
            }).Where(m => m.CodeId == CodeId).FirstOrDefault();

            return Json(new { ItemCodeInfo }, JsonRequestBehavior.AllowGet);

        }
        public JsonResult GetVendorInfo(int VendorId)
        {
            var VendorStaffQuery = (from m in db.Vendors
                                    join n in db.VendorStaffs on m.vendorId equals n.vendorId
                                    where m.vendorId == VendorId
                                    select new NewPRModel()
                                    {
                                        VendorStaffId = n.staffId,
                                        VendorContactName = n.vendorContactName
                                    }).ToList();
            string html = "<select class='custom-select g-width-100vw g-height-50 g- g-color-gray-light-v1 g-color-black--hover text-left g-rounded-20' data-val-required='The VendorId field is required.' id='VendorStaffId'><option value='' >Select vendor contact name here</option>";
            foreach (var item in VendorStaffQuery)
            {
                html += "<option value='" + item.VendorStaffId + "' >" + item.VendorContactName + "</option>";
            }
            html += "</select>";

            return Json(new { html }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetVendorStaffInfo(int VendorStaffId)
        {
            var VendorStaffIdInfo = db.VendorStaffs.Select(m => new NewPRModel()
            {
                VendorStaffId = m.staffId,
                VendorEmail = m.vendorEmail,
                VendorContactNo = m.vendorContactNo
            }).Where(m => m.VendorStaffId == VendorStaffId).FirstOrDefault();

            return Json(new { VendorStaffIdInfo }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult NewPR(string type)
        {
            if (User.Identity.IsAuthenticated && Session["UserId"] != null)
            {
                int CustId = Int32.Parse(Session["CompanyId"].ToString());
                var ProjectNameQuery = (from m in db.Projects
                                        where m.custId == CustId
                                        select new
                                        {
                                            ProjectId = m.projectId,
                                            Dimension = m.dimension,
                                            Description = m.dimension + " - " + m.projectCode + " - " + m.projectName,
                                            Code = m.projectCode,
                                        }).OrderBy(c => c.Dimension).ThenBy(c => c.Code);
                List <SelectListItem> BudgetedList = new List<SelectListItem>();
                BudgetedList.Add(new SelectListItem
                {
                    Text = "Yes",
                    Value = true.ToString()
                });
                BudgetedList.Add(new SelectListItem
                {
                    Text = "No",
                    Value = false.ToString()
                });
                var PurchaseTypeQuery = db.PurchaseTypes.Select(c => new { PurchaseTypeId = c.purchaseTypeId, PurchaseType = c.purchaseType1 }).OrderBy(c => c.PurchaseType);
                //var ReviewerNameQuery = (from m in db.Users
                //                         join n in db.Users_Roles on m.userId equals n.userId
                //                         where n.roleId == "R02"
                //                         select new NewPRModel()
                //                         {
                //                             ReviewerId = m.userId,
                //                             ReviewerName = m.firstName + " " + m.lastName
                //                         }).ToList();
                //var ApproverNameQuery = (from m in db.Users
                //                         join n in db.Users_Roles on m.userId equals n.userId
                //                         where n.roleId == "R04" && m.jobTitle.Trim() == "HOD" 
                //                         select new NewPRModel()
                //                         {
                //                             ApproverId = m.userId,
                //                             ApproverName = m.firstName + " " + m.lastName
                //                         }).First();

                ViewBag.ProjectNameList = new SelectList(ProjectNameQuery.AsEnumerable(), "projectId", "description");
                ViewBag.BudgetedList = new SelectList(BudgetedList.AsEnumerable(), "Value", "Text");
                ViewBag.PurchaseTypeList = new SelectList(PurchaseTypeQuery.AsEnumerable(), "purchaseTypeId", "purchaseType");
                //ViewBag.ReviewerNameList = new SelectList(ReviewerNameQuery.AsEnumerable(), "reviewerId", "reviewerName");
                //ViewBag.ApproverNameList = new SelectList(ApproverNameQuery.AsEnumerable(), "approverId", "approverName");

                int UserId = Int32.Parse(Session["UserId"].ToString());
                var getRole = (from m in db.Users
                               join n in db.Users_Roles on m.userId equals n.userId
                               where m.userId == UserId
                               select new RoleList()
                               {
                                   RoleId = n.roleId,
                                   RoleName = n.Role.name
                               }).ToList();

                PRModel model = new PRModel
                {
                    UserId = UserId,
                    RoleIdList = getRole,
                    Type = type
                };

                return View(model);
            }
            else
            {
                return Redirect("~/Home/Index");
            }
        }

        [HttpPost]
        public ActionResult NewPR(PRModel model)
        {
            if (User.Identity.IsAuthenticated && Session["UserId"] != null)
            {
                int CustId = Int32.Parse(Session["CompanyId"].ToString());
                var ProjectNameQuery = (from m in db.Projects
                                        where m.custId == CustId
                                        select new
                                        {
                                            ProjectId = m.projectId,
                                            Dimension = m.dimension,
                                            Description = m.dimension + " - " + m.projectCode + " - " + m.projectName,
                                            Code = m.projectCode,
                                        }).OrderBy(c => c.Dimension).ThenBy(c => c.Code);
                var PurchaseTypeQuery = db.PurchaseTypes.Select(c => new { PurchaseTypeId = c.purchaseTypeId, PurchaseType = c.purchaseType1 }).OrderBy(c => c.PurchaseType);
                List<SelectListItem> BudgetedList = new List<SelectListItem>();

                BudgetedList.Add(new SelectListItem
                {
                    Text = "Yes",
                    Value = true.ToString()
                });
                BudgetedList.Add(new SelectListItem
                {
                    Text = "No",
                    Value = false.ToString()
                });
                //var ReviewerNameQuery = (from m in db.Users
                //                         join n in db.Users_Roles on m.userId equals n.userId
                //                         where n.roleId == "R02"
                //                         select new NewPRModel()
                //                         {
                //                             ReviewerId = m.userId,
                //                             ReviewerName = m.firstName + " " + m.lastName
                //                         }).ToList();
                //var ApproverNameQuery = (from m in db.Users
                //                         join n in db.Users_Roles on m.userId equals n.userId
                //                         where n.roleId == "R04" && m.jobTitle.Trim() == "HOD"
                //                         select new NewPRModel()
                //                         {
                //                             ApproverId = m.userId,
                //                             ApproverName = m.firstName + " " + m.lastName
                //                         }).First();
                if (model.NewPRForm.PaperRefNo != null && model.PaperRefNoFile == null)
                {
                    ModelState.AddModelError("PaperRefNoFileName", "GMD paper attachment needed.");                    
                }
                if (model.NewPRForm.BidWaiverRefNo != null && model.BidWaiverRefNoFile == null)
                {
                    ModelState.AddModelError("BidWaiverRefNoFileName", "Bid Waiver attachment needed");
                }
                if (model.NewPRForm.PaperRefNo == null && model.PaperRefNoFile != null)
                {
                    ModelState.AddModelError("NewPRForm.PaperRefNo", "GMD paper ref no needed.");                    
                }
                if (model.NewPRForm.BidWaiverRefNo == null && model.BidWaiverRefNoFile != null)
                {
                    ModelState.AddModelError("NewPRForm.BidWaiverRefNo", "Bid Waiver ref no needed");                   
                }
                if (model.PaperRefNoFile != null && Path.GetExtension(model.PaperRefNoFile.FileName) != ".pdf")
                {
                    ModelState.AddModelError("PaperRefNoFileName", "Only pdf file accepted.");
                }
                if (model.BidWaiverRefNoFile != null && Path.GetExtension(model.BidWaiverRefNoFile.FileName) != ".pdf")
                {
                    ModelState.AddModelError("BidWaiverRefNoFileName", "Only pdf file accepted.");
                }
                if (!ModelState.IsValid)
                {
                    ViewBag.ProjectNameList = new SelectList(ProjectNameQuery.AsEnumerable(), "projectId", "description",model.NewPRForm.ProjectId);
                    ViewBag.BudgetedList = new SelectList(BudgetedList.AsEnumerable(), "Value", "Text",model.NewPRForm.Unbudgeted);
                    ViewBag.PurchaseTypeList = new SelectList(PurchaseTypeQuery.AsEnumerable(), "purchaseTypeId", "purchaseType",model.NewPRForm.PurchaseTypeId);
                    //ViewBag.ReviewerNameList = new SelectList(ReviewerNameQuery.AsEnumerable(), "reviewerId", "reviewerName");
                    //ViewBag.ApproverNameList = new SelectList(ApproverNameQuery.AsEnumerable(), "approverId", "approverName");
                    var errors = ModelState.Select(x => x.Value.Errors)
                           .Where(y => y.Count > 0)
                           .ToList();

                    return new JsonResult
                    {
                        Data = new
                        {
                            success = false,
                            exception = true,
                            type = model.Type,
                            PRId = model.PRId,
                            ProjectId = model.NewPRForm.ProjectId,
                            Saved = model.NewPRForm.SelectSave,
                            Submited = model.NewPRForm.SelectSubmit,
                            NewPR = true,
                            view = this.RenderPartialView("NewPR", model)
                        },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                model.CustId = Int32.Parse(Session["CompanyId"].ToString());
                model.NewPRForm.PreparedById = Int32.Parse(Session["UserId"].ToString());
                model.FullName = Session["FullName"].ToString();
                model.UserId = Int32.Parse(Session["UserId"].ToString());
                model.NewPRForm.HODApproverId = db.Users.Select(m => new { SuperiorId = m.superiorId, UserId = m.userId }).Where(m => m.UserId == model.UserId).First().SuperiorId.Value;
                DBLogicController.PRSaveDbLogic(model);

                bool checkPaperVerified = db.Projects.FirstOrDefault(m => m.projectId == model.NewPRForm.ProjectId).paperVerified;
                FileUpload fileUploadModel = new FileUpload();
                FileUpload fileUploadModel1 = new FileUpload();
                DateTime createDate = DateTime.Now;

                if (model.PaperRefNoFile != null && model.NewPRForm.PaperRefNo != null)
                {
                    string PaperRefNoFileFilename = model.PaperRefNoFile.FileName.Replace("\\", ",");
                    string filename = PaperRefNoFileFilename.Split(',')[PaperRefNoFileFilename.Split(',').Length - 1].ToString();
                    string fullPath = model.PaperRefNoFile.FileName;
                    string extension = Path.GetExtension(fullPath);
                    byte[] uploadFile = new byte[model.PaperRefNoFile.InputStream.Length];

                    fileUploadModel.uuid = Guid.NewGuid();
                    fileUploadModel.uploadUserId = Int32.Parse(Session["UserId"].ToString());
                    fileUploadModel.uploadDate = createDate;
                    fileUploadModel.FullPath = fullPath;
                    fileUploadModel.FileName = filename;
                    fileUploadModel.Extension = extension;
                    fileUploadModel.archivedFlag = false;
                    model.PaperRefNoFile.InputStream.Read(uploadFile, 0, uploadFile.Length);
                    fileUploadModel.File = uploadFile;
                    db.FileUploads.Add(fileUploadModel);
                    db.SaveChanges();

                    PRs_FileUpload fileUploadModel2 = new PRs_FileUpload
                    {
                        uuid = Guid.NewGuid(),
                        fileEntryId = fileUploadModel.fileEntryId,
                        PRId = model.PRId
                    };
                    db.PRs_FileUpload.Add(fileUploadModel2);
                    db.SaveChanges();

                    NotificationMsg _objSendMessage = new NotificationMsg
                    {
                        uuid = Guid.NewGuid(),
                        PRId = model.PRId,
                        msgDate = DateTime.Now,
                        fromUserId = Int32.Parse(Session["UserId"].ToString()),
                        message = Session["FullName"].ToString() + " has uploaded " + filename,
                        msgType = "Trail"
                    };
                    db.NotificationMsgs.Add(_objSendMessage);
                }
                if (model.NewPRForm.BidWaiverRefNo == null && model.BidWaiverRefNoFile != null)
                {                                     
                    string BidWaiverRefNoFile = model.BidWaiverRefNoFile.FileName.Replace("\\", ",");                  
                    string filename1 = BidWaiverRefNoFile.Split(',')[BidWaiverRefNoFile.Split(',').Length - 1].ToString();                   
                    string fullPath1 = model.BidWaiverRefNoFile.FileName;                   
                    string extension1 = Path.GetExtension(fullPath1);                   
                    byte[] uploadFile1 = new byte[model.BidWaiverRefNoFile.InputStream.Length];

                    fileUploadModel1.uuid = Guid.NewGuid();
                    fileUploadModel1.uploadUserId = Int32.Parse(Session["UserId"].ToString());
                    fileUploadModel1.uploadDate = createDate;
                    fileUploadModel1.FullPath = fullPath1;
                    fileUploadModel1.FileName = filename1;
                    fileUploadModel1.Extension = extension1;
                    fileUploadModel1.archivedFlag = false;
                    model.BidWaiverRefNoFile.InputStream.Read(uploadFile1, 0, uploadFile1.Length);
                    fileUploadModel1.File = uploadFile1;
                    db.FileUploads.Add(fileUploadModel1);
                    db.SaveChanges();
                    
                    PRs_FileUpload fileUploadModel3 = new PRs_FileUpload
                    {
                        uuid = Guid.NewGuid(),
                        fileEntryId = fileUploadModel1.fileEntryId,
                        PRId = model.PRId
                    };
                    
                    db.PRs_FileUpload.Add(fileUploadModel3);
                    db.SaveChanges();

                    NotificationMsg _objSendMessage1 = new NotificationMsg
                    {
                        uuid = Guid.NewGuid(),
                        PRId = model.PRId,
                        msgDate = DateTime.Now,
                        fromUserId = Int32.Parse(Session["UserId"].ToString()),
                        message = Session["FullName"].ToString() + " has uploaded " + filename1,
                        msgType = "Trail"
                    };
                    db.NotificationMsgs.Add(_objSendMessage1);
                    db.SaveChanges();
                }
                ViewBag.ProjectNameList = new SelectList(ProjectNameQuery.AsEnumerable(), "projectId", "description");
                ViewBag.BudgetedList = new SelectList(BudgetedList.AsEnumerable(), "Value", "Text");
                ViewBag.PurchaseTypeList = new SelectList(PurchaseTypeQuery.AsEnumerable(), "purchaseTypeId", "purchaseType");
                //ViewBag.ReviewerNameList = new SelectList(ReviewerNameQuery.AsEnumerable(), "reviewerId", "reviewerName");
                //ViewBag.ApproverNameList = new SelectList(ApproverNameQuery.AsEnumerable(), "approverId", "approverName");

                return new JsonResult
                {
                    Data = new
                    {
                        success = true,
                        exception = false,
                        type = model.Type,
                        PRId = model.PRId,
                        Saved = model.NewPRForm.SelectSave,
                        Submited = model.NewPRForm.SelectSubmit,
                        NewPR = true,
                        view = this.RenderPartialView("NewPR", model)
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            else
            {
                return Redirect("~/Home/Index");
            }
        }       
        public ActionResult PRList(string type)
        {
            if (User.Identity.IsAuthenticated && Session["UserId"] != null)
            {
                int UserId = Int32.Parse(Session["UserId"].ToString());
                var ifRequestor = Session["ifRequestor"];
                var ifHOD = Session["ifHOD"];
                var ifProcurement = Session["ifProcurement"];
                var ifHOC = Session["ifHOC"];
                var ifAdmin = Session["ifAdmin"];
                var ifSuperAdmin = Session["ifSuperAdmin"];                
                var ifIT = Session["ifIT"];
                var ifPMO = Session["ifPMO"];
                var ifHSE = Session["ifHSE"];
                var ifHOGPSS = Session["ifHOGPSS"];
                //var ifProcurement = KUBHelper.CheckRole("R03") ? true : false;

                PRModel PRList = new PRModel();
                while (ifSuperAdmin != null)
                {
                    PRList.PRListObject = (from m in db.PurchaseRequisitions
                                           join n in db.Users on m.PreparedById equals n.userId into ah
                                           join o in db.Vendors on m.VendorId equals o.vendorId into q
                                           join p in db.PRStatus on m.StatusId equals p.statusId
                                           //join s in db.PR_Admin on m.PRId equals s.PRId into t
                                           //join v in db.PR_Approver on m.PRId equals v.PRId into w
                                           //join y in db.PR_HOD on m.PRId equals y.PRId into z
                                           //join ab in db.PR_Recommender on m.PRId equals ab.PRId into ac
                                           //join ae in db.PR_Reviewer on m.PRId equals ae.PRId into af
                                           from r in q.DefaultIfEmpty()
                                           //from u in t.DefaultIfEmpty()
                                           //from x in w.DefaultIfEmpty()
                                           //from aa in z.DefaultIfEmpty()
                                           //from ad in ac.DefaultIfEmpty()
                                           //from ag in af.DefaultIfEmpty()
                                           from ai in ah.DefaultIfEmpty()
                                           where m.PRType == type 
                                           //&& (m.PreparedById == UserId || u.adminId == UserId || x.approverId == UserId || aa.HODId == UserId || ad.recommenderId == UserId || ag.reviewerId == UserId)
                                           select new PRListTable()
                                           {
                                               PRId = m.PRId,
                                               PRNo = m.PRNo,
                                               PRDate = m.PreparedDate,
                                               RequestorName = ai.firstName + " " + ai.lastName,
                                               VendorCompany = r.name,
                                               AmountRequired = m.AmountRequired,
                                               //PRAging = m.aging,
                                               Status = p.status
                                           }).ToList();
                    PRList.Type = type;

                    return View("PRList", PRList);
                }
                if (ifRequestor != null)
                {
                    var addedQuery = ExpressionHelper.GetExpressionText("&& m.PreparedById == UserId");
                    PRList.PRListObject = (from m in db.PurchaseRequisitions
                                           join n in db.Users on m.PreparedById equals n.userId
                                           join o in db.Vendors on m.VendorId equals o.vendorId into q
                                           join p in db.PRStatus on m.StatusId equals p.statusId
                                           from r in q.DefaultIfEmpty()
                                           where m.PRType == type && m.PreparedById == UserId
                                           select new PRListTable()
                                           {
                                               PRId = m.PRId,
                                               PRNo = m.PRNo,
                                               PRDate = m.PreparedDate,
                                               RequestorName = n.firstName + " " + n.lastName,
                                               VendorCompany = r.name,
                                               AmountRequired = m.AmountRequired,
                                               //PRAging = m.aging,
                                               Status = p.status
                                           }).ToList();
                }
                else if (ifIT != null || ifPMO != null || ifHSE != null)
                {
                    PRList.PRListObject = (from m in db.PurchaseRequisitions
                                           join n in db.Users on m.PreparedById equals n.userId
                                           join o in db.Vendors on m.VendorId equals o.vendorId into q
                                           join p in db.PRStatus on m.StatusId equals p.statusId
                                           join s in db.PR_Reviewer on m.PRId equals s.PRId into t
                                           from r in q.DefaultIfEmpty()
                                           from u in t.DefaultIfEmpty()
                                           where m.PRType == type && u.reviewerId == UserId
                                           select new PRListTable()
                                           {
                                               PRId = m.PRId,
                                               PRNo = m.PRNo,
                                               PRDate = m.PreparedDate,
                                               RequestorName = n.firstName + " " + n.lastName,
                                               VendorCompany = r.name,
                                               AmountRequired = m.AmountRequired,
                                               //PRAging = m.aging,
                                               Status = p.status
                                           }).ToList();
                }
                else if (ifHOD != null)
                {
                    PRList.PRListObject = (from m in db.PurchaseRequisitions
                                           join n in db.Users on m.PreparedById equals n.userId
                                           join o in db.Vendors on m.VendorId equals o.vendorId into q
                                           join p in db.PRStatus on m.StatusId equals p.statusId
                                           join s in db.PR_HOD on m.PRId equals s.PRId into t
                                           from r in q.DefaultIfEmpty()
                                           from u in t.DefaultIfEmpty()
                                           where m.PRType == type && u.HODId == UserId
                                           select new PRListTable()
                                           {
                                               PRId = m.PRId,
                                               PRNo = m.PRNo,
                                               PRDate = m.PreparedDate,
                                               RequestorName = n.firstName + " " + n.lastName,
                                               VendorCompany = r.name,
                                               AmountRequired = m.AmountRequired,
                                               //PRAging = m.aging,
                                               Status = p.status
                                           }).ToList();
                }
                else if (ifHOC != null || ifHOGPSS != null)
                {
                    List<PRListTable> HOCList = new List<PRListTable>();
                    HOCList = (from m in db.PurchaseRequisitions
                                           join n in db.Users on m.PreparedById equals n.userId
                                           join o in db.Vendors on m.VendorId equals o.vendorId into q
                                           join p in db.PRStatus on m.StatusId equals p.statusId
                                           join s in db.PR_Approver on m.PRId equals s.PRId into t
                                           from r in q.DefaultIfEmpty()
                                           from u in t.DefaultIfEmpty()
                                           where m.PRType == type && u.approverId == UserId
                                           select new PRListTable()
                                           {
                                               PRId = m.PRId,
                                               PRNo = m.PRNo,
                                               PRDate = m.PreparedDate,
                                               RequestorName = n.firstName + " " + n.lastName,
                                               VendorCompany = r.name,
                                               AmountRequired = m.AmountRequired,
                                               //PRAging = m.aging,
                                               Status = p.status
                                           }).ToList();
                    if (ifHOGPSS != null)
                    {
                        PRList.PRListObject = HOCList.Concat(from m in db.PurchaseRequisitions
                                                      join n in db.Users on m.PreparedById equals n.userId
                                                      join o in db.Vendors on m.VendorId equals o.vendorId into q
                                                      join p in db.PRStatus on m.StatusId equals p.statusId
                                                      join s in db.PR_PaperApprover on m.PRId equals s.PRId into t
                                                      from r in q.DefaultIfEmpty()
                                                      from u in t.DefaultIfEmpty()
                                                      where m.PRType == type && u.approverId == UserId
                                                      select new PRListTable()
                                                      {
                                                          PRId = m.PRId,
                                                          PRNo = m.PRNo,
                                                          PRDate = m.PreparedDate,
                                                          RequestorName = n.firstName + " " + n.lastName,
                                                          VendorCompany = r.name,
                                                          AmountRequired = m.AmountRequired,
                                                          //PRAging = m.aging,
                                                          Status = p.status
                                                      }).ToList();
                    } else { PRList.PRListObject = HOCList; }
                }
                else if (ifProcurement != null)
                {
                    PRList.PRListObject = (from m in db.PurchaseRequisitions
                                           join n in db.Users on m.PreparedById equals n.userId
                                           join o in db.Vendors on m.VendorId equals o.vendorId into q
                                           join p in db.PRStatus on m.StatusId equals p.statusId
                                           from r in q.DefaultIfEmpty()
                                           where m.PRType == type && p.statusId == "PR02"
                                           select new PRListTable()
                                           {
                                               PRId = m.PRId,
                                               PRNo = m.PRNo,
                                               PRDate = m.PreparedDate,
                                               RequestorName = n.firstName + " " + n.lastName,
                                               VendorCompany = r.name,
                                               AmountRequired = m.AmountRequired,
                                               //PRAging = m.aging,
                                               Status = p.status
                                           }).ToList();
                }
                else if (ifAdmin != null)
                {
                    PRList.PRListObject = (from m in db.PurchaseRequisitions
                                           join n in db.Users on m.PreparedById equals n.userId
                                           join o in db.Vendors on m.VendorId equals o.vendorId into q
                                           join p in db.PRStatus on m.StatusId equals p.statusId
                                           join s in db.PR_Admin on m.PRId equals s.PRId into t
                                           from r in q.DefaultIfEmpty()
                                           from u in t.DefaultIfEmpty()
                                           where m.PRType == type && u.adminId == UserId
                                           select new PRListTable()
                                           {
                                               PRId = m.PRId,
                                               PRNo = m.PRNo,
                                               PRDate = m.PreparedDate,
                                               RequestorName = n.firstName + " " + n.lastName,
                                               VendorCompany = r.name,
                                               AmountRequired = m.AmountRequired,
                                               //PRAging = m.aging,
                                               Status = p.status
                                           }).ToList();
                }
                PRList.Type = type;

                return View("PRList", PRList);
            }
            else
            {
                return Redirect("~/Home/Index");
            }
        }

        [HttpPost]
        public JsonResult ViewPRTabs(int PRId, string PRType)
        {
            Session["PRId"] = PRId; Session["PRType"] = PRType;
            var redirectUrl = new UrlHelper(Request.RequestContext).Action("PRTabs", "PR", new { /* no params */ });
            return Json(new { success = true, url = redirectUrl });
        }
        public ActionResult PRTabs()
        {
            if (User.Identity.IsAuthenticated && Session["UserId"] != null)
            {
                PRModel PRTabs = new PRModel
                {
                    PRId = Int32.Parse(Session["PRId"].ToString()),
                    UserId = Int32.Parse(Session["UserId"].ToString()),
                    Type = Session["PRType"].ToString()
                };
                PRTabs.NewPRForm = (from m in db.PurchaseRequisitions
                                    where m.PRId == PRTabs.PRId
                                    select new NewPRModel()
                                    {
                                        PRNo = m.PRNo
                                    }).FirstOrDefault();

                return View(PRTabs);
            }
            else
            {
                return Redirect("~/Home/Index");
            }
        }
        public ActionResult PRDetails()
        {
            if (User.Identity.IsAuthenticated && Session["UserId"] != null)
            {
                int CustId = Int32.Parse(Session["CompanyId"].ToString());
                var ProjectNameQuery = (from m in db.Projects
                                        where m.custId == CustId
                                        select new
                                        {
                                            ProjectId = m.projectId,
                                            Dimension = m.dimension,
                                            Description = m.dimension + " - " + m.projectCode + " - " + m.projectName,
                                            Code = m.projectCode,
                                        }).OrderBy(c => c.Dimension).ThenBy(c => c.Code);
                List<SelectListItem> BudgetedList = new List<SelectListItem>();
                BudgetedList.Add(new SelectListItem
                {
                    Text = "Yes",
                    Value = true.ToString()
                });
                BudgetedList.Add(new SelectListItem
                {
                    Text = "No",
                    Value = false.ToString()
                });
                var PurchaseTypeQuery = db.PurchaseTypes.Select(c => new { PurchaseTypeId = c.purchaseTypeId, PurchaseType = c.purchaseType1 }).OrderBy(c => c.PurchaseType);
                var VendorListQuery = db.Vendors.Select(c => new { VendorId = c.vendorId, VendorName = c.name }).OrderBy(c => c.VendorName);
                var VendorStaffQuery = db.VendorStaffs.Select(c => new { StaffId = c.staffId, VendorContactName = c.vendorContactName }).OrderBy(c => c.VendorContactName);
                var ItemCodeListQuery = db.ItemCodes.Select(c => new { CodeId = c.codeId, ItemCode = c.ItemCode1 }).OrderBy(c => c.ItemCode);
                PRModel PRDetail = new PRModel
                {
                    PRId = Int32.Parse(Session["PRId"].ToString()),
                    UserId = Int32.Parse(Session["UserId"].ToString()),
                    Type = Session["PRType"].ToString()
                    //RoleIdList = getRole
                };
                var ReviewerNameQuery = (from m in db.Users
                                         join n in db.PR_Reviewer on m.userId equals n.reviewerId
                                         where n.PRId == PRDetail.PRId
                                         select new NewPRModel()
                                         {
                                             ReviewerId = m.userId,
                                             ReviewerName = m.firstName + " " + m.lastName
                                         }).FirstOrDefault();
                var HODApproverNameQuery = (from m in db.Users
                                         join n in db.PR_HOD on m.userId equals n.HODId
                                            where n.PRId == PRDetail.PRId
                                            select new NewPRModel()
                                         {
                                             HODApproverId = n.HODId,
                                             HODApproverName = m.firstName + " " + m.lastName
                                         }).FirstOrDefault();
                //var ApproverNameQuery = (from m in db.Users
                //                         join n in db.Users_Roles on m.userId equals n.userId
                //                         where n.roleId == "R04" && m.jobTitle.Trim() != "HOD"
                //                         select new NewPRModel()
                //                         {
                //                             ApproverId = m.userId,
                //                             ApproverName = m.firstName + " " + m.lastName
                //                         }).FirstOrDefault();
                //var RecommenderNameQuery = (from m in db.Users
                //                            join n in db.Users_Roles on m.userId equals n.userId
                //                            where n.roleId == "R04"
                //                            select new NewPRModel()
                //                            {
                //                                RecommenderId = m.userId,
                //                                RecommenderName = m.firstName + " " + m.lastName
                //                            }).FirstOrDefault();
                
                var getRole = (from m in db.Users
                               join n in db.Users_Roles on m.userId equals n.userId
                               where m.userId == PRDetail.UserId
                               select new RoleList()
                               {
                                   RoleId = n.roleId,
                                   RoleName = n.Role.name
                               }).ToList();
                
                PRDetail.NewPRForm = (from a in db.PurchaseRequisitions
                                             join b in db.Users on a.PreparedById equals b.userId into s
                                             join c in db.PRStatus on a.StatusId equals c.statusId
                                             join d in db.Projects on a.ProjectId equals d.projectId
                                             join e in db.Vendors on a.VendorId equals e.vendorId into f
                                             join g in db.VendorStaffs on a.VendorStaffId equals g.staffId into l
                                             join h in db.PR_Items on a.PRId equals h.PRId
                                             join j in db.ItemCodes on h.codeId equals j.codeId into k
                                             from w in l.DefaultIfEmpty()
                                             from x in s.DefaultIfEmpty()
                                             from y in f.DefaultIfEmpty()
                                             from z in k.DefaultIfEmpty()
                                                where a.PRId == PRDetail.PRId
                                             select new NewPRModel()
                                             {
                                                 PRNo = a.PRNo,
                                                 ProjectId = a.ProjectId,
                                                 //PaperRefNo = a.PaperRefNo,
                                                 Value = a.Unbudgeted,
                                                 VendorId = a.VendorId,
                                                 VendorEmail = w.vendorEmail,
                                                 VendorStaffId = w.staffId,
                                                 VendorContactNo = w.vendorContactNo,
                                                 PurchaseTypeId = a.PurchaseTypeId,
                                                 BudgetDescription = a.BudgetDescription,
                                                 BudgetedAmount = d.budgetedAmount,
                                                 Justification = a.Justification,
                                                 UtilizedToDate = d.utilizedToDate,
                                                 AmountRequired = a.AmountRequired,
                                                 BudgetBalance = d.budgetBalance,
                                                 PaperAttachment = a.PaperAttachment,
                                                 PaperVerified = d.paperVerified,
                                                 PreparedById = a.PreparedById,
                                                 PreparedDate = a.PreparedDate,
                                                 //HODApproverId = a.HODApproverId,
                                                 //ApproverId = a.ApproverId,
                                                 //ReviewerId = a.ReviewerId,
                                                 //RecommenderId = a.RecommenderId,
                                                 Saved = a.Saved,
                                                 Submited = a.Submited,
                                                 Rejected = a.Rejected,
                                                 StatusId = a.StatusId.Trim(),
                                                 PRStatus = c.status
                                             }).FirstOrDefault();
                PRDetail.NewPRForm.PRItemListObject = (from m in db.PR_Items
                                                       join n in db.ItemCodes on m.codeId equals n.codeId into o
                                                       from p in o.DefaultIfEmpty()
                                                 where m.PRId == PRDetail.PRId
                                                 select new PRItemsTable()
                                                 {
                                                     ItemsId = m.itemsId,
                                                     DateRequired = m.dateRequired,
                                                     Description = m.description,
                                                     CodeId = p.codeId,
                                                     CustPONo = m.custPONo,
                                                     Quantity = m.quantity,
                                                     UnitPrice = m.unitPrice,
                                                     TotalPrice = m.totalPrice,
                                                     UOM = p.UoM
                                                 }).ToList();

                
                //Session["Details_POIssueDate"] = PODetailList.POListDetail.Details_POIssueDate;
                //Session["Details_Update"] = PODetailList.POListDetail.Details_Update;

                ViewBag.ProjectNameList = new SelectList(ProjectNameQuery.AsEnumerable(), "projectId", "Description", PRDetail.NewPRForm.ProjectId);
                ViewBag.BudgetedList = new SelectList(BudgetedList.AsEnumerable(), "Value", "Text", PRDetail.NewPRForm.Value);
                ViewBag.PurchaseTypeList = new SelectList(PurchaseTypeQuery.AsEnumerable(), "purchaseTypeId", "purchaseType", PRDetail.NewPRForm.PurchaseTypeId);
                //ViewBag.ReviewerNameList = new SelectList(ReviewerNameQuery.AsEnumerable(), "reviewerId", "reviewerName", PRDetail.NewPRForm.ReviewerId);
                //ViewBag.ApproverNameList = new SelectList(ApproverNameQuery.AsEnumerable(), "approverId", "approverName", PRDetail.NewPRForm.ApproverId);
                ViewBag.VendorList = new SelectList(VendorListQuery.AsEnumerable(), "vendorId", "VendorName", PRDetail.NewPRForm.VendorId);
                ViewBag.VendorStaffList = new SelectList(VendorStaffQuery.AsEnumerable(), "staffId", "VendorContactName", PRDetail.NewPRForm.VendorStaffId);
                int i = 1;
                foreach (var item in PRDetail.NewPRForm.PRItemListObject)
                {
                    ViewData["ItemCodeList" + i] = new SelectList(ItemCodeListQuery.AsEnumerable(), "codeId", "itemCode",item.CodeId);
                    i++;
                }

                return PartialView(PRDetail);
            }
            else
            {
                return Redirect("~/Home/Index");
            }
        }

        [HttpPost]
        public ActionResult PRDetails(PRModel PRModel)
        {
            if (User.Identity.IsAuthenticated && Session["UserId"] != null)
            {
                int CustId = Int32.Parse(Session["CompanyId"].ToString());
                var ProjectNameQuery = (from m in db.Projects
                                        where m.custId == CustId
                                        select new
                                        {
                                            ProjectId = m.projectId,
                                            Dimension = m.dimension,
                                            Description = m.dimension + " - " + m.projectCode + " - " + m.projectName,
                                            Code = m.projectCode,
                                        }).OrderBy(c => c.Dimension).ThenBy(c => c.Code);
                List<SelectListItem> BudgetedList = new List<SelectListItem>();
                BudgetedList.Add(new SelectListItem
                {
                    Text = "Yes",
                    Value = true.ToString()
                });
                BudgetedList.Add(new SelectListItem
                {
                    Text = "No",
                    Value = false.ToString()
                });
                var PurchaseTypeQuery = db.PurchaseTypes.Select(c => new { PurchaseTypeId = c.purchaseTypeId, PurchaseType = c.purchaseType1 }).OrderBy(c => c.PurchaseType);
                var VendorListQuery = db.Vendors.Select(c => new { VendorId = c.vendorId, VendorName = c.name }).OrderBy(c => c.VendorName);
                var VendorStaffQuery = db.VendorStaffs.Select(c => new { StaffId = c.staffId, VendorContactName = c.vendorContactName }).OrderBy(c => c.VendorContactName);
                var ItemCodeListQuery = db.ItemCodes.Select(c => new { CodeId = c.codeId, ItemCode = c.ItemCode1 }).OrderBy(c => c.ItemCode);
                var ReviewerNameQuery = (from m in db.Users
                                         join n in db.PR_Reviewer on m.userId equals n.reviewerId
                                         where n.PRId == PRModel.PRId
                                         select new NewPRModel()
                                         {
                                             ReviewerId = m.userId,
                                             ReviewerName = m.firstName + " " + m.lastName
                                         }).FirstOrDefault();
                var HODApproverNameQuery = (from m in db.Users
                                            join n in db.PR_HOD on m.userId equals n.HODId
                                            where n.PRId == PRModel.PRId
                                            select new NewPRModel()
                                            {
                                                HODApproverId = m.userId,
                                                HODApproverName = m.firstName + " " + m.lastName
                                            }).FirstOrDefault();
                var ApproverNameQuery = (from m in db.Users
                                         join n in db.Users_Roles on m.userId equals n.userId
                                         where n.roleId == "R04"
                                         select new NewPRModel()
                                         {
                                             ApproverId = m.userId,
                                             ApproverName = m.firstName + " " + m.lastName
                                         }).FirstOrDefault();
                var RecommenderNameQuery = (from m in db.Users
                                            join n in db.Users_Roles on m.userId equals n.userId
                                            where n.roleId == "R03"
                                            select new NewPRModel()
                                            {
                                                RecommenderId = m.userId,
                                                RecommenderName = m.firstName + " " + m.lastName
                                            }).FirstOrDefault();
                int i = 1;
                if (!ModelState.IsValid)
                {
                    ViewBag.ProjectNameList = new SelectList(ProjectNameQuery.AsEnumerable(), "projectId", "Description", PRModel.NewPRForm.ProjectId);
                    ViewBag.BudgetedList = new SelectList(BudgetedList.AsEnumerable(), "Value", "Text", PRModel.NewPRForm.Value);
                    ViewBag.PurchaseTypeList = new SelectList(PurchaseTypeQuery.AsEnumerable(), "purchaseTypeId", "purchaseType", PRModel.NewPRForm.PurchaseTypeId);
                    //ViewBag.ReviewerNameList = new SelectList(ReviewerNameQuery.AsEnumerable(), "reviewerId", "reviewerName", PRModel.NewPRForm.ReviewerId);
                    //ViewBag.ApproverNameList = new SelectList(ApproverNameQuery.AsEnumerable(), "approverId", "approverName", PRModel.NewPRForm.ApproverId);
                    ViewBag.VendorList = new SelectList(VendorListQuery.AsEnumerable(), "vendorId", "VendorName", PRModel.NewPRForm.VendorId);
                    ViewBag.VendorStaffList = new SelectList(VendorStaffQuery.AsEnumerable(), "staffId", "VendorContactName", PRModel.NewPRForm.VendorStaffId);

                    foreach (var item in PRModel.NewPRForm.PRItemListObject)
                    {
                        ViewData["ItemCodeList" + i] = new SelectList(ItemCodeListQuery.AsEnumerable(), "codeId", "itemCode", item.CodeId);
                        i++;
                    }
                    return new JsonResult
                    {
                        Data = new
                        {
                            success = false,
                            exception = true,
                            type = PRModel.Type,
                            PRId = PRModel.PRId,
                            Saved = PRModel.NewPRForm.SelectSave,
                            Submited = PRModel.NewPRForm.SelectSubmit,
                            view = this.RenderPartialView("PRDetails", PRModel)
                        },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                PRModel.UserId = Int32.Parse(Session["UserId"].ToString());
                PRModel.FullName = Session["FullName"].ToString();
                //var getRole = (from m in db.Users
                //               join n in db.Users_Roles on m.userId equals n.userId
                //               where m.userId == UserId
                //               select new RoleList()
                //               {
                //                   RoleId = n.roleId,
                //                   RoleName = n.Role.name
                //               }).ToList();
                DBLogicController.PRUpdateDbLogic(PRModel);

                bool checkPaperVerified = db.Projects.FirstOrDefault(m => m.projectId == PRModel.NewPRForm.ProjectId).paperVerified;
                FileUpload fileUploadModel = new FileUpload();
                FileUpload fileUploadModel1 = new FileUpload();
                DateTime createDate = DateTime.Now;

                if (PRModel.PaperRefNoFile != null && PRModel.NewPRForm.PaperRefNo != null)
                {
                    string PaperRefNoFileFilename = PRModel.PaperRefNoFile.FileName.Replace("\\", ",");
                    string filename = PaperRefNoFileFilename.Split(',')[PaperRefNoFileFilename.Split(',').Length - 1].ToString();
                    string fullPath = PRModel.PaperRefNoFile.FileName;
                    string extension = Path.GetExtension(fullPath);
                    byte[] uploadFile = new byte[PRModel.PaperRefNoFile.InputStream.Length];

                    fileUploadModel.uuid = Guid.NewGuid();
                    fileUploadModel.uploadUserId = Int32.Parse(Session["UserId"].ToString());
                    fileUploadModel.uploadDate = createDate;
                    fileUploadModel.FullPath = fullPath;
                    fileUploadModel.FileName = filename;
                    fileUploadModel.Extension = extension;
                    fileUploadModel.archivedFlag = false;
                    PRModel.PaperRefNoFile.InputStream.Read(uploadFile, 0, uploadFile.Length);
                    fileUploadModel.File = uploadFile;
                    db.FileUploads.Add(fileUploadModel);
                    db.SaveChanges();

                    PRs_FileUpload fileUploadModel2 = new PRs_FileUpload
                    {
                        uuid = Guid.NewGuid(),
                        fileEntryId = fileUploadModel.fileEntryId,
                        PRId = PRModel.PRId
                    };
                    db.PRs_FileUpload.Add(fileUploadModel2);
                    db.SaveChanges();

                    NotificationMsg _objSendMessage = new NotificationMsg
                    {
                        uuid = Guid.NewGuid(),
                        PRId = PRModel.PRId,
                        msgDate = DateTime.Now,
                        fromUserId = Int32.Parse(Session["UserId"].ToString()),
                        message = Session["FullName"].ToString() + " has uploaded " + filename,
                        msgType = "Trail"
                    };
                    db.NotificationMsgs.Add(_objSendMessage);
                }
                if (PRModel.NewPRForm.BidWaiverRefNo == null && PRModel.BidWaiverRefNoFile != null)
                {
                    string BidWaiverRefNoFile = PRModel.BidWaiverRefNoFile.FileName.Replace("\\", ",");
                    string filename1 = BidWaiverRefNoFile.Split(',')[BidWaiverRefNoFile.Split(',').Length - 1].ToString();
                    string fullPath1 = PRModel.BidWaiverRefNoFile.FileName;
                    string extension1 = Path.GetExtension(fullPath1);
                    byte[] uploadFile1 = new byte[PRModel.BidWaiverRefNoFile.InputStream.Length];

                    fileUploadModel1.uuid = Guid.NewGuid();
                    fileUploadModel1.uploadUserId = Int32.Parse(Session["UserId"].ToString());
                    fileUploadModel1.uploadDate = createDate;
                    fileUploadModel1.FullPath = fullPath1;
                    fileUploadModel1.FileName = filename1;
                    fileUploadModel1.Extension = extension1;
                    fileUploadModel1.archivedFlag = false;
                    PRModel.BidWaiverRefNoFile.InputStream.Read(uploadFile1, 0, uploadFile1.Length);
                    fileUploadModel1.File = uploadFile1;
                    db.FileUploads.Add(fileUploadModel1);
                    db.SaveChanges();

                    PRs_FileUpload fileUploadModel3 = new PRs_FileUpload
                    {
                        uuid = Guid.NewGuid(),
                        fileEntryId = fileUploadModel1.fileEntryId,
                        PRId = PRModel.PRId
                    };

                    db.PRs_FileUpload.Add(fileUploadModel3);
                    db.SaveChanges();

                    NotificationMsg _objSendMessage1 = new NotificationMsg
                    {
                        uuid = Guid.NewGuid(),
                        PRId = PRModel.PRId,
                        msgDate = DateTime.Now,
                        fromUserId = Int32.Parse(Session["UserId"].ToString()),
                        message = Session["FullName"].ToString() + " has uploaded " + filename1,
                        msgType = "Trail"
                    };
                    db.NotificationMsgs.Add(_objSendMessage1);
                    db.SaveChanges();
                }

                ViewBag.ProjectNameList = new SelectList(ProjectNameQuery.AsEnumerable(), "projectId", "Description", PRModel.NewPRForm.ProjectId);
                ViewBag.BudgetedList = new SelectList(BudgetedList.AsEnumerable(), "Value", "Text", PRModel.NewPRForm.Value);
                ViewBag.PurchaseTypeList = new SelectList(PurchaseTypeQuery.AsEnumerable(), "purchaseTypeId", "purchaseType", PRModel.NewPRForm.PurchaseTypeId);
                //ViewBag.ReviewerNameList = new SelectList(ReviewerNameQuery.AsEnumerable(), "reviewerId", "reviewerName", PRModel.NewPRForm.ReviewerId);
                //ViewBag.ApproverNameList = new SelectList(ApproverNameQuery.AsEnumerable(), "approverId", "approverName", PRModel.NewPRForm.ApproverId);
                ViewBag.VendorList = new SelectList(VendorListQuery.AsEnumerable(), "vendorId", "VendorName", PRModel.NewPRForm.VendorId);
                ViewBag.VendorStaffList = new SelectList(VendorStaffQuery.AsEnumerable(), "staffId", "VendorContactName", PRModel.NewPRForm.VendorStaffId);
                foreach (var item in PRModel.NewPRForm.PRItemListObject)
                {
                    ViewData["ItemCodeList" + i] = new SelectList(ItemCodeListQuery.AsEnumerable(), "codeId", "itemCode", item.CodeId);
                    i++;
                }

                return new JsonResult
                {
                    Data = new
                    {
                        success = true,
                        exception = false,
                        type = PRModel.Type,
                        PRId = PRModel.PRId,
                        Saved = PRModel.NewPRForm.SelectSave,
                        Submited = PRModel.NewPRForm.SelectSubmit,
                        view = this.RenderPartialView("PRDetails", PRModel)
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            else
            {
                return Redirect("~/Home/Index");
            }
        }
        public ActionResult PRConversations()
        {
            NotiModel NewNotiList = new NotiModel();
            int userId = Int32.Parse(Session["UserId"].ToString());
            int PRId = Int32.Parse(Session["PRId"].ToString());
            //var haha = db.NotificationMsgs.ToList();
            NewNotiList.NotiListObject = (from m in db.NotificationMsgs
                                          join n in db.Users on m.fromUserId equals n.userId
                                          join o in db.NotiGroups on m.msgId equals o.msgId into p           
                                          join r in db.PurchaseRequisitions on m.PRId equals r.PRId into s
                                          from q in p.DefaultIfEmpty()
                                          from t in s.DefaultIfEmpty()
                                          join u in db.Users on q.toUserId equals u.userId into v
                                          from w in v.DefaultIfEmpty()
                                          where t.PRId == PRId
                                          select new NotiListTable()
                                          {
                                              MsgId = m.msgId,
                                              Message = m.message,
                                              PRId = m.PRId,
                                              //RequestorName = n.firstName + " " + n.lastName,
                                              POId = m.POId,
                                              FromUserId = m.fromUserId,
                                              FromFullName = n.firstName + " " + n.lastName,
                                              ToUserId = q.toUserId,
                                              ToFullName = w.firstName + " " + w.lastName,
                                              MsgDate = m.msgDate,
                                              PRType = t.PRType,
                                              Done = m.done
                                          }).ToList();

            return PartialView(NewNotiList);
        }
        public ActionResult PRFiles()
        {
            if (User.Identity.IsAuthenticated && Session["UserId"] != null)
            {
                PRModel model = new PRModel();
                int userid = Int32.Parse(Session["UserId"].ToString());
                //model.RoleId = Session["RoleId"].ToString();
                model.PRId = Int32.Parse(Session["PRId"].ToString());
                model.PRDocListObject = (from m in db.PurchaseRequisitions
                                         join n in db.PRs_FileUpload on m.PRId equals n.PRId
                                         join o in db.FileUploads on n.fileEntryId equals o.fileEntryId
                                           join p in db.Users on o.uploadUserId equals p.userId
                                         where m.PRId == model.PRId
                                         select new PRDocListTable()
                                           {
                                               fileEntryId = o.fileEntryId,
                                               uploadUserName = p.firstName + " " + p.lastName,
                                               uploadDate = o.uploadDate,
                                               FileName = o.FileName,
                                               File = o.File,
                                               Extension = o.Extension
                                           }).ToList();
                return PartialView(model);
            }
            else
            {
                return Redirect("~/Home/Index");
            }
            
        }
        public FileContentResult FileDownload(int id)
        {
            byte[] fileData;

            FileUpload fileRecord = db.FileUploads.Find(id);

            string fileName;
            string filename1 = fileRecord.FileName.Replace("\\", ",");
            string filename2 = filename1.Split(',')[filename1.Split(',').Length - 1].ToString();
            fileData = (byte[])fileRecord.File.ToArray();
            fileName = filename2;

            return File(fileData, "text", fileName);
        }

        #region Approval & Rejected Phase 1
        [HttpPost]
        //public ActionResult Approval(int PRId, string PRType, bool Reviewer, bool Approver)
        public ActionResult Approval(int PRId, string PRType, string Approver)
        {
            if (User.Identity.IsAuthenticated && Session["UserId"] != null)
            {
                PurchaseRequisition objPRDetails = db.PurchaseRequisitions.First(m => m.PRId == PRId);
                var objPRItemList = db.PR_Items.ToList().Where(m => m.PRId == PRId);
                int UserId = Int32.Parse(Session["UserId"].ToString());
                var requestorDone = (from m in db.NotificationMsgs
                                     join n in db.NotiGroups on m.msgId equals n.msgId
                                     where n.toUserId == UserId && m.PRId == PRId
                                     select new PRModel()
                                     {
                                         MsgId = m.msgId
                                     }).First();

                //if (Reviewer == true)
                //{
                //    PR_Reviewer objGetReviewer = db.PR_Reviewer.First(m => m.PRId == PRId && m.reviewerId == UserId);
                //    objGetReviewer.reviewed = objGetReviewer.reviewed + 1;
                //    objGetReviewer.reviewedDate = DateTime.Now;
                //    objPRDetails.StatusId = "PR05";

                //    NotificationMsg objNotification = new NotificationMsg()
                //    {
                //        uuid = Guid.NewGuid(),
                //        message = Session["FullName"].ToString() + " has reviewed the PR No: " + objPRDetails.PRNo + " application. ",
                //        fromUserId = UserId,
                //        msgDate = DateTime.Now,
                //        msgType = "Trail",
                //        PRId = PRId
                //    };
                //    db.NotificationMsgs.Add(objNotification);
                //    db.SaveChanges();

                //    NotificationMsg getDone = db.NotificationMsgs.First(m => m.msgId == requestorDone.MsgId);
                //    getDone.done = true;

                //    if (objPRDetails.AmountRequired < 20000)
                //    {
                //        var getHODApprover = db.PR_HOD.Select(m => new { HODId = m.HODId, PRId = m.PRId }).Where(m => m.PRId == PRId).ToList();
                //        foreach (var item in getHODApprover)
                //        {
                //            NotificationMsg objTask = new NotificationMsg()
                //            {
                //                uuid = Guid.NewGuid(),
                //                message = objPRDetails.PRNo + " pending for your approval",
                //                fromUserId = UserId,
                //                msgDate = DateTime.Now,
                //                msgType = "Task",
                //                PRId = PRId
                //            };
                //            db.NotificationMsgs.Add(objTask);

                //            NotiGroup Task = new NotiGroup()
                //            {
                //                uuid = Guid.NewGuid(),
                //                msgId = objTask.msgId,
                //                toUserId = item.HODId
                //            };
                //            db.NotiGroups.Add(Task);
                //            db.SaveChanges();
                //        }
                //    } else
                //    {
                //        var getApprover = db.PR_Approver.Select(m => new { ApproverId = m.approverId, PRId = m.PRId }).Where(m => m.PRId == PRId).ToList();
                //        foreach (var item in getApprover)
                //        {
                //            NotificationMsg objTask = new NotificationMsg()
                //            {
                //                uuid = Guid.NewGuid(),
                //                message = objPRDetails.PRNo + " pending for your approval",
                //                fromUserId = UserId,
                //                msgDate = DateTime.Now,
                //                msgType = "Task",
                //                PRId = PRId
                //            };
                //            db.NotificationMsgs.Add(objTask);

                //            NotiGroup Task = new NotiGroup()
                //            {
                //                uuid = Guid.NewGuid(),
                //                msgId = objTask.msgId,
                //                toUserId = item.ApproverId
                //            };
                //            db.NotiGroups.Add(Task);
                //            db.SaveChanges();
                //        }
                //    }
                //}
                Project updateProject = db.Projects.First(m => m.projectId == objPRDetails.ProjectId);

                if (Approver == "HOD")
                {                    
                    if (Session["ifHOD"] != null && objPRDetails.Phase1Completed == false && objPRDetails.PaperAttachment == false)
                    {
                        PR_HOD getApprover = db.PR_HOD.First(m => m.PRId == PRId && m.HODId == UserId);
                        getApprover.HODApprovedP1 = getApprover.HODApprovedP1 + 1;
                        getApprover.HODApprovedDate1 = DateTime.Now;
                        objPRDetails.StatusId = "PR02";
                        objPRDetails.Phase1Completed = true;

                        NotificationMsg objNotification = new NotificationMsg()
                        {
                            uuid = Guid.NewGuid(),
                            message = Session["FullName"].ToString() + " has approved the PR No: " + objPRDetails.PRNo + " application. ",
                            fromUserId = Int32.Parse(Session["UserId"].ToString()),
                            msgDate = DateTime.Now,
                            msgType = "Trail",
                            PRId = PRId
                        };
                        db.NotificationMsgs.Add(objNotification);
                        db.SaveChanges();

                        NotificationMsg getDone = db.NotificationMsgs.First(m => m.msgId == requestorDone.MsgId);
                        getDone.done = true;

                        var getAdmin = (from m in db.Users
                                        join n in db.Users_Roles on m.userId equals n.userId
                                        where n.roleId == "R03"
                                        select new PRModel()
                                        {
                                            UserId = m.userId
                                        }).ToList();
                        NotificationMsg objTask = new NotificationMsg()
                        {
                            uuid = Guid.NewGuid(),
                            message = objPRDetails.PRNo + " pending for you to procure",
                            fromUserId = Int32.Parse(Session["UserId"].ToString()),
                            msgDate = DateTime.Now,
                            msgType = "Task",
                            PRId = PRId
                        };
                        db.NotificationMsgs.Add(objTask);
                        db.SaveChanges();

                        foreach (var item in getAdmin)
                        {
                            NotiGroup Task = new NotiGroup()
                            {
                                uuid = Guid.NewGuid(),
                                msgId = objTask.msgId,
                                toUserId = item.UserId
                            };
                            db.NotiGroups.Add(Task);
                            db.SaveChanges();
                        }
                    }
                    else if (Session["ifHOD"] != null && objPRDetails.Phase1Completed == false && objPRDetails.PaperAttachment == true)
                    {
                        PR_HOD getApprover = db.PR_HOD.First(m => m.PRId == PRId && m.HODId == UserId);
                        getApprover.HODApprovedP1 = getApprover.HODApprovedP1 + 1;
                        getApprover.HODApprovedDate1 = DateTime.Now;
                        objPRDetails.StatusId = "PR10";
                        objPRDetails.Phase1Completed = false;

                        NotificationMsg objNotification = new NotificationMsg()
                        {
                            uuid = Guid.NewGuid(),
                            message = Session["FullName"].ToString() + " has approved the PR No: " + objPRDetails.PRNo + " application. ",
                            fromUserId = Int32.Parse(Session["UserId"].ToString()),
                            msgDate = DateTime.Now,
                            msgType = "Trail",
                            PRId = PRId
                        };
                        db.NotificationMsgs.Add(objNotification);
                        db.SaveChanges();

                        NotificationMsg getDone = db.NotificationMsgs.First(m => m.msgId == requestorDone.MsgId);
                        getDone.done = true;

                        var getPaperApprover = (from m in db.Users
                                                join n in db.Users_Roles on m.userId equals n.userId
                                                where n.roleId == "R10"
                                                select new PRModel()
                                                {
                                                    UserId = m.userId
                                                }).ToList();
                       
                        NotificationMsg objTask = new NotificationMsg()
                        {
                            uuid = Guid.NewGuid(),
                            message = objPRDetails.PRNo + " pending for you to review the papers",
                            fromUserId = Int32.Parse(Session["UserId"].ToString()),
                            msgDate = DateTime.Now,
                            msgType = "Task",
                            PRId = PRId
                        };
                        db.NotificationMsgs.Add(objTask);
                        db.SaveChanges();

                        foreach (var item in getPaperApprover)
                        {
                            NotiGroup Task = new NotiGroup()
                            {
                                uuid = Guid.NewGuid(),
                                msgId = objTask.msgId,
                                toUserId = item.UserId
                            };
                            db.NotiGroups.Add(Task);
                            db.SaveChanges();

                            PR_PaperApprover _objHOGPSS = new PR_PaperApprover
                            {
                                uuid = Guid.NewGuid(),
                                approverId = item.UserId,
                                PRId = PRId
                            };
                            db.PR_PaperApprover.Add(_objHOGPSS);
                            db.SaveChanges();
                        }
                    }
                }
                else if (Approver == "ifHOGPSS")
                {
                    PR_PaperApprover getApprover = db.PR_PaperApprover.First(m => m.PRId == PRId && m.approverId == UserId);
                    getApprover.approverApproved = getApprover.approverApproved + 1;
                    getApprover.approverApprovedDate = DateTime.Now;
                    objPRDetails.StatusId = "PR02";
                    objPRDetails.Phase1Completed = true;
                    updateProject.paperVerified = true;
                    db.SaveChanges();

                    NotificationMsg objNotification = new NotificationMsg()
                    {
                        uuid = Guid.NewGuid(),
                        message = Session["FullName"].ToString() + " has verified BOD/GMD paper for the PR No: " + objPRDetails.PRNo,
                        fromUserId = Int32.Parse(Session["UserId"].ToString()),
                        msgDate = DateTime.Now,
                        msgType = "Trail",
                        PRId = PRId
                    };
                    db.NotificationMsgs.Add(objNotification);
                    db.SaveChanges();

                    NotificationMsg getDone = db.NotificationMsgs.First(m => m.msgId == requestorDone.MsgId);
                    getDone.done = true;

                    var getAdmin = (from m in db.Users
                                    join n in db.Users_Roles on m.userId equals n.userId
                                    where n.roleId == "R03"
                                    select new PRModel()
                                    {
                                        UserId = m.userId
                                    }).ToList();
                    NotificationMsg objTask = new NotificationMsg()
                    {
                        uuid = Guid.NewGuid(),
                        message = objPRDetails.PRNo + " pending for you to procure",
                        fromUserId = Int32.Parse(Session["UserId"].ToString()),
                        msgDate = DateTime.Now,
                        msgType = "Task",
                        PRId = PRId
                    };
                    db.NotificationMsgs.Add(objTask);
                    db.SaveChanges();

                    foreach (var item in getAdmin)
                    {
                        NotiGroup Task = new NotiGroup()
                        {
                            uuid = Guid.NewGuid(),
                            msgId = objTask.msgId,
                            toUserId = item.UserId
                        };
                        db.NotiGroups.Add(Task);
                        db.SaveChanges();
                    }
                }
                    
                    //else
                    //{
                    //    if (objPRDetails.AmountRequired < 20000)
                    //    {
                    //        PR_HOD getHODApprover = db.PR_HOD.First(m => m.PRId == PRId && m.HODId == UserId);
                    //        getHODApprover.HODApprovedP2 = getHODApprover.HODApprovedP2 + 1;
                    //        getHODApprover.HODApprovedDate2 = DateTime.Now;
                    //    }
                    //    else
                    //    {
                    //        PR_Approver getApprover = db.PR_Approver.First(m => m.PRId == PRId && m.approverId == UserId);
                    //        getApprover.approverApproved = getApprover.approverApproved + 1;
                    //        getApprover.approverApprovedDate = DateTime.Now;
                    //    }
                    //    objPRDetails.StatusId = "PR06";
                    //    objPRDetails.Phase2Completed = true;
                    //    db.SaveChanges();

                    //    NotificationMsg objNotification = new NotificationMsg()
                    //    {
                    //        uuid = Guid.NewGuid(),
                    //        message = Session["FullName"].ToString() + " has approved the PR No: " + objPRDetails.PRNo + " procurement application. ",
                    //        fromUserId = UserId,
                    //        msgDate = DateTime.Now,
                    //        msgType = "Trail",
                    //        PRId = PRId
                    //    };
                    //    db.NotificationMsgs.Add(objNotification);                       

                    //    NotificationMsg getDone = db.NotificationMsgs.First(m => m.msgId == requestorDone.MsgId);
                    //    getDone.done = true;
                    //    db.SaveChanges();                       
                    //}
                return Json(new { success = true });
            }
            else
            {
                return Redirect("~/Home/Index");
            }
        }

        [HttpPost]
        //public ActionResult Rejected(int PRId, string PRType, bool Reviewer, bool Approver, string RejectRemark)
        public ActionResult Rejected(int PRId, string PRType, bool Approver, string RejectRemark)
        {
            if (User.Identity.IsAuthenticated && Session["UserId"] != null)
            {
                PurchaseRequisition objPRDetails = db.PurchaseRequisitions.First(m => m.PRId == PRId);
                int UserId = Int32.Parse(Session["UserId"].ToString());
                objPRDetails.Rejected = objPRDetails.Rejected + 1;
                objPRDetails.RejectedDate = DateTime.Now;
                objPRDetails.RejectedById = UserId;
                objPRDetails.RejectedRemark = RejectRemark;
                objPRDetails.StatusId = "PR07";

                NotificationMsg objNotification = new NotificationMsg()
                {
                    uuid = Guid.NewGuid(),
                    message = Session["FullName"].ToString() + " has rejected the PR No: " + objPRDetails.PRNo + " application. ",
                    fromUserId = UserId,
                    msgDate = DateTime.Now,
                    msgType = "Trail",
                    PRId = PRId
                };
                db.NotificationMsgs.Add(objNotification);

                var getRequestor = (from m in db.Users
                                    join n in db.Users_Roles on m.userId equals n.userId
                                    join o in db.Roles on n.roleId equals o.roleId
                                    join p in db.PurchaseRequisitions on m.userId equals p.PreparedById
                                    where p.PRId == PRId
                                    select new PRModel()
                                    {
                                        UserId = m.userId
                                    }).First();
                NotificationMsg objTask = new NotificationMsg()
                {
                    uuid = Guid.NewGuid(),
                    message = objPRDetails.PRNo + " has been rejected. Click to resubmit.",
                    fromUserId = Int32.Parse(Session["UserId"].ToString()),
                    msgDate = DateTime.Now,
                    msgType = "Task",
                    PRId = PRId
                };
                db.NotificationMsgs.Add(objTask);
                db.SaveChanges();

                NotiGroup Task = new NotiGroup()
                {
                    uuid = Guid.NewGuid(),
                    msgId = objTask.msgId,
                    toUserId = getRequestor.UserId
                };
                db.NotiGroups.Add(Task);
                db.SaveChanges();

                return Json(new { success = true });
            }
            else
            {
                return Redirect("~/Home/Index");
            }
        }
        #endregion
        public ActionResult PRForm()
        {
            if (User.Identity.IsAuthenticated && Session["UserId"] != null)
            {
                PRModel PRDetail = new PRModel
                {
                    PRId = Int32.Parse(Session["PRId"].ToString()),
                    UserId = Int32.Parse(Session["UserId"].ToString()),
                    Type = Session["PRType"].ToString()
                    //RoleIdList = getRole
                };
                PurchaseRequisition getAmount = db.PurchaseRequisitions.First(m => m.PRId == PRDetail.PRId);
                if (getAmount.AmountRequired < 20000)
                {
                    PRDetail.NewPRForm = (from a in db.PurchaseRequisitions
                                          join b in db.Users on a.PreparedById equals b.userId
                                          join c in db.PRStatus on a.StatusId equals c.statusId
                                          join d in db.Projects on a.ProjectId equals d.projectId
                                          join e in db.Vendors on a.VendorId equals e.vendorId into f
                                          join g in db.VendorStaffs on a.VendorStaffId equals g.staffId into l
                                          join h in db.PR_Items on a.PRId equals h.PRId
                                          join j in db.ItemCodes on h.codeId equals j.codeId into k
                                          join m in db.PurchaseTypes on a.PurchaseTypeId equals m.purchaseTypeId
                                          join n in db.Customers on b.companyId equals n.custId
                                          join s in db.PR_HOD on a.PRId equals s.PRId
                                          join t in db.Users on s.HODId equals t.userId
                                          from x in l.DefaultIfEmpty()
                                          from y in f.DefaultIfEmpty()
                                          from z in k.DefaultIfEmpty()
                                          where a.PRId == PRDetail.PRId
                                          select new NewPRModel()
                                          {
                                              CompanyName = n.name,
                                              RequestorName = b.firstName + " " + b.lastName,
                                              Designation = b.jobTitle,
                                              //DivDept = b.
                                              PRNo = a.PRNo,
                                              ProjectId = a.ProjectId,
                                              PaperRefNo = a.PaperRefNo,
                                              Unbudgeted = a.Unbudgeted,
                                              VendorId = a.VendorId,
                                              VendorName = y.name,
                                              VendorEmail = x.vendorEmail,
                                              VendorStaffId = x.staffId,
                                              VendorContactName = x.vendorContactName,
                                              VendorContactNo = x.vendorContactNo,
                                              PurchaseTypeId = a.PurchaseTypeId,
                                              PurchaseType = m.purchaseType1,
                                              BudgetDescription = a.BudgetDescription,
                                              BudgetedAmount = d.budgetedAmount,
                                              Justification = a.Justification,
                                              UtilizedToDate = d.utilizedToDate,
                                              AmountRequired = a.AmountRequired,
                                              BudgetBalance = d.budgetBalance,
                                              PreparedById = a.PreparedById,
                                              PreparedDate = a.PreparedDate,
                                              HODApproverId = s.HODId,
                                              HODApproverName = t.firstName + " " + t.lastName,
                                              HODApprovedDate1 = s.HODApprovedDate1.Value,
                                              HODApprovedDate2 = s.HODApprovedDate2.Value,
                                              Saved = a.Saved,
                                              Submited = a.Submited,
                                              Rejected = a.Rejected,
                                              StatusId = a.StatusId.Trim()
                                          }).FirstOrDefault();
                    PR_Reviewer getReviewer = db.PR_Reviewer.First(m => m.PRId == PRDetail.PRId && m.reviewed == 1);
                    PRDetail.NewPRForm.ReviewerId = getReviewer.reviewerId;

                    User getReviewerName = db.Users.First(m => m.userId == getReviewer.reviewerId);
                    PRDetail.NewPRForm.ReviewerName = getReviewerName.firstName + " " + getReviewerName.lastName;
                    PRDetail.NewPRForm.ApproverName = PRDetail.NewPRForm.HODApproverName;
                } else
                {
                    PRDetail.NewPRForm = (from a in db.PurchaseRequisitions
                                          join b in db.Users on a.PreparedById equals b.userId
                                          join c in db.PRStatus on a.StatusId equals c.statusId
                                          join d in db.Projects on a.ProjectId equals d.projectId
                                          join e in db.Vendors on a.VendorId equals e.vendorId into f
                                          join g in db.VendorStaffs on a.VendorStaffId equals g.staffId into l
                                          join h in db.PR_Items on a.PRId equals h.PRId
                                          join j in db.ItemCodes on h.codeId equals j.codeId into k
                                          join m in db.PurchaseTypes on a.PurchaseTypeId equals m.purchaseTypeId
                                          join n in db.Customers on b.companyId equals n.custId
                                          join o in db.PR_Reviewer on a.PRId equals o.PRId into p
                                          join q in db.PR_Approver on a.PRId equals q.PRId into r
                                          join s in db.PR_HOD on a.PRId equals s.PRId
                                          join t in db.Users on s.HODId equals t.userId
                                          join u in db.PR_Recommender on a.PRId equals u.PRId into aa
                                          from v in r.DefaultIfEmpty()
                                          from w in p.DefaultIfEmpty()
                                          from x in l.DefaultIfEmpty()
                                          from y in f.DefaultIfEmpty()
                                          from z in k.DefaultIfEmpty()
                                          from ab in aa.DefaultIfEmpty()
                                          where a.PRId == PRDetail.PRId
                                          select new NewPRModel()
                                          {
                                              CompanyName = n.name,
                                              RequestorName = b.firstName + " " + b.lastName,
                                              Designation = b.jobTitle,
                                              //DivDept = b.
                                              PRNo = a.PRNo,
                                              ProjectId = a.ProjectId,
                                              PaperRefNo = a.PaperRefNo,
                                              Unbudgeted = a.Unbudgeted,
                                              VendorId = a.VendorId,
                                              VendorName = y.name,
                                              VendorEmail = x.vendorEmail,
                                              VendorStaffId = x.staffId,
                                              VendorContactName = x.vendorContactName,
                                              VendorContactNo = x.vendorContactNo,
                                              PurchaseTypeId = a.PurchaseTypeId,
                                              PurchaseType = m.purchaseType1,
                                              BudgetDescription = a.BudgetDescription,
                                              BudgetedAmount = d.budgetedAmount,
                                              Justification = a.Justification,
                                              UtilizedToDate = d.utilizedToDate,
                                              AmountRequired = a.AmountRequired,
                                              BudgetBalance = d.budgetBalance,
                                              PreparedById = a.PreparedById,
                                              PreparedDate = a.PreparedDate,
                                              HODApproverId = s.HODId,
                                              HODApproverName = t.firstName + " " + t.lastName,
                                              HODApprovedDate1 = s.HODApprovedDate1.Value,
                                              HODApprovedDate2 = s.HODApprovedDate2.Value,
                                              ApproverId = v.approverId,
                                              ApprovedDate = v.approverApprovedDate.Value,
                                              ReviewerId = w.reviewerId,
                                              ReviewedDate = w.reviewedDate.Value,
                                              RecommenderId = ab.recommenderId,
                                              RecommendDate = ab.recommendedDate.Value,
                                              Saved = a.Saved,
                                              Submited = a.Submited,
                                              Rejected = a.Rejected,
                                              StatusId = a.StatusId.Trim()
                                          }).FirstOrDefault();
                    if (PRDetail.NewPRForm.ApproverId != 0)
                    {
                        User getFullname = db.Users.First(m => m.userId == PRDetail.NewPRForm.ApproverId);
                        PRDetail.NewPRForm.ApproverName = getFullname.firstName + " " + getFullname.lastName;
                    }
                    if (PRDetail.NewPRForm.ReviewerId != 0)
                    {
                        User getFullname = db.Users.First(m => m.userId == PRDetail.NewPRForm.ReviewerId);
                        PRDetail.NewPRForm.ReviewerName = getFullname.firstName + " " + getFullname.lastName;
                    }
                    if (PRDetail.NewPRForm.RecommenderId != 0)
                    {
                        User getFullname = db.Users.First(m => m.userId == PRDetail.NewPRForm.RecommenderId);
                        PRDetail.NewPRForm.RecommenderName = getFullname.firstName + " " + getFullname.lastName;
                    }
                }

                PRDetail.NewPRForm.PRItemListObject = (from m in db.PR_Items
                                                       join n in db.ItemCodes on m.codeId equals n.codeId into o
                                                       from p in o.DefaultIfEmpty()
                                                       where m.PRId == PRDetail.PRId
                                                       select new PRItemsTable()
                                                       {
                                                           ItemsId = m.itemsId,
                                                           DateRequired = m.dateRequired,
                                                           Description = m.description,
                                                           CodeId = p.codeId,
                                                           CustPONo = m.custPONo,
                                                           Quantity = m.quantity,
                                                           UnitPrice = m.unitPrice,
                                                           TotalPrice = m.totalPrice,
                                                           UOM = p.UoM
                                                       }).ToList();

                return new RazorPDF.PdfActionResult("PRForm", PRDetail);
            }
            else
            {
                return Redirect("~/Home/Index");
            }
        }

        [HttpPost]
        public JsonResult UpdatePRProcurement()
        {
            int PrId = Int32.Parse(Request["PrId"]);
            String SpecsReviewer = Request["SpecsReviewer"];

            var PR = db.PurchaseRequisitions.First(x => x.PRId == PrId);

            //Wait until Hassan add SpecReviewer table
            //PR.SpecsReviewer = SpecsReviewer;

            if (db.SaveChanges() > 0)
            {
                return Json("Sucessfully update");
            }
            else {
                return Json("Something is wrong");
            }
            
        }

        public JsonResult SubmitPRProcurement()
        {
            // todo check session if login as procurement or admin only

            int PrId = Int32.Parse(Request["PrId"]); int UserId = Int32.Parse(Session["UserId"].ToString());
            var getFullName = db.Users.First(m => m.userId == UserId);
            String SpecsReviewer = Request["SpecsReviewer"];

            var PR = db.PurchaseRequisitions.First(x => x.PRId == PrId);
            //Wait until Hassan add SpecReviewer table
            //PR.SpecsReviewer = SpecsReviewer;
            db.SaveChanges();

            Debug.WriteLine("SubmitPRProcurement");
            decimal amountBudget = PR.AmountRequired;

            if (PRService.CheckAmountScenarioOne(amountBudget) && PRService.IncludeInBudget(PR))
            {
                PR.StatusId = "PR03";
                db.SaveChanges();
                // send notifications to requestor
                NotificationMsg _objSubmited = new NotificationMsg
                {
                    uuid = Guid.NewGuid(),
                    PRId = PrId,
                    msgDate = DateTime.Now,
                    fromUserId = UserId,
                    msgType = "Trail",
                    message = getFullName.firstName + " " + getFullName.lastName + " has submit PR application for PR No. " + PR.PRNo + " subject for reviewal"
                };
                db.NotificationMsgs.Add(_objSubmited);

                // send notifications to hod

                // send notifications to reviewer
                var getReviewer = (from m in db.PurchaseRequisitions
                                   join n in db.PR_Reviewer on m.PRId equals n.PRId
                                   //join o in db.Roles on n.roleId equals o.roleId
                                   where m.PRId == PrId
                                   select new PRModel()
                                   {
                                       UserId = n.reviewerId
                                   }).ToList();
                NotificationMsg objTask = new NotificationMsg()
                {
                    uuid = Guid.NewGuid(),
                    message = PR.PRNo + " pending for your reviewal",
                    fromUserId = UserId,
                    msgDate = DateTime.Now,
                    msgType = "Task",
                    PRId = PrId
                };
                db.NotificationMsgs.Add(objTask);
                db.SaveChanges();

                foreach (var item in getReviewer)
                {
                    NotiGroup ReviewerTask = new NotiGroup()
                    {
                        uuid = Guid.NewGuid(),
                        msgId = objTask.msgId,
                        toUserId = item.UserId,
                        resubmit = false
                    };
                    db.NotiGroups.Add(ReviewerTask);
                    db.SaveChanges();
                }

                return Json("Scenario 1 success");
            }
            else if ((PRService.IncludeInBudget(PR) == false && PRService.CheckAmountScenarioOne(amountBudget)) || PRService.CheckAmountScenarioTwo(amountBudget))
            {
                PR.StatusId = "PR03";
                db.SaveChanges();

                return Json("Scenario 2 success");
            }
            else if (PRService.CheckAmountScenarioThree(amountBudget))
            {
                PR.StatusId = "PR03";
                db.SaveChanges();

                return Json("Scenario 3 success");
            }
            else {
                return Json("Does not fit to any conditions. Please recheck. ");
            }
        }


    }
}