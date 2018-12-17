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
using System.Net.Mail;
using System.Collections;

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

        public JsonResult DashboardPurchaseRequisition()
        {
            var sql = "select count(*) as quantity,Year(SubmitDate) as description from PurchaseRequisition group by Year(SubmitDate)";

            //dummy
            var collections = new List<DashboardPOModel>();
            collections.Add(new DashboardPOModel() { quantity = 22, description = "Open" });
            collections.Add(new DashboardPOModel() { quantity = 7, description = "Close" });
            collections.Add(new DashboardPOModel() { quantity = 38, description = "Close with PO" });
            collections.Add(new DashboardPOModel() { quantity = 5, description = "Cancel" });

            //var collections = db.Database.SqlQuery<DashboardPOModel>(sql);

            return Json(collections, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DashboardPurchaseOrder()
        {
            var sql = "select count(*) as sum_yearly,Year(SubmitDate) as yearly from PurchaseRequisition group by Year(SubmitDate)";

            //dummy
            var collections = new List<DashboardPOModel>();
            collections.Add(new DashboardPOModel() { quantity = 1, description = "Open" });
            collections.Add(new DashboardPOModel() { quantity = 14, description = "Close" });
            collections.Add(new DashboardPOModel() { quantity = 3, description = "Force Close" });
            collections.Add(new DashboardPOModel() { quantity = 20, description = "Release" });

            //var collections = db.Database.SqlQuery<DashboardPOModel>(sql);

            return Json(collections, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DashboardOpenPO()
        {
            var sql = "select count(*) as sum_yearly,Year(SubmitDate) as yearly from PurchaseRequisition group by Year(SubmitDate)";

            //dummy
            var collections = new List<DashboardPOModel>();
            collections.Add(new DashboardPOModel () { quantity = 10, description = "2016" });
            collections.Add(new DashboardPOModel() { quantity = 5, description = "2017" });
            collections.Add(new DashboardPOModel() { quantity = 4, description = "2018" });

            //var collections = db.Database.SqlQuery<DashboardPOModel>(sql);

            return Json( collections ,JsonRequestBehavior.AllowGet);
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
        public JsonResult GetItemTypeInfo(int ItemTypeId, int Selectlistid)
        {
            int CustId = Int32.Parse(Session["CompanyId"].ToString());
            var ItemCodeQuery = db.PopulateItemLists.Select(m => new { CodeId = m.codeId, ItemDescription = m.ItemDescription, ItemTypeId = m.itemTypeId, CustId = m.custId  }).Where(m => m.ItemTypeId == ItemTypeId && m.CustId == CustId).ToList();

            string html = "<select class='CodeId custom-select g-height-40 g-color-black g-color-black--hover text-left g-rounded-20' data-val-required='The CodeId field is required.' id='CodeId" + Selectlistid + "'><option value='' >Select item code here</option>";
            foreach (var item in ItemCodeQuery)
            {
                html += "<option value='" + item.CodeId + "' >" + item.ItemDescription + "</option>";
            }
            html += "</select>";

            return Json(new { html }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetItemCodeInfo(int CodeId)
        {
            int CustId = Int32.Parse(Session["CompanyId"].ToString());
            var ItemCodeInfo = db.PopulateItemLists.Select(m => new PRItemsTable()
            {
                UOM = m.UoM,
                CodeId = m.codeId,
                CustId = m.custId
            }).Where(m => m.CodeId == CodeId && m.CustId == CustId).FirstOrDefault();

            return Json(new { ItemCodeInfo }, JsonRequestBehavior.AllowGet);

        }
        public JsonResult GetJobNoInfo(string JobNoId, int Selectlistid)
        {
            int CustId = Int32.Parse(Session["CompanyId"].ToString());
            var ItemCodeQuery = db.JobTasks.Select(m => new { JobNo = m.jobNo, JobTaskNo = m.jobTaskNo, CustId = m.custId }).Where(m => m.JobNo == JobNoId && m.CustId == CustId).ToList();

            string html = "<select class='JobTaskNoId custom-select g-height-40 g-color-black g-color-black--hover text-left g-rounded-20' data-val-required='The Job Task No field is required.' id='JobTaskNoId" + Selectlistid + "'><option value='' >Select job task no here</option>";
            for(int i = 0; i < ItemCodeQuery.Count; i++)
            {
                html += "<option value='" + i + "' >" + ItemCodeQuery[i].JobTaskNo + "</option>";
            }
            html += "</select>";

            return Json(new { html }, JsonRequestBehavior.AllowGet);
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
            string html = "<select class='custom-select g-height-50 g- g-color-gray-light-v1 g-color-black--hover text-left g-rounded-20' data-val-required='The VendorId field is required.' id='VendorStaffId'><option value='' >Select vendor contact name here</option>";
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
                                            Description = m.dimension + " - " + m.projectCode,
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
                                            Description = m.dimension + " - " + m.projectCode,
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
                    ViewBag.BudgetedList = new SelectList(BudgetedList.AsEnumerable(), "Value", "Text",model.NewPRForm.Budgeted);
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
                    fileUploadModel.description = model.NewPRForm.PaperRefNo;
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
                if (model.NewPRForm.BidWaiverRefNo != null && model.BidWaiverRefNoFile != null)
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
                    fileUploadModel1.description = model.NewPRForm.BidWaiverRefNo;
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
                                           from r in q.DefaultIfEmpty()
                                           from ai in ah.DefaultIfEmpty()
                                           where m.PRType == type 
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
                while (ifAdmin != null)
                {
                    int CustId = Int32.Parse(Session["CompanyId"].ToString());
                    PRList.PRListObject = (from m in db.PurchaseRequisitions
                                           join n in db.Users on m.PreparedById equals n.userId into ah
                                           join o in db.Vendors on m.VendorId equals o.vendorId into q
                                           join p in db.PRStatus on m.StatusId equals p.statusId
                                           from r in q.DefaultIfEmpty()
                                           from ai in ah.DefaultIfEmpty()
                                           where m.PRType == type && ai.companyId == CustId
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
                                           from s in db.PR_Reviewer.Where(x => x.PRId == m.PRId && x.reviewerId == UserId).DefaultIfEmpty()
                                           from t in db.PR_Recommender.Where(x => x.PRId == m.PRId && x.recommenderId == UserId).DefaultIfEmpty()
                                           from r in q.DefaultIfEmpty()
                                           where m.PRType == type
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
                else if (ifHOC != null)
                {
                    PRList.PRListObject = (from m in db.PurchaseRequisitions
                                           join n in db.Users on m.PreparedById equals n.userId
                                           join o in db.Vendors on m.VendorId equals o.vendorId into q
                                           join p in db.PRStatus on m.StatusId equals p.statusId
                                           from s in db.PR_Approver.Where(x => x.PRId == m.PRId && x.approverId == UserId).DefaultIfEmpty()
                                           from t in db.PR_RecommenderII.Where(x => x.PRId == m.PRId && x.recommenderId == UserId).DefaultIfEmpty()
                                           from r in q.DefaultIfEmpty()
                                           where m.PRType == type
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
                                           from s in db.PR_HOD.Where( x=> x.PRId == m.PRId && x.HODId == UserId).DefaultIfEmpty()
                                           from t in db.PR_RecommenderII.Where(x => x.PRId == m.PRId && x.recommenderId == UserId).DefaultIfEmpty()
                                           from r in q.DefaultIfEmpty()
                                           where m.PRType == type
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
                else if (ifHOGPSS != null)
                {
                    PRList.PRListObject = (from m in db.PurchaseRequisitions
                                           join n in db.Users on m.PreparedById equals n.userId
                                           join o in db.Vendors on m.VendorId equals o.vendorId into q
                                           join p in db.PRStatus on m.StatusId equals p.statusId
                                           from s in db.PR_PaperApprover.Where(x => x.PRId == m.PRId && x.approverId == UserId).DefaultIfEmpty()
                                           from t in db.PR_Reviewer.Where(x => x.PRId == m.PRId && x.reviewerId == UserId).DefaultIfEmpty()
                                           from r in q.DefaultIfEmpty()
                                           where m.PRType == type
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
                else if (ifProcurement != null)
                {
                    PRList.PRListObject = (from m in db.PurchaseRequisitions
                                           join n in db.Users on m.PreparedById equals n.userId
                                           join o in db.Vendors on m.VendorId equals o.vendorId into q
                                           join p in db.PRStatus on m.StatusId equals p.statusId
                                           from s in db.PR_Admin.Where(x => x.PRId == m.PRId && x.adminId == UserId).DefaultIfEmpty()
                                           from r in q.DefaultIfEmpty()
                                           where m.PRType == type
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
                                            Description = m.dimension + " - " + m.projectCode,
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
                List<SelectListItem> SpecReviewerList = new List<SelectListItem>();
                SpecReviewerList.Add(new SelectListItem
                {
                    Text = "IT",
                    Value = "IT"
                });
                SpecReviewerList.Add(new SelectListItem
                {
                    Text = "HSE",
                    Value = "HSE"
                });
                SpecReviewerList.Add(new SelectListItem
                {
                    Text = "PMO",
                    Value = "PMO"
                });

                var PurchaseTypeQuery = db.PurchaseTypes.Select(c => new { PurchaseTypeId = c.purchaseTypeId, PurchaseType = c.purchaseType1 }).OrderBy(c => c.PurchaseType);
                var VendorListQuery = (from m in db.Vendors
                                       where m.custId == CustId
                                       select new
                                       {
                                           VendorId = m.vendorId,
                                           VendorName = m.vendorNo + " - " + m.name,
                                           VendorNo = m.vendorNo,
                                       }).OrderBy(c => c.VendorNo).ThenBy(c => c.VendorName);
                var VendorStaffQuery = db.VendorStaffs.Select(c => new { StaffId = c.staffId, VendorContactName = c.vendorContactName }).OrderBy(c => c.VendorContactName);
                var ItemTypeListQuery = db.ItemTypes.Select(m => new { ItemTypeId = m.itemTypeId, ItemType = m.type }).OrderBy(m => m.ItemType);
                var ItemCodeListQuery = db.PopulateItemLists.Select(c => new { CodeId = c.codeId, ItemCode = c.ItemDescription, CustId = c.custId }).Where(c => c.CustId == CustId).OrderBy(c => c.ItemCode);
                var JobNoListQuery = db.Projects.Select(m => new { JobNoId = m.projectId, JobNo = m.projectCode, CustId = m.custId, Dimension = m.dimension }).Where(m => m.CustId == CustId && m.Dimension == "PROJECT").OrderBy(m => m.JobNo);
                var JobTaskListQuery = db.JobTasks.Select(m => new { JobTaskNoId = m.jobNo, JobTaskNo = m.jobTaskNo, CustId = m.custId }).Where(m => m.CustId == CustId).OrderBy(m => m.JobTaskNoId);

                PRModel PRDetail = new PRModel
                {
                    PRId = Int32.Parse(Session["PRId"].ToString()),
                    UserId = Int32.Parse(Session["UserId"].ToString()),
                    Type = Session["PRType"].ToString(),
                    CustId = CustId
                    //RoleIdList = getRole
                };

                int PRCustId = db.PurchaseRequisitions.First(m => m.PRId == PRDetail.PRId).CustId;
                if (Session["ifHOGPSS"] != null || (Session["ifHOC"] != null && CustId == 2))
                {
                    ProjectNameQuery = (from m in db.Projects
                                        where m.custId == PRCustId
                                        select new
                                        {
                                            ProjectId = m.projectId,
                                            Dimension = m.dimension,
                                            Description = m.dimension + " - " + m.projectCode,
                                            Code = m.projectCode,
                                        }).OrderBy(c => c.Dimension).ThenBy(c => c.Code);
                    ItemCodeListQuery = db.PopulateItemLists.Select(c => new { CodeId = c.codeId, ItemCode = c.ItemDescription, CustId = c.custId }).Where(c => c.CustId == PRCustId).OrderBy(c => c.ItemCode);
                    JobNoListQuery = db.Projects.Select(m => new { JobNoId = m.projectId, JobNo = m.projectCode, CustId = m.custId, Dimension = m.dimension }).Where(m => m.CustId == PRCustId && m.Dimension == "PROJECT").OrderBy(m => m.JobNo);
                    JobTaskListQuery = db.JobTasks.Select(m => new { JobTaskNoId = m.jobNo, JobTaskNo = m.jobTaskNo, CustId = m.custId }).Where(m => m.CustId == PRCustId).OrderBy(m => m.JobTaskNoId);
                }

                var getReviewer = (from m in db.Users
                                   join n in db.PR_Reviewer on m.userId equals n.reviewerId
                                   where n.PRId == PRDetail.PRId && m.userId == PRDetail.UserId
                                   select new PRModel()
                                   {
                                       UserId = m.userId
                                   }).FirstOrDefault();
                if (getReviewer != null)
                {
                    PRDetail.PRReviewer = getReviewer.UserId;
                }
                //var HODApproverNameQuery = (from m in db.Users
                //                         join n in db.PR_HOD on m.userId equals n.HODId
                //                            where n.PRId == PRDetail.PRId
                //                            select new NewPRModel()
                //                         {
                //                             HODApproverId = n.HODId,
                //                             HODApproverName = m.firstName + " " + m.lastName
                //                         }).FirstOrDefault();
                //var ApproverNameQuery = (from m in db.Users
                //                         join n in db.Users_Roles on m.userId equals n.userId
                //                         where n.roleId == "R04" && m.jobTitle.Trim() != "HOD"
                //                         select new NewPRModel()
                //                         {
                //                             ApproverId = m.userId,
                //                             ApproverName = m.firstName + " " + m.lastName
                //                         }).FirstOrDefault();
                var getRecommender = (from m in db.Users
                                      join n in db.PR_Recommender on m.userId equals n.recommenderId
                                      where n.PRId == PRDetail.PRId && m.userId == PRDetail.UserId
                                      select new PRModel()
                                      {
                                          UserId = m.userId
                                      }).FirstOrDefault();
                if (getRecommender != null)
                {
                    PRDetail.PRRecommender = getRecommender.UserId;
                }

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
                                      join j in db.PopulateItemLists on h.codeId equals j.codeId into k
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
                                          Value = a.Budgeted,
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
                                          SpecReviewer = a.SpecsReviewer,
                                          Saved = a.Saved,
                                          Submited = a.Submited,
                                          Rejected = a.Rejected,
                                          StatusId = a.StatusId.Trim(),
                                          PRStatus = c.status,
                                          Scenario = a.Scenario
                                      }).FirstOrDefault();

                var checkCodeId = (from m in db.PurchaseRequisitions
                                   join n in db.PR_Items on m.PRId equals n.PRId
                                   where m.PRId == PRDetail.PRId
                                   select new PRItemsTable()
                                   {
                                       CodeId = n.codeId
                                   }).FirstOrDefault();
                //if (PRDetail.NewPRForm.StatusId == "PR01" || PRDetail.NewPRForm.StatusId == "PR02" || PRDetail.NewPRForm.StatusId == "PR09" || PRDetail.NewPRForm.StatusId == "PR10")
                if (checkCodeId.CodeId == null)
                {
                    PRDetail.NewPRForm.PRItemListObject = (from m in db.PR_Items
                                                           where m.PRId == PRDetail.PRId
                                                           select new PRItemsTable()
                                                           {
                                                               ItemsId = m.itemsId,
                                                               DateRequired = m.dateRequired,
                                                               Description = m.description,
                                                               CustPONo = m.custPONo,
                                                               Quantity = m.quantity,
                                                               UnitPrice = m.unitPrice,
                                                               TotalPrice = m.totalPrice
                                                           }).ToList();
                    var QueryNull = db.PopulateItemLists.Select(c => new { CodeId = c.codeId, ItemCode = c.ItemDescription, CustId = c.custId }).Where(c => c.CustId == 0).OrderBy(c => c.ItemCode);
                    int i = 1;
                    //if (PRDetail.NewPRForm.ProjectId != 0)
                    //{
                    //    foreach (var item in PRDetail.NewPRForm.PRItemListObject)
                    //    {
                    //        ViewData["ItemTypeList" + i] = new SelectList(ItemTypeListQuery.AsEnumerable(), "itemTypeId", "itemType");
                    //        ViewData["ItemCodeList" + i] = new SelectList(QueryNull.AsEnumerable(), "codeId", "itemCode");
                    //        ViewData["JobNoList" + i] = new SelectList(JobNoListQuery.AsEnumerable(), "jobNoId", "jobNo");
                    //        ViewData["JobTaskList" + i] = new SelectList(QueryNull.AsEnumerable(), "jobTaskNoId", "jobTaskNo");
                    //        i++;
                    //    }
                    //} else
                    //{
                    JobNoListQuery = db.Projects.Select(m => new { JobNoId = m.projectId, JobNo = m.projectCode, CustId = m.custId, Dimension = m.dimension }).Where(m => m.CustId == CustId && m.JobNoId == PRDetail.NewPRForm.ProjectId).OrderBy(m => m.JobNo);
                    foreach (var item in PRDetail.NewPRForm.PRItemListObject)
                    {
                        ViewData["ItemTypeList" + i] = new SelectList(ItemTypeListQuery.AsEnumerable(), "itemTypeId", "itemType");
                        ViewData["ItemCodeList" + i] = new SelectList(QueryNull.AsEnumerable(), "codeId", "itemCode");
                        ViewData["JobNoList" + i] = new SelectList(JobNoListQuery.AsEnumerable(), "jobNoId", "jobNo");
                        ViewData["JobTaskList" + i] = new SelectList(QueryNull.AsEnumerable(), "jobTaskNoId", "jobTaskNo");
                        i++;
                    }
                    //}                                    
                }
                else if (checkCodeId.CodeId != null)
                {
                    PRDetail.NewPRForm.PRItemListObject = (from m in db.PR_Items
                                                           from n in db.PopulateItemLists.Where(x => m.codeId == x.codeId && m.itemTypeId == x.itemTypeId).DefaultIfEmpty()
                                                           join o in db.Projects on m.jobNo equals o.projectCode into p
                                                           from q in p.DefaultIfEmpty()
                                                           where m.PRId == PRDetail.PRId
                                                           select new PRItemsTable()
                                                           {
                                                               ItemsId = m.itemsId,
                                                               DateRequired = m.dateRequired,
                                                               ItemTypeId = m.itemTypeId,
                                                               CodeId = m.codeId,
                                                               Description = m.description,
                                                               CustPONo = m.custPONo,
                                                               Quantity = m.quantity,
                                                               UOM = n.UoM,
                                                               JobNoId = q.projectId,
                                                               JobTaskNoId = m.jobNo,
                                                               UnitPrice = m.unitPrice,
                                                               TotalPrice = m.totalPrice
                                                           }).ToList();
                    int i = 1;
                    foreach (var item in PRDetail.NewPRForm.PRItemListObject)
                    {
                        ViewData["ItemCodeList" + i] = new SelectList(ItemCodeListQuery.AsEnumerable(), "codeId", "itemCode", item.CodeId);
                        ViewData["ItemTypeList" + i] = new SelectList(ItemTypeListQuery.AsEnumerable(), "itemTypeId", "itemType", item.ItemTypeId);
                        ViewData["JobNoList" + i] = new SelectList(JobNoListQuery.AsEnumerable(), "jobNoId", "jobNo", item.JobNoId);
                        ViewData["JobTaskList" + i] = new SelectList(JobTaskListQuery.AsEnumerable(), "jobTaskNoId", "jobTaskNo", item.JobTaskNoId);
                        i++;
                    }
                }

                //Session["Details_POIssueDate"] = PODetailList.POListDetail.Details_POIssueDate;
                //Session["Details_Update"] = PODetailList.POListDetail.Details_Update;

                ViewBag.ProjectNameList = new SelectList(ProjectNameQuery.AsEnumerable(), "projectId", "Description", PRDetail.NewPRForm.ProjectId);
                ViewBag.BudgetedList = new SelectList(BudgetedList.AsEnumerable(), "Value", "Text", PRDetail.NewPRForm.Value);
                ViewBag.SpecReviewerList = new SelectList(SpecReviewerList.AsEnumerable(), "Value", "Text", PRDetail.NewPRForm.SpecReviewer);
                ViewBag.PurchaseTypeList = new SelectList(PurchaseTypeQuery.AsEnumerable(), "purchaseTypeId", "purchaseType", PRDetail.NewPRForm.PurchaseTypeId);
                //ViewBag.ReviewerNameList = new SelectList(ReviewerNameQuery.AsEnumerable(), "reviewerId", "reviewerName", PRDetail.NewPRForm.ReviewerId);
                //ViewBag.ApproverNameList = new SelectList(ApproverNameQuery.AsEnumerable(), "approverId", "approverName", PRDetail.NewPRForm.ApproverId);
                ViewBag.VendorList = new SelectList(VendorListQuery.AsEnumerable(), "vendorId", "VendorName", PRDetail.NewPRForm.VendorId);
                ViewBag.VendorStaffList = new SelectList(VendorStaffQuery.AsEnumerable(), "staffId", "VendorContactName", PRDetail.NewPRForm.VendorStaffId);

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
                                            Description = m.dimension + " - " + m.projectCode,
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
                List<SelectListItem> SpecReviewerList = new List<SelectListItem>();
                SpecReviewerList.Add(new SelectListItem
                {
                    Text = "IT",
                    Value = "IT"
                });
                SpecReviewerList.Add(new SelectListItem
                {
                    Text = "HSE",
                    Value = "HSE"
                });
                SpecReviewerList.Add(new SelectListItem
                {
                    Text = "PMO",
                    Value = "PMO"
                });
                var PurchaseTypeQuery = db.PurchaseTypes.Select(c => new { PurchaseTypeId = c.purchaseTypeId, PurchaseType = c.purchaseType1 }).OrderBy(c => c.PurchaseType);
                var VendorListQuery = (from m in db.Vendors
                                       where m.custId == CustId
                                       select new
                                       {
                                           VendorId = m.vendorId,
                                           VendorName = m.vendorNo + " - " + m.name,
                                           VendorNo = m.vendorNo,
                                       }).OrderBy(c => c.VendorNo).ThenBy(c => c.VendorName);
                var VendorStaffQuery = db.VendorStaffs.Select(c => new { StaffId = c.staffId, VendorContactName = c.vendorContactName }).OrderBy(c => c.VendorContactName);
                var ItemTypeListQuery = db.ItemTypes.Select(m => new { ItemTypeId = m.itemTypeId, ItemType = m.type }).OrderBy(m => m.ItemType);
                var ItemCodeListQuery = db.PopulateItemLists.Select(c => new { CodeId = c.codeId, ItemCode = c.ItemDescription, CustId = c.custId }).Where(c => c.CustId == CustId).OrderBy(c => c.ItemCode);
                var JobNoListQuery = db.Projects.Select(m => new { JobNoId = m.projectId, JobNo = m.projectCode, CustId = m.custId }).Where(m => m.CustId == CustId).OrderBy(m => m.JobNo);
                var JobTaskListQuery = db.JobTasks.Select(m => new { JobTaskId = m.jobNo, JobTaskNo = m.jobTaskNo, CustId = m.custId }).Where(m => m.CustId == CustId).OrderBy(m => m.JobTaskNo);
                //var ReviewerNameQuery = (from m in db.Users
                //                         join n in db.PR_Reviewer on m.userId equals n.reviewerId
                //                         where n.PRId == PRModel.PRId
                //                         select new NewPRModel()
                //                         {
                //                             ReviewerId = m.userId,
                //                             ReviewerName = m.firstName + " " + m.lastName
                //                         }).FirstOrDefault();
                //var HODApproverNameQuery = (from m in db.Users
                //                            join n in db.PR_HOD on m.userId equals n.HODId
                //                            where n.PRId == PRModel.PRId
                //                            select new NewPRModel()
                //                            {
                //                                HODApproverId = m.userId,
                //                                HODApproverName = m.firstName + " " + m.lastName
                //                            }).FirstOrDefault();
                //var ApproverNameQuery = (from m in db.Users
                //                         join n in db.Users_Roles on m.userId equals n.userId
                //                         where n.roleId == "R04"
                //                         select new NewPRModel()
                //                         {
                //                             ApproverId = m.userId,
                //                             ApproverName = m.firstName + " " + m.lastName
                //                         }).FirstOrDefault();
                //var RecommenderNameQuery = (from m in db.Users
                //                            join n in db.Users_Roles on m.userId equals n.userId
                //                            where n.roleId == "R03"
                //                            select new NewPRModel()
                //                            {
                //                                RecommenderId = m.userId,
                //                                RecommenderName = m.firstName + " " + m.lastName
                //                            }).FirstOrDefault();
                int i = 1;
                if (!ModelState.IsValid)
                {
                    ViewBag.ProjectNameList = new SelectList(ProjectNameQuery.AsEnumerable(), "projectId", "Description", PRModel.NewPRForm.ProjectId);
                    ViewBag.BudgetedList = new SelectList(BudgetedList.AsEnumerable(), "Value", "Text", PRModel.NewPRForm.Value);
                    ViewBag.PurchaseTypeList = new SelectList(PurchaseTypeQuery.AsEnumerable(), "purchaseTypeId", "purchaseType", PRModel.NewPRForm.PurchaseTypeId);
                    ViewBag.SpecReviewerList = new SelectList(SpecReviewerList.AsEnumerable(), "Value", "Text", PRModel.NewPRForm.SpecReviewer);
                    //ViewBag.ReviewerNameList = new SelectList(ReviewerNameQuery.AsEnumerable(), "reviewerId", "reviewerName", PRModel.NewPRForm.ReviewerId);
                    //ViewBag.ApproverNameList = new SelectList(ApproverNameQuery.AsEnumerable(), "approverId", "approverName", PRModel.NewPRForm.ApproverId);
                    ViewBag.VendorList = new SelectList(VendorListQuery.AsEnumerable(), "vendorId", "VendorName", PRModel.NewPRForm.VendorId);
                    ViewBag.VendorStaffList = new SelectList(VendorStaffQuery.AsEnumerable(), "staffId", "VendorContactName", PRModel.NewPRForm.VendorStaffId);

                    foreach (var item in PRModel.NewPRForm.PRItemListObject)
                    {
                        ViewData["ItemCodeList" + i] = new SelectList(ItemCodeListQuery.AsEnumerable(), "codeId", "itemCode", item.CodeId);
                        ViewData["ItemTypeList" + i] = new SelectList(ItemTypeListQuery.AsEnumerable(), "itemTypeId", "itemType", item.ItemTypeId);
                        ViewData["JobNoList" + i] = new SelectList(JobNoListQuery.AsEnumerable(), "jobNoId", "jobNo", item.JobNoId);
                        ViewData["JobTaskList" + i] = new SelectList(JobTaskListQuery.AsEnumerable(), "jobTaskNoId", "jobTaskNo", item.JobTaskNoId);
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

                if (PRModel.NewPRForm.PaperRefNo != null && PRModel.PaperRefNoFile == null)
                {
                    ModelState.AddModelError("PaperRefNoFileName", "GMD paper attachment needed.");
                }
                if (PRModel.NewPRForm.BidWaiverRefNo != null && PRModel.BidWaiverRefNoFile == null)
                {
                    ModelState.AddModelError("BidWaiverRefNoFileName", "Bid Waiver attachment needed");
                }
                if (PRModel.NewPRForm.PaperRefNo == null && PRModel.PaperRefNoFile != null)
                {
                    ModelState.AddModelError("NewPRForm.PaperRefNo", "GMD paper ref no needed.");
                }
                if (PRModel.NewPRForm.BidWaiverRefNo == null && PRModel.BidWaiverRefNoFile != null)
                {
                    ModelState.AddModelError("NewPRForm.BidWaiverRefNo", "Bid Waiver ref no needed");
                }
                if (PRModel.PaperRefNoFile != null && Path.GetExtension(PRModel.PaperRefNoFile.FileName) != ".pdf")
                {
                    ModelState.AddModelError("PaperRefNoFileName", "Only pdf file accepted.");
                }
                if (PRModel.BidWaiverRefNoFile != null && Path.GetExtension(PRModel.BidWaiverRefNoFile.FileName) != ".pdf")
                {
                    ModelState.AddModelError("BidWaiverRefNoFileName", "Only pdf file accepted.");
                }
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
                    fileUploadModel.description = PRModel.NewPRForm.PaperRefNo;
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
                if (PRModel.NewPRForm.BidWaiverRefNo != null && PRModel.BidWaiverRefNoFile != null)
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
                    fileUploadModel1.description = PRModel.NewPRForm.BidWaiverRefNo;
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
                ViewBag.SpecReviewerList = new SelectList(SpecReviewerList.AsEnumerable(), "Value", "Text", PRModel.NewPRForm.SpecReviewer);
                ViewBag.PurchaseTypeList = new SelectList(PurchaseTypeQuery.AsEnumerable(), "purchaseTypeId", "purchaseType", PRModel.NewPRForm.PurchaseTypeId);
                //ViewBag.ReviewerNameList = new SelectList(ReviewerNameQuery.AsEnumerable(), "reviewerId", "reviewerName", PRModel.NewPRForm.ReviewerId);
                //ViewBag.ApproverNameList = new SelectList(ApproverNameQuery.AsEnumerable(), "approverId", "approverName", PRModel.NewPRForm.ApproverId);
                ViewBag.VendorList = new SelectList(VendorListQuery.AsEnumerable(), "vendorId", "VendorName", PRModel.NewPRForm.VendorId);
                ViewBag.VendorStaffList = new SelectList(VendorStaffQuery.AsEnumerable(), "staffId", "VendorContactName", PRModel.NewPRForm.VendorStaffId);
                foreach (var item in PRModel.NewPRForm.PRItemListObject)
                {
                    ViewData["ItemCodeList" + i] = new SelectList(ItemCodeListQuery.AsEnumerable(), "codeId", "itemCode", item.CodeId);
                    ViewData["ItemTypeList" + i] = new SelectList(ItemTypeListQuery.AsEnumerable(), "itemTypeId", "itemType", item.ItemTypeId);
                    ViewData["JobNoList" + i] = new SelectList(JobNoListQuery.AsEnumerable(), "jobNoId", "jobNo", item.JobNoId);
                    ViewData["JobTaskList" + i] = new SelectList(JobTaskListQuery.AsEnumerable(), "jobTaskNoId", "jobTaskNo", item.JobTaskNoId);
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
            int userId = Int32.Parse(Session["UserId"].ToString());
            int PRId = Int32.Parse(Session["PRId"].ToString());
            int CustId = Int32.Parse(Session["CompanyId"].ToString());

            NotiModel NewNotiList = new NotiModel()
            {
                MessageListModel = DBLogicController.getMessageList(PRId, userId)
            };           
            //var haha = db.NotificationMsgs.ToList();
            var NotiMemberList = (from m in db.Users
                                  join n in db.Customers on m.companyId equals n.custId into gy
                                  from x in gy.DefaultIfEmpty()
                                      //where m.status == true && m.companyId == CustId
                                  where m.companyId == CustId
                                  select new NotiMemberList()
                                  {
                                      Id = m.userId,
                                      Name = m.firstName + " " + m.lastName
                                  }).ToList().OrderBy(c => c.Name);

            ViewBag.NotiMemberList = new MultiSelectList(NotiMemberList, "Id", "Name");

            return PartialView(NewNotiList);
        }

        [HttpPost]
        public JsonResult SendMessages(int PRId, string Message, List<int> SelectedUserId)
        {
            int fromUserID = Int32.Parse(Session["UserId"].ToString());
            int custId = Int32.Parse(Session["CompanyId"].ToString());
            var myEmail = db.Users.SingleOrDefault(x => x.userId == fromUserID);

            NotificationMsg _objSendMessage = new NotificationMsg
            {
                uuid = Guid.NewGuid(),
                PRId = PRId,
                msgDate = DateTime.Now,
                fromUserId = fromUserID,
                message = Message,
                msgType = "Message"
                //flag = 0
            };
            db.NotificationMsgs.Add(_objSendMessage);
            db.SaveChanges();

            foreach (var model in SelectedUserId)
            {
                //toUserId = Int32.Parse(((string[])(selectedUserId))[i].Split(',')[0]);
                //var UserEmail = db.Users.SingleOrDefault(x => x.userId == model);
                //var POInfo = (from m in db.PurchaseRequisitions
                //              join n in db.Users on m.PreparedById equals n.userId
                //              join o in db.Projects on m.ProjectId equals o.projectId
                //              join p in db.Customers on o.custId equals p.custId
                //              //join p in db.OpportunityTypes on m.typeId equals p.typeId
                //              where m.PRId == PRId
                //              select new MailModel()
                //              {
                //                  CustName = p.name + " (" + p.abbreviation + ")",
                //                  CustAbbreviation = p.abbreviation,
                //                  ContractName = o.name,
                //                  Description = o.description,
                //                  PODescription = m.Details_projectDescription,
                //                  PONo = m.Details_PONo,
                //                  //ContractType = p.type,
                //                  AssignTo = n.firstName + " " + n.lastName
                //              }).FirstOrDefault();

                //string templateFile = System.IO.File.ReadAllText(HttpContext.Server.MapPath("~/Views/Shared/POEmailTemplate.cshtml"));
                //POInfo.Content = Content;
                //POInfo.FromName = myEmail.emailAddress;
                //POInfo.BackLink = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority,
                //    Url.Content("~/POList/ViewPO") + "?cId=" + cId + "&pId=" + pId + "#POQuestionAnswer-1");
                //var result = Engine.Razor.RunCompile(new LoadedTemplateSource(templateFile), "POTemplateKey", null, POInfo);

                //MailMessage mail = new MailMessage
                //{
                //    From = new MailAddress("hr@kubtel.com")
                //};
                ////mail.From = new MailAddress("hr@kubtel.com");
                //mail.To.Add(new MailAddress(UserEmail.emailAddress));
                ////mail.To.Add(new MailAddress("hr@kubtel.com"));
                //mail.Subject = "OCM: Updates on " + POInfo.ContractName + " for " + POInfo.CustAbbreviation;
                //mail.Body = result;
                //mail.IsBodyHtml = true;
                //SmtpClient smtp = new SmtpClient
                //{
                //    //smtp.Host = "pops.kub.com";
                //    Host = "outlook.office365.com",
                //    //smtp.Host = "smtp.gmail.com";
                //    Port = 587,
                //    //smtp.Port = 25;
                //    EnableSsl = true,
                //    Credentials = new System.Net.NetworkCredential("hr@kubtel.com", "welcome123$")
                //};
                ////smtp.Credentials = new System.Net.NetworkCredential("ocm@kubtel.com", "welcome123$");
                //smtp.Send(mail);

                NotiGroup _objSendToUserId = new NotiGroup
                {
                    uuid = Guid.NewGuid(),
                    msgId = _objSendMessage.msgId,
                    toUserId = model
                };
                db.NotiGroups.Add(_objSendToUserId);
            }

            if (db.SaveChanges() > 0)
            {
                return Json("Sucessfully update");
            }
            else
            {
                return Json("Something is wrong");
            }
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
                    if (Session["ifHOD"] != null && objPRDetails.Phase1Completed == false && (objPRDetails.PaperAttachment == false || updateProject.paperVerified == true))
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
        public ActionResult Rejected(int PRId, string Approver, string RejectRemark)
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

                var requestorDone = (from m in db.NotificationMsgs
                                     join n in db.NotiGroups on m.msgId equals n.msgId
                                     where n.toUserId == UserId && m.PRId == PRId
                                     select new PRModel()
                                     {
                                         MsgId = m.msgId
                                     }).First();

                NotificationMsg getDone = db.NotificationMsgs.First(m => m.msgId == requestorDone.MsgId);
                getDone.done = true;
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
                int CustId = Int32.Parse(Session["CompanyId"].ToString());
                PRModel PRDetail = new PRModel
                {
                    PRId = Int32.Parse(Session["PRId"].ToString()),
                    UserId = Int32.Parse(Session["UserId"].ToString()),
                    Type = Session["PRType"].ToString()
                    //RoleIdList = getRole
                };
                PurchaseRequisition getScenario = db.PurchaseRequisitions.First(m => m.PRId == PRDetail.PRId);
                if (getScenario.Scenario == 1)
                {
                    PRDetail.NewPRForm = (from a in db.PurchaseRequisitions
                                          join b in db.Users on a.PreparedById equals b.userId
                                          join c in db.PRStatus on a.StatusId equals c.statusId
                                          join d in db.Projects on a.ProjectId equals d.projectId
                                          join e in db.Vendors on a.VendorId equals e.vendorId into f
                                          //join g in db.VendorStaffs on a.VendorStaffId equals g.staffId into l
                                          join h in db.PR_Items on a.PRId equals h.PRId
                                          join j in db.PopulateItemLists on h.codeId equals j.codeId into k
                                          join m in db.PurchaseTypes on a.PurchaseTypeId equals m.purchaseTypeId
                                          join n in db.Customers on b.companyId equals n.custId
                                          join s in db.PR_HOD on a.PRId equals s.PRId
                                          join t in db.Users on s.HODId equals t.userId
                                          //from x in l.DefaultIfEmpty()
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
                                              Budgeted = a.Budgeted,
                                              VendorId = a.VendorId,
                                              VendorName = y.name,
                                              //VendorEmail = x.vendorEmail,
                                              //VendorStaffId = x.staffId,
                                              //VendorContactName = x.vendorContactName,
                                              //VendorContactNo = x.vendorContactNo,
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
                                              //HODApprovedDate2 = s.HODApprovedDate2.Value,
                                              Saved = a.Saved,
                                              Submited = a.Submited,
                                              Rejected = a.Rejected,
                                              Scenario = a.Scenario,
                                              StatusId = a.StatusId.Trim()
                                          }).FirstOrDefault();
                    PR_Reviewer getReviewer = db.PR_Reviewer.First(m => m.PRId == PRDetail.PRId && m.reviewed == 1);
                    PRDetail.NewPRForm.ReviewerId = getReviewer.reviewerId;
                    User getReviewerName = db.Users.First(m => m.userId == getReviewer.reviewerId);
                    PRDetail.NewPRForm.ReviewerName = getReviewerName.firstName + " " + getReviewerName.lastName;

                    PR_Approver getApprover = db.PR_Approver.First(m => m.PRId == PRDetail.PRId && m.approverApproved == 1);
                    PRDetail.NewPRForm.ApproverId = getApprover.approverId;
                    User getApproverName = db.Users.First(m => m.userId == getApprover.approverId);
                    PRDetail.NewPRForm.ApproverName = getApproverName.firstName + " " + getApproverName.lastName;
                } else if (getScenario.Scenario == 2)
                {
                    PRDetail.NewPRForm = (from a in db.PurchaseRequisitions
                                          join b in db.Users on a.PreparedById equals b.userId
                                          join c in db.PRStatus on a.StatusId equals c.statusId
                                          join d in db.Projects on a.ProjectId equals d.projectId
                                          join e in db.Vendors on a.VendorId equals e.vendorId into f
                                          //join g in db.VendorStaffs on a.VendorStaffId equals g.staffId into l
                                          join h in db.PR_Items on a.PRId equals h.PRId
                                          join j in db.PopulateItemLists on h.codeId equals j.codeId into k
                                          join m in db.PurchaseTypes on a.PurchaseTypeId equals m.purchaseTypeId
                                          join n in db.Customers on b.companyId equals n.custId
                                          join o in db.PR_Reviewer on a.PRId equals o.PRId into p
                                          join q in db.PR_Approver on a.PRId equals q.PRId into r
                                          join s in db.PR_HOD on a.PRId equals s.PRId
                                          join t in db.Users on s.HODId equals t.userId
                                          join u in db.PR_Recommender on a.PRId equals u.PRId into aa
                                          from v in r.DefaultIfEmpty()
                                          from w in p.DefaultIfEmpty()
                                          //from x in l.DefaultIfEmpty()
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
                                              Budgeted = a.Budgeted,
                                              VendorId = a.VendorId,
                                              VendorName = y.name,
                                              //VendorEmail = x.vendorEmail,
                                              //VendorStaffId = x.staffId,
                                              //VendorContactName = x.vendorContactName,
                                              //VendorContactNo = x.vendorContactNo,
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
                                              //HODApprovedDate2 = s.HODApprovedDate2.Value,
                                              ApproverId = v.approverId,
                                              ApprovedDate = v.approverApprovedDate.Value,
                                              ReviewerId = w.reviewerId,
                                              ReviewedDate = w.reviewedDate.Value,
                                              RecommenderId = ab.recommenderId,
                                              RecommendDate = ab.recommendedDate.Value,
                                              Saved = a.Saved,
                                              Submited = a.Submited,
                                              Rejected = a.Rejected,
                                              Scenario = a.Scenario,
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
                 else
                {
                    PRDetail.NewPRForm = (from a in db.PurchaseRequisitions
                                          join b in db.Users on a.PreparedById equals b.userId
                                          join c in db.PRStatus on a.StatusId equals c.statusId
                                          join d in db.Projects on a.ProjectId equals d.projectId
                                          join e in db.Vendors on a.VendorId equals e.vendorId into f
                                          //join g in db.VendorStaffs on a.VendorStaffId equals g.staffId into l
                                          join h in db.PR_Items on a.PRId equals h.PRId
                                          join j in db.PopulateItemLists on h.codeId equals j.codeId into k
                                          join m in db.PurchaseTypes on a.PurchaseTypeId equals m.purchaseTypeId
                                          join n in db.Customers on b.companyId equals n.custId
                                          join o in db.PR_Reviewer on a.PRId equals o.PRId into p
                                          join q in db.PR_Approver on a.PRId equals q.PRId into r
                                          join s in db.PR_HOD on a.PRId equals s.PRId
                                          join t in db.Users on s.HODId equals t.userId
                                          join u in db.PR_Recommender on a.PRId equals u.PRId into aa
                                          join ac in db.PR_RecommenderII on a.PRId equals ac.PRId
                                          join ad in db.Users on ac.recommenderId equals ad.userId
                                          from v in r.DefaultIfEmpty()
                                          from w in p.DefaultIfEmpty()
                                              //from x in l.DefaultIfEmpty()
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
                                              Budgeted = a.Budgeted,
                                              VendorId = a.VendorId,
                                              VendorName = y.name,
                                              //VendorEmail = x.vendorEmail,
                                              //VendorStaffId = x.staffId,
                                              //VendorContactName = x.vendorContactName,
                                              //VendorContactNo = x.vendorContactNo,
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
                                              //HODApprovedDate2 = s.HODApprovedDate2.Value,
                                              ApproverId = v.approverId,
                                              ApprovedDate = v.approverApprovedDate.Value,
                                              ReviewerId = w.reviewerId,
                                              ReviewedDate = w.reviewedDate.Value,
                                              RecommenderId = ab.recommenderId,
                                              RecommendDate = ab.recommendedDate.Value,
                                              RecommenderIdII = ac.recommenderId,
                                              RecommenderNameII = ad.firstName + " " + ad.lastName,
                                              RecommendDateII = ac.recommendedDate.Value,
                                              Saved = a.Saved,
                                              Submited = a.Submited,
                                              Rejected = a.Rejected,
                                              Scenario = a.Scenario,
                                              StatusId = a.StatusId.Trim()
                                          }).FirstOrDefault();
                }

                PRDetail.NewPRForm.PRItemListObject = (from m in db.PR_Items
                                                       from n in db.PopulateItemLists.Where(x => m.codeId == x.codeId && m.itemTypeId == x.itemTypeId)
                                                       //from p in o.DefaultIfEmpty()
                                                       where m.PRId == PRDetail.PRId && n.custId == CustId
                                                       select new PRItemsTable()
                                                       {
                                                           ItemsId = m.itemsId,
                                                           DateRequired = m.dateRequired,
                                                           Description = m.description,
                                                           CodeId = n.codeId,
                                                           CustPONo = m.custPONo,
                                                           Quantity = m.quantity,
                                                           UnitPrice = m.unitPrice,
                                                           TotalPrice = m.totalPrice,
                                                           UOM = n.UoM
                                                       }).ToList();

                return new RazorPDF.PdfActionResult("PRForm", PRDetail);
            }
            else
            {
                return Redirect("~/Home/Index");
            }
        }

        [HttpPost]
        public JsonResult UpdatePRProcurement(PRModel PR)
        {
            var FormerPRDetails = db.PurchaseRequisitions.First(x => x.PRId == PR.PRId);
            PR.FullName = Session["FullName"].ToString();
            if (FormerPRDetails.SpecsReviewer != PR.NewPRForm.SpecReviewer)
            {
                if (FormerPRDetails.SpecsReviewer != null)
                {
                    NotificationMsg _objDetails_SpecsReviewer = new NotificationMsg
                    {
                        uuid = Guid.NewGuid(),
                        PRId = PR.PRId,
                        msgDate = DateTime.Now,
                        fromUserId = PR.UserId,
                        msgType = "Trail",
                        message = PR.FullName + " change Specs Reviewer value from " + FormerPRDetails.SpecsReviewer + " to " + PR.NewPRForm.SpecReviewer + " for PR No: " + FormerPRDetails.PRNo
                    };
                    db.NotificationMsgs.Add(_objDetails_SpecsReviewer);
                }                
                FormerPRDetails.SpecsReviewer = PR.NewPRForm.SpecReviewer;
            }
            if (FormerPRDetails.AmountRequired != PR.NewPRForm.AmountRequired)
            {
                if (FormerPRDetails.AmountRequired != 0)
                {
                    NotificationMsg _objDetails_AmountRequired = new NotificationMsg
                    {
                        uuid = Guid.NewGuid(),
                        PRId = PR.PRId,
                        msgDate = DateTime.Now,
                        fromUserId = PR.UserId,
                        msgType = "Trail",
                        message = PR.FullName + " change Amount Required from " + FormerPRDetails.AmountRequired + " to " + PR.NewPRForm.AmountRequired + " for PR No: " + FormerPRDetails.PRNo
                    };
                    db.NotificationMsgs.Add(_objDetails_AmountRequired);
                }               
                FormerPRDetails.AmountRequired = PR.NewPRForm.AmountRequired;
            }
            if (FormerPRDetails.VendorId != PR.NewPRForm.VendorId)
            {
                if (FormerPRDetails.VendorId != null)
                {
                    var FormerVendorName = db.Vendors.Select(m => new { VendorName = m.name, VendorId = m.vendorId }).First(m => m.VendorId == FormerPRDetails.VendorId);
                    var NewVendorName = db.Vendors.Select(m => new { VendorName = m.name, VendorId = m.vendorId }).First(m => m.VendorId == PR.NewPRForm.VendorId);
                    NotificationMsg _objDetails_VendorId = new NotificationMsg
                    {
                        uuid = Guid.NewGuid(),
                        PRId = PR.PRId,
                        msgDate = DateTime.Now,
                        fromUserId = PR.UserId,
                        msgType = "Trail",
                        message = PR.FullName + " change Vendor Name from " + FormerVendorName.VendorName + " to " + NewVendorName.VendorName + " for PR No: " + FormerPRDetails.PRNo
                    };
                    db.NotificationMsgs.Add(_objDetails_VendorId);
                }                              
                FormerPRDetails.VendorId = PR.NewPRForm.VendorId;
            }
            db.SaveChanges();

            foreach (var value in PR.NewPRForm.PRItemListObject)
            {
                PR_Items FormerPRItem = db.PR_Items.FirstOrDefault(m => m.itemsId == value.ItemsId);
                if (FormerPRItem.itemTypeId != value.ItemTypeId)
                {
                    if (FormerPRItem.itemTypeId != null)
                    {
                        var FormerItemType = db.ItemTypes.Select(m => new { ItemType = m.Items, ItemTypeId = m.itemTypeId }).First(m => m.ItemTypeId == FormerPRItem.itemTypeId);
                        var NewItemType = db.ItemTypes.Select(m => new { ItemType = m.Items, ItemTypeId = m.itemTypeId }).First(m => m.ItemTypeId == value.ItemTypeId);
                        NotificationMsg _objDetails_ItemTypeId = new NotificationMsg
                        {
                            uuid = Guid.NewGuid(),
                            PRId = PR.PRId,
                            msgDate = DateTime.Now,
                            fromUserId = PR.UserId,
                            msgType = "Trail",
                            message = PR.FullName + " change Item Type from " + FormerItemType.ItemType + " to " + NewItemType.ItemType + " for PR No: " + FormerPRDetails.PRNo
                        };
                        db.NotificationMsgs.Add(_objDetails_ItemTypeId);
                    }                    
                    FormerPRItem.itemTypeId = value.ItemTypeId;
                }
                if (FormerPRItem.codeId != value.CodeId.Value)
                {
                    if (FormerPRItem.codeId != null)
                    {
                        var FormerItemCode = db.PopulateItemLists.Select(m => new { ItemCode = m.ItemDescription, CodeId = m.codeId }).First(m => m.CodeId == FormerPRItem.codeId);
                        var NewItemCode = db.PopulateItemLists.Select(m => new { ItemCode = m.ItemDescription, CodeId = m.codeId }).First(m => m.CodeId == value.CodeId);
                        NotificationMsg _objDetails_CodeId = new NotificationMsg
                        {
                            uuid = Guid.NewGuid(),
                            PRId = PR.PRId,
                            msgDate = DateTime.Now,
                            fromUserId = PR.UserId,
                            msgType = "Trail",
                            message = PR.FullName + " change Item Code from " + FormerItemCode.ItemCode + " to " + NewItemCode.ItemCode + " for PR No: " + FormerPRDetails.PRNo
                        };
                        db.NotificationMsgs.Add(_objDetails_CodeId);
                    }                    
                    FormerPRItem.codeId = value.CodeId.Value;
                }
                if (FormerPRItem.jobNo != value.JobNo)
                {
                    if (FormerPRItem.jobNo != null)
                    {
                        NotificationMsg _objDetails_JobNo = new NotificationMsg
                        {
                            uuid = Guid.NewGuid(),
                            PRId = PR.PRId,
                            msgDate = DateTime.Now,
                            fromUserId = PR.UserId,
                            msgType = "Trail",
                            message = PR.FullName + " change Job No from " + FormerPRItem.jobNo + " to " + value.JobNo + " for PR No: " + FormerPRDetails.PRNo
                        };
                        db.NotificationMsgs.Add(_objDetails_JobNo);
                    }                    
                    FormerPRItem.jobNo = value.JobNo;
                }
                if (FormerPRItem.jobTaskNo != value.JobTaskNo)
                {
                    if (FormerPRItem.jobTaskNo != null)
                    {
                        NotificationMsg _objDetails_JobTaskNo = new NotificationMsg
                        {
                            uuid = Guid.NewGuid(),
                            PRId = PR.PRId,
                            msgDate = DateTime.Now,
                            fromUserId = PR.UserId,
                            msgType = "Trail",
                            message = PR.FullName + " change Job Task No from " + FormerPRItem.jobTaskNo + " to " + value.JobTaskNo + " for PR No: " + FormerPRDetails.PRNo
                        };
                        db.NotificationMsgs.Add(_objDetails_JobTaskNo);
                    }                        
                    FormerPRItem.jobTaskNo = value.JobTaskNo;
                }
                if (FormerPRItem.unitPrice != value.UnitPrice.Value)
                {
                    if (FormerPRItem.unitPrice != null)
                    {
                        NotificationMsg _objDetails_UnitPrice = new NotificationMsg
                        {
                            uuid = Guid.NewGuid(),
                            PRId = PR.PRId,
                            msgDate = DateTime.Now,
                            fromUserId = PR.UserId,
                            msgType = "Trail",
                            message = PR.FullName + " change Unit Price from " + FormerPRItem.unitPrice + " to " + value.UnitPrice.Value + " for PR No: " + FormerPRDetails.PRNo
                        };
                        db.NotificationMsgs.Add(_objDetails_UnitPrice);
                    }                    
                    FormerPRItem.unitPrice = value.UnitPrice.Value;
                }
                if (FormerPRItem.totalPrice != value.TotalPrice.Value)
                {
                    if (FormerPRItem.totalPrice != null)
                    {
                        NotificationMsg _objDetails_TotalPrice = new NotificationMsg
                        {
                            uuid = Guid.NewGuid(),
                            PRId = PR.PRId,
                            msgDate = DateTime.Now,
                            fromUserId = PR.UserId,
                            msgType = "Trail",
                            message = PR.FullName + " change Total Price from " + FormerPRItem.totalPrice + " to " + value.TotalPrice.Value + " for PR No: " + FormerPRDetails.PRNo
                        };
                        db.NotificationMsgs.Add(_objDetails_TotalPrice);
                    }                       
                    FormerPRItem.totalPrice = value.TotalPrice.Value;
                }
                db.SaveChanges();
            }

            PR_Admin SaveAdminInfo = db.PR_Admin.First(m => m.PRId == PR.PRId);
            SaveAdminInfo.adminSaved = SaveAdminInfo.adminSaved + 1;
            SaveAdminInfo.lastModifyDate = DateTime.Now;
            FormerPRDetails.Saved = FormerPRDetails.Saved + 1;
            NotificationMsg _objSaved = new NotificationMsg
            {
                uuid = Guid.NewGuid(),
                PRId = PR.PRId,
                msgDate = DateTime.Now,
                fromUserId = PR.UserId,
                msgType = "Trail",
                message = PR.FullName + " has saved PR application for PR No. " + FormerPRDetails.PRNo + " " + FormerPRDetails.Saved + " time."
            };
            db.NotificationMsgs.Add(_objSaved);

            if (db.SaveChanges() > 0)
            {
                return Json("Sucessfully update");
            }
            else {
                return Json("Something is wrong");
            }
            
        }

        public JsonResult SubmitPRProcurement(PRModel PRModel)
        {
            // todo check session if login as procurement or admin only

            int PrId = Int32.Parse(Request["PrId"]); int UserId = Int32.Parse(Session["UserId"].ToString());
            var getFullName = db.Users.First(m => m.userId == UserId);
            //String SpecsReviewer = Request["SpecsReviewer"];

            var PR = db.PurchaseRequisitions.First(x => x.PRId == PrId);
            PR.SpecsReviewer = PRModel.NewPRForm.SpecReviewer;
            PR.AmountRequired = PRModel.NewPRForm.AmountRequired;
            PR.VendorId = PRModel.NewPRForm.VendorId;
            PR.PRAging = PRModel.NewPRForm.PRAging;
            db.SaveChanges();

            foreach (var value in PRModel.NewPRForm.PRItemListObject)
            {
                PR_Items _objNewPRItem = db.PR_Items.FirstOrDefault(m => m.itemsId == value.ItemsId);
                _objNewPRItem.itemTypeId = value.ItemTypeId;
                _objNewPRItem.codeId = value.CodeId.Value;
                _objNewPRItem.jobNo = value.JobNo;
                _objNewPRItem.jobTaskNo = value.JobTaskNo;
                _objNewPRItem.unitPrice = value.UnitPrice.Value;
                _objNewPRItem.totalPrice = value.TotalPrice.Value;
                db.SaveChanges();
            }

            var requestorDone = (from m in db.NotificationMsgs
                                 join n in db.NotiGroups on m.msgId equals n.msgId
                                 where n.toUserId == UserId && m.PRId == PrId
                                 select new PRModel()
                                 {
                                     MsgId = m.msgId
                                 }).First();

            NotificationMsg getDone = db.NotificationMsgs.First(m => m.msgId == requestorDone.MsgId);
            getDone.done = true;
            db.SaveChanges();

            Debug.WriteLine("SubmitPRProcurement");
            decimal amountBudget = PRModel.NewPRForm.AmountRequired;

            PR_Admin SaveAdminInfo = db.PR_Admin.First(m => m.PRId == PR.PRId);
            SaveAdminInfo.adminSubmited = SaveAdminInfo.adminSubmited + 1;
            SaveAdminInfo.adminSubmitDate = DateTime.Now;
            db.SaveChanges();

            var getSpecReviewerId = (from m in db.PurchaseRequisitions
                                 join n in db.Roles on m.SpecsReviewer equals n.name
                                 where m.PRId == PR.PRId
                                 select new
                                 {
                                     RoleId = n.roleId
                                 }).FirstOrDefault();
            if (PRService.CheckAmountScenarioOne(amountBudget) && PRService.IncludeInBudget(PR))
            {
                PR.StatusId = "PR03";
                PR.Scenario = 1;
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
                var getReviewer = (from m in db.Users
                                   join n in db.Users_Roles on m.userId equals n.userId
                                   //join o in db.Roles on n.roleId equals o.roleId
                                   where n.roleId == getSpecReviewerId.RoleId && m.companyId == PR.CustId
                                   select new PRModel()
                                   {
                                       UserId = n.userId
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

                    PR_Reviewer _objSaveReviewer = new PR_Reviewer
                    {
                        uuid = Guid.NewGuid(),
                        reviewerId = item.UserId,
                        PRId = PR.PRId
                    };
                    db.PR_Reviewer.Add(_objSaveReviewer);
                    db.SaveChanges();
                }

                return Json("Scenario 1 success");
            }
            else if ((PRService.IncludeInBudget(PR) == false && PRService.CheckAmountScenarioOne(amountBudget)) || PRService.CheckAmountScenarioTwo(amountBudget))
            {
                PR.StatusId = "PR04";
                PR.Scenario = 2;
                db.SaveChanges();

                var getRecommender = (from m in db.Users
                                      join n in db.Users_Roles on m.userId equals n.userId
                                      //join o in db.Roles on n.roleId equals o.roleId
                                      where n.roleId == getSpecReviewerId.RoleId && m.companyId == PR.CustId
                                      select new PRModel()
                                      {
                                          UserId = n.userId
                                      }).ToList();
                NotificationMsg objTask = new NotificationMsg()
                {
                    uuid = Guid.NewGuid(),
                    message = PR.PRNo + " pending for your recommendal",
                    fromUserId = UserId,
                    msgDate = DateTime.Now,
                    msgType = "Task",
                    PRId = PR.PRId
                };
                db.NotificationMsgs.Add(objTask);
                db.SaveChanges();

                foreach (var item in getRecommender)
                {
                    NotiGroup Task = new NotiGroup()
                    {
                        uuid = Guid.NewGuid(),
                        msgId = objTask.msgId,
                        toUserId = item.UserId
                    };
                    db.NotiGroups.Add(Task);

                    PR_Recommender saveRecommender = new PR_Recommender()
                    {
                        uuid = Guid.NewGuid(),
                        recommenderId = item.UserId,
                        PRId = PR.PRId
                    };
                    db.PR_Recommender.Add(saveRecommender);
                    db.SaveChanges();
                }

                return Json("Scenario 2 success");
            }
            else if (PRService.CheckAmountScenarioThree(amountBudget))
            {
                PR.StatusId = "PR04";
                PR.Scenario = 3;
                db.SaveChanges();

                var getRecommender = (from m in db.Users
                                      join n in db.Users_Roles on m.userId equals n.userId
                                      //join o in db.Roles on n.roleId equals o.roleId
                                      where n.roleId == getSpecReviewerId.RoleId && m.companyId == PR.CustId
                                      select new PRModel()
                                      {
                                          UserId = n.userId
                                      }).ToList();
                NotificationMsg objTask = new NotificationMsg()
                {
                    uuid = Guid.NewGuid(),
                    message = PR.PRNo + " pending for your recommendal",
                    fromUserId = UserId,
                    msgDate = DateTime.Now,
                    msgType = "Task",
                    PRId = PR.PRId
                };
                db.NotificationMsgs.Add(objTask);
                db.SaveChanges();

                foreach (var item in getRecommender)
                {
                    NotiGroup Task = new NotiGroup()
                    {
                        uuid = Guid.NewGuid(),
                        msgId = objTask.msgId,
                        toUserId = item.UserId
                    };
                    db.NotiGroups.Add(Task);

                    PR_Recommender saveRecommender = new PR_Recommender()
                    {
                        uuid = Guid.NewGuid(),
                        recommenderId = item.UserId,
                        PRId = PR.PRId
                    };
                    db.PR_Recommender.Add(saveRecommender);
                    db.SaveChanges();
                }

                return Json("Scenario 3 success");
            }
            else {
                return Json("Does not fit to any conditions. Please recheck. ");
            }
        }

        public JsonResult RejectPRReview()
        {
            // todo the session must be IT / PMO /
            int UserId = Int32.Parse(Session["UserId"].ToString());
            int PrId = Int32.Parse(Request["PrId"]);
            string RejectRemark = Request["RejectRemark"].ToString();
            var PR = db.PurchaseRequisitions.First(x => x.PRId == PrId);
            PR.StatusId = "PR07";
            PR.Rejected = PR.Rejected + 1;
            PR.RejectedById = UserId;
            PR.RejectedDate = DateTime.Now;
            PR.RejectedRemark = RejectRemark;
            db.SaveChanges();

            NotificationMsg objNotification = new NotificationMsg()
            {
                uuid = Guid.NewGuid(),
                message = Session["FullName"].ToString() + " has rejected the PR No: " + PR.PRNo + " application. ",
                fromUserId = UserId,
                msgDate = DateTime.Now,
                msgType = "Trail",
                PRId = PrId
            };
            db.NotificationMsgs.Add(objNotification);

            var requestorDone = (from m in db.NotificationMsgs
                                 join n in db.NotiGroups on m.msgId equals n.msgId
                                 where n.toUserId == UserId && m.PRId == PrId
                                 select new PRModel()
                                 {
                                     MsgId = m.msgId
                                 }).First();

            NotificationMsg getDone = db.NotificationMsgs.First(m => m.msgId == requestorDone.MsgId);
            getDone.done = true;
            if (db.SaveChanges() > 0)
            {
                return Json("Successfully reject");
            } else {
                return Json("System failure. Please contact admin");
            }
            
        }

        public JsonResult ApprovePRReview()
        {
            int PrId = Int32.Parse(Request["PrId"]);
            var PR = db.PurchaseRequisitions.First(x => x.PRId == PrId);
            PR.StatusId = "PR05";
            db.SaveChanges();

            PR_Reviewer SaveReviewerInfo = db.PR_Reviewer.First(m => m.PRId == PR.PRId);
            SaveReviewerInfo.reviewedDate = DateTime.Now;
            SaveReviewerInfo.reviewed = 1;
            db.SaveChanges();

            int UserId = Int32.Parse(Session["UserId"].ToString());
            var requestorDone = (from m in db.NotificationMsgs
                                 join n in db.NotiGroups on m.msgId equals n.msgId
                                 where n.toUserId == UserId && m.PRId == PrId
                                 select new PRModel()
                                 {
                                     MsgId = m.msgId
                                 }).First();

            NotificationMsg getDone = db.NotificationMsgs.First(m => m.msgId == requestorDone.MsgId);
            getDone.done = true;
            db.SaveChanges();

            var getApprover = new List<PRModel>();
            if (PR.Scenario == 3)
            {
                getApprover = (from m in db.Users
                                   join n in db.Users_Roles on m.userId equals n.userId
                                   //join o in db.Roles on n.roleId equals o.roleId
                                   where n.roleId == "R04" && m.companyId == 2
                                   select new PRModel()
                                   {
                                       UserId = n.userId
                                   }).ToList();
            } else if (PR.Scenario == 2 || PR.Scenario == 1)
            {
                getApprover = (from m in db.Users
                               join n in db.Users_Roles on m.userId equals n.userId
                               //join o in db.Roles on n.roleId equals o.roleId
                               where n.roleId == "R04" && m.companyId == PR.CustId
                               select new PRModel()
                               {
                                   UserId = n.userId
                               }).ToList();
            }
            
            NotificationMsg objTask = new NotificationMsg()
            {
                uuid = Guid.NewGuid(),
                message = PR.PRNo + " pending for your approval",
                fromUserId = UserId,
                msgDate = DateTime.Now,
                msgType = "Task",
                PRId = PR.PRId
            };
            db.NotificationMsgs.Add(objTask);
            db.SaveChanges();

            foreach (var item in getApprover)
            {
                NotiGroup Task = new NotiGroup()
                {
                    uuid = Guid.NewGuid(),
                    msgId = objTask.msgId,
                    toUserId = item.UserId
                };
                db.NotiGroups.Add(Task);

                PR_Approver saveApprover = new PR_Approver()
                {
                    uuid = Guid.NewGuid(),
                    approverId = item.UserId,
                    PRId = PR.PRId
                };
                db.PR_Approver.Add(saveApprover);
                db.SaveChanges();
            }

            return Json("Successfully reviewed");
        }

        public JsonResult RejectPRApprover()
        {
            int UserId = Int32.Parse(Session["UserId"].ToString());
            int PrId = Int32.Parse(Request["PrId"]);
            string RejectRemark = Request["RejectRemark"].ToString();
            var PR = db.PurchaseRequisitions.First(x => x.PRId == PrId);
            PR.StatusId = "PR07";
            PR.Rejected = PR.Rejected + 1;
            PR.RejectedById = UserId;
            PR.RejectedDate = DateTime.Now;
            PR.RejectedRemark = RejectRemark;
            db.SaveChanges();

            NotificationMsg objNotification = new NotificationMsg()
            {
                uuid = Guid.NewGuid(),
                message = Session["FullName"].ToString() + " has rejected the PR No: " + PR.PRNo + " application. ",
                fromUserId = UserId,
                msgDate = DateTime.Now,
                msgType = "Trail",
                PRId = PrId
            };
            db.NotificationMsgs.Add(objNotification);

            var requestorDone = (from m in db.NotificationMsgs
                                 join n in db.NotiGroups on m.msgId equals n.msgId
                                 where n.toUserId == UserId && m.PRId == PrId
                                 select new PRModel()
                                 {
                                     MsgId = m.msgId
                                 }).First();

            NotificationMsg getDone = db.NotificationMsgs.First(m => m.msgId == requestorDone.MsgId);
            getDone.done = true;
            if (db.SaveChanges() > 0)
            {
                return Json("Successfully reject");
            }
            else
            {
                return Json("System failure. Please contact admin");
            }
        }
        public JsonResult ApprovePRApprover()
        {
            int PrId = Int32.Parse(Request["PrId"]);
            var PR = db.PurchaseRequisitions.First(x => x.PRId == PrId);
            PR.StatusId = "PR06";
            db.SaveChanges();

            PR_Approver SaveApproverInfo = db.PR_Approver.First(m => m.PRId == PR.PRId);
            SaveApproverInfo.approverApprovedDate = DateTime.Now;
            SaveApproverInfo.approverApproved = 1;
            db.SaveChanges();

            int UserId = Int32.Parse(Session["UserId"].ToString());
            var requestorDone = (from m in db.NotificationMsgs
                                 join n in db.NotiGroups on m.msgId equals n.msgId
                                 where n.toUserId == UserId && m.PRId == PrId
                                 select new PRModel()
                                 {
                                     MsgId = m.msgId
                                 }).First();

            NotificationMsg getDone = db.NotificationMsgs.First(m => m.msgId == requestorDone.MsgId);
            getDone.done = true;
            db.SaveChanges();

            int CustId = Int32.Parse(Session["CompanyId"].ToString());
            var getAdmin = (from m in db.Users
                            join n in db.Users_Roles on m.userId equals n.userId
                            where n.roleId == "R03" && m.companyId == CustId
                            select new PRModel()
                            {
                                UserId = m.userId
                            }).ToList();
            NotificationMsg objTask = new NotificationMsg()
            {
                uuid = Guid.NewGuid(),
                message = PR.PRNo + " pending for you to issue PO or cancel",
                fromUserId = Int32.Parse(Session["UserId"].ToString()),
                msgDate = DateTime.Now,
                msgType = "Task",
                PRId = PrId
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
            // todo the session must be HOC

            return Json("Successfully approve");
        }
        public JsonResult RejectPRRecommended()
        {
            int UserId = Int32.Parse(Session["UserId"].ToString());
            int PrId = Int32.Parse(Request["PrId"]);
            string RejectRemark = Request["RejectRemark"].ToString();
            var PR = db.PurchaseRequisitions.First(x => x.PRId == PrId);
            PR.StatusId = "PR07";
            PR.Rejected = PR.Rejected + 1;
            PR.RejectedById = UserId;
            PR.RejectedDate = DateTime.Now;
            PR.RejectedRemark = RejectRemark;
            db.SaveChanges();

            NotificationMsg objNotification = new NotificationMsg()
            {
                uuid = Guid.NewGuid(),
                message = Session["FullName"].ToString() + " has rejected the PR No: " + PR.PRNo + " application. ",
                fromUserId = UserId,
                msgDate = DateTime.Now,
                msgType = "Trail",
                PRId = PrId
            };
            db.NotificationMsgs.Add(objNotification);

            var requestorDone = (from m in db.NotificationMsgs
                                 join n in db.NotiGroups on m.msgId equals n.msgId
                                 where n.toUserId == UserId && m.PRId == PrId
                                 select new PRModel()
                                 {
                                     MsgId = m.msgId
                                 }).First();

            NotificationMsg getDone = db.NotificationMsgs.First(m => m.msgId == requestorDone.MsgId);
            getDone.done = true;

            if (db.SaveChanges() > 0)
            {
                return Json("Successfully reject");
            }
            else
            {
                return Json("System failure. Please contact admin");
            }
        }
        public JsonResult RecommendedPRRecommended()
        {
            // todo the session must be IT / HSE / PMO
            // set status to PR03 and head of GPSS will proceed

            int PrId = Int32.Parse(Request["PrId"]);
            var PR = db.PurchaseRequisitions.First(x => x.PRId == PrId);

            int UserId = Int32.Parse(Session["UserId"].ToString());
            var requestorDone = (from m in db.NotificationMsgs
                                 join n in db.NotiGroups on m.msgId equals n.msgId
                                 where n.toUserId == UserId && m.PRId == PrId
                                 select new PRModel()
                                 {
                                     MsgId = m.msgId
                                 }).First();

            NotificationMsg getDone = db.NotificationMsgs.First(m => m.msgId == requestorDone.MsgId);
            getDone.done = true;
            db.SaveChanges();

            PR_Recommender SaveRecommenderInfo = db.PR_Recommender.First(m => m.PRId == PR.PRId);
            SaveRecommenderInfo.recommendedDate = DateTime.Now;
            SaveRecommenderInfo.recommended = 1;
            db.SaveChanges();

            if (PR.Scenario == 3) {
                PR.StatusId = "PR11"; var getRecommenderII = new List<PRModel>();
                if (PR.CustId == 2)
                {
                    getRecommenderII = (from m in db.Users
                                        join n in db.Users_Roles on m.userId equals n.userId
                                        //join o in db.Roles on n.roleId equals o.roleId
                                        where n.roleId == "R02" && m.companyId == 2
                                        select new PRModel()
                                        {
                                            UserId = n.userId
                                        }).ToList();
                }
                else
                {
                    getRecommenderII = (from m in db.Users
                                        join n in db.Users_Roles on m.userId equals n.userId
                                        //join o in db.Roles on n.roleId equals o.roleId
                                        where n.roleId == "R04" && m.companyId == PR.CustId
                                        select new PRModel()
                                        {
                                            UserId = n.userId
                                        }).ToList();
                }
                NotificationMsg objTask = new NotificationMsg()
                {
                    uuid = Guid.NewGuid(),
                    message = PR.PRNo + " pending for your recommendal",
                    fromUserId = UserId,
                    msgDate = DateTime.Now,
                    msgType = "Task",
                    PRId = PR.PRId
                };
                db.NotificationMsgs.Add(objTask);
                db.SaveChanges();

                foreach (var item in getRecommenderII)
                {
                    NotiGroup Task = new NotiGroup()
                    {
                        uuid = Guid.NewGuid(),
                        msgId = objTask.msgId,
                        toUserId = item.UserId
                    };
                    db.NotiGroups.Add(Task);

                    PR_RecommenderII saveRecommender = new PR_RecommenderII()
                    {
                        uuid = Guid.NewGuid(),
                        recommenderId = item.UserId,
                        PRId = PR.PRId
                    };
                    db.PR_RecommenderII.Add(saveRecommender);
                    db.SaveChanges();
                }
            } else if (PR.Scenario == 2)
            {
                PR.StatusId = "PR03";
                //scenario 2, review by head of GPSS
                var getReviewer = (from m in db.Users
                                   join n in db.Users_Roles on m.userId equals n.userId
                                   //join o in db.Roles on n.roleId equals o.roleId
                                   where n.roleId == "R10" && m.companyId == 2
                                   select new PRModel()
                                   {
                                       UserId = n.userId
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

                    PR_Reviewer _objSaveReviewer = new PR_Reviewer
                    {
                        uuid = Guid.NewGuid(),
                        reviewerId = item.UserId,
                        PRId = PR.PRId
                    };
                    db.PR_Reviewer.Add(_objSaveReviewer);
                    db.SaveChanges();
                }
            }             
            db.SaveChanges();            

            return Json("Successfully recommend");
        }
        public JsonResult ApprovePRJointRecommended()
        {
            int PrId = Int32.Parse(Request["PrId"]);
            var PR = db.PurchaseRequisitions.First(x => x.PRId == PrId);
            PR.StatusId = "PR03";
            db.SaveChanges();
            // todo the session must be HOC
            int UserId = Int32.Parse(Session["UserId"].ToString());
            var requestorDone = (from m in db.NotificationMsgs
                                 join n in db.NotiGroups on m.msgId equals n.msgId
                                 where n.toUserId == UserId && m.PRId == PrId
                                 select new PRModel()
                                 {
                                     MsgId = m.msgId
                                 }).First();

            NotificationMsg getDone = db.NotificationMsgs.First(m => m.msgId == requestorDone.MsgId);
            getDone.done = true;
            db.SaveChanges();

            PR_RecommenderII SaveRecommenderInfo = db.PR_RecommenderII.First(m => m.PRId == PR.PRId);
            SaveRecommenderInfo.recommendedDate = DateTime.Now;
            SaveRecommenderInfo.recommended = 1;
            db.SaveChanges();

            int CustId = Int32.Parse(Session["CompanyId"].ToString());
            var getReviewer = (from m in db.Users
                               join n in db.Users_Roles on m.userId equals n.userId
                               //join o in db.Roles on n.roleId equals o.roleId
                               where n.roleId == "R10" && m.companyId == CustId
                               select new PRModel()
                               {
                                   UserId = n.userId
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

                PR_Reviewer _objSaveReviewer = new PR_Reviewer
                {
                    uuid = Guid.NewGuid(),
                    reviewerId = item.UserId,
                    PRId = PR.PRId
                };
                db.PR_Reviewer.Add(_objSaveReviewer);
                db.SaveChanges();
            }
            return Json("Successfully joint recommended");
        }
        public JsonResult CancelPR()
        {
            int UserId = Int32.Parse(Session["UserId"].ToString());
            int PrId = Int32.Parse(Request["PRId"].ToString());
            int CustId = Int32.Parse(Session["CompanyId"].ToString());
            string CancelType = Request["CancelType"].ToString();
            var UpdatePRStatus = db.PurchaseRequisitions.Where(m => m.PRId == PrId).First();
            if (CancelType == "CancelPR")
            {
                UpdatePRStatus.StatusId = "PR08";
                NotificationMsg objNotification = new NotificationMsg()
                {
                    uuid = Guid.NewGuid(),
                    message = Session["FullName"].ToString() + " has cancel the PR No: " + UpdatePRStatus.PRNo + " application. ",
                    fromUserId = Int32.Parse(Session["UserId"].ToString()),
                    msgDate = DateTime.Now,
                    msgType = "Trail",
                    PRId = PrId
                };
                db.NotificationMsgs.Add(objNotification);
                db.SaveChanges();
            } else
            {
                UpdatePRStatus.StatusId = "PR15";

                NotificationMsg objNotification = new NotificationMsg()
                {
                    uuid = Guid.NewGuid(),
                    message = Session["FullName"].ToString() + " has request to cancel the PR No: " + UpdatePRStatus.PRNo + " subject recommendal from HOD. ",
                    fromUserId = Int32.Parse(Session["UserId"].ToString()),
                    msgDate = DateTime.Now,
                    msgType = "Trail",
                    PRId = PrId
                };
                db.NotificationMsgs.Add(objNotification);
                db.SaveChanges();

                var getHOD = (from m in db.Users
                                join n in db.Users_Roles on m.userId equals n.userId
                                where n.roleId == "R02" && m.companyId == CustId
                              select new PRModel()
                                {
                                    UserId = m.userId
                                }).ToList();
                NotificationMsg objTask = new NotificationMsg()
                {
                    uuid = Guid.NewGuid(),
                    message = UpdatePRStatus.PRNo + " pending for you to recommend PR cancellation",
                    fromUserId = Int32.Parse(Session["UserId"].ToString()),
                    msgDate = DateTime.Now,
                    msgType = "Task",
                    PRId = PrId
                };
                db.NotificationMsgs.Add(objTask);
                db.SaveChanges();

                foreach (var item in getHOD)
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
            //todo noti to expected user
            if (db.SaveChanges() > 0)
            {
                return Json("Successfully cancel");
            }
            else
            {
                return Json("System failure. Please contact admin");
            }
        }
        public JsonResult RecommendCancel()
        {
            int UserId = Int32.Parse(Session["UserId"].ToString());
            int PrId = Int32.Parse(Request["PRId"].ToString());
            int CustId = Int32.Parse(Session["CompanyId"].ToString());
            var UpdatePRStatus = db.PurchaseRequisitions.Where(m => m.PRId == PrId).First();

            NotificationMsg objNotification = new NotificationMsg()
            {
                uuid = Guid.NewGuid(),
                message = Session["FullName"].ToString() + " has recommended to cancel the PR No: " + UpdatePRStatus.PRNo + " subject for acceptance from Procurement. ",
                fromUserId = Int32.Parse(Session["UserId"].ToString()),
                msgDate = DateTime.Now,
                msgType = "Trail",
                PRId = PrId
            };
            db.NotificationMsgs.Add(objNotification);
            db.SaveChanges();

            var getProcurement = (from m in db.Users
                          join n in db.Users_Roles on m.userId equals n.userId
                          where n.roleId == "R03" && m.companyId == CustId
                          select new PRModel()
                          {
                              UserId = m.userId
                          }).ToList();
            NotificationMsg objTask = new NotificationMsg()
            {
                uuid = Guid.NewGuid(),
                message = UpdatePRStatus.PRNo + " pending for you to accept PR cancellation",
                fromUserId = Int32.Parse(Session["UserId"].ToString()),
                msgDate = DateTime.Now,
                msgType = "Task",
                PRId = PrId
            };
            db.NotificationMsgs.Add(objTask);
            db.SaveChanges();

            foreach (var item in getProcurement)
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

            UpdatePRStatus.StatusId = "PR16";
            if (db.SaveChanges() > 0)
            {
                return Json("Successfully cancel");
            }
            else
            {
                return Json("System failure. Please contact admin");
            }
        }
        public JsonResult AcceptCancel()
        {
            int UserId = Int32.Parse(Session["UserId"].ToString());
            int PrId = Int32.Parse(Request["PRId"].ToString());
            //int CustId = Int32.Parse(Session["CompanyId"].ToString());
            var UpdatePRStatus = db.PurchaseRequisitions.Where(m => m.PRId == PrId).First();
            UpdatePRStatus.StatusId = "PR08";

            NotificationMsg objNotification = new NotificationMsg()
            {
                uuid = Guid.NewGuid(),
                message = Session["FullName"].ToString() + " has accept the PR No: " + UpdatePRStatus.PRNo + " cancellation request.",
                fromUserId = Int32.Parse(Session["UserId"].ToString()),
                msgDate = DateTime.Now,
                msgType = "Trail",
                PRId = PrId
            };
            db.NotificationMsgs.Add(objNotification);

            if (db.SaveChanges() > 0)
            {
                return Json("Successfully cancel");
            }
            else
            {
                return Json("System failure. Please contact admin");
            }
        }
    }

    internal class DashboardPOModel
    {
        public int quantity { get; set; }
        public string description { get; set; }
    }
}