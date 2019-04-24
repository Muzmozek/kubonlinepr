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
using System.Web.Security;
using System.Web.Routing;
using System.Data.Entity.Validation;
using System.Configuration;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

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

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class CheckSessionOutAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName.ToLower();
            if (!controllerName.Contains("home"))
            {
                HttpSessionStateBase session = filterContext.HttpContext.Session;
                var user = session["Username"]; //Key 2 should be User or UserName
                if (((user == null) && (!session.IsNewSession)) || (session.IsNewSession))
                {
                    FormsAuthentication.SignOut();
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary {
                        { "action", "Index" },
                        { "controller", "Home" },
                        { "returnUrl", filterContext.HttpContext.Request.RawUrl}
                    });

                    return;
                }
                base.OnActionExecuting(filterContext);
            }
        }
    }

    public class ValidateAjaxAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!filterContext.HttpContext.Request.IsAjaxRequest())
                return;

            var modelState = filterContext.Controller.ViewData.ModelState;
            var PRModel = filterContext.ActionParameters.Values.FirstOrDefault() as PRModel;
            var POModel = filterContext.ActionParameters.Values.FirstOrDefault() as POModel;
            if (PRModel != null)
            {
                if (PRModel.NewPRForm.StatusId == "PR02") {
                    if (PRModel.NewPRForm.VendorId == null)
                    {
                        modelState.AddModelError("NewPRForm.VendorId", "Please select vendor company name.");
                    }
                    for (int i = 0; i < PRModel.NewPRForm.PRItemListObject.Count; i++)
                    {
                        if (PRModel.NewPRForm.PRItemListObject[i].UnitPrice == null)
                        {
                            modelState.AddModelError("NewPRForm.PRItemListObject[" + (i+1) + "].UnitPrice", "Please insert unit price (RM)");
                        }
                        if (PRModel.NewPRForm.PRItemListObject[i].TaxCodeId == null)
                        {
                            modelState.AddModelError("NewPRForm.PRItemListObject[" + (i+1) + "].TaxCodeId", "Please select SST code");
                        }
                    }
                    //todo: add other field validation
                }
                if (PRModel.NewPRForm.StatusId == "PR10" && PRModel.FinalApproverId == 0)
                {
                    modelState.AddModelError("FinalApproverId", "Please select final approver name.");
                }
                if (PRModel.NewPRForm.PaperRefNo != null && PRModel.PaperRefNoFile == null)
                {
                    modelState.AddModelError("PaperRefNoFileName", "GMD paper attachment needed.");
                }
                if (PRModel.NewPRForm.BidWaiverRefNo != null && PRModel.BidWaiverRefNoFile == null)
                {
                    modelState.AddModelError("BidWaiverRefNoFileName", "Bid Waiver attachment needed");
                }
                if (PRModel.NewPRForm.PaperRefNo == null && PRModel.PaperRefNoFile != null)
                {
                    modelState.AddModelError("NewPRForm.PaperRefNo", "GMD paper ref no needed.");
                }
                if (PRModel.NewPRForm.BidWaiverRefNo == null && PRModel.BidWaiverRefNoFile != null)
                {
                    modelState.AddModelError("NewPRForm.BidWaiverRefNo", "Bid Waiver ref no needed");
                }
                if (PRModel.PaperRefNoFile != null && Path.GetExtension(PRModel.PaperRefNoFile.FileName) != ".pdf")
                {
                    modelState.AddModelError("PaperRefNoFileName", "Only pdf file accepted.");
                }
                if (PRModel.BidWaiverRefNoFile != null && Path.GetExtension(PRModel.BidWaiverRefNoFile.FileName) != ".pdf")
                {
                    modelState.AddModelError("BidWaiverRefNoFileName", "Only pdf file accepted.");
                }                
            }
            if (POModel != null)
            {               
            }
            if (!modelState.IsValid)
            {
                var errorModel =
                        from x in modelState.Keys
                        where modelState[x].Errors.Count > 0
                        select new
                        {
                            key = x,
                            errors = modelState[x].Errors.
                                                          Select(y => y.ErrorMessage).
                                                          ToArray()
                        };
                filterContext.Result = new JsonResult()
                {
                    Data = new
                    {
                        success = false,
                        exception = false,
                        data = errorModel
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
                //filterContext.HttpContext.Response.StatusCode = 200;
            }
        }
    }

    public class PRController : DBLogicController
    {
        private KUBOnlinePREntities db = new KUBOnlinePREntities();
        private KUBHelper KUBHelper = new KUBHelper();
        private PRService PRService = new PRService();

        [CheckSessionOut]
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
            var sql = "SELECT 'Open'        AS [description], " +
                               "Sum(quantity) AS quantity " +
                        "FROM   (SELECT m.status      AS [description], " +
                                       "Count(n.prid) AS quantity " +
                                "FROM   prstatus m " +
                                       "LEFT JOIN purchaserequisition n " +
                                              "ON ( m.statusid = n.statusid ) " +
                                "WHERE  m.statusid <> 'PR08' " +
                                       "AND m.statusid <> 'PR06' " +
                                       "AND m.statusid <> 'PR14' " +
                                       "AND m.statusid <> 'PR07' " +
                                "GROUP  BY m.status) AS T " +
                        "UNION " +
                        "SELECT TOP 3 m.status      AS [description], " +
                                     "Count(n.prid) AS quantity " +
                        "FROM   prstatus m " +
                               "LEFT JOIN purchaserequisition n " +
                                      "ON ( m.statusid = n.statusid ) " +
                        //"--where m.status = 'Cancel' " +
                        "GROUP  BY m.status " +
                        "UNION " +
                        "SELECT m.status      AS [description], " +
                               "Count(n.prid) AS quantity " +
                        "FROM   prstatus m " +
                               "LEFT JOIN purchaserequisition n " +
                                      "ON ( m.statusid = n.statusid " +
                                           "AND m.statusid = 'PR07' ) " +
                        "WHERE  m.status = 'Reject' " +
                        "GROUP  BY m.status ";

            //dummy
            //var collections = new List<DashboardPOModel>();
            //collections.Add(new DashboardPOModel() { quantity = 22, description = "Open" });
            //collections.Add(new DashboardPOModel() { quantity = 7, description = "Close" });
            //collections.Add(new DashboardPOModel() { quantity = 38, description = "Close with PO" });
            //collections.Add(new DashboardPOModel() { quantity = 5, description = "Cancel" });

            var collections = db.Database.SqlQuery<DashboardPOModel>(sql);

            return Json(collections, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DashboardPurchaseOrder()
        {
            var sql = "SELECT m.status      AS [description], " +
                                     "Count(n.POId) AS quantity " +
                        "FROM   POStatus m " +
                               "LEFT JOIN PurchaseOrder n " +
                                      "ON ( m.statusid = n.statusid ) " +
                        "GROUP  BY m.status ";

            var collections = db.Database.SqlQuery<DashboardPOModel>(sql);

            return Json(collections, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DashboardOpenPO()
        {
            var sql = "select count(*) as quantity,CAST(Year(SubmitDate) AS nvarchar) as description from PurchaseOrder where StatusId = 'PO01' group by Year(SubmitDate)";

            //dummy
            //var collections = new List<DashboardPOModel>();
            //collections.Add(new DashboardPOModel () { quantity = 10, description = "2016" });
            //collections.Add(new DashboardPOModel() { quantity = 5, description = "2017" });
            //collections.Add(new DashboardPOModel() { quantity = 4, description = "2018" });

            var collections = db.Database.SqlQuery<DashboardPOModel>(sql);

            return Json(collections, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetProjectInfo(int projectId)
        {
            var projectInfo = db.Projects.Select(m => new NewPRModel()
            {
                ProjectId = m.projectId,
                PaperVerified = m.paperVerified,
                ChildCustId = m.custId
                //BudgetedAmount = m.budgetedAmount,
                //UtilizedToDate = m.utilizedToDate,
                //BudgetBalance = m.budgetBalance
            }).Where(m => m.ProjectId == projectId).FirstOrDefault();

            return Json(new { projectInfo }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetItemTypeInfo(int ItemTypeId, int Selectlistid, int PRCustId)
        {
            string html = "<select class='CodeId custom-select g-height-40 g-color-black g-color-black--hover text-left g-rounded-20' data-val-required='The CodeId field is required.' id='CodeId" + Selectlistid + "'><option value='' >Select item code here</option>";
            
            if (ItemTypeId != 0)
            {
                var ItemCodeQuery = db.PopulateItemLists.Select(m => new { CodeId = m.codeId, ItemCode = m.ItemCode, ItemDescription = m.ItemDescription, ItemTypeId = m.itemTypeId, CustId = m.custId }).Where(m => m.ItemTypeId == ItemTypeId && m.CustId == PRCustId).OrderBy(m => m.ItemCode).ToList();
                foreach (var item in ItemCodeQuery)
                {
                    html += "<option value='" + item.CodeId + "' >" + item.ItemDescription + "</option>";
                }
            }                        
            html += "</select>";
            
            return Json(new { html,  }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetItemCodeInfo(int CodeId, int PRCustId)
        {
            var ItemCodeInfo = db.PopulateItemLists.Select(m => new PRItemsTable()
            {
                UOM = m.UoM,
                CodeId = m.codeId,
                CustId = m.custId,
                ItemTypeId = m.itemTypeId
            }).Where(m => m.CodeId == CodeId && m.CustId == PRCustId).FirstOrDefault();

            if (ItemCodeInfo.ItemTypeId == 1)
            {
                ItemCodeInfo.BudgetAmount = db.GLs.First(m => m.codeId == ItemCodeInfo.CodeId).budgetAmount;
                ItemCodeInfo.UtilizedToDate = db.GLs.First(m => m.codeId == ItemCodeInfo.CodeId).budgetUtilized;
                if (ItemCodeInfo.BudgetAmount == null)
                {
                    ItemCodeInfo.BudgetAmount = 0;
                }
                if (ItemCodeInfo.UtilizedToDate == null)
                {
                    ItemCodeInfo.UtilizedToDate = 0;
                }
            }

            return Json(new { ItemCodeInfo }, JsonRequestBehavior.AllowGet);

        }

        public JsonResult GetTaxCode(int TaxCodeId, int PRCustId)
        {
            if (TaxCodeId == 0)
            {
                TaxCodeId = PRCustId;
            }
            var TaxCodeInfo = db.TaxCodes.Select(m => new PRItemsTable()
            {
                TaxCodeId = m.TaxCodeId,
                SST = m.GST,
                CustId = m.CustId
            }).Where(m => m.TaxCodeId == TaxCodeId && m.CustId == PRCustId).FirstOrDefault();

            return Json(new { TaxCodeInfo }, JsonRequestBehavior.AllowGet);

        }        

        public JsonResult GetJobNoInfo(string JobNoId, int Selectlistid)
        {
            int CustId = Int32.Parse(Session["CompanyId"].ToString());
            var ItemCodeQuery = db.JobTasks.Select(m => new { JobTaskNoId = m.jobTaskNoId, JobNo = m.jobNo, JobTaskNo = m.jobTaskNo, CustId = m.custId }).Where(m => m.JobNo == JobNoId && m.CustId == CustId).ToList();

            string html = "<select class='JobTaskNoId custom-select g-height-40 g-color-black g-color-black--hover text-left g-rounded-20' data-val-required='The Job Task No field is required.' id='JobTaskNoId" + Selectlistid + "'><option value='' >Select job task no here</option>";
            for (int i = 0; i < ItemCodeQuery.Count; i++)
            {
                html += "<option value='" + ItemCodeQuery[i].JobTaskNoId + "' >" + ItemCodeQuery[i].JobTaskNo + "</option>";
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

        public JsonResult GetUoM(int Selectlistid, int PRCustId)
        {
            var UoMCodeQuery = db.UOMs.Select(m => new { UoMId = m.UoMId, UoMCode = m.UoMCode, Description = m.Description, CustId = m.CustId }).Where(m => m.CustId == PRCustId).OrderBy(m => m.UoMCode).ToList();

            string html = "<select class='UOM custom-select g-height-40 g-color-black g-color-black--hover text-left g-rounded-20' data-val-required='The UoM Code field is required.' id='UoMCodeId" + Selectlistid + "' name='item.UoMCodeId'><option value='' >Select UOM Code here</option>";
            for (int i = 0; i < UoMCodeQuery.Count; i++)
            {
                html += "<option value='" + UoMCodeQuery[i].UoMId + "' >" + UoMCodeQuery[i].UoMCode + "</option>";
            }
            html += "</select>";

            return Json(new { html }, JsonRequestBehavior.AllowGet);
        }

        [CheckSessionOut]
        public ActionResult NewPR(string type)
        {
            int CustId = Int32.Parse(Session["CompanyId"].ToString()); int ChildCustId;
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

            model.NewPRForm = new NewPRModel();
            model.NewPRForm.PRItemListObject = new List<PRItemsTable>
                                                {
                                                    new PRItemsTable
                                                    {
                                                        ItemsId = 0,
                                                        DateRequired = DateTime.Now,
                                                        Description = null,
                                                        CustPONo = null,
                                                        Quantity = 0,
                                                        UnitPrice = decimal.Parse("0.00"),
                                                        TotalPrice = decimal.Parse("0.00")
                                                    }
                                                };
            
            if (Session["ChildCompanyId"] != null)
            {
                ChildCustId = Int32.Parse(Session["ChildCompanyId"].ToString());
                switch (ChildCustId)
                {
                    case 16:
                        CustId = 6; break;
                    case 17:
                        CustId = 7; break;
                    case 18:
                        CustId = 8; break;
                }
                model.NewPRForm.ChildCustId = ChildCustId;
            }

            IOrderedQueryable<dynamic> ProjectNameQuery = null;
            if (UserId == 258)
            {
                ProjectNameQuery = (from m in db.Projects
                                    join n in db.Customers on m.custId equals n.custId
                                    where m.projectCode == "HQ"
                                    select new
                                    {
                                        ProjectId = m.projectId,
                                        Dimension = m.dimension,
                                        Description = m.dimension + " - " + m.projectCode + " (" + n.name + ")",
                                        Code = m.projectCode,
                                    }).OrderBy(c => c.Dimension).ThenBy(c => c.Code);
            } else
            {
                ProjectNameQuery = (from m in db.Projects
                                    where m.custId == CustId
                                    select new
                                    {
                                        ProjectId = m.projectId,
                                        Dimension = m.dimension,
                                        Description = m.dimension + " - " + m.projectCode,
                                        Code = m.projectCode,
                                    }).OrderBy(c => c.Dimension).ThenBy(c => c.Code);
            }
            
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
            var ItemTypeListQuery = db.ItemTypes.Select(m => new { ItemTypeId = m.itemTypeId, ItemType = m.type }).OrderBy(m => m.ItemType);
            var ItemCodeListQuery = db.PopulateItemLists.Select(c => new { CodeId = c.codeId, ItemCode = c.ItemCode, ItemDesc = c.ItemDescription, CustId = c.custId }).Where(c => c.CustId == CustId).OrderBy(c => c.ItemCode);
            var UOMListQuery = db.UOMs.Select(m => new { UoMId = m.UoMId, UoMCode = m.UoMCode, Description = m.Description, CustId = m.CustId }).Where(m => m.CustId == CustId).OrderBy(m => m.UoMCode);
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
            int i = 1;
            foreach (var item in model.NewPRForm.PRItemListObject)
            {
                ViewData["ItemCodeList" + i] = new SelectList(ItemCodeListQuery.AsEnumerable(), "codeId", "itemDesc");
                ViewData["ItemTypeList" + i] = new SelectList(ItemTypeListQuery.AsEnumerable(), "itemTypeId", "itemType");
                ViewData["UOMList" + i] = new SelectList(UOMListQuery.AsEnumerable(), "UoMId", "UoMCode");
                i++;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAjax]
        public JsonResult NewPR(PRModel model)
        {

            if (User.Identity.IsAuthenticated && Session["UserId"] != null)
            {
                try
                {        
                    model.CustId = Int32.Parse(Session["CompanyId"].ToString());
                    model.NewPRForm.PreparedById = Int32.Parse(Session["UserId"].ToString());
                    model.FullName = Session["FullName"].ToString();
                    model.UserId = Int32.Parse(Session["UserId"].ToString());

                    PRSaveDbLogic(model);

                    int? ProjectId = db.Projects.FirstOrDefault(m => m.projectId == model.NewPRForm.ProjectId).projectId;
                    FileUpload fileUploadModel = new FileUpload();
                    FileUpload fileUploadModel1 = new FileUpload();
                    DateTime createDate = DateTime.Now;

                    if (model.PaperRefNoFile != null && model.NewPRForm.PaperRefNo != null)
                    {
                        try
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
                                PRId = model.PRId,
                                ProjectId = ProjectId
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
                            db.SaveChanges();
                        }
                        catch (DbEntityValidationException e)
                        {
                            foreach (var eve in e.EntityValidationErrors)
                            {
                                Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                                    eve.Entry.Entity.GetType().Name, eve.Entry.State);
                                foreach (var ve in eve.ValidationErrors)
                                {
                                    Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                        ve.PropertyName, ve.ErrorMessage);
                                }
                            }
                            throw;
                        }
                    }
                    if (model.NewPRForm.BidWaiverRefNo != null && model.BidWaiverRefNoFile != null)
                    {
                        try
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
                        catch (DbEntityValidationException e)
                        {
                            foreach (var eve in e.EntityValidationErrors)
                            {
                                Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                                    eve.Entry.Entity.GetType().Name, eve.Entry.State);
                                foreach (var ve in eve.ValidationErrors)
                                {
                                    Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                        ve.PropertyName, ve.ErrorMessage);
                                }
                            }
                            throw;
                        }
                    }

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
                            NewPR = true
                            //view = this.RenderPartialView("NewPR", model)
                        },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                catch (DbEntityValidationException e)
                {
                    StringWriter ExMessage = new StringWriter();
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        ExMessage.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            ExMessage.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                ve.PropertyName, ve.ErrorMessage);
                        }
                    }
                    ExMessage.Close();
                    return Json(new { success = false, exception = true, message = ExMessage.ToString() });
                }

            }
            else
            {
                var redirectUrl = new UrlHelper(Request.RequestContext).Action("Index", "Home", new { /* no params */ });
                return Json(new { success = false, url = redirectUrl });
            }
        }

        [CheckSessionOut]
        public ActionResult PRList(string type)
        {
            if (User.Identity.IsAuthenticated && Session["UserId"] != null)
            {
                int UserId = Int32.Parse(Session["UserId"].ToString());
                var ifRequestor = Session["ifRequestor"];
                var ifHOD = Session["ifHOD"];
                var ifFinance = Session["ifFinance"];
                var ifProcurement = Session["ifProcurement"];
                var ifHOC = Session["ifHOC"];
                var ifAdmin = Session["ifAdmin"];
                var ifSuperAdmin = Session["ifSuperAdmin"];
                var ifIT = Session["ifIT"];
                var ifPMO = Session["ifPMO"];
                var ifHSE = Session["ifHSE"];
                var ifHOGPSS = Session["ifHOGPSS"];
                var ifCFO = Session["ifCFO"];
                var ifGMD = Session["ifGMD"];
                //var ifProcurement = KUBHelper.CheckRole("R03") ? true : false;
                int CustId = Int32.Parse(Session["CompanyId"].ToString());
                PRModel PRList = new PRModel();

                while (ifSuperAdmin != null || ifHOGPSS != null || ifCFO != null || ifGMD != null)
                {
                    PRList.PRListObject = (from m in db.PurchaseRequisitions
                                           join n in db.Users on m.PreparedById equals n.userId into ah
                                           join o in db.Vendors on m.VendorId equals o.vendorId into q
                                           join p in db.PRStatus on m.StatusId equals p.statusId
                                           join t in db.Projects on m.ProjectId equals t.projectId
                                           join s in db.Customers on t.custId equals s.custId
                                           from r in q.DefaultIfEmpty()
                                           from ai in ah.DefaultIfEmpty()
                                           where m.PRType == type
                                           select new PRListTable()
                                           {
                                               PRId = m.PRId,
                                               PRNo = m.PRNo,
                                               PRDate = m.PreparedDate,
                                               CompanyName = s.name,
                                               RequestorName = ai.firstName + " " + ai.lastName,
                                               DeptName = ai.department,
                                               VendorCompany = r.name,
                                               AmountRequired = m.AmountRequired,
                                               //PRAging = m.aging,
                                               Status = p.status
                                           }).OrderByDescending(m => m.PRId).ToList();
                    PRList.Type = type;

                    return View("PRList", PRList);
                }

                List<PRListTable> getPRList = new List<PRListTable>();
                if (ifRequestor != null)
                {
                    getPRList = (from m in db.PurchaseRequisitions
                                 join n in db.Users on m.PreparedById equals n.userId
                                 join o in db.Vendors on m.VendorId equals o.vendorId into q
                                 join p in db.PRStatus on m.StatusId equals p.statusId
                                 join t in db.Projects on m.ProjectId equals t.projectId
                                 join s in db.Customers on t.custId equals s.custId
                                 from r in q.DefaultIfEmpty()
                                 where m.PRType == type && m.PreparedById == UserId && m.CustId == CustId
                                 select new PRListTable()
                                 {
                                     PRId = m.PRId,
                                     PRNo = m.PRNo,
                                     PRDate = m.PreparedDate,
                                     CompanyName = s.name,
                                     RequestorName = n.firstName + " " + n.lastName,
                                     DeptName = n.department,
                                     VendorCompany = r.name,
                                     AmountRequired = m.AmountRequired,
                                     //PRAging = m.aging,
                                     Status = p.status
                                 }).OrderByDescending(m => m.PRId).ToList();
                }
                if (ifHOC != null)
                {
                    getPRList = getPRList.Union((from m in db.PurchaseRequisitions
                                                  join n in db.Users on m.PreparedById equals n.userId
                                                  join o in db.Vendors on m.VendorId equals o.vendorId into q
                                                  join p in db.PRStatus on m.StatusId equals p.statusId
                                                  from s in db.PR_Approver.Where(x => x.PRId == m.PRId && x.approverId == UserId)
                                                 join t in db.Projects on m.ProjectId equals t.projectId
                                                 join u in db.Customers on t.custId equals u.custId
                                                  from r in q.DefaultIfEmpty()
                                                  where m.PRType == type
                                                  select new PRListTable()
                                                  {
                                                      PRId = m.PRId,
                                                      PRNo = m.PRNo,
                                                      PRDate = m.PreparedDate,
                                                      CompanyName = u.name,
                                                      RequestorName = n.firstName + " " + n.lastName,
                                                      DeptName = n.department,
                                                      VendorCompany = r.name,
                                                      AmountRequired = m.AmountRequired,
                                                      //PRAging = m.aging,
                                                      Status = p.status
                                                  }).OrderByDescending(m => m.PRId).ToList()).OrderByDescending(m => m.PRId).ToList();
                    getPRList = getPRList.Union((from m in db.PurchaseRequisitions
                                                  join n in db.Users on m.PreparedById equals n.userId
                                                  join o in db.Vendors on m.VendorId equals o.vendorId into q
                                                  join p in db.PRStatus on m.StatusId equals p.statusId
                                                  from t in db.PR_RecommenderHOC.Where(x => x.PRId == m.PRId && x.recommenderId == UserId)
                                                 join v in db.Projects on m.ProjectId equals v.projectId
                                                 join u in db.Customers on v.custId equals u.custId
                                                  from r in q.DefaultIfEmpty()
                                                  where m.PRType == type
                                                  select new PRListTable()
                                                  {
                                                      PRId = m.PRId,
                                                      PRNo = m.PRNo,
                                                      PRDate = m.PreparedDate,
                                                      CompanyName = u.name,
                                                      RequestorName = n.firstName + " " + n.lastName,
                                                      DeptName = n.department,
                                                      VendorCompany = r.name,
                                                      AmountRequired = m.AmountRequired,
                                                      //PRAging = m.aging,
                                                      Status = p.status
                                                  }).OrderByDescending(m => m.PRId).ToList()).OrderByDescending(m => m.PRId).ToList();
                }                
                if (ifFinance != null)
                {
                    getPRList = getPRList.Union((from m in db.PurchaseRequisitions
                                                  join n in db.Users on m.PreparedById equals n.userId
                                                  join o in db.Vendors on m.VendorId equals o.vendorId into q
                                                  join p in db.PRStatus on m.StatusId equals p.statusId
                                                  from s in db.PR_Finance.Where(x => x.PRId == m.PRId && x.reviewerId == UserId)
                                                 join t in db.Projects on m.ProjectId equals t.projectId
                                                 join v in db.Customers on t.custId equals v.custId
                                                  from r in q.DefaultIfEmpty()
                                                  where m.PRType == type && m.CustId == CustId
                                                  select new PRListTable()
                                                  {
                                                      PRId = m.PRId,
                                                      PRNo = m.PRNo,
                                                      PRDate = m.PreparedDate,
                                                      CompanyName = v.name,
                                                      RequestorName = n.firstName + " " + n.lastName,
                                                      DeptName = n.department,
                                                      VendorCompany = r.name,
                                                      AmountRequired = m.AmountRequired,
                                                      //PRAging = m.aging,
                                                      Status = p.status
                                                  }).OrderByDescending(m => m.PRId).ToList()).OrderByDescending(m => m.PRId).ToList();
                }
                if (ifIT != null || ifPMO != null || ifHSE != null || ifAdmin != null)
                {
                    getPRList = getPRList.Union((from m in db.PurchaseRequisitions
                                                  join n in db.Users on m.PreparedById equals n.userId
                                                  join o in db.Vendors on m.VendorId equals o.vendorId into q
                                                  join p in db.PRStatus on m.StatusId equals p.statusId
                                                  from s in db.PR_Reviewer.Where(x => x.PRId == m.PRId && x.reviewerId == UserId)
                                                 join t in db.Projects on m.ProjectId equals t.projectId
                                                 join u in db.Customers on t.custId equals u.custId
                                                  from r in q.DefaultIfEmpty()
                                                  where m.PRType == type
                                                  select new PRListTable()
                                                  {
                                                      PRId = m.PRId,
                                                      PRNo = m.PRNo,
                                                      PRDate = m.PreparedDate,
                                                      CompanyName = u.name,
                                                      RequestorName = n.firstName + " " + n.lastName,
                                                      DeptName = n.department,
                                                      VendorCompany = r.name,
                                                      AmountRequired = m.AmountRequired,
                                                      //PRAging = m.aging,
                                                      Status = p.status
                                                  }).OrderByDescending(m => m.PRId).ToList()).OrderByDescending(m => m.PRId).ToList();
                    getPRList = getPRList.Union((from m in db.PurchaseRequisitions
                                                  join n in db.Users on m.PreparedById equals n.userId
                                                  join o in db.Vendors on m.VendorId equals o.vendorId into q
                                                  join p in db.PRStatus on m.StatusId equals p.statusId
                                                  from t in db.PR_Recommender.Where(x => x.PRId == m.PRId && x.recommenderId == UserId)
                                                 join v in db.Projects on m.ProjectId equals v.projectId
                                                 join u in db.Customers on v.custId equals u.custId
                                                  from r in q.DefaultIfEmpty()
                                                  where m.PRType == type
                                                  select new PRListTable()
                                                  {
                                                      PRId = m.PRId,
                                                      PRNo = m.PRNo,
                                                      PRDate = m.PreparedDate,
                                                      CompanyName = u.name,
                                                      RequestorName = n.firstName + " " + n.lastName,
                                                      DeptName = n.department,
                                                      VendorCompany = r.name,
                                                      AmountRequired = m.AmountRequired,
                                                      //PRAging = m.aging,
                                                      Status = p.status
                                                  }).OrderByDescending(m => m.PRId).ToList()).OrderByDescending(m => m.PRId).ToList();
                }
                if (ifHOD != null)
                {
                    getPRList = getPRList.Union((from m in db.PurchaseRequisitions
                                                  join n in db.Users on m.PreparedById equals n.userId
                                                  join o in db.Vendors on m.VendorId equals o.vendorId into q
                                                  join p in db.PRStatus on m.StatusId equals p.statusId
                                                  from s in db.PR_HOD.Where(x => x.PRId == m.PRId && x.HODId == UserId)
                                                 join t in db.Projects on m.ProjectId equals t.projectId
                                                 join v in db.Customers on t.custId equals v.custId
                                                  from r in q.DefaultIfEmpty()
                                                  where m.PRType == type && m.CustId == CustId
                                                  select new PRListTable()
                                                  {
                                                      PRId = m.PRId,
                                                      PRNo = m.PRNo,
                                                      PRDate = m.PreparedDate,
                                                      CompanyName = v.name,
                                                      RequestorName = n.firstName + " " + n.lastName,
                                                      DeptName = n.department,
                                                      VendorCompany = r.name,
                                                      AmountRequired = m.AmountRequired,
                                                      //PRAging = m.aging,
                                                      Status = p.status
                                                  }).OrderByDescending(m => m.PRId).ToList()).OrderByDescending(m => m.PRId).ToList();
                }
                if (ifProcurement != null)
                {
                    getPRList = getPRList.Union((from m in db.PurchaseRequisitions
                                                  join n in db.Users on m.PreparedById equals n.userId
                                                  join o in db.Vendors on m.VendorId equals o.vendorId into q
                                                  join p in db.PRStatus on m.StatusId equals p.statusId
                                                  from s in db.PR_Admin.Where(x => x.PRId == m.PRId && x.adminId == UserId)
                                                 join v in db.Projects on m.ProjectId equals v.projectId
                                                 join t in db.Customers on v.custId equals t.custId
                                                  from r in q.DefaultIfEmpty()
                                                  where m.PRType == type
                                                  select new PRListTable()
                                                  {
                                                      PRId = m.PRId,
                                                      PRNo = m.PRNo,
                                                      PRDate = m.PreparedDate,
                                                      CompanyName = t.name,
                                                      RequestorName = n.firstName + " " + n.lastName,
                                                      DeptName = n.department,
                                                      VendorCompany = r.name,
                                                      AmountRequired = m.AmountRequired,
                                                      //PRAging = m.aging,
                                                      Status = p.status
                                                  }).OrderByDescending(m => m.PRId).ToList()).OrderByDescending(m => m.PRId).ToList();
                }

                    getPRList = getPRList.Union((from m in db.PurchaseRequisitions
                                                 join n in db.Users on m.PreparedById equals n.userId
                                                 join o in db.Vendors on m.VendorId equals o.vendorId into q
                                                 join p in db.PRStatus on m.StatusId equals p.statusId
                                                 from s in db.PR_Approver.Where(x => x.PRId == m.PRId && x.approverId == UserId)
                                                 join t in db.Projects on m.ProjectId equals t.projectId
                                                 join u in db.Customers on t.custId equals u.custId
                                                 from r in q.DefaultIfEmpty()
                                                 where m.PRType == type
                                                 select new PRListTable()
                                                 {
                                                     PRId = m.PRId,
                                                     PRNo = m.PRNo,
                                                     PRDate = m.PreparedDate,
                                                     CompanyName = u.name,
                                                     RequestorName = n.firstName + " " + n.lastName,
                                                     DeptName = n.department,
                                                     VendorCompany = r.name,
                                                     AmountRequired = m.AmountRequired,
                                                     //PRAging = m.aging,
                                                     Status = p.status
                                                 }).OrderByDescending(m => m.PRId).ToList()).OrderByDescending(m => m.PRId).ToList();
                
                int PRId = 0; int newPRId = 0;
                for (int i = 0; i <= getPRList.Count - 1; i++)
                {
                    newPRId = getPRList[i].PRId;
                    if (PRId == getPRList[i].PRId)
                    {
                        getPRList.RemoveAt(i);
                    }
                    PRId = newPRId;
                }
                PRList.PRListObject = getPRList;
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

        [CheckSessionOut]
        public ActionResult PRTabs()
        {
            PRModel PRTabs = new PRModel();
            if (Session["PRId"] == null)
            {
                PRTabs.PRId = Int32.Parse(Request["PRId"].ToString());
                PRTabs.Type = Request["PRType"].ToString();
                Session["PRId"] = PRTabs.PRId; Session["PRType"] = PRTabs.Type;
            }
            else
            {
                PRTabs.PRId = Int32.Parse(Session["PRId"].ToString());
                PRTabs.Type = Session["PRType"].ToString();
            }
            PRTabs.UserId = Int32.Parse(Session["UserId"].ToString());
            PRTabs.NewPRForm = (from m in db.PurchaseRequisitions
                                join n in db.Users on m.PreparedById equals n.userId
                                where m.PRId == PRTabs.PRId
                                select new NewPRModel()
                                {
                                    PRNo = m.PRNo,
                                    ChildCustId = n.childCompanyId,
                                    Phase1Completed = m.Phase1Completed
                                }).FirstOrDefault();

            return View(PRTabs);
        }

        public ActionResult PRDetails()
        {
            if (User.Identity.IsAuthenticated && Session["UserId"] != null)
            {
                int ChildCustId = 0;
                PRModel PRDetail = new PRModel();
                PRDetail.PRId = Int32.Parse(Session["PRId"].ToString());
                PRDetail.Type = Session["PRType"].ToString();
                PRDetail.UserId = Int32.Parse(Session["UserId"].ToString());
                //PRDetail.CustId = Int32.Parse(Session["CompanyId"].ToString());
                PRDetail.CustId = (from m in db.PurchaseRequisitions
                                   join n in db.Projects on m.ProjectId equals n.projectId
                                   where m.PRId == PRDetail.PRId
                                   select new PRModel()
                                   {
                                       CustId = n.custId
                                   }).First().CustId;
                //if (Session["ChildCompanyId"] != null && PRDetail.CustId != 2)
                //{
                //    ChildCustId = Int32.Parse(Session["ChildCompanyId"].ToString());
                //    switch (ChildCustId)
                //    {
                //        case 16:
                //            PRDetail.CustId = 6; break;
                //        case 17:
                //            PRDetail.CustId = 7; break;
                //        case 18:
                //            PRDetail.CustId = 8; break;
                //    }

                //}
                //else
                //{
                //    var GetChildCustId = (from m in db.PurchaseRequisitions
                //                       join n in db.Users on m.PreparedById equals n.userId
                //                       where m.PRId == PRDetail.PRId
                //                          select new { ChildCustId = n.childCompanyId, CustId = n.companyId.Value }).FirstOrDefault();
                //    if (GetChildCustId != null)
                //    {
                //        switch (GetChildCustId.ChildCustId)
                //        {
                //            case 16:
                //                PRDetail.CustId = 6; break;
                //            case 17:
                //                PRDetail.CustId = 7; break;
                //            case 18:
                //                PRDetail.CustId = 8; break;
                //            default:
                //                PRDetail.CustId = GetChildCustId.CustId; break;
                //        }
                //    }
                //}

                IOrderedQueryable<dynamic> ProjectNameQuery = null;
                if (PRDetail.UserId == 258)
                {
                    ProjectNameQuery = (from m in db.Projects
                                        join n in db.Customers on m.custId equals n.custId
                                        where m.projectCode == "HQ"
                                        select new
                                        {
                                            ProjectId = m.projectId,
                                            Dimension = m.dimension,
                                            Description = m.dimension + " - " + m.projectCode + " (" + n.name + ")",
                                            Code = m.projectCode,
                                        }).OrderBy(c => c.Dimension).ThenBy(c => c.Code);
                }
                else
                {
                    ProjectNameQuery = (from m in db.Projects
                                        where m.custId == PRDetail.CustId
                                        select new
                                        {
                                            ProjectId = m.projectId,
                                            Dimension = m.dimension,
                                            Description = m.dimension + " - " + m.projectCode,
                                            Code = m.projectCode,
                                        }).OrderBy(c => c.Dimension).ThenBy(c => c.Code);
                }

                
                List < SelectListItem > BudgetedList = new List<SelectListItem>();
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
                var VendorListQuery = (from m in db.Vendors
                                       where m.custId == PRDetail.CustId
                                       select new
                                       {
                                           VendorId = m.vendorId,
                                           VendorName = m.vendorNo + " - " + m.name,
                                           VendorNo = m.vendorNo,
                                       }).OrderBy(c => c.VendorNo).ThenBy(c => c.VendorName);
                var VendorStaffQuery = db.VendorStaffs.Select(c => new { StaffId = c.staffId, VendorContactName = c.vendorContactName }).OrderBy(c => c.VendorContactName);
                var ItemTypeListQuery = db.ItemTypes.Select(m => new { ItemTypeId = m.itemTypeId, ItemType = m.type }).OrderBy(m => m.ItemType);
                var ItemCodeListQuery = db.PopulateItemLists.Select(c => new { CodeId = c.codeId, ItemTypeId = c.itemTypeId, ItemCode = c.ItemCode, ItemDesc = c.ItemDescription, CustId = c.custId }).Where(c => c.CustId == PRDetail.CustId).OrderBy(c => c.ItemCode);
                var JobNoListQuery = db.Projects.Select(m => new { JobNoId = m.projectId, JobNo = m.projectCode, CustId = m.custId, Dimension = m.dimension }).Where(m => m.CustId == PRDetail.CustId).Where(m => m.Dimension == "PROJECT").OrderBy(m => m.JobNo);
                var JobTaskListQuery = db.JobTasks.Select(m => new { JobTaskNoId = m.jobTaskNoId, JobTaskNo = m.jobTaskNo, CustId = m.custId }).Where(m => m.CustId == PRDetail.CustId).OrderBy(m => m.JobTaskNoId);
                var TaxCodeListQuery = db.TaxCodes.Select(m => new { TaxCodeId = m.TaxCodeId, Code = m.Code, CustId = m.CustId }).Where(m => m.CustId == PRDetail.CustId).OrderBy(m => m.Code);
                var DimProjectListQuery = db.Projects.Select(m => new { ProjectId = m.projectId, Dimension = m.dimension, Description = m.dimension + " - " + m.projectCode, Code = m.projectCode, CustId = m.custId }).Where(m => m.Dimension == "PROJECT").Where(m => m.CustId == PRDetail.CustId).OrderBy(m => m.Code);
                var DimDeptListQuery = db.Projects.Select(m => new { ProjectId = m.projectId, Dimension = m.dimension, Description = m.dimension + " - " + m.projectCode, Code = m.projectCode, CustId = m.custId }).Where(m => m.Dimension == "DEPARTMENT").Where(m => m.CustId == PRDetail.CustId).OrderBy(m => m.Code);
                var UOMListQuery = db.UOMs.Select(m => new { UoMId = m.UoMId, UoMCode = m.UoMCode, Description = m.Description, CustId = m.CustId }).Where(m => m.CustId == PRDetail.CustId).OrderBy(m => m.UoMCode);

                PRDetail.ChildCustId = ChildCustId;
                IOrderedQueryable<GroupList> SpecReviewerQuery = null;
                if ((PRDetail.CustId == 3 || PRDetail.CustId == 2) && ChildCustId == 16)
                {
                    SpecReviewerQuery = db.Groups.Select(c => new GroupList { GroupId = c.groupId, GroupName = c.groupName }).Where(c => c.GroupId != "G01" && c.GroupId != "G04").OrderBy(c => c.GroupName);
                } else if (PRDetail.CustId == 1)
                {
                    SpecReviewerQuery = db.Groups.Select(c => new GroupList { GroupId = c.groupId, GroupName = c.groupName }).Where(c => c.GroupId != "G02").OrderBy(c => c.GroupName);
                } else
                {
                    SpecReviewerQuery = db.Groups.Select(c => new GroupList { GroupId = c.groupId, GroupName = c.groupName }).OrderBy(c => c.GroupName);
                }
                    
                //var getPRCustId = db.PurchaseRequisitions.First(m => m.PRId == PRDetail.PRId);
                //var getCustId = db.Users.First(m => m.userId == getPRCustId.PreparedById);
                var HOGPSSQuery = getHOGPSSList(PRDetail.CustId);

                PR_PaperApprover checkGMDpaper = db.PR_PaperApprover.FirstOrDefault(m => m.PRId == PRDetail.PRId);
                if (checkGMDpaper != null && checkGMDpaper.finalApproverUserId != null)
                {
                    PRDetail.FinalApproverId = checkGMDpaper.finalApproverUserId.Value;
                }

                //switch (getCustId.childCompanyId)
                //{
                //    case 16:
                //        getPRCustId.CustId = 6; break;
                //    case 17:
                //        getPRCustId.CustId = 7; break;
                //    case 18:
                //        getPRCustId.CustId = 8; break;
                //}
                if (((Session["ifHOGPSS"] != null || Session["ifGMD"] != null || Session["ifCFO"] != null || Session["ifHSE"] != null || Session["ifPMO"] != null) && PRDetail.CustId == 2) || Session["ifSuperAdmin"] != null || Session["ifFinance"] != null || Session["ifHOC"] != null || Session["ifProcurement"] != null)
                {
                    ProjectNameQuery = (from m in db.Projects
                                        where m.custId == PRDetail.CustId
                                        select new
                                        {
                                            ProjectId = m.projectId,
                                            Dimension = m.dimension,
                                            Description = m.dimension + " - " + m.projectCode,
                                            Code = m.projectCode,
                                        }).OrderBy(c => c.Dimension).ThenBy(c => c.Code);
                    VendorListQuery = (from m in db.Vendors
                                       where m.custId == PRDetail.CustId
                                       select new
                                       {
                                           VendorId = m.vendorId,
                                           VendorName = m.vendorNo + " - " + m.name,
                                           VendorNo = m.vendorNo,
                                       }).OrderBy(c => c.VendorNo).ThenBy(c => c.VendorName);
                    ItemCodeListQuery = db.PopulateItemLists.Select(c => new { CodeId = c.codeId, ItemTypeId = c.itemTypeId, ItemCode = c.ItemCode, ItemDesc = c.ItemDescription, CustId = c.custId }).Where(c => c.CustId == PRDetail.CustId).OrderBy(c => c.ItemCode);
                    JobNoListQuery = db.Projects.Select(m => new { JobNoId = m.projectId, JobNo = m.projectCode, CustId = m.custId, Dimension = m.dimension }).Where(m => m.CustId == PRDetail.CustId && m.Dimension == "PROJECT").OrderBy(m => m.JobNo);
                    JobTaskListQuery = db.JobTasks.Select(m => new { JobTaskNoId = m.jobTaskNoId, JobTaskNo = m.jobTaskNo, CustId = m.custId }).Where(m => m.CustId == PRDetail.CustId).OrderBy(m => m.JobTaskNoId);
                    TaxCodeListQuery = db.TaxCodes.Select(m => new { TaxCodeId = m.TaxCodeId, Code = m.Code, CustId = m.CustId }).Where(m => m.CustId == PRDetail.CustId).OrderBy(m => m.Code);
                    DimProjectListQuery = db.Projects.Select(m => new { ProjectId = m.projectId, Dimension = m.dimension, Description = m.dimension + " - " + m.projectCode, Code = m.projectCode, CustId = m.custId }).Where(m => m.Dimension == "PROJECT").Where(m => m.CustId == PRDetail.CustId).OrderBy(m => m.Code);
                    DimDeptListQuery = db.Projects.Select(m => new { ProjectId = m.projectId, Dimension = m.dimension, Description = m.dimension + " - " + m.projectCode, Code = m.projectCode, CustId = m.custId }).Where(m => m.Dimension == "DEPARTMENT").Where(m => m.CustId == PRDetail.CustId).OrderBy(m => m.Code);
                    UOMListQuery = db.UOMs.Select(m => new { UoMId = m.UoMId, UoMCode = m.UoMCode, Description = m.Description, CustId = m.CustId }).Where(m => m.CustId == PRDetail.CustId).OrderBy(m => m.UoMCode);
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
                                          ChildCustId = x.childCompanyId,
                                          Value = a.Budgeted,
                                          VendorId = a.VendorId,
                                          VendorCompanyId = a.VendorCompanyId,
                                          VendorQuoteNo = a.VendorQuoteNo,
                                          VendorEmail = w.vendorEmail,
                                          VendorStaffId = w.staffId,
                                          VendorContactNo = w.vendorContactNo,
                                          PurchaseTypeId = a.PurchaseTypeId,
                                          BudgetDescription = a.BudgetDescription,
                                          BudgetedAmount = a.budgetedAmount,
                                          Justification = a.Justification,
                                          UtilizedToDate = a.utilizedToDate,
                                          AmountRequired = a.AmountRequired,
                                          BudgetBalance = a.budgetBalance,
                                          PaperAttachment = a.PaperAttachment,
                                          PaperVerified = d.paperVerified,
                                          PreparedById = a.PreparedById,
                                          PreparedDate = a.PreparedDate,
                                          Phase1Completed = a.Phase1Completed,
                                          DiscountAmount = a.DiscountAmount,
                                          DiscountPerc = a.Discount_,
                                          TotalExclSST = a.TotalExclSST,
                                          TotalSST = a.TotalSST,
                                          TotalIncSST = a.TotalIncSST,
                                          SpecReviewerId = a.SpecsReviewerId,
                                          Saved = a.Saved,
                                          Submited = a.Submited,
                                          Rejected = a.Rejected,
                                          RejectedRemark = a.RejectedRemark,
                                          StatusId = a.StatusId.Trim(),
                                          PRStatus = c.status,
                                          Scenario = a.Scenario
                                      }).FirstOrDefault();

                var getApprover = (from m in db.PurchaseRequisitions
                                   from s in db.PR_Approver.Where(x => x.PRId == m.PRId && x.approverId == PRDetail.UserId)
                                   where m.PRId == PRDetail.PRId && s.approverId == PRDetail.UserId
                                   select new
                                   {
                                       ApproverId = s.approverId
                                   }).FirstOrDefault();

                if (getApprover != null)
                {
                    PRDetail.NewPRForm.ApproverId = getApprover.ApproverId;
                }

                var getProcurement = (from m in db.PurchaseRequisitions
                                      from s in db.PR_Admin.Where(x => x.PRId == m.PRId && x.adminId == PRDetail.UserId)
                                      where m.PRId == PRDetail.PRId && s.adminId == PRDetail.UserId
                                      select new
                                      {
                                          ProcurementId = s.adminId
                                      }).FirstOrDefault();
                if (getProcurement != null)
                {
                    PRDetail.NewPRForm.AdminId = getProcurement.ProcurementId;
                }

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
                                                           join n in db.UOMs on m.UoMId equals n.UoMId into o
                                                           from p in o.DefaultIfEmpty()
                                                           where m.PRId == PRDetail.PRId
                                                           select new PRItemsTable()
                                                           {
                                                               ItemsId = m.itemsId,
                                                               DateRequired = m.dateRequired,
                                                               Description = m.description,
                                                               CustPONo = m.custPONo,
                                                               Quantity = m.quantity,
                                                               UnitPrice = m.unitPrice,
                                                               TotalPrice = m.totalPrice,
                                                               UoMId = m.UoMId,
                                                               UOM = p.UoMCode
                                                           }).ToList();
                    var QueryNull = db.PopulateItemLists.Select(c => new { CodeId = c.codeId, ItemCode = c.ItemCode, ItemDesc = c.ItemDescription, CustId = c.custId }).Where(c => c.CustId == 0).OrderBy(c => c.ItemCode);
                    int i = 1;
                    foreach (var item in PRDetail.NewPRForm.PRItemListObject)
                    {
                        ViewData["ItemCodeList" + i] = new SelectList(QueryNull.AsEnumerable(), "codeId", "itemDesc");
                        ViewData["ItemTypeList" + i] = new SelectList(ItemTypeListQuery.AsEnumerable(), "itemTypeId", "itemType");
                        if (PRDetail.NewPRForm.StatusId != "PR02")
                        {
                            ViewData["JobNoList" + i] = new SelectList(QueryNull.AsEnumerable(), "jobNoId", "jobNo");
                        } else
                        {
                            ViewData["JobNoList" + i] = new SelectList(JobNoListQuery.AsEnumerable(), "jobNoId", "jobNo");
                        }                       
                        ViewData["JobTaskList" + i] = new SelectList(QueryNull.AsEnumerable(), "jobTaskNoId", "jobTaskNo");
                        ViewData["TaxCodeList" + i] = new SelectList(TaxCodeListQuery.AsEnumerable(), "TaxCodeId", "Code");
                        ViewData["DimProjectList" + i] = new SelectList(DimProjectListQuery.AsEnumerable(), "ProjectId", "Description");
                        ViewData["DimDeptList" + i] = new SelectList(DimDeptListQuery.AsEnumerable(), "ProjectId", "Description");
                        ViewData["UOMList" + i] = new SelectList(UOMListQuery.AsEnumerable(), "UoMId", "UoMCode", item.UoMId);
                        i++;
                    }
                    //}                                    
                }
                else if (checkCodeId.CodeId != null)
                {
                    if (PRDetail.NewPRForm.Submited == 0 || PRDetail.NewPRForm.StatusId == "PR19" || PRDetail.NewPRForm.StatusId == "PR09")
                    {
                        PRDetail.NewPRForm.PRItemListObject = (from m in db.PR_Items
                                                               join n in db.UOMs on m.UoMId equals n.UoMId into o
                                                               from p in o.DefaultIfEmpty()
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
                                                                   UnitPrice = m.unitPrice,
                                                                   TotalPrice = m.totalPrice,
                                                                   TaxCodeId = m.taxCodeId,
                                                                   UnitPriceIncSST = m.unitPriceIncSST,
                                                                   TotalPriceIncSST = m.totalPriceIncSST,
                                                                   DimProjectId = m.dimProjectId,
                                                                   DimDeptId = m.dimDeptId,
                                                                   UoMId = m.UoMId,
                                                                   UOM = p.UoMCode
                                                               }).ToList();
                        var QueryNull = db.PopulateItemLists.Select(c => new { CodeId = c.codeId, ItemCode = c.ItemCode, ItemDesc = c.ItemDescription, CustId = c.custId }).Where(c => c.CustId == 0).OrderBy(c => c.ItemCode);
                        int i = 1;

                        foreach (var item in PRDetail.NewPRForm.PRItemListObject)
                        {
                            ItemCodeListQuery = db.PopulateItemLists.Select(c => new { CodeId = c.codeId, ItemTypeId = c.itemTypeId, ItemCode = c.ItemCode, ItemDesc = c.ItemDescription, CustId = c.custId }).Where(c => c.CustId == PRDetail.CustId && c.ItemTypeId == item.ItemTypeId).OrderBy(c => c.ItemCode);
                            if (PRDetail.NewPRForm.StatusId == "PR19" || PRDetail.NewPRForm.StatusId == "PR09")
                            {
                                if (item.ItemTypeId != null)
                                {
                                    item.ItemType = db.ItemTypes.Where(x => x.itemTypeId == item.ItemTypeId).First().type;
                                }
                                if (item.CodeId != null && item.ItemTypeId != null)
                                {
                                    item.ItemCode = db.PopulateItemLists.Where(x => x.codeId == item.CodeId && x.itemTypeId == item.ItemTypeId && x.custId == PRDetail.CustId).First().ItemDescription;
                                }
                                if (item.UoMId != null)
                                {
                                    item.UOM = db.UOMs.Where(x => x.UoMId == item.UoMId).First().UoMCode;
                                }
                            }
                            else
                            {
                                ViewData["ItemCodeList" + i] = new SelectList(ItemCodeListQuery.AsEnumerable(), "codeId", "itemDesc", item.CodeId);
                                ViewData["ItemTypeList" + i] = new SelectList(ItemTypeListQuery.AsEnumerable(), "itemTypeId", "itemType", item.ItemTypeId);
                                ViewData["JobNoList" + i] = new SelectList(QueryNull.AsEnumerable(), "jobNoId", "jobNo");
                                ViewData["JobTaskList" + i] = new SelectList(QueryNull.AsEnumerable(), "jobTaskNoId", "jobTaskNo");
                                ViewData["TaxCodeList" + i] = new SelectList(QueryNull.AsEnumerable(), "TaxCodeId", "Code");
                                ViewData["DimProjectList" + i] = new SelectList(QueryNull.AsEnumerable(), "ProjectId", "Description");
                                ViewData["DimDeptList" + i] = new SelectList(QueryNull.AsEnumerable(), "ProjectId", "Description");
                                ViewData["UOMList" + i] = new SelectList(UOMListQuery.AsEnumerable(), "UoMId", "UoMCode", item.UoMId);
                                i++;
                            }
                        }
                    }
                    else
                    {
                        PRDetail.NewPRForm.PRItemListObject = (from m in db.PR_Items.DefaultIfEmpty()
                                                               from n in db.PopulateItemLists.Where(x => m.codeId == x.codeId && m.itemTypeId == x.itemTypeId).DefaultIfEmpty()
                                                               join o in db.Projects on m.jobNoId equals o.projectId into p
                                                               join r in db.ItemTypes on m.itemTypeId equals r.itemTypeId into s
                                                               join u in db.JobTasks on m.jobTaskNoId equals u.jobTaskNoId into v
                                                               join x in db.TaxCodes on m.taxCodeId equals x.TaxCodeId into y
                                                               join a in db.UOMs on m.UoMId equals a.UoMId into b
                                                               from q in p.DefaultIfEmpty()
                                                               from t in s.DefaultIfEmpty()
                                                               from w in v.DefaultIfEmpty()
                                                               from z in y.DefaultIfEmpty()
                                                               from c in b.DefaultIfEmpty()
                                                               where m.PRId == PRDetail.PRId
                                                               select new PRItemsTable()
                                                               {
                                                                   ItemsId = m.itemsId,
                                                                   DateRequired = m.dateRequired,
                                                                   ItemTypeId = m.itemTypeId,
                                                                   ItemType = t.type,
                                                                   CodeId = m.codeId,
                                                                   ItemCode = n.Description,
                                                                   Description = m.description,
                                                                   CustPONo = m.custPONo,
                                                                   Quantity = m.quantity,
                                                                   UoMId = m.UoMId,
                                                                   UOM = c.UoMCode,
                                                                   JobNoId = q.projectId,
                                                                   JobNo = q.projectCode,
                                                                   JobTaskNoId = m.jobTaskNoId,
                                                                   JobTaskNo = w.jobTaskNo,
                                                                   UnitPrice = m.unitPrice,
                                                                   TaxCodeId = m.taxCodeId,
                                                                   Code = z.Code,
                                                                   TotalPrice = m.totalPrice,
                                                                   UnitPriceIncSST = m.unitPriceIncSST,
                                                                   TotalPriceIncSST = m.totalPriceIncSST,
                                                                   DimProjectId = m.dimProjectId,
                                                                   DimDeptId = m.dimDeptId
                                                               }).ToList();
                        int i = 1;
                        if (PRDetail.NewPRForm.Scenario > 0 || PRDetail.NewPRForm.StatusId == "PR02" || PRDetail.NewPRForm.StatusId == "PR01" || PRDetail.NewPRForm.StatusId == "PR07" || PRDetail.NewPRForm.StatusId == "PR08" || PRDetail.NewPRForm.StatusId == "PR15" || PRDetail.NewPRForm.StatusId == "PR16")
                        {
                            foreach (var item in PRDetail.NewPRForm.PRItemListObject)
                            {
                                ItemCodeListQuery = db.PopulateItemLists.Select(c => new { CodeId = c.codeId, ItemTypeId = c.itemTypeId, ItemCode = c.ItemCode, ItemDesc = c.ItemDescription, CustId = c.custId }).Where(c => c.CustId == PRDetail.CustId && c.ItemTypeId == item.ItemTypeId).OrderBy(c => c.ItemCode);

                                ViewData["ItemCodeList" + i] = new SelectList(ItemCodeListQuery.AsEnumerable(), "codeId", "itemDesc", item.CodeId);
                                ViewData["ItemTypeList" + i] = new SelectList(ItemTypeListQuery.AsEnumerable(), "itemTypeId", "itemType", item.ItemTypeId);
                                ViewData["JobNoList" + i] = new SelectList(JobNoListQuery.AsEnumerable(), "jobNoId", "jobNo", item.JobNoId);
                                ViewData["JobTaskList" + i] = new SelectList(JobTaskListQuery.AsEnumerable(), "jobTaskNoId", "jobTaskNo", item.JobTaskNoId);
                                ViewData["TaxCodeList" + i] = new SelectList(TaxCodeListQuery.AsEnumerable(), "TaxCodeId", "Code", item.TaxCodeId);
                                ViewData["DimProjectList" + i] = new SelectList(DimProjectListQuery.AsEnumerable(), "ProjectId", "Description", item.DimProjectId);
                                ViewData["DimDeptList" + i] = new SelectList(DimDeptListQuery.AsEnumerable(), "ProjectId", "Description", item.DimDeptId);
                                ViewData["UOMList" + i] = new SelectList(UOMListQuery.AsEnumerable(), "UoMId", "UoMCode",item.UoMId);
                                i++;
                            }
                        }
                    }
                }

                //Session["Details_POIssueDate"] = PODetailList.POListDetail.Details_POIssueDate;
                //Session["Details_Update"] = PODetailList.POListDetail.Details_Update;
                ViewBag.HOGPSSList = new SelectList(HOGPSSQuery.AsEnumerable(), "FinalApproverId", "UserName", PRDetail.FinalApproverId);
                ViewBag.ProjectNameList = new SelectList(ProjectNameQuery.AsEnumerable(), "projectId", "Description", PRDetail.NewPRForm.ProjectId);
                ViewBag.BudgetedList = new SelectList(BudgetedList.AsEnumerable(), "Value", "Text", PRDetail.NewPRForm.Value);
                ViewBag.SpecReviewerList = new SelectList(SpecReviewerQuery.AsEnumerable(), "GroupId", "GroupName", PRDetail.NewPRForm.SpecReviewerId);
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
        [ValidateAjax]
        public JsonResult PRDetails(PRModel PRModel)
        {
            if (User.Identity.IsAuthenticated && Session["UserId"] != null)
            {
                try
                {
                    int CustId = Int32.Parse(Session["CompanyId"].ToString()); int ChildCustId;
                    if (Session["ChildCompanyId"] != null)
                    {
                        ChildCustId = Int32.Parse(Session["ChildCompanyId"].ToString());
                        switch (ChildCustId)
                        {
                            case 16:
                                CustId = 6; break;
                            case 17:
                                CustId = 7; break;
                            case 18:
                                CustId = 8; break;
                        }
                    }
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
                    var SpecReviewerQuery = db.Groups.Select(c => new { GroupId = c.groupId, GroupName = c.groupName }).OrderBy(c => c.GroupName);
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
                    var ItemCodeListQuery = db.PopulateItemLists.Select(c => new { CodeId = c.codeId, ItemCode = c.ItemCode, ItemDesc = c.ItemDescription, CustId = c.custId }).Where(c => c.CustId == CustId).OrderBy(c => c.ItemCode);
                    var JobNoListQuery = db.Projects.Select(m => new { JobNoId = m.projectId, JobNo = m.projectCode, CustId = m.custId, Dimension = m.dimension }).Where(m => m.CustId == CustId).Where(m => m.Dimension == "PROJECT").OrderBy(m => m.JobNo);
                    var JobTaskListQuery = db.JobTasks.Select(m => new { JobTaskId = m.jobTaskNoId, JobTaskNo = m.jobTaskNo, CustId = m.custId }).Where(m => m.CustId == CustId).OrderBy(m => m.JobTaskNo);

                    PRModel.UserId = Int32.Parse(Session["UserId"].ToString());
                    PRModel.FullName = Session["FullName"].ToString();
                    PRModel.CustId = Int32.Parse(Session["CompanyId"].ToString());
                    PRUpdateDbLogic(PRModel);

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
                    ViewBag.SpecReviewerList = new SelectList(SpecReviewerQuery.AsEnumerable(), "GroupId", "GroupName", PRModel.NewPRForm.SpecReviewerId);
                    ViewBag.PurchaseTypeList = new SelectList(PurchaseTypeQuery.AsEnumerable(), "purchaseTypeId", "purchaseType", PRModel.NewPRForm.PurchaseTypeId);
                    //ViewBag.ReviewerNameList = new SelectList(ReviewerNameQuery.AsEnumerable(), "reviewerId", "reviewerName", PRModel.NewPRForm.ReviewerId);
                    //ViewBag.ApproverNameList = new SelectList(ApproverNameQuery.AsEnumerable(), "approverId", "approverName", PRModel.NewPRForm.ApproverId);
                    ViewBag.VendorList = new SelectList(VendorListQuery.AsEnumerable(), "vendorId", "VendorName", PRModel.NewPRForm.VendorId);
                    ViewBag.VendorStaffList = new SelectList(VendorStaffQuery.AsEnumerable(), "staffId", "VendorContactName", PRModel.NewPRForm.VendorStaffId);
                    int i = 1;
                    foreach (var item in PRModel.NewPRForm.PRItemListObject)
                    {
                        ViewData["ItemCodeList" + i] = new SelectList(ItemCodeListQuery.AsEnumerable(), "codeId", "itemDesc", item.CodeId);
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
                            Submited = PRModel.NewPRForm.SelectSubmit
                            //view = this.RenderPartialView("PRDetails", PRModel)
                        },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                catch (DbEntityValidationException e)
                {
                    StringWriter ExMessage = new StringWriter();
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        ExMessage.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            ExMessage.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                ve.PropertyName, ve.ErrorMessage);
                        }
                    }
                    ExMessage.Close();
                    return Json(new { success = false, exception = true, message = ExMessage.ToString() });
                }
            }
            else
            {
                var redirectUrl = new UrlHelper(Request.RequestContext).Action("Index", "Home", new { /* no params */ });
                return Json(new { success = false, url = redirectUrl });
            }
        }

        public ActionResult PRConversations()
        {
            int userId = Int32.Parse(Session["UserId"].ToString());
            int PRId = Int32.Parse(Session["PRId"].ToString());
            int CustId = Int32.Parse(Session["CompanyId"].ToString());

            NotiModel NewNotiList = new NotiModel()
            {
                MessageListModel = getMessageList(PRId, userId)
            };
            //var haha = db.NotificationMsgs.ToList();
            IOrderedEnumerable<NotiMemberList> NotiMemberList = null;

            if (Session["ifSuperAdmin"] != null)
            {
                NotiMemberList = (from m in db.Users
                                  join n in db.Customers on m.companyId equals n.custId into gy
                                  from x in gy.DefaultIfEmpty()
                                  select new NotiMemberList()
                                  {
                                      Id = m.userId,
                                      Name = m.firstName + " " + m.lastName
                                  }).ToList().OrderBy(c => c.Name);
            } else
            {
                NotiMemberList = (from m in db.Users
                                  join n in db.Customers on m.companyId equals n.custId into gy
                                  from x in gy.DefaultIfEmpty()
                                      //where m.status == true && m.companyId == CustId
                                  where m.companyId == CustId
                                  select new NotiMemberList()
                                  {
                                      Id = m.userId,
                                      Name = m.firstName + " " + m.lastName
                                  }).ToList().OrderBy(c => c.Name);
            }
            
            ViewBag.NotiMemberList = new MultiSelectList(NotiMemberList, "Id", "Name");

            return PartialView(NewNotiList);
        }

        [HttpPost]
        public JsonResult SendMessages(int PRId, string Message, List<int> SelectedUserId)
        {
            if (User.Identity.IsAuthenticated && Session["UserId"] != null)
            {
                int fromUserID = Int32.Parse(Session["UserId"].ToString());
                int custId = Int32.Parse(Session["CompanyId"].ToString());
                var myEmail = db.Users.SingleOrDefault(x => x.userId == fromUserID);

                //try
                //{
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

                foreach (var userId in SelectedUserId)
                {
                    var PRInfo = (from m in db.PurchaseRequisitions
                                  join n in db.Users on m.PreparedById equals n.userId
                                  join o in db.Projects on m.ProjectId equals o.projectId
                                  join p in db.Customers on o.custId equals p.custId
                                  join q in db.Vendors on m.VendorId equals q.vendorId into r
                                  from s in r.DefaultIfEmpty()
                                      //join p in db.OpportunityTypes on m.typeId equals p.typeId
                                  where m.PRId == PRId
                                  select new MailModel()
                                  {
                                      //PRId = m.PRId,
                                      CustName = p.name + " (" + p.abbreviation + ")",
                                      //abb = p.abbreviation,
                                      ProjectName = o.projectName,
                                      //Description = o.des,
                                      //PODescription = m.des,
                                      PRNo = m.PRNo,
                                      PRDate = m.PreparedDate,
                                      VendorCompany = s.name,
                                      //ContractType = p.type,
                                      Requestor = n.firstName + " " + n.lastName
                                  }).FirstOrDefault();

                    string templateFile = System.IO.File.ReadAllText(HttpContext.Server.MapPath("~/Views/Shared/MessageEmailTemplate.cshtml"));
                    var UserDetails = (from m in db.Users
                                       where m.userId == userId
                                       select new PRModel()
                                       {
                                           UserName = m.userName,
                                           EmailAddress = m.emailAddress,
                                           FullName = m.firstName + " " + m.lastName
                                       }).First();
                    PRInfo.Content = "~/Home/Index?Username=" + UserDetails.UserName + "&PRId=" + PRId + "&PRType=Generic";
                    PRInfo.Messages = Message;
                    PRInfo.FromName = Session["FullName"].ToString();
                    PRInfo.BackLink = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority,
                            Url.Content(PRInfo.Content));
                    var result = Engine.Razor.RunCompile(new LoadedTemplateSource(templateFile), "MessageTemplateKey", null, PRInfo);

                    MailMessage mail = new MailMessage
                    {
                        //From = new MailAddress("pr-admin@kub.com"),
                        From = new MailAddress("info@kubtel.com"),
                        Subject = "[PR Online] A new message for - " + PRInfo.PRNo,
                        Body = result,
                        IsBodyHtml = true
                    };

                    if (UserDetails.EmailAddress != null)
                    {
                        mail.To.Add(new MailAddress(UserDetails.EmailAddress));
                    }
                    else
                    {
                        mail.To.Add(new MailAddress("muzhafar@kubtel.com"));
                    }

                    SmtpClient smtp = new SmtpClient
                    {
                        //Host = "relay.kub.com",
                        //Host = "mail.kub.local",
                        Host = "outlook.office365.com",
                        //Host = "smtp.gmail.com";
                        Port = 587,
                        //Port = 110,
                        //smtp.Port = 25;
                        EnableSsl = true,
                        Credentials = new System.Net.NetworkCredential("info@kubtel.com", "welcome123$")
                        //Credentials = new System.Net.NetworkCredential("pr-admin@kub.com", "welcome123$")
                    };
                    if (ConfigurationManager.AppSettings["SentEmail"] == "true")
                    {
                        smtp.Send(mail);
                    }

                    NotiGroup _objSendToUserId = new NotiGroup
                    {
                        uuid = Guid.NewGuid(),
                        msgId = _objSendMessage.msgId,
                        toUserId = userId
                    };
                    db.NotiGroups.Add(_objSendToUserId);
                    db.SaveChanges();
                }

                return Json(new { success = true, message = "Sucessfully send the message" });
                //}
                //catch (DbEntityValidationException e)
                //{
                //    StringWriter ExMessage = new StringWriter();
                //    foreach (var eve in e.EntityValidationErrors)
                //    {
                //        ExMessage.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                //            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                //        foreach (var ve in eve.ValidationErrors)
                //        {
                //            ExMessage.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                //                ve.PropertyName, ve.ErrorMessage);
                //        }                      
                //    }
                //    ExMessage.Close();
                //    return Json(new { success = false, exception = true, message = ExMessage.ToString() });
                //}
            }
            else
            {
                var redirectUrl = new UrlHelper(Request.RequestContext).Action("Index", "Home", new { /* no params */ });
                return Json(new { success = false, url = redirectUrl });
            }
        }

        public ActionResult PRFiles()
        {
            PRModel model = new PRModel();
            int userid = Int32.Parse(Session["UserId"].ToString());
            //model.RoleId = Session["RoleId"].ToString();
            model.PRId = Int32.Parse(Session["PRId"].ToString());
            var firstPRList = (from m in db.PurchaseRequisitions
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
                                   Extension = o.Extension,
                                   Description = o.description
                               }).ToList();
            var getProjectId = db.PurchaseRequisitions.First(m => m.PRId == model.PRId).ProjectId;
            var getPaperVerified = db.Projects.FirstOrDefault(m => m.projectId == getProjectId).paperVerified;

            if (getPaperVerified == true)
            {
                model.PRDocListObject = firstPRList.Concat((from m in db.Projects
                                                            join n in db.PRs_FileUpload on m.projectId equals n.ProjectId
                                                            join o in db.FileUploads on n.fileEntryId equals o.fileEntryId
                                                            join p in db.Users on o.uploadUserId equals p.userId
                                                            where m.projectId == getProjectId
                                                            select new PRDocListTable()
                                                            {
                                                                fileEntryId = o.fileEntryId,
                                                                uploadUserName = p.firstName + " " + p.lastName,
                                                                uploadDate = o.uploadDate,
                                                                FileName = o.FileName,
                                                                File = o.File,
                                                                Extension = o.Extension
                                                            }).ToList()).ToList();
            }
            else
            {
                model.PRDocListObject = firstPRList;
            }

            return PartialView(model);
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

        [HttpPost]
        public JsonResult FileDelete(int id)
        {
            if (User.Identity.IsAuthenticated && Session["UserId"] != null)
            {
                FileUpload x = db.FileUploads.First(m => m.fileEntryId == id);
                PRs_FileUpload y = db.PRs_FileUpload.First(m => m.fileEntryId == id);
                db.FileUploads.Remove(x);
                db.PRs_FileUpload.Remove(y);
                //db.SaveChanges();

                NotificationMsg _objSendMessage = new NotificationMsg
                {
                    uuid = Guid.NewGuid(),
                    PRId = Int32.Parse(Session["PRId"].ToString()),
                    msgDate = DateTime.Now,
                    fromUserId = Int32.Parse(Session["UserId"].ToString()),
                    msgType = "Trail",
                    message = Session["FullName"].ToString() + " has delete " + x.FileName
                };
                db.NotificationMsgs.Add(_objSendMessage);
                db.SaveChanges();

                return Json(new { success = true, message = "Sucessfully delete the file" });
            }
            else
            {
                var redirectUrl = new UrlHelper(Request.RequestContext).Action("Index", "Home", new { /* no params */ });
                return Json(new { success = false, url = redirectUrl });
            }
        }

        [HttpPost]
        public JsonResult FileUpload(PRModel fd)
        {
            if (User.Identity.IsAuthenticated && Session["UserId"] != null)
            {
                FileUpload fileUploadModel = new FileUpload();
                int userId = Int32.Parse(Session["UserId"].ToString());
                DateTime createDate = DateTime.Now;
                string filename1 = fd.UploadFile.FileName.Replace("\\", ",");
                string filename = filename1.Split(',')[filename1.Split(',').Length - 1].ToString();
                string fullPath = fd.UploadFile.FileName;
                string extension = Path.GetExtension(fullPath);
                byte[] uploadFile = new byte[fd.UploadFile.InputStream.Length];

                fileUploadModel.uuid = Guid.NewGuid();
                fileUploadModel.uploadUserId = userId;
                fileUploadModel.uploadDate = createDate;
                fileUploadModel.FullPath = fullPath;
                fileUploadModel.FileName = filename;
                fileUploadModel.description = fd.FileDescription;
                fileUploadModel.Extension = extension;
                fileUploadModel.archivedFlag = false;
                fd.UploadFile.InputStream.Read(uploadFile, 0, uploadFile.Length);
                fileUploadModel.File = uploadFile;
                db.FileUploads.Add(fileUploadModel);
                db.SaveChanges();

                PRs_FileUpload fileUploadModel2 = new PRs_FileUpload
                {
                    uuid = Guid.NewGuid(),
                    fileEntryId = fileUploadModel.fileEntryId,
                    PRId = fd.PRId
                };
                db.PRs_FileUpload.Add(fileUploadModel2);

                NotificationMsg _objSendMessage = new NotificationMsg
                {
                    uuid = Guid.NewGuid(),
                    PRId = Int32.Parse(Session["PRId"].ToString()),
                    msgDate = DateTime.Now,
                    fromUserId = Int32.Parse(Session["UserId"].ToString()),
                    msgType = "Trail",
                    message = Session["FullName"].ToString() + " has uploaded " + filename
                };
                db.NotificationMsgs.Add(_objSendMessage);

                db.SaveChanges();

                //return PartialView(db.FileUploads.ToList());
                return Json(new { success = true, message = "File uploaded successfully" });
            }
            else
            {
                var redirectUrl = new UrlHelper(Request.RequestContext).Action("Index", "Home", new { /* no params */ });
                return Json(new { success = false, url = redirectUrl });
            }
        }

        #region Approval & Rejected Phase 1        
        //public ActionResult Approval(int PRId, string PRType, bool Reviewer, bool Approver)
        [HttpPost]
        public JsonResult ApproveInitialPRReview(PRModel model)
        {
            if (User.Identity.IsAuthenticated && Session["UserId"] != null)
            {
                var PR = db.PurchaseRequisitions.First(x => x.PRId == model.PRId);                
                PR.PurchaseTypeId = model.NewPRForm.PurchaseTypeId;
                PR.BudgetDescription = model.NewPRForm.BudgetDescription;
                PR.Justification = model.NewPRForm.Justification;
                PR.Phase1Completed = true;
                PR.budgetedAmount = model.NewPRForm.BudgetedAmount;
                PR.utilizedToDate = model.NewPRForm.UtilizedToDate;
                PR.budgetBalance = model.NewPRForm.BudgetBalance;
                db.SaveChanges();

                var Project = db.Projects.First(x => x.projectId == model.NewPRForm.ProjectId);
                //Project.budgetedAmount = model.NewPRForm.BudgetedAmount;
                //Project.utilizedToDate = model.NewPRForm.UtilizedToDate;
                //Project.budgetBalance = model.NewPRForm.BudgetBalance;
                //db.SaveChanges();

                int UserId = Int32.Parse(Session["UserId"].ToString());
                PR_Finance SaveReviewerInfo = db.PR_Finance.First(m => m.PRId == PR.PRId && m.reviewerId == UserId);
                SaveReviewerInfo.reviewedDate = DateTime.Now;
                SaveReviewerInfo.reviewed = 1;
                db.SaveChanges();
                
                var requestorDone = (from m in db.NotificationMsgs
                                     join n in db.NotiGroups on m.msgId equals n.msgId
                                     join o in db.Users_Roles on n.toUserId equals o.userId
                                     where m.PRId == model.PRId && m.msgType == "Task" && o.roleId == "R13"
                                     select new PRModel()
                                     {
                                         MsgId = m.msgId
                                     }).ToList();

                foreach(var item in requestorDone)
                {
                    NotificationMsg getDone = db.NotificationMsgs.First(m => m.msgId == item.MsgId);
                    getDone.done = true;
                    db.SaveChanges();
                }
                
                if (PR.PaperAttachment == true && Project.paperVerified == false)
                {
                    var getPaperApprover = (from m in db.Users
                                            join n in db.Users_Roles on m.userId equals n.userId
                                            where n.roleId == "R10"
                                            select new PRModel()
                                            {
                                                UserId = m.userId,
                                                FullName = m.firstName + " " + m.lastName,
                                                EmailAddress = m.emailAddress
                                            }).ToList();

                    PR.StatusId = "PR10";
                    NotificationMsg objTask = new NotificationMsg()
                    {
                        uuid = Guid.NewGuid(),
                        message = PR.PRNo + " pending for you to review the papers",
                        fromUserId = Int32.Parse(Session["UserId"].ToString()),
                        msgDate = DateTime.Now,
                        msgType = "Task",
                        PRId = model.PRId
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
                            PRId = model.PRId
                        };
                        db.PR_PaperApprover.Add(_objHOGPSS);
                        db.SaveChanges();

                        model.UserId = UserId;
                        model.FullName = Session["FullName"].ToString();
                        model.EmailAddress = item.EmailAddress;
                        model.NewPRForm.PRNo = PR.PRNo;
                        model.NewPRForm.ApproverId = item.UserId;
                        model.NewPRForm.ApproverName = item.FullName;
                        SendEmailPRNotification(model, "ToHOGPSSNoti");
                    }
                } else
                {
                    var getPRPreparerChildCustId = db.Users.First(m => m.userId == PR.PreparedById);
                    var getProcurement = new List<PRModel>();
                    if (PR.CustId == 3 && getPRPreparerChildCustId.childCompanyId == 16)
                    {
                        getProcurement = (from m in db.Users
                                          join n in db.Users_Roles on m.userId equals n.userId
                                          where n.roleId == "R03" && (m.companyId == PR.CustId || m.companyId == 2) && m.childCompanyId == getPRPreparerChildCustId.childCompanyId
                                          select new PRModel()
                                          {
                                              UserId = m.userId,
                                              FullName = m.firstName + " " + m.lastName,
                                              EmailAddress = m.emailAddress
                                          }).ToList();
                    }
                    else
                    {
                        getProcurement = (from m in db.Users
                                          join n in db.Users_Roles on m.userId equals n.userId
                                          where n.roleId == "R03" && (m.companyId == PR.CustId && m.childCompanyId != 16)
                                          select new PRModel()
                                          {
                                              UserId = m.userId,
                                              FullName = m.firstName + " " + m.lastName,
                                              EmailAddress = m.emailAddress
                                          }).ToList();
                    }

                    PR.StatusId = "PR02";
                    NotificationMsg objTask = new NotificationMsg()
                    {
                        uuid = Guid.NewGuid(),
                        message = PR.PRNo + " pending for you to procure",
                        fromUserId = Int32.Parse(Session["UserId"].ToString()),
                        msgDate = DateTime.Now,
                        msgType = "Task",
                        PRId = model.PRId
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

                        PRModel x = new PRModel();
                        x.PRId = model.PRId;
                        x.UserId = UserId;
                        x.EmailAddress = item.EmailAddress;
                        x.NewPRForm = new NewPRModel();
                        x.NewPRForm.PRNo = PR.PRNo;
                        x.NewPRForm.ApproverId = item.UserId;
                        x.NewPRForm.ApproverName = item.FullName;
                        SendEmailPRNotification(x, "ToProcurementNoti");
                    }
                }
                
                //PR_HOD getHOD = db.PR_HOD.First(m => m.PRId == PR.PRId);              
                return Json(new { success = true, message = "Successfully review PR" });
            }
            else
            {
                var redirectUrl = new UrlHelper(Request.RequestContext).Action("Index", "Home", new { /* no params */ });
                return Json(new { success = false, url = redirectUrl });
            }
        }

        [HttpPost]
        public JsonResult RejectInitialPRReview()
        {
            if (User.Identity.IsAuthenticated && Session["UserId"] != null)
            {
                try
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
                                         where n.toUserId == UserId && m.PRId == PrId && m.msgType == "Task"
                                         select new PRModel()
                                         {
                                             MsgId = m.msgId
                                         }).First();

                    NotificationMsg getDone = db.NotificationMsgs.First(m => m.msgId == requestorDone.MsgId);
                    getDone.done = true;
                    db.SaveChanges();

                    //PRModel x = new PRModel();
                    //x.PRId = PR.PRId;
                    //x.UserId = UserId;
                    //x.EmailAddress = item.ApproverEmail;
                    //x.NewPRForm = new NewPRModel();
                    //x.NewPRForm.PRNo = PR.PRNo;
                    //x.NewPRForm.PreparedById = PR.PreparedById;
                    //x.NewPRForm.RequestorName = PR.PreparedById;
                    //SendEmailPRNotification(x, "RejectInitialReview");

                    return Json(new { success = true, message = "Successfully reject PR" });
                }
                catch (DbEntityValidationException e)
                {
                    StringWriter ExMessage = new StringWriter();
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        ExMessage.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            ExMessage.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                ve.PropertyName, ve.ErrorMessage);
                        }
                    }
                    ExMessage.Close();
                    return Json(new { success = false, exception = true, message = ExMessage.ToString() });
                }
            }
            else
            {
                var redirectUrl = new UrlHelper(Request.RequestContext).Action("Index", "Home", new { /* no params */ });
                return Json(new { success = false, url = redirectUrl });
            }
        }

        [HttpPost]
        [ValidateAjax]
        public JsonResult Approval(PRModel model)
        {
            if (User.Identity.IsAuthenticated && Session["UserId"] != null)
            {
                PurchaseRequisition objPRDetails = db.PurchaseRequisitions.First(m => m.PRId == model.PRId);
                var objPRItemList = db.PR_Items.ToList().Where(m => m.PRId == model.PRId);
                int UserId = Int32.Parse(Session["UserId"].ToString());
                var requestorDone = (from m in db.NotificationMsgs
                                     join n in db.NotiGroups on m.msgId equals n.msgId
                                     where n.toUserId == UserId && m.PRId == model.PRId && m.msgType == "Task"
                                     select new PRModel()
                                     {
                                         MsgId = m.msgId
                                     }).First();
                int PRCustId = objPRDetails.CustId;
                Project updateProject = db.Projects.First(m => m.projectId == objPRDetails.ProjectId);

                if (model.UserName == "HOD")
                {
                    bool v = (Session["ifHOD"] != null || Session["ifPMO"] != null);
                    if (v && objPRDetails.Phase1Completed == false)
                    {
                        PR_HOD getApprover = db.PR_HOD.First(m => m.PRId == model.PRId && m.HODId == UserId);
                        getApprover.HODApprovedP1 = getApprover.HODApprovedP1 + 1;
                        getApprover.HODApprovedDate1 = DateTime.Now;
                        objPRDetails.StatusId = "PR19";
                        objPRDetails.Phase1Completed = false;

                        NotificationMsg objNotification = new NotificationMsg()
                        {
                            uuid = Guid.NewGuid(),
                            message = Session["FullName"].ToString() + " has approved the PR No: " + objPRDetails.PRNo + " application. ",
                            fromUserId = Int32.Parse(Session["UserId"].ToString()),
                            msgDate = DateTime.Now,
                            msgType = "Trail",
                            PRId = model.PRId
                        };
                        db.NotificationMsgs.Add(objNotification);
                        db.SaveChanges();

                        NotificationMsg getDone = db.NotificationMsgs.First(m => m.msgId == requestorDone.MsgId);
                        getDone.done = true;

                        var getFinance = (from m in db.Users
                                          join n in db.Users_Roles on m.userId equals n.userId
                                          where n.roleId == "R13" && m.companyId == PRCustId
                                          select new NewPRModel()
                                          {
                                              ReviewerId = m.userId,
                                              ReviewerName = m.firstName + " " + m.lastName,
                                              ReviewerEmail = m.emailAddress
                                          }).ToList();

                        foreach (var item in getFinance)
                        {
                            PR_Finance _objReviewer = new PR_Finance
                            {
                                uuid = Guid.NewGuid(),
                                reviewerId = item.ReviewerId.Value,
                                PRId = objPRDetails.PRId,
                                reviewed = model.NewPRForm.Reviewed
                            };
                            db.PR_Finance.Add(_objReviewer);
                            db.SaveChanges();

                            NotificationMsg objTask = new NotificationMsg()
                            {
                                uuid = Guid.NewGuid(),
                                message = objPRDetails.PRNo + " pending for your initial reviewal",
                                fromUserId = UserId,
                                msgDate = DateTime.Now,
                                msgType = "Task",
                                PRId = model.PRId
                            };
                            db.NotificationMsgs.Add(objTask);
                            db.SaveChanges();

                            NotiGroup ReviewerTask = new NotiGroup()
                            {
                                uuid = Guid.NewGuid(),
                                msgId = objTask.msgId,
                                toUserId = item.ReviewerId.Value,
                                resubmit = false
                            };
                            db.NotiGroups.Add(ReviewerTask);
                            db.SaveChanges();

                            model.UserId = UserId;
                            model.FullName = Session["FullName"].ToString();
                            model.EmailAddress = item.ReviewerEmail;
                            model.NewPRForm.PRNo = objPRDetails.PRNo;
                            model.NewPRForm.ApproverId = item.ReviewerId.Value;
                            model.NewPRForm.ApproverName = item.ReviewerName;
                            SendEmailPRNotification(model, "ReviewalInitial");
                        }
                    }
                }
                else if (model.UserName == "ifHOGPSS")
                {
                    PR_PaperApprover getApprover = db.PR_PaperApprover.First(m => m.PRId == model.PRId && m.approverId == UserId);
                    getApprover.approverApproved = getApprover.approverApproved + 1;
                    getApprover.approverApprovedDate = DateTime.Now;
                    getApprover.finalApproverUserId = model.FinalApproverId;
                    objPRDetails.StatusId = "PR02";
                    objPRDetails.Phase1Completed = true;
                    if (updateProject.dimension == "PROJECT")
                    {
                        updateProject.paperVerified = true;
                    }
                    db.SaveChanges();

                    NotificationMsg objNotification = new NotificationMsg()
                    {
                        uuid = Guid.NewGuid(),
                        message = Session["FullName"].ToString() + " has verified BOD/GMD paper for the PR No: " + objPRDetails.PRNo,
                        fromUserId = Int32.Parse(Session["UserId"].ToString()),
                        msgDate = DateTime.Now,
                        msgType = "Trail",
                        PRId = model.PRId
                    };
                    db.NotificationMsgs.Add(objNotification);
                    db.SaveChanges();

                    NotificationMsg getDone = db.NotificationMsgs.First(m => m.msgId == requestorDone.MsgId);
                    getDone.done = true;

                    var getPRPreparerChildCustId = db.Users.First(m => m.userId == objPRDetails.PreparedById);
                    var getProcurement = new List<PRModel>();
                    if (objPRDetails.CustId == 3 && getPRPreparerChildCustId.childCompanyId == 16)
                    {
                        getProcurement = (from m in db.Users
                                          join n in db.Users_Roles on m.userId equals n.userId
                                          where n.roleId == "R03" && (m.companyId == objPRDetails.CustId || m.companyId == 2) && m.childCompanyId == getPRPreparerChildCustId.childCompanyId
                                          select new PRModel()
                                          {
                                              UserId = m.userId,
                                              FullName = m.firstName + " " + m.lastName,
                                              EmailAddress = m.emailAddress
                                          }).ToList();
                    }
                    else
                    {
                        getProcurement = (from m in db.Users
                                          join n in db.Users_Roles on m.userId equals n.userId
                                          where n.roleId == "R03" && (m.companyId == objPRDetails.CustId && m.childCompanyId != 16)
                                          select new PRModel()
                                          {
                                              UserId = m.userId,
                                              FullName = m.firstName + " " + m.lastName,
                                              EmailAddress = m.emailAddress
                                          }).ToList();
                    }

                    NotificationMsg objTask = new NotificationMsg()
                    {
                        uuid = Guid.NewGuid(),
                        message = objPRDetails.PRNo + " pending for you to procure",
                        fromUserId = Int32.Parse(Session["UserId"].ToString()),
                        msgDate = DateTime.Now,
                        msgType = "Task",
                        PRId = model.PRId
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

                        model.UserId = UserId;
                        model.FullName = Session["FullName"].ToString();
                        model.EmailAddress = item.EmailAddress;
                        model.NewPRForm.PRNo = objPRDetails.PRNo;
                        model.NewPRForm.ApproverId = item.UserId;
                        model.NewPRForm.ApproverName = item.FullName;
                        SendEmailPRNotification(model, "ToProcurementNoti");
                    }

                    var getFinalApprover = (from m in db.Users
                                  join n in db.Users_Roles on m.userId equals n.userId
                                  //join o in db.Roles on n.roleId equals o.roleId
                                  where m.userId == model.FinalApproverId
                                  select new PRModel()
                                  {
                                      UserId = n.userId,
                                      FullName = m.firstName + " " + m.lastName,
                                      EmailAddress = m.emailAddress
                                  }).ToList();

                    foreach (var item in getFinalApprover)
                    {
                        PR_Approver saveApprover = new PR_Approver()
                        {
                            uuid = Guid.NewGuid(),
                            approverId = item.UserId,
                            PRId = model.PRId
                        };
                        db.PR_Approver.Add(saveApprover);
                        db.SaveChanges();
                    }
                }

                return Json(new { success = true });
            }
            else
            {
                var redirectUrl = new UrlHelper(Request.RequestContext).Action("Index", "Home", new { /* no params */ });
                return Json(new { success = false, url = redirectUrl });
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
                                     where n.toUserId == UserId && m.PRId == PRId && m.msgType == "Task"
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

                PRModel PRDetail = new PRModel
                {
                    PRId = Int32.Parse(Session["PRId"].ToString()),
                    UserId = Int32.Parse(Session["UserId"].ToString()),
                    Type = Session["PRType"].ToString()
                    //RoleIdList = getRole
                };
                PurchaseRequisition getScenario = db.PurchaseRequisitions.First(m => m.PRId == PRDetail.PRId);
                var getFinalApprover = db.PR_PaperApprover.FirstOrDefault(m => m.PRId == PRDetail.PRId);
                if (getFinalApprover != null)
                {
                    PRDetail.FinalApproverId = getFinalApprover.finalApproverUserId.Value;
                }    
                
                int PRCustId = db.Projects.First(m => m.projectId == getScenario.ProjectId).custId;

                if (getScenario.Scenario == 1 || PRDetail.FinalApproverId != 0)
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
                                          from o in db.PR_Reviewer.Where(x => x.PRId == PRDetail.PRId && x.reviewed == 1).DefaultIfEmpty()
                                          from q in db.PR_Approver.Where(x => x.PRId == PRDetail.PRId && x.approverApproved == 1)
                                          from s in db.PR_HOD.Where(x => x.PRId == PRDetail.PRId && x.HODApprovedP1 == 1)
                                          join t in db.Users on s.HODId equals t.userId
                                          //from x in l.DefaultIfEmpty()
                                          //from v in r.DefaultIfEmpty()
                                          //from w in p.DefaultIfEmpty()
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
                                              BudgetedAmount = a.budgetedAmount,
                                              Justification = a.Justification,
                                              UtilizedToDate = a.utilizedToDate,
                                              TotalIncSST = a.TotalIncSST,
                                              BudgetBalance = a.budgetBalance,
                                              PreparedById = a.PreparedById,
                                              PreparedDate = a.PreparedDate,
                                              HODApproverId = s.HODId,
                                              HODApproverName = t.firstName + " " + t.lastName,
                                              HODApprovedDate1 = s.HODApprovedDate1.Value,
                                              ApproverId = q.approverId,
                                              ApprovedDate = q.approverApprovedDate.Value,
                                              ReviewerId = o.reviewerId,
                                              ReviewedDate = o.reviewedDate,
                                              //HODApprovedDate2 = s.HODApprovedDate2.Value,
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
                    if (PRDetail.NewPRForm.ReviewerId != null && PRDetail.NewPRForm.ReviewerId != 0)
                    {
                        User getFullname = db.Users.First(m => m.userId == PRDetail.NewPRForm.ReviewerId);
                        PRDetail.NewPRForm.ReviewerName = getFullname.firstName + " " + getFullname.lastName;
                    }
                }
                else if (getScenario.Scenario == 2)
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
                                          from o in db.PR_Reviewer.Where(x => x.PRId == PRDetail.PRId && x.reviewed == 1)
                                          from q in db.PR_Approver.Where(x => x.PRId == PRDetail.PRId && x.approverApproved == 1)
                                          from s in db.PR_HOD.Where(x => x.PRId == PRDetail.PRId && x.HODApprovedP1 == 1)
                                          join t in db.Users on s.HODId equals t.userId
                                          from u in db.PR_Recommender.Where(x => x.PRId == PRDetail.PRId && x.recommended == 1).DefaultIfEmpty()
                                              //from v in r.DefaultIfEmpty()
                                              //from w in p.DefaultIfEmpty()
                                              //from x in l.DefaultIfEmpty()
                                          from y in f.DefaultIfEmpty()
                                              //from z in k.DefaultIfEmpty()
                                              //from ab in aa.DefaultIfEmpty()
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
                                              BudgetedAmount = a.budgetedAmount,
                                              Justification = a.Justification,
                                              UtilizedToDate = a.utilizedToDate,
                                              TotalIncSST = a.TotalIncSST,
                                              BudgetBalance = a.budgetBalance,
                                              PreparedById = a.PreparedById,
                                              PreparedDate = a.PreparedDate,
                                              HODApproverId = s.HODId,
                                              HODApproverName = t.firstName + " " + t.lastName,
                                              HODApprovedDate1 = s.HODApprovedDate1.Value,
                                              //HODApprovedDate2 = s.HODApprovedDate2.Value,
                                              ApproverId = q.approverId,
                                              ApprovedDate = q.approverApprovedDate.Value,
                                              ReviewerId = o.reviewerId,
                                              ReviewedDate = o.reviewedDate.Value,
                                              RecommenderId = u.recommenderId,
                                              RecommendDate = u.recommendedDate,
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
                    if (PRDetail.NewPRForm.RecommenderId != null && PRDetail.NewPRForm.RecommenderId != 0)
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
                                          from o in db.PR_Reviewer.Where(x => x.PRId == PRDetail.PRId && x.reviewed == 1)
                                          from q in db.PR_Approver.Where(x => x.PRId == PRDetail.PRId && x.approverApproved == 1)
                                          from s in db.PR_HOD.Where(x => x.PRId == PRDetail.PRId && x.HODApprovedP1 == 1)
                                          join t in db.Users on s.HODId equals t.userId
                                          from u in db.PR_Recommender.Where(x => x.PRId == PRDetail.PRId && x.recommended == 1).DefaultIfEmpty()
                                          from ac in db.PR_RecommenderCFO.Where(x => x.PRId == PRDetail.PRId && x.recommended == 1)
                                          join ad in db.Users on ac.recommenderId equals ad.userId
                                          //from v in r.DefaultIfEmpty()
                                          //from w in p.DefaultIfEmpty()
                                          //from x in l.DefaultIfEmpty()
                                          from y in f.DefaultIfEmpty()
                                          from z in k.DefaultIfEmpty()
                                              //from ab in aa.DefaultIfEmpty()
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
                                              BudgetedAmount = a.budgetedAmount,
                                              Justification = a.Justification,
                                              UtilizedToDate = a.utilizedToDate,
                                              TotalIncSST = a.TotalIncSST,
                                              BudgetBalance = a.budgetBalance,
                                              PreparedById = a.PreparedById,
                                              PreparedDate = a.PreparedDate,
                                              HODApproverId = s.HODId,
                                              HODApproverName = t.firstName + " " + t.lastName,
                                              HODApprovedDate1 = s.HODApprovedDate1.Value,
                                              //HODApprovedDate2 = s.HODApprovedDate2.Value,
                                              ApproverId = q.approverId,
                                              ApprovedDate = q.approverApprovedDate.Value,
                                              ReviewerId = o.reviewerId,
                                              ReviewedDate = o.reviewedDate.Value,
                                              RecommenderId = u.recommenderId,
                                              RecommendDate = u.recommendedDate,
                                              RecommenderIdII = ac.recommenderId,
                                              RecommenderNameII = ad.firstName + " " + ad.lastName,
                                              RecommendDateII = ac.recommendedDate.Value,
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
                    if (PRDetail.NewPRForm.RecommenderId != null && PRDetail.NewPRForm.RecommenderId != 0)
                    {
                        User getFullname = db.Users.First(m => m.userId == PRDetail.NewPRForm.RecommenderId);
                        PRDetail.NewPRForm.RecommenderName = getFullname.firstName + " " + getFullname.lastName;
                    }
                    //if (PRDetail.NewPRForm.RecommenderIdII != 0)
                    //{
                    //    User getFullname = db.Users.First(m => m.userId == PRDetail.NewPRForm.RecommenderId);
                    //    PRDetail.NewPRForm.RecommenderNameII = getFullname.firstName + " " + getFullname.lastName;
                    //}
                }

                PRDetail.NewPRForm.PRItemListObject = (from m in db.PR_Items
                                                       from n in db.PopulateItemLists.Where(x => m.codeId == x.codeId && m.itemTypeId == x.itemTypeId)
                                                       join o in db.UOMs on m.UoMId equals o.UoMId into p
                                                       from q in p.DefaultIfEmpty()
                                                       where m.PRId == PRDetail.PRId && n.custId == PRCustId
                                                       select new PRItemsTable()
                                                       {
                                                           ItemsId = m.itemsId,
                                                           DateRequired = m.dateRequired,
                                                           Description = n.Description,
                                                           CodeId = n.codeId,
                                                           ItemCode = n.ItemCode,
                                                           CustPONo = m.custPONo,
                                                           Quantity = m.quantity,
                                                           UnitPrice = m.unitPrice,
                                                           TaxCodeId = m.taxCodeId,
                                                           TotalPrice = m.totalPrice,
                                                           SST = m.totalPriceIncSST - m.totalPrice,                                                           
                                                           TotalPriceIncSST = m.totalPriceIncSST,
                                                           UOM = q.UoMCode
                                                       }).ToList();

                return new RazorPDF.PdfActionResult("PRForm", PRDetail);
            }
            else
            {
                return Redirect("~/Home/Index");
            }
        }

        [HttpPost]
        [ValidateAjax]
        public JsonResult UpdatePRProcurement(PRModel PR)
        {
            if (User.Identity.IsAuthenticated && Session["UserId"] != null)
            {
                var FormerPRDetails = db.PurchaseRequisitions.First(x => x.PRId == PR.PRId);
                PR.FullName = Session["FullName"].ToString();
                if (FormerPRDetails.SpecsReviewerId != PR.NewPRForm.SpecReviewerId)
                {
                    if (FormerPRDetails.SpecsReviewerId != null)
                    {
                        var FormerGroupName = db.Groups.Select(m => new { GroupName = m.groupName, GroupId = m.groupId }).First(m => m.GroupId == FormerPRDetails.SpecsReviewerId);
                        GroupList NewGroupName = new GroupList();
                        if (PR.NewPRForm.SpecReviewerId != null)
                        {
                            NewGroupName = db.Groups.Select(m => new GroupList { GroupName = m.groupName, GroupId = m.groupId }).First(m => m.GroupId == PR.NewPRForm.SpecReviewerId);
                        } else
                        {
                            NewGroupName.GroupName = "null";
                        }
                            NotificationMsg _objDetails_SpecsReviewer = new NotificationMsg
                            {
                                uuid = Guid.NewGuid(),
                                PRId = PR.PRId,
                                msgDate = DateTime.Now,
                                fromUserId = PR.UserId,
                                msgType = "Trail",
                                message = PR.FullName + " change Specs Reviewer value from " + FormerGroupName.GroupName + " to " + NewGroupName.GroupName + " for PR No: " + FormerPRDetails.PRNo
                            };                     
                        
                        db.NotificationMsgs.Add(_objDetails_SpecsReviewer);
                    }
                    FormerPRDetails.SpecsReviewerId = PR.NewPRForm.SpecReviewerId;
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
                if (FormerPRDetails.VendorCompanyId != PR.NewPRForm.VendorCompanyId)
                {
                    if (FormerPRDetails.VendorCompanyId != null)
                    {
                        NotificationMsg _objDetails_VendorCompanyId = new NotificationMsg
                        {
                            uuid = Guid.NewGuid(),
                            PRId = PR.PRId,
                            msgDate = DateTime.Now,
                            fromUserId = PR.UserId,
                            msgType = "Trail",
                            message = PR.FullName + " change Vendor Company Id value from " + FormerPRDetails.VendorCompanyId + " to " + PR.NewPRForm.VendorCompanyId + " for PR No: " + FormerPRDetails.PRNo
                        };
                        db.NotificationMsgs.Add(_objDetails_VendorCompanyId);
                    }
                    FormerPRDetails.VendorCompanyId = PR.NewPRForm.VendorCompanyId;
                }
                if (FormerPRDetails.VendorQuoteNo != PR.NewPRForm.VendorQuoteNo)
                {
                    if (FormerPRDetails.VendorQuoteNo != null)
                    {
                        NotificationMsg _objDetails_VendorQuoteNo = new NotificationMsg
                        {
                            uuid = Guid.NewGuid(),
                            PRId = PR.PRId,
                            msgDate = DateTime.Now,
                            fromUserId = PR.UserId,
                            msgType = "Trail",
                            message = PR.FullName + " change Vendor Quote No value from " + FormerPRDetails.VendorQuoteNo + " to " + PR.NewPRForm.VendorQuoteNo + " for PR No: " + FormerPRDetails.PRNo
                        };
                        db.NotificationMsgs.Add(_objDetails_VendorQuoteNo);
                    }
                    FormerPRDetails.VendorQuoteNo = PR.NewPRForm.VendorQuoteNo;
                }
                if (FormerPRDetails.DiscountAmount != PR.NewPRForm.DiscountAmount)
                {
                    if (FormerPRDetails.DiscountAmount != 0)
                    {
                        NotificationMsg _objDetails_DiscountAmount = new NotificationMsg
                        {
                            uuid = Guid.NewGuid(),
                            PRId = PR.PRId,
                            msgDate = DateTime.Now,
                            fromUserId = PR.UserId,
                            msgType = "Trail",
                            message = PR.FullName + " change Discount Amount from " + FormerPRDetails.DiscountAmount + " to " + PR.NewPRForm.DiscountAmount + " for PR No: " + FormerPRDetails.PRNo
                        };
                        db.NotificationMsgs.Add(_objDetails_DiscountAmount);
                    }
                    FormerPRDetails.DiscountAmount = PR.NewPRForm.DiscountAmount;
                }
                if (FormerPRDetails.Discount_ != PR.NewPRForm.DiscountPerc)
                {
                    if (FormerPRDetails.Discount_ != 0)
                    {
                        NotificationMsg _objDetails_DiscountPerc = new NotificationMsg
                        {
                            uuid = Guid.NewGuid(),
                            PRId = PR.PRId,
                            msgDate = DateTime.Now,
                            fromUserId = PR.UserId,
                            msgType = "Trail",
                            message = PR.FullName + " change Discount Percentage from " + FormerPRDetails.Discount_ + " to " + PR.NewPRForm.DiscountPerc + " for PR No: " + FormerPRDetails.PRNo
                        };
                        db.NotificationMsgs.Add(_objDetails_DiscountPerc);
                    }
                    FormerPRDetails.Discount_ = PR.NewPRForm.DiscountPerc;
                }
                if (FormerPRDetails.TotalExclSST != PR.NewPRForm.TotalExclSST)
                {
                    if (FormerPRDetails.TotalExclSST != 0)
                    {
                        NotificationMsg _objDetails_TotalExclSST = new NotificationMsg
                        {
                            uuid = Guid.NewGuid(),
                            PRId = PR.PRId,
                            msgDate = DateTime.Now,
                            fromUserId = PR.UserId,
                            msgType = "Trail",
                            message = PR.FullName + " change Total Excluded SST from " + FormerPRDetails.TotalExclSST + " to " + PR.NewPRForm.TotalExclSST + " for PR No: " + FormerPRDetails.PRNo
                        };
                        db.NotificationMsgs.Add(_objDetails_TotalExclSST);
                    }
                    FormerPRDetails.TotalExclSST = PR.NewPRForm.TotalExclSST;
                }
                if (FormerPRDetails.TotalSST != PR.NewPRForm.TotalSST)
                {
                    if (FormerPRDetails.TotalSST != 0)
                    {
                        NotificationMsg _objDetails_TotalSST = new NotificationMsg
                        {
                            uuid = Guid.NewGuid(),
                            PRId = PR.PRId,
                            msgDate = DateTime.Now,
                            fromUserId = PR.UserId,
                            msgType = "Trail",
                            message = PR.FullName + " change Total SST from " + FormerPRDetails.TotalSST + " to " + PR.NewPRForm.TotalSST + " for PR No: " + FormerPRDetails.PRNo
                        };
                        db.NotificationMsgs.Add(_objDetails_TotalSST);
                    }
                    FormerPRDetails.TotalSST = PR.NewPRForm.TotalSST;
                }
                if (FormerPRDetails.TotalIncSST != PR.NewPRForm.TotalIncSST)
                {
                    if (FormerPRDetails.TotalIncSST != 0)
                    {
                        NotificationMsg _objDetails_TotalIncSST = new NotificationMsg
                        {
                            uuid = Guid.NewGuid(),
                            PRId = PR.PRId,
                            msgDate = DateTime.Now,
                            fromUserId = PR.UserId,
                            msgType = "Trail",
                            message = PR.FullName + " change Total Included SST from " + FormerPRDetails.TotalIncSST + " to " + PR.NewPRForm.TotalIncSST + " for PR No: " + FormerPRDetails.PRNo
                        };
                        db.NotificationMsgs.Add(_objDetails_TotalIncSST);
                    }
                    FormerPRDetails.TotalIncSST = PR.NewPRForm.TotalIncSST;
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
                    if (value.CodeId != null)
                    {
                        if (FormerPRItem.codeId != value.CodeId.Value)
                        {
                            if (FormerPRItem.codeId != null)
                            {
                                var formerCodeId = db.PR_Items.First(m => m.PRId == PR.PRId); int CustId = 0;
                                var getPRPreparerChildCustId = db.Users.FirstOrDefault(m => m.userId == FormerPRDetails.PreparedById);
                                if (getPRPreparerChildCustId.childCompanyId != null)
                                {
                                    switch (getPRPreparerChildCustId.childCompanyId)
                                    {
                                        case 16:
                                            CustId = 6; break;
                                        case 17:
                                            CustId = 7; break;
                                        case 18:
                                            CustId = 8; break;
                                        default:
                                            CustId = FormerPRDetails.CustId; break;
                                    }
                                }
                                var FormerItemCode = db.PopulateItemLists.Select(m => new { ItemDescription = m.ItemDescription, CodeId = m.codeId, CustId = m.custId }).Where(m => m.CustId == CustId).Where(m => m.CodeId == formerCodeId.codeId).First();
                                var NewItemCode = db.PopulateItemLists.Select(m => new { ItemDescription = m.ItemDescription, CodeId = m.codeId, CustId = m.custId }).Where(m => m.CustId == CustId).Where(m => m.CodeId == value.CodeId).First();
                                NotificationMsg _objDetails_CodeId = new NotificationMsg
                                {
                                    uuid = Guid.NewGuid(),
                                    PRId = PR.PRId,
                                    msgDate = DateTime.Now,
                                    fromUserId = PR.UserId,
                                    msgType = "Trail",
                                    message = PR.FullName + " change Item Code from " + FormerItemCode.ItemDescription + " to " + NewItemCode.ItemDescription + " for PR No: " + FormerPRDetails.PRNo
                                };
                                db.NotificationMsgs.Add(_objDetails_CodeId);
                            }
                            FormerPRItem.codeId = value.CodeId.Value;
                        }
                    }
                    if (FormerPRItem.quantity != value.Quantity)
                    {
                        NotificationMsg _objDetails_Quantity = new NotificationMsg
                        {
                            uuid = Guid.NewGuid(),
                            PRId = PR.PRId,
                            msgDate = DateTime.Now,
                            fromUserId = PR.UserId,
                            msgType = "Trail",
                            message = PR.FullName + " change Quantity in PRNo : " + FormerPRDetails.PRNo + " items from " + FormerPRItem.quantity + " to " + value.Quantity
                        };
                        db.NotificationMsgs.Add(_objDetails_Quantity);
                        FormerPRItem.quantity = value.Quantity;
                        FormerPRItem.outStandingQuantity = value.Quantity;
                    }
                    if (FormerPRItem.UoMId != value.UoMId)
                    {
                        UOM formerUOMs = db.UOMs.FirstOrDefault(m => m.UoMId == FormerPRItem.UoMId);
                        UOM UOMsChanges = db.UOMs.FirstOrDefault(m => m.UoMId == value.UoMId);
                        NotificationMsg _objDetails_NotificationMsg = new NotificationMsg();
                        _objDetails_NotificationMsg.uuid = Guid.NewGuid();
                        _objDetails_NotificationMsg.PRId = PR.PRId;
                        _objDetails_NotificationMsg.msgDate = DateTime.Now;
                        _objDetails_NotificationMsg.fromUserId = PR.UserId;
                        _objDetails_NotificationMsg.msgType = "Trail";
                        if (formerUOMs == null)
                        {
                            _objDetails_NotificationMsg.message = PR.FullName + " set UOM to " + UOMsChanges.UoMCode + " in PRNo : " + FormerPRDetails.PRNo;
                        }
                        else
                        {
                            _objDetails_NotificationMsg.message = PR.FullName + " change UOM in PRNo : " + FormerPRDetails.PRNo + " items from " + formerUOMs.UoMCode + " to " + UOMsChanges.UoMCode;
                        }
                        db.NotificationMsgs.Add(_objDetails_NotificationMsg);
                        FormerPRItem.UoMId = value.UoMId;
                    }
                    if (FormerPRItem.jobNoId != value.JobNoId)
                    {
                        if (FormerPRItem.jobNoId != null)
                        {
                            var FormerJobNo = db.Projects.Select(m => new { JobNo = m.projectCode, JobNoId = m.projectId }).First(m => m.JobNoId == FormerPRItem.jobNoId);
                            var NewJobNo = db.Projects.Select(m => new { JobNo = m.projectCode, JobNoId = m.projectId }).First(m => m.JobNoId == value.JobNoId);
                            NotificationMsg _objDetails_JobNo = new NotificationMsg
                            {
                                uuid = Guid.NewGuid(),
                                PRId = PR.PRId,
                                msgDate = DateTime.Now,
                                fromUserId = PR.UserId,
                                msgType = "Trail",
                                message = PR.FullName + " change Job No from " + FormerJobNo.JobNo + " to " + NewJobNo.JobNo + " for PR No: " + FormerPRDetails.PRNo
                            };
                            db.NotificationMsgs.Add(_objDetails_JobNo);
                        }
                        FormerPRItem.jobNoId = value.JobNoId;
                    }
                    if (FormerPRItem.jobTaskNoId != value.JobTaskNoId)
                    {
                        if (FormerPRItem.jobTaskNoId != null)
                        {
                            var FormerJobTaskNo = db.JobTasks.Select(m => new { JobTaskNo = m.jobTaskNo, JobTaskNoId = m.jobTaskNoId }).First(m => m.JobTaskNoId == FormerPRItem.jobTaskNoId);
                            var NewJobTaskNo = db.JobTasks.Select(m => new { JobTaskNo = m.jobTaskNo, JobTaskNoId = m.jobTaskNoId }).First(m => m.JobTaskNoId == value.JobTaskNoId);
                            NotificationMsg _objDetails_JobTaskNo = new NotificationMsg
                            {
                                uuid = Guid.NewGuid(),
                                PRId = PR.PRId,
                                msgDate = DateTime.Now,
                                fromUserId = PR.UserId,
                                msgType = "Trail",
                                message = PR.FullName + " change Job Task No from " + FormerJobTaskNo.JobTaskNo + " to " + NewJobTaskNo.JobTaskNo + " for PR No: " + FormerPRDetails.PRNo
                            };
                            db.NotificationMsgs.Add(_objDetails_JobTaskNo);
                        }
                        FormerPRItem.jobTaskNoId = value.JobTaskNoId;
                    }
                    if (value.TaxCodeId != null)
                    {
                        if (FormerPRItem.taxCodeId != value.TaxCodeId)
                        {
                            if (FormerPRItem.taxCodeId != null)
                            {
                                NotificationMsg _objDetails_TotalPrice = new NotificationMsg
                                {
                                    uuid = Guid.NewGuid(),
                                    PRId = PR.PRId,
                                    msgDate = DateTime.Now,
                                    fromUserId = PR.UserId,
                                    msgType = "Trail",
                                    message = PR.FullName + " change SST value from " + FormerPRItem.totalPrice + " to " + value.TotalPrice.Value + " for PR No: " + FormerPRDetails.PRNo
                                };
                                db.NotificationMsgs.Add(_objDetails_TotalPrice);
                            }
                            FormerPRItem.taxCodeId = value.TaxCodeId;
                        }
                    }
                    if (value.UnitPrice != null)
                    {
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
                            if (value.TaxCodeId != null)
                            {
                                TaxCode getPerc = db.TaxCodes.First(m => m.TaxCodeId == value.TaxCodeId);
                                FormerPRItem.unitPriceIncSST = FormerPRItem.unitPrice + (getPerc.GST / 100 * FormerPRItem.unitPrice);
                            }
                        }
                    }
                    if (value.TotalPrice != null)
                    {
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
                            if (value.TaxCodeId != null)
                            {
                                TaxCode getPerc = db.TaxCodes.First(m => m.TaxCodeId == value.TaxCodeId);
                                FormerPRItem.totalPriceIncSST = FormerPRItem.totalPrice + (getPerc.GST / 100 * FormerPRItem.totalPrice);
                            }
                        }
                    }
                    if (value.UnitPriceIncSST != null)
                    {
                        if (FormerPRItem.unitPriceIncSST != value.UnitPriceIncSST.Value)
                        {
                            if (FormerPRItem.unitPriceIncSST != null)
                            {
                                NotificationMsg _objDetails_UnitPriceIncSST = new NotificationMsg
                                {
                                    uuid = Guid.NewGuid(),
                                    PRId = PR.PRId,
                                    msgDate = DateTime.Now,
                                    fromUserId = PR.UserId,
                                    msgType = "Trail",
                                    message = PR.FullName + " change Unit Price Included SST from " + FormerPRItem.unitPriceIncSST + " to " + value.UnitPriceIncSST.Value + " for PR No: " + FormerPRDetails.PRNo
                                };
                                db.NotificationMsgs.Add(_objDetails_UnitPriceIncSST);
                            }
                            FormerPRItem.unitPriceIncSST = value.UnitPriceIncSST.Value;
                        }
                    }
                    if (value.TotalPriceIncSST != null)
                    {
                        if (FormerPRItem.totalPriceIncSST != value.TotalPriceIncSST.Value)
                        {
                            if (FormerPRItem.totalPriceIncSST != null)
                            {
                                NotificationMsg _objDetails_TotalPriceIncSST = new NotificationMsg
                                {
                                    uuid = Guid.NewGuid(),
                                    PRId = PR.PRId,
                                    msgDate = DateTime.Now,
                                    fromUserId = PR.UserId,
                                    msgType = "Trail",
                                    message = PR.FullName + " change Total Price Included SST from " + FormerPRItem.totalPriceIncSST + " to " + value.TotalPriceIncSST.Value + " for PR No: " + FormerPRDetails.PRNo
                                };
                                db.NotificationMsgs.Add(_objDetails_TotalPriceIncSST);
                            }
                            FormerPRItem.totalPriceIncSST = value.TotalPriceIncSST.Value;
                        }
                    }
                    if (FormerPRItem.dimProjectId != value.DimProjectId)
                    {
                        Project formerDimProject = db.Projects.FirstOrDefault(m => m.projectId == FormerPRItem.dimProjectId);
                        Project DimProjectChanges = db.Projects.FirstOrDefault(m => m.projectId == value.DimProjectId);
                        NotificationMsg _objDetails_NotificationMsg = new NotificationMsg();
                        _objDetails_NotificationMsg.uuid = Guid.NewGuid();
                        _objDetails_NotificationMsg.PRId = PR.PRId;
                        _objDetails_NotificationMsg.msgDate = DateTime.Now;
                        _objDetails_NotificationMsg.fromUserId = PR.UserId;
                        _objDetails_NotificationMsg.msgType = "Trail";
                        if (formerDimProject == null)
                        {
                            _objDetails_NotificationMsg.message = PR.FullName + " set Dim Project to " + DimProjectChanges.projectName + " in PRNo : " + FormerPRDetails.PRNo;
                        }
                        else
                        {
                            _objDetails_NotificationMsg.message = PR.FullName + " change Dim Project in PRNo : " + FormerPRDetails.PRNo + " items from " + formerDimProject.projectName + " to " + DimProjectChanges.projectName;
                        }
                        db.NotificationMsgs.Add(_objDetails_NotificationMsg);
                        FormerPRItem.dimProjectId = value.DimProjectId;
                    }
                    if (FormerPRItem.dimDeptId != value.DimDeptId)
                    {
                        Project formerDimDept = db.Projects.FirstOrDefault(m => m.projectId == FormerPRItem.dimProjectId);
                        Project DimDeptChanges = db.Projects.FirstOrDefault(m => m.projectId == value.DimProjectId);
                        NotificationMsg _objDetails_NotificationMsg = new NotificationMsg();
                        _objDetails_NotificationMsg.uuid = Guid.NewGuid();
                        _objDetails_NotificationMsg.PRId = PR.PRId;
                        _objDetails_NotificationMsg.msgDate = DateTime.Now;
                        _objDetails_NotificationMsg.fromUserId = PR.UserId;
                        _objDetails_NotificationMsg.msgType = "Trail";
                        if (formerDimDept == null)
                        {
                            _objDetails_NotificationMsg.message = PR.FullName + " set Dim Dept to " + DimDeptChanges.projectName + " in PRNo : " + FormerPRDetails.PRNo;
                        }
                        else
                        {
                            _objDetails_NotificationMsg.message = PR.FullName + " change Dim Dept in PRNo : " + FormerPRDetails.PRNo + " items from " + formerDimDept.projectName + " to " + DimDeptChanges.projectName;
                        }
                        db.NotificationMsgs.Add(_objDetails_NotificationMsg);
                        FormerPRItem.UoMId = value.UoMId;
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
                db.SaveChanges();

                return Json(new { success = true, message = "Sucessfully update" });
            }
            else
            {
                var redirectUrl = new UrlHelper(Request.RequestContext).Action("Index", "Home", new { /* no params */ });
                return Json(new { success = false, url = redirectUrl });
            }
        }

        [HttpPost]
        [ValidateAjax]
        public JsonResult SubmitPRProcurement(PRModel PRModel)
        {
            if (User.Identity.IsAuthenticated && Session["UserId"] != null)
            {
                // todo check session if login as procurement or admin only

                int PrId = Int32.Parse(Request["PrId"]); int UserId = Int32.Parse(Session["UserId"].ToString());
                var getFullName = db.Users.First(m => m.userId == UserId);
                //String SpecsReviewer = Request["SpecsReviewer"];

                var PR = db.PurchaseRequisitions.First(x => x.PRId == PrId);
                PR.SpecsReviewerId = PRModel.NewPRForm.SpecReviewerId;
                PR.AmountRequired = PRModel.NewPRForm.AmountRequired;
                PR.VendorId = PRModel.NewPRForm.VendorId;
                PR.VendorCompanyId = PRModel.NewPRForm.VendorCompanyId;
                PR.VendorQuoteNo = PRModel.NewPRForm.VendorQuoteNo;
                PR.PRAging = PRModel.NewPRForm.PRAging;
                PR.DiscountAmount = PRModel.NewPRForm.DiscountAmount;
                PR.Discount_ = PRModel.NewPRForm.DiscountPerc;
                PR.TotalExclSST = PRModel.NewPRForm.TotalExclSST;
                PR.TotalSST = PRModel.NewPRForm.TotalSST;
                PR.TotalIncSST = PRModel.NewPRForm.TotalIncSST;
                db.SaveChanges();

                foreach (var value in PRModel.NewPRForm.PRItemListObject)
                {
                    PR_Items _objNewPRItem = db.PR_Items.FirstOrDefault(m => m.itemsId == value.ItemsId);
                    _objNewPRItem.itemTypeId = value.ItemTypeId;
                    _objNewPRItem.codeId = value.CodeId.Value;
                    _objNewPRItem.quantity = value.Quantity;
                    _objNewPRItem.UoMId = value.UoMId;
                    _objNewPRItem.jobNoId = value.JobNoId;
                    _objNewPRItem.jobTaskNoId = value.JobTaskNoId;
                    _objNewPRItem.taxCodeId = value.TaxCodeId;
                    _objNewPRItem.unitPrice = value.UnitPrice.Value;
                    _objNewPRItem.totalPrice = value.TotalPrice.Value;
                    _objNewPRItem.unitPriceIncSST = value.UnitPriceIncSST.Value;
                    _objNewPRItem.totalPriceIncSST = value.TotalPriceIncSST.Value;
                    _objNewPRItem.dimProjectId = value.DimProjectId;
                    _objNewPRItem.dimDeptId = value.DimDeptId;
                    db.SaveChanges();
                }

                var requestorDone = (from m in db.NotificationMsgs
                                     join n in db.NotiGroups on m.msgId equals n.msgId
                                     where n.toUserId == UserId && m.PRId == PrId && m.msgType == "Task"
                                     select new PRModel()
                                     {
                                         MsgId = m.msgId
                                     }).First();

                NotificationMsg getDone = db.NotificationMsgs.First(m => m.msgId == requestorDone.MsgId);
                getDone.done = true;
                db.SaveChanges();

                decimal amountBudget = PRModel.NewPRForm.TotalIncSST.Value;

                PR_Admin SaveAdminInfo = db.PR_Admin.First(m => m.PRId == PR.PRId);
                SaveAdminInfo.adminSubmited = SaveAdminInfo.adminSubmited + 1;
                SaveAdminInfo.adminSubmitDate = DateTime.Now;
                db.SaveChanges();

                var getSpecReviewerId = (from m in db.PurchaseRequisitions
                                         join n in db.Groups on m.SpecsReviewerId equals n.groupId
                                         join o in db.Roles on n.groupName equals o.name
                                         where m.PRId == PR.PRId
                                         select new
                                         {
                                             RoleId = o.roleId
                                         }).FirstOrDefault();
                PR_PaperApprover checkGMDpaper = db.PR_PaperApprover.FirstOrDefault(m => m.PRId == PrId);
                var getReqChildCustId = db.Users.First(m => m.userId == PR.PreparedById);

                if (checkGMDpaper != null && PRModel.NewPRForm.SpecReviewerId == null)
                {
                    var getApprover = (from m in db.Users
                                   join n in db.Users_Roles on m.userId equals n.userId
                                   //join o in db.Roles on n.roleId equals o.roleId
                                   where m.userId == checkGMDpaper.finalApproverUserId
                                   select new PRModel()
                                   {
                                       UserId = n.userId,
                                       FullName = m.firstName + " " + m.lastName,
                                       EmailAddress = m.emailAddress
                                   }).ToList();

                    if (PRService.CheckAmountScenarioOne(amountBudget, PR.CustId) && PRService.IncludeInBudget(PR))
                    {
                        PR.Scenario = 1;
                    } else if ((PRService.IncludeInBudget(PR) == false && PRService.CheckAmountScenarioOne(amountBudget, PR.CustId)) || PRService.CheckAmountScenarioTwo(amountBudget, PR.CustId))
                    {
                        PR.Scenario = 2;
                    } else if (PRService.CheckAmountScenarioThree(amountBudget, PR.CustId))
                    {
                        PR.Scenario = 3;
                    }
                    PR.StatusId = "PR05";
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

                        PRModel x = new PRModel();
                        x.PRId = PR.PRId;
                        x.UserId = UserId;
                        x.EmailAddress = item.EmailAddress;
                        x.NewPRForm = new NewPRModel();
                        x.NewPRForm.PRNo = PR.PRNo;
                        x.NewPRForm.ApproverId = item.UserId;
                        x.NewPRForm.ApproverName = item.FullName;
                        SendEmailPRNotification(x, "ToApproverNoti");
                    }

                    return Json(new { success = true, message = "Sucessfully submit the procurement" });
                }
                else if (PRService.CheckAmountScenarioOne(amountBudget, PR.CustId) && PRService.IncludeInBudget(PR))
                {
                    PR.Scenario = 1;
                    db.SaveChanges();

                    // send notifications to reviewer
                    if (PRModel.NewPRForm.SpecReviewerId != null)
                    {
                        PR.StatusId = "PR03";
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

                        var getReviewer = new List<PRModel>();
                        if (PR.CustId == 3 && getReqChildCustId.childCompanyId == 16)
                        {
                            getReviewer = (from m in db.Users
                                           join n in db.Users_Roles on m.userId equals n.userId
                                           //join o in db.Roles on n.roleId equals o.roleId
                                           where n.roleId == getSpecReviewerId.RoleId && m.companyId == 2
                                           select new PRModel()
                                           {
                                               UserId = n.userId,
                                               FullName = m.firstName + " " + m.lastName,
                                               EmailAddress = m.emailAddress
                                           }).ToList();
                        } else
                        {
                            getReviewer = (from m in db.Users
                                           join n in db.Users_Roles on m.userId equals n.userId
                                           //join o in db.Roles on n.roleId equals o.roleId
                                           where n.roleId == getSpecReviewerId.RoleId && m.companyId == PR.CustId
                                           select new PRModel()
                                           {
                                               UserId = n.userId,
                                               FullName = m.firstName + " " + m.lastName,
                                               EmailAddress = m.emailAddress
                                           }).ToList();
                        }
                        

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

                            PRModel.PRId = PrId;
                            PRModel.UserId = UserId;
                            PRModel.NewPRForm.PRNo = PR.PRNo;
                            PRModel.NewPRForm.ApproverId = item.UserId;
                            PRModel.NewPRForm.ApproverName = item.FullName;
                            PRModel.EmailAddress = item.EmailAddress;
                            SendEmailPRNotification(PRModel, "ToReviewerNoti");
                        }
                    }
                    else
                    {
                        PR.StatusId = "PR05";
                        NotificationMsg _objSubmited = new NotificationMsg
                        {
                            uuid = Guid.NewGuid(),
                            PRId = PrId,
                            msgDate = DateTime.Now,
                            fromUserId = UserId,
                            msgType = "Trail",
                            message = getFullName.firstName + " " + getFullName.lastName + " has submit PR application for PR No. " + PR.PRNo + " subject for approval"
                        };
                        db.NotificationMsgs.Add(_objSubmited);
                        var getApprover = new List<PRModel>();

                        switch (PR.CustId)
                        {
                            case 2:
                                if (amountBudget < 10000 && getReqChildCustId.childCompanyId == 7)
                                {
                                    //get En. Shahril
                                    getApprover = (from m in db.Users
                                                   where m.userId == 172 && m.companyId == PR.CustId
                                                   select new PRModel()
                                                   {
                                                       UserId = m.userId,
                                                       FullName = m.firstName + " " + m.lastName,
                                                       EmailAddress = m.emailAddress
                                                   }).ToList();
                                }
                                else
                                {
                                    getApprover = (from m in db.Users
                                                   join o in db.Users on m.superiorId equals o.userId
                                                   join n in db.Users_Roles on o.userId equals n.userId
                                                   where n.roleId == "R02" && m.companyId == PR.CustId && m.userId == PR.PreparedById
                                                   select new PRModel()
                                                   {
                                                       UserId = n.userId,
                                                       FullName = o.firstName + " " + o.lastName,
                                                       EmailAddress = o.emailAddress
                                                   }).ToList();
                                }                                    
                                break;
                            case 3:
                            case 4:
                                if (amountBudget < 5000 && PR.CustId == 4)
                                {
                                    //get Pn. Liz
                                    getApprover = (from m in db.Users                                                   
                                                   where m.userId == 60 && m.companyId == PR.CustId
                                                   select new PRModel()
                                                   {
                                                       UserId = m.userId,
                                                       FullName = m.firstName + " " + m.lastName,
                                                       EmailAddress = m.emailAddress
                                                   }).ToList();
                                }
                                else if (PR.CustId == 3 && getReqChildCustId.childCompanyId == 16)
                                {
                                    //get Datuk Rahim
                                    getApprover = (from m in db.Users
                                                   where m.userId == 165
                                                   select new PRModel()
                                                   {
                                                       UserId = m.userId,
                                                       FullName = m.firstName + " " + m.lastName,
                                                       EmailAddress = m.emailAddress
                                                   }).ToList();
                                } else
                                {
                                    getApprover = (from m in db.Users
                                                   join n in db.Users_Roles on m.userId equals n.userId
                                                   //join o in db.Roles on n.roleId equals o.roleId
                                                   where n.roleId == "R04" && m.companyId == PR.CustId
                                                   select new PRModel()
                                                   {
                                                       UserId = n.userId,
                                                       FullName = m.firstName + " " + m.lastName,
                                                       EmailAddress = m.emailAddress
                                                   }).ToList();
                                }
                                break;
                            default:
                                getApprover = (from m in db.Users
                                               join n in db.Users_Roles on m.userId equals n.userId
                                               //join o in db.Roles on n.roleId equals o.roleId
                                               where n.roleId == "R04" && m.companyId == PR.CustId
                                               select new PRModel()
                                               {
                                                   UserId = n.userId,
                                                   FullName = m.firstName + " " + m.lastName,
                                                   EmailAddress = m.emailAddress
                                               }).ToList();
                                break;

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

                            PRModel x = new PRModel();
                            x.PRId = PR.PRId;
                            x.UserId = UserId;
                            x.EmailAddress = item.EmailAddress;
                            x.NewPRForm = new NewPRModel();
                            x.NewPRForm.PRNo = PR.PRNo;
                            x.NewPRForm.ApproverId = item.UserId;
                            x.NewPRForm.ApproverName = item.FullName;
                            SendEmailPRNotification(x, "ToApproverNoti");
                        }
                    }

                    //return Json("Scenario 1 success");
                    return Json(new { success = true, message = "Sucessfully submit the procurement" });
                }
                else if ((PRService.IncludeInBudget(PR) == false && PRService.CheckAmountScenarioOne(amountBudget, PR.CustId)) || PRService.CheckAmountScenarioTwo(amountBudget, PR.CustId))
                {
                    PR.Scenario = 2;
                    db.SaveChanges();

                    if (PRModel.NewPRForm.SpecReviewerId != null)
                    {
                        PR.StatusId = "PR04";
                        var getRecommender = new List<PRModel>();
                        if (PR.CustId == 3 && getReqChildCustId.childCompanyId == 16)
                        {
                            getRecommender = (from m in db.Users
                                           join n in db.Users_Roles on m.userId equals n.userId
                                           //join o in db.Roles on n.roleId equals o.roleId
                                           where n.roleId == getSpecReviewerId.RoleId && m.companyId == 2
                                           select new PRModel()
                                           {
                                               UserId = n.userId,
                                               FullName = m.firstName + " " + m.lastName,
                                               EmailAddress = m.emailAddress
                                           }).ToList();
                        }
                        else
                        {
                            getRecommender = (from m in db.Users
                                                  join n in db.Users_Roles on m.userId equals n.userId
                                                  //join o in db.Roles on n.roleId equals o.roleId
                                                  where n.roleId == getSpecReviewerId.RoleId && m.companyId == PR.CustId
                                                  select new PRModel()
                                                  {
                                                      UserId = n.userId,
                                                      FullName = m.firstName + " " + m.lastName,
                                                      EmailAddress = m.emailAddress
                                                  }).ToList();
                        }
                        NotificationMsg objTask = new NotificationMsg()
                        {
                            uuid = Guid.NewGuid(),
                            message = PR.PRNo + " pending for your recommendation",
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

                            PRModel.PRId = PrId;
                            PRModel.UserId = UserId;
                            PRModel.NewPRForm.PRNo = PR.PRNo;
                            PRModel.NewPRForm.ApproverId = item.UserId;
                            PRModel.NewPRForm.ApproverName = item.FullName;
                            PRModel.EmailAddress = item.EmailAddress;
                            SendEmailPRNotification(PRModel, "ToRecommenderNoti");
                        }
                    }
                    else
                    {
                        PR.StatusId = "PR03";
                        //scenario 2, review by head of GPSS
                        var getReviewer = (from m in db.Users
                                           join n in db.Users_Roles on m.userId equals n.userId
                                           //join o in db.Roles on n.roleId equals o.roleId
                                           where n.roleId == "R10" && m.companyId == 2
                                           select new PRModel()
                                           {
                                               UserId = n.userId,
                                               FullName = m.firstName + " " + m.lastName,
                                               EmailAddress = m.emailAddress
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

                            PRModel x = new PRModel();
                            x.PRId = PR.PRId;
                            x.UserId = UserId;
                            x.EmailAddress = item.EmailAddress;
                            x.NewPRForm = new NewPRModel();
                            x.NewPRForm.PRNo = PR.PRNo;
                            x.NewPRForm.ApproverId = item.UserId;
                            x.NewPRForm.ApproverName = item.FullName;
                            SendEmailPRNotification(x, "ToReviewerNoti");
                        }
                    }

                    return Json(new { success = true, message = "Sucessfully submit the procurement" });
                }
                else if (PRService.CheckAmountScenarioThree(amountBudget, PR.CustId))
                {
                    PR.Scenario = 3;
                    db.SaveChanges();

                    if (PRModel.NewPRForm.SpecReviewerId != null)
                    {
                        PR.StatusId = "PR04";
                        var getRecommender = new List<PRModel>();
                        if (PR.CustId == 3 && getReqChildCustId.childCompanyId == 16)
                        {
                            getRecommender = (from m in db.Users
                                              join n in db.Users_Roles on m.userId equals n.userId
                                              //join o in db.Roles on n.roleId equals o.roleId
                                              where n.roleId == getSpecReviewerId.RoleId && m.companyId == 2
                                              select new PRModel()
                                              {
                                                  UserId = n.userId,
                                                  FullName = m.firstName + " " + m.lastName,
                                                  EmailAddress = m.emailAddress
                                              }).ToList();
                        }
                        else
                        {
                            getRecommender = (from m in db.Users
                                                  join n in db.Users_Roles on m.userId equals n.userId
                                                  //join o in db.Roles on n.roleId equals o.roleId
                                                  where n.roleId == getSpecReviewerId.RoleId && m.companyId == PR.CustId
                                                  select new PRModel()
                                                  {
                                                      UserId = n.userId,
                                                      FullName = m.firstName + " " + m.lastName,
                                                      EmailAddress = m.emailAddress
                                                  }).ToList();
                        }

                        NotificationMsg objTask = new NotificationMsg()
                        {
                            uuid = Guid.NewGuid(),
                            message = PR.PRNo + " pending for your recommendation",
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

                            PRModel.PRId = PrId;
                            PRModel.UserId = UserId;
                            PRModel.EmailAddress = item.EmailAddress;
                            PRModel.NewPRForm.PRNo = PR.PRNo;
                            PRModel.NewPRForm.ApproverId = item.UserId;
                            PRModel.NewPRForm.ApproverName = item.FullName;
                            SendEmailPRNotification(PRModel, "ToRecommenderNoti");
                        }
                    }
                    else
                    {
                        PR.StatusId = "PR03";
                        var getReviewer = (from m in db.Users
                                           join n in db.Users_Roles on m.userId equals n.userId
                                           //join o in db.Roles on n.roleId equals o.roleId
                                           where n.roleId == "R10" && m.companyId == 2
                                           select new PRModel()
                                           {
                                               UserId = n.userId,
                                               FullName = m.firstName + " " + m.lastName,
                                               EmailAddress = m.emailAddress
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

                            PRModel x = new PRModel();
                            x.PRId = PR.PRId;
                            x.UserId = UserId;
                            x.EmailAddress = item.EmailAddress;
                            x.NewPRForm = new NewPRModel();
                            x.NewPRForm.PRNo = PR.PRNo;
                            x.NewPRForm.ApproverId = item.UserId;
                            x.NewPRForm.ApproverName = item.FullName;
                            SendEmailPRNotification(x, "ToReviewerNoti");
                        }
                    }

                    return Json(new { success = true, message = "Sucessfully submit the procurement" });
                }
                else
                {
                    //return Json("Does not fit to any conditions. Please recheck. ");
                    return Json(new { success = true, message = "Does not fit to any conditions. Please recheck." });
                }

            }
            else
            {
                var redirectUrl = new UrlHelper(Request.RequestContext).Action("Index", "Home", new { /* no params */ });
                return Json(new { success = false, url = redirectUrl });
            }
        }

        public ActionResult RejectPRRecommended()
        {
            if (User.Identity.IsAuthenticated && Session["UserId"] != null)
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
                                     where n.toUserId == UserId && m.PRId == PrId && m.msgType == "Task"
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
            else
            {
                return Redirect("~/Home/Index");
            }
        }

        public ActionResult RecommendedPRRecommended()
        {
            if (User.Identity.IsAuthenticated && Session["UserId"] != null)
            {
                // todo the session must be IT / HSE / PMO
                // set status to PR03 and head of GPSS will proceed

                int PrId = Int32.Parse(Request["PrId"]);
                var PR = db.PurchaseRequisitions.First(x => x.PRId == PrId);

                int UserId = Int32.Parse(Session["UserId"].ToString());
                var requestorDone = (from m in db.NotificationMsgs
                                     join n in db.NotiGroups on m.msgId equals n.msgId
                                     where n.toUserId == UserId && m.PRId == PrId && m.msgType == "Task"
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

                PR_PaperApprover checkGMDpaper = db.PR_PaperApprover.FirstOrDefault(m => m.PRId == PrId);
                if (checkGMDpaper != null)
                {
                    PR.StatusId = "PR05";
                    var getApprover = (from m in db.Users
                                       join n in db.Users_Roles on m.userId equals n.userId
                                       //join o in db.Roles on n.roleId equals o.roleId
                                       where m.userId == checkGMDpaper.finalApproverUserId
                                       select new PRModel()
                                       {
                                           UserId = n.userId,
                                           FullName = m.firstName + " " + m.lastName,
                                           EmailAddress = m.emailAddress
                                       }).ToList();

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

                        PRModel x = new PRModel();
                        x.PRId = PR.PRId;
                        x.UserId = UserId;
                        x.EmailAddress = item.EmailAddress;
                        x.NewPRForm = new NewPRModel();
                        x.NewPRForm.PRNo = PR.PRNo;
                        x.NewPRForm.ApproverId = item.UserId;
                        x.NewPRForm.ApproverName = item.FullName;
                        SendEmailPRNotification(x, "ToApproverNoti");
                    }
                }
                else
                {
                    PR.StatusId = "PR03";
                    var getReviewer = (from m in db.Users
                                       join n in db.Users_Roles on m.userId equals n.userId
                                       //join o in db.Roles on n.roleId equals o.roleId
                                       where n.roleId == "R10" && m.companyId == 2
                                       select new PRModel()
                                       {
                                           UserId = n.userId,
                                           FullName = m.firstName + " " + m.lastName,
                                           EmailAddress = m.emailAddress
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

                        PRModel x = new PRModel();
                        x.PRId = PR.PRId;
                        x.UserId = UserId;
                        x.EmailAddress = item.EmailAddress;
                        x.NewPRForm = new NewPRModel();
                        x.NewPRForm.PRNo = PR.PRNo;
                        x.NewPRForm.ApproverId = item.UserId;
                        x.NewPRForm.ApproverName = item.FullName;
                        SendEmailPRNotification(x, "ToReviewerNoti");
                    }
                }

                return Json("Successfully recommend");
            }
            else
            {
                return Redirect("~/Home/Index");
            }
        }

        public ActionResult RejectPRReview()
        {
            if (User.Identity.IsAuthenticated && Session["UserId"] != null)
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
                                     where n.toUserId == UserId && m.PRId == PrId && m.msgType == "Task"
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
            else
            {
                return Redirect("~/Home/Index");
            }
        }

        public ActionResult ApprovePRReview()
        {
            if (User.Identity.IsAuthenticated && Session["UserId"] != null)
            {
                int PrId = Int32.Parse(Request["PrId"]);
                int UserId = Int32.Parse(Session["UserId"].ToString());
                var PR = db.PurchaseRequisitions.First(x => x.PRId == PrId);
                PR.StatusId = "PR05";
                db.SaveChanges();

                PR_Reviewer SaveReviewerInfo = db.PR_Reviewer.First(m => m.PRId == PR.PRId && m.reviewerId == UserId);
                SaveReviewerInfo.reviewedDate = DateTime.Now;
                SaveReviewerInfo.reviewed = 1;
                db.SaveChanges();

                var requestorDone = (from m in db.NotificationMsgs
                                     join n in db.NotiGroups on m.msgId equals n.msgId
                                     where n.toUserId == UserId && m.PRId == PrId && m.msgType == "Task"
                                     select new PRModel()
                                     {
                                         MsgId = m.msgId
                                     }).First();

                NotificationMsg getDone = db.NotificationMsgs.First(m => m.msgId == requestorDone.MsgId);
                getDone.done = true;
                db.SaveChanges();

                var getApprover = new List<PRModel>();
                PR_PaperApprover checkGMDpaper = db.PR_PaperApprover.FirstOrDefault(m => m.PRId == PrId);
                if (checkGMDpaper != null) {
                    getApprover = (from m in db.Users
                                   join n in db.Users_Roles on m.userId equals n.userId
                                   //join o in db.Roles on n.roleId equals o.roleId
                                   where m.userId == checkGMDpaper.finalApproverUserId
                                   select new PRModel()
                                   {
                                       UserId = n.userId,
                                       FullName = m.firstName + " " + m.lastName,
                                       EmailAddress = m.emailAddress
                                   }).ToList();

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

                        PRModel x = new PRModel();
                        x.PRId = PR.PRId;
                        x.UserId = UserId;
                        x.EmailAddress = item.EmailAddress;
                        x.NewPRForm = new NewPRModel();
                        x.NewPRForm.PRNo = PR.PRNo;
                        x.NewPRForm.ApproverId = item.UserId;
                        x.NewPRForm.ApproverName = item.FullName;
                        SendEmailPRNotification(x, "ToApproverNoti");
                    }
                }
                else
                {
                    int CustId = db.Projects.First(m => m.custId == PR.CustId).custId;
                    switch (PR.Scenario)
                    {                       
                        case 3:
                            {
                                var getRecommenderII = new List<PRModel>();
                                if (CustId == 2 || CustId == 6 || CustId == 4)
                                {
                                    PR.StatusId = "PR18";
                                    getRecommenderII = (from m in db.Users
                                                        join n in db.Users_Roles on m.userId equals n.userId
                                                        where n.roleId == "R12" && m.companyId == 2
                                                        select new PRModel()
                                                        {
                                                            UserId = n.userId,
                                                            FullName = m.firstName + " " + m.lastName,
                                                            EmailAddress = m.emailAddress
                                                        }).ToList();
                                }
                                else
                                {
                                    PR.StatusId = "PR11";
                                    getRecommenderII = (from m in db.Users
                                                        join n in db.Users_Roles on m.userId equals n.userId
                                                        //join o in db.Roles on n.roleId equals o.roleId
                                                        where n.roleId == "R04" && m.companyId == PR.CustId
                                                        select new PRModel()
                                                        {
                                                            UserId = n.userId,
                                                            FullName = m.firstName + " " + m.lastName,
                                                            EmailAddress = m.emailAddress
                                                        }).ToList();
                                }

                                NotificationMsg objTask = new NotificationMsg()
                                {
                                    uuid = Guid.NewGuid(),
                                    message = PR.PRNo + " pending for your recommendation",
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

                                    if (CustId == 2 || CustId == 6 || CustId == 4)
                                    {
                                        PR_RecommenderCFO saveRecommender = new PR_RecommenderCFO()
                                        {
                                            uuid = Guid.NewGuid(),
                                            recommenderId = item.UserId,
                                            PRId = PR.PRId
                                        };
                                        db.PR_RecommenderCFO.Add(saveRecommender);
                                        db.SaveChanges();
                                    }
                                    else
                                    {
                                        PR_RecommenderHOC saveRecommender = new PR_RecommenderHOC()
                                        {
                                            uuid = Guid.NewGuid(),
                                            recommenderId = item.UserId,
                                            PRId = PR.PRId
                                        };
                                        db.PR_RecommenderHOC.Add(saveRecommender);
                                        db.SaveChanges();
                                    }
                                    PRModel x = new PRModel();
                                    x.PRId = PR.PRId;
                                    x.UserId = UserId;
                                    x.EmailAddress = item.EmailAddress;
                                    x.NewPRForm = new NewPRModel();
                                    x.NewPRForm.PRNo = PR.PRNo;
                                    x.NewPRForm.ApproverId = item.UserId;
                                    x.NewPRForm.ApproverName = item.FullName;
                                    SendEmailPRNotification(x, "ToRecommenderIINoti");
                                }

                                break;
                            }

                        case 2:
                            {
                                if (CustId == 2)
                                {
                                    getApprover = (from m in db.Users
                                                   join o in db.Users on m.superiorId equals o.userId
                                                   join n in db.Users_Roles on o.userId equals n.userId
                                                   where n.roleId == "R02" && m.companyId == PR.CustId && m.userId == PR.PreparedById
                                                   select new PRModel()
                                                   {
                                                       UserId = m.userId,
                                                       FullName = m.firstName + " " + m.lastName,
                                                       EmailAddress = m.emailAddress
                                                   }).ToList();
                                }
                                else if (CustId == 6)
                                {
                                    //get Datuk Rahim
                                    getApprover = (from m in db.Users                                                   
                                                   where m.userId == 165
                                                   select new PRModel()
                                                   {
                                                       UserId = m.userId,
                                                       FullName = m.firstName + " " + m.lastName,
                                                       EmailAddress = m.emailAddress
                                                   }).ToList();
                                }                                
                                else
                                {
                                    getApprover = (from m in db.Users
                                                   join n in db.Users_Roles on m.userId equals n.userId
                                                   //join o in db.Roles on n.roleId equals o.roleId
                                                   where n.roleId == "R04" && m.companyId == PR.CustId
                                                   select new PRModel()
                                                   {
                                                       UserId = n.userId,
                                                       FullName = m.firstName + " " + m.lastName,
                                                       EmailAddress = m.emailAddress
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

                                    PRModel x = new PRModel();
                                    x.PRId = PR.PRId;
                                    x.UserId = UserId;
                                    x.EmailAddress = item.EmailAddress;
                                    x.NewPRForm = new NewPRModel();
                                    x.NewPRForm.PRNo = PR.PRNo;
                                    x.NewPRForm.ApproverId = item.UserId;
                                    x.NewPRForm.ApproverName = item.FullName;
                                    SendEmailPRNotification(x, "ToApproverNoti");
                                }

                                break;
                            }

                        case 1:
                            {                                
                                switch (CustId)
                                {
                                    case 2:
                                        //actual HOD
                                        getApprover = (from m in db.Users
                                                       join o in db.Users on m.superiorId equals o.userId
                                                       join n in db.Users_Roles on o.userId equals n.userId
                                                       where n.roleId == "R02" && m.companyId == PR.CustId && m.userId == PR.PreparedById
                                                       select new PRModel()
                                                       {
                                                           UserId = o.userId,
                                                           FullName = o.firstName + " " + o.lastName,
                                                           EmailAddress = o.emailAddress
                                                       }).ToList();
                                        break;
                                    case 4:
                                        if (PR.AmountRequired < 5000)
                                        {
                                            //get Pn. Liz
                                            getApprover = (from m in db.Users
                                                           where m.userId == 60 && m.companyId == PR.CustId
                                                           select new PRModel()
                                                           {
                                                               UserId = m.userId,
                                                               FullName = m.firstName + " " + m.lastName,
                                                               EmailAddress = m.emailAddress
                                                           }).ToList();
                                        }
                                        else
                                        {
                                            getApprover = (from m in db.Users
                                                           join n in db.Users_Roles on m.userId equals n.userId
                                                           //join o in db.Roles on n.roleId equals o.roleId
                                                           where n.roleId == "R04" && m.companyId == PR.CustId
                                                           select new PRModel()
                                                           {
                                                               UserId = n.userId,
                                                               FullName = m.firstName + " " + m.lastName,
                                                               EmailAddress = m.emailAddress
                                                           }).ToList();
                                        }
                                        break;
                                    case 6:
                                            //get Datuk Rahim
                                            getApprover = (from m in db.Users
                                                           where m.userId == 165
                                                           select new PRModel()
                                                           {
                                                               UserId = m.userId,
                                                               FullName = m.firstName + " " + m.lastName,
                                                               EmailAddress = m.emailAddress
                                                           }).ToList();
                                    break;
                                    default:
                                        getApprover = (from m in db.Users
                                                       join n in db.Users_Roles on m.userId equals n.userId
                                                       //join o in db.Roles on n.roleId equals o.roleId
                                                       where n.roleId == "R04" && m.companyId == PR.CustId
                                                       select new PRModel()
                                                       {
                                                           UserId = n.userId,
                                                           FullName = m.firstName + " " + m.lastName,
                                                           EmailAddress = m.emailAddress
                                                       }).ToList();
                                        break;


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

                                    PRModel x = new PRModel();
                                    x.PRId = PR.PRId;
                                    x.UserId = UserId;
                                    x.EmailAddress = item.EmailAddress;
                                    x.NewPRForm = new NewPRModel();
                                    x.NewPRForm.PRNo = PR.PRNo;
                                    x.NewPRForm.ApproverId = item.UserId;
                                    x.NewPRForm.ApproverName = item.FullName;
                                    SendEmailPRNotification(x, "ToApproverNoti");
                                }

                                break;
                            }
                    }
                }

                return Json("Successfully reviewed");
            }
            else
            {
                return Redirect("~/Home/Index");
            }
        }

        public ActionResult ApprovePRJointRecommended()
        {
            if (User.Identity.IsAuthenticated && Session["UserId"] != null)
            {
                int PrId = Int32.Parse(Request["PrId"]);
                var PR = db.PurchaseRequisitions.First(x => x.PRId == PrId);
                // todo the session must be HOC
                int UserId = Int32.Parse(Session["UserId"].ToString());
                var requestorDone = (from m in db.NotificationMsgs
                                     join n in db.NotiGroups on m.msgId equals n.msgId
                                     where n.toUserId == UserId && m.PRId == PrId && m.msgType == "Task"
                                     select new PRModel()
                                     {
                                         MsgId = m.msgId
                                     }).First();

                NotificationMsg getDone = db.NotificationMsgs.First(m => m.msgId == requestorDone.MsgId);
                getDone.done = true;
                db.SaveChanges();

                int CustId = db.Projects.First(m => m.custId == PR.CustId).custId;
                var getRecommenderIII = new List<PRModel>();
                switch (CustId)
                {
                    case 2:
                    case 6:
                    case 4:
                        {
                            PR.StatusId = "PR05";
                            db.SaveChanges();

                            PR_RecommenderCFO SaveRecommenderInfo = db.PR_RecommenderCFO.First(m => m.PRId == PR.PRId);
                            SaveRecommenderInfo.recommendedDate = DateTime.Now;
                            SaveRecommenderInfo.recommended = 1;
                            db.SaveChanges();

                            var getGMD = (from m in db.Users
                                          join n in db.Users_Roles on m.userId equals n.userId
                                          //join o in db.Roles on n.roleId equals o.roleId
                                          where n.roleId == "R14" && m.companyId == 2
                                          select new PRModel()
                                          {
                                              UserId = n.userId,
                                              FullName = m.firstName + " " + m.lastName,
                                              EmailAddress = m.emailAddress
                                          }).ToList();                            
                            break;
                        }

                    default:
                        {
                            PR.StatusId = "PR18";
                            db.SaveChanges();

                            PR_RecommenderHOC SaveRecommenderInfo = db.PR_RecommenderHOC.First(m => m.PRId == PR.PRId);
                            SaveRecommenderInfo.recommendedDate = DateTime.Now;
                            SaveRecommenderInfo.recommended = 1;
                            db.SaveChanges();

                            getRecommenderIII = (from m in db.Users
                                                 join n in db.Users_Roles on m.userId equals n.userId
                                                 //join o in db.Roles on n.roleId equals o.roleId
                                                 where n.roleId == "R12" && m.companyId == 2
                                                 select new PRModel()
                                                 {
                                                     UserId = n.userId,
                                                     FullName = m.firstName + " " + m.lastName,
                                                     EmailAddress = m.emailAddress
                                                 }).ToList();
                            break;
                        }
                }

                NotificationMsg objTask = new NotificationMsg()
                {
                    uuid = Guid.NewGuid(),
                    message = PR.PRNo + " pending for your recommendation",
                    fromUserId = UserId,
                    msgDate = DateTime.Now,
                    msgType = "Task",
                    PRId = PrId
                };
                db.NotificationMsgs.Add(objTask);
                db.SaveChanges();

                foreach (var item in getRecommenderIII)
                {
                    NotiGroup RecommenderIIITask = new NotiGroup()
                    {
                        uuid = Guid.NewGuid(),
                        msgId = objTask.msgId,
                        toUserId = item.UserId,
                        resubmit = false
                    };
                    db.NotiGroups.Add(RecommenderIIITask);

                    if (CustId == 2 || CustId == 6 || CustId == 4)
                    {
                        PR_Approver _objSaveGMD = new PR_Approver
                        {
                            uuid = Guid.NewGuid(),
                            approverId = item.UserId,
                            PRId = PR.PRId
                        };
                        db.PR_Approver.Add(_objSaveGMD);
                        db.SaveChanges();
                    }
                    else
                    {
                        PR_RecommenderCFO _objSaveCFO = new PR_RecommenderCFO
                        {
                            uuid = Guid.NewGuid(),
                            recommenderId = item.UserId,
                            PRId = PR.PRId
                        };
                        db.PR_RecommenderCFO.Add(_objSaveCFO);
                        db.SaveChanges();
                    }

                    PRModel x = new PRModel();
                    x.PRId = PR.PRId;
                    x.UserId = UserId;
                    x.EmailAddress = item.EmailAddress;
                    x.NewPRForm = new NewPRModel();
                    x.NewPRForm.PRNo = PR.PRNo;
                    x.NewPRForm.ApproverId = item.UserId;
                    x.NewPRForm.ApproverName = item.FullName;
                    SendEmailPRNotification(x, "ToRecommenderIINoti");
                }

                return Json("Successfully joint recommended");
            }
            else
            {
                return Redirect("~/Home/Index");
            }
        }

        public ActionResult ApprovePRJointRecommendedII()
        {
            if (User.Identity.IsAuthenticated && Session["UserId"] != null)
            {
                int PrId = Int32.Parse(Request["PrId"]);
                var PR = db.PurchaseRequisitions.First(x => x.PRId == PrId);
                // todo the session must be HOC
                int UserId = Int32.Parse(Session["UserId"].ToString());
                var requestorDone = (from m in db.NotificationMsgs
                                     join n in db.NotiGroups on m.msgId equals n.msgId
                                     where n.toUserId == UserId && m.PRId == PrId && m.msgType == "Task"
                                     select new PRModel()
                                     {
                                         MsgId = m.msgId
                                     }).First();

                NotificationMsg getDone = db.NotificationMsgs.First(m => m.msgId == requestorDone.MsgId);
                getDone.done = true;
                db.SaveChanges();

                    PR.StatusId = "PR05";
                    db.SaveChanges();

                    PR_RecommenderCFO SaveRecommenderInfo = db.PR_RecommenderCFO.First(m => m.PRId == PR.PRId);
                    SaveRecommenderInfo.recommendedDate = DateTime.Now;
                    SaveRecommenderInfo.recommended = 1;
                    db.SaveChanges();

                    var getGMD = (from m in db.Users
                                  join n in db.Users_Roles on m.userId equals n.userId
                                  //join o in db.Roles on n.roleId equals o.roleId
                                  where n.roleId == "R14" && m.companyId == 2
                                  select new PRModel()
                                  {
                                      UserId = n.userId,
                                      FullName = m.firstName + " " + m.lastName,
                                      EmailAddress = m.emailAddress
                                  }).ToList();

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

                    foreach (var item in getGMD)
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

                        PRModel x = new PRModel();
                        x.PRId = PR.PRId;
                        x.UserId = UserId;
                        x.EmailAddress = item.EmailAddress;
                        x.NewPRForm = new NewPRModel();
                        x.NewPRForm.PRNo = PR.PRNo;
                        x.NewPRForm.ApproverId = item.UserId;
                        x.NewPRForm.ApproverName = item.FullName;
                        SendEmailPRNotification(x, "ToApproverNoti");
                    }

                return Json("Successfully joint recommended");
            }
            else
            {
                return Redirect("~/Home/Index");
            }
        }

        public ActionResult ApprovePRApprover()
        {
            if (User.Identity.IsAuthenticated && Session["UserId"] != null)
            {
                int PrId = Int32.Parse(Request["PrId"]);
                string PRType = Request["PRType"].ToString();
                var PR = db.PurchaseRequisitions.First(x => x.PRId == PrId);
                PR.StatusId = "PR06";
                db.SaveChanges();

                PR_Approver SaveApproverInfo = db.PR_Approver.First(m => m.PRId == PR.PRId);
                SaveApproverInfo.approverApprovedDate = DateTime.Now;
                SaveApproverInfo.approverApproved = 1;
                db.SaveChanges();

                int UserId = Int32.Parse(Session["UserId"].ToString());
                var requestorDone = (from m in db.NotificationMsgs
                                     where m.PRId == PrId && m.msgType == "Task"
                                     select new PRModel()
                                     {
                                         MsgId = m.msgId
                                     }).OrderByDescending(m => m.MsgId).First();

                NotificationMsg getDone = db.NotificationMsgs.First(m => m.msgId == requestorDone.MsgId);
                getDone.done = true;
                db.SaveChanges();

                if (PRType != "Blanket")
                {
                    var getPRPreparerChildCustId = db.Users.First(m => m.userId == PR.PreparedById);
                    var getProcurement = new List<PRModel>();
                    if (PR.CustId == 3 && getPRPreparerChildCustId.childCompanyId == 16)
                    {
                        getProcurement = (from m in db.Users
                                          join n in db.Users_Roles on m.userId equals n.userId
                                          where n.roleId == "R03" && (m.companyId == PR.CustId || m.companyId == 2) && m.childCompanyId == getPRPreparerChildCustId.childCompanyId
                                          select new PRModel()
                                          {
                                              UserId = m.userId,
                                              FullName = m.firstName + " " + m.lastName,
                                              EmailAddress = m.emailAddress
                                          }).ToList();
                    }
                    else
                    {
                        getProcurement = (from m in db.Users
                                          join n in db.Users_Roles on m.userId equals n.userId
                                          where n.roleId == "R03" && (m.companyId == PR.CustId && m.childCompanyId != 16)
                                          select new PRModel()
                                          {
                                              UserId = m.userId,
                                              FullName = m.firstName + " " + m.lastName,
                                              EmailAddress = m.emailAddress
                                          }).ToList();
                    }
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

                        PRModel x = new PRModel();
                        x.PRId = PR.PRId;
                        x.UserId = UserId;
                        x.EmailAddress = item.EmailAddress;
                        x.NewPRForm = new NewPRModel();
                        x.NewPRForm.PRNo = PR.PRNo;
                        x.NewPRForm.ApproverId = item.UserId;
                        x.NewPRForm.ApproverName = item.FullName;
                        SendEmailPRNotification(x, "ToProcurementNoti");
                    }
                }
                else
                {
                    NotificationMsg objTask = new NotificationMsg()
                    {
                        uuid = Guid.NewGuid(),
                        message = PR.PRNo + " pending for you to issue new PO (blanket)",
                        fromUserId = Int32.Parse(Session["UserId"].ToString()),
                        msgDate = DateTime.Now,
                        msgType = "Task",
                        PRId = PrId
                    };
                    db.NotificationMsgs.Add(objTask);
                    db.SaveChanges();

                    NotiGroup Task = new NotiGroup()
                    {
                        uuid = Guid.NewGuid(),
                        msgId = objTask.msgId,
                        toUserId = PR.PreparedById
                    };
                    db.NotiGroups.Add(Task);
                    db.SaveChanges();
                }
                // todo the session must be HOC

                return Json("Successfully approve");
            }
            else
            {
                return Redirect("~/Home/Index");
            }
        }

        public ActionResult RejectPRApprover()
        {
            if (User.Identity.IsAuthenticated && Session["UserId"] != null)
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
                                     where m.PRId == PrId && m.msgType == "Task"
                                     select new PRModel()
                                     {
                                         MsgId = m.msgId
                                     }).OrderByDescending(m => m.MsgId).First();

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
            else
            {
                return Redirect("~/Home/Index");
            }
        }
        //public ActionResult ApprovePRJointRecommendedIII()
        //{
        //    if (User.Identity.IsAuthenticated && Session["UserId"] != null)
        //    {
        //        int PrId = Int32.Parse(Request["PrId"]);
        //        var PR = db.PurchaseRequisitions.First(x => x.PRId == PrId);
        //        PR.StatusId = "PR05";
        //        db.SaveChanges();

        //        // todo the session must be HOC
        //        int UserId = Int32.Parse(Session["UserId"].ToString());
        //        var requestorDone = (from m in db.NotificationMsgs
        //                             join n in db.NotiGroups on m.msgId equals n.msgId
        //                             where n.toUserId == UserId && m.PRId == PrId && m.msgType == "Task"
        //                             select new PRModel()
        //                             {
        //                                 MsgId = m.msgId
        //                             }).First();

        //        NotificationMsg getDone = db.NotificationMsgs.First(m => m.msgId == requestorDone.MsgId);
        //        getDone.done = true;
        //        db.SaveChanges();

        //        PR_RecommenderCFO SaveRecommenderInfo = db.PR_RecommenderCFO.First(m => m.PRId == PR.PRId);
        //        SaveRecommenderInfo.recommendedDate = DateTime.Now;
        //        SaveRecommenderInfo.recommended = 1;
        //        db.SaveChanges();

        //        var getGMD = (from m in db.Users
        //                      join n in db.Users_Roles on m.userId equals n.userId
        //                      //join o in db.Roles on n.roleId equals o.roleId
        //                      where n.roleId == "R14" && m.companyId == 2
        //                      select new PRModel()
        //                      {
        //                          UserId = n.userId,
        //                          FullName = m.firstName + " " + m.lastName,
        //                          EmailAddress = m.emailAddress
        //                      }).ToList();

        //        NotificationMsg objTask = new NotificationMsg()
        //        {
        //            uuid = Guid.NewGuid(),
        //            message = PR.PRNo + " pending for your approval",
        //            fromUserId = UserId,
        //            msgDate = DateTime.Now,
        //            msgType = "Task",
        //            PRId = PR.PRId
        //        };
        //        db.NotificationMsgs.Add(objTask);
        //        db.SaveChanges();

        //        foreach (var item in getGMD)
        //        {
        //            NotiGroup Task = new NotiGroup()
        //            {
        //                uuid = Guid.NewGuid(),
        //                msgId = objTask.msgId,
        //                toUserId = item.UserId
        //            };
        //            db.NotiGroups.Add(Task);

        //            PR_Approver saveApprover = new PR_Approver()
        //            {
        //                uuid = Guid.NewGuid(),
        //                approverId = item.UserId,
        //                PRId = PR.PRId
        //            };
        //            db.PR_Approver.Add(saveApprover);
        //            db.SaveChanges();

        //            PRModel x = new PRModel();
        //            x.PRId = PR.PRId;
        //            x.UserId = UserId;
        //            x.EmailAddress = item.EmailAddress;
        //            x.NewPRForm = new NewPRModel();
        //            x.NewPRForm.PRNo = PR.PRNo;
        //            x.NewPRForm.ApproverId = item.UserId;
        //            x.NewPRForm.ApproverName = item.FullName;
        //            SendEmailPRNotification(x, "ToApproverNoti");
        //        }

        //        return Json("Successfully joint recommended");
        //    }
        //    else
        //    {
        //        return Redirect("~/Home/Index");
        //    }
        //}

        public JsonResult CancelPR()
        {
            if (User.Identity.IsAuthenticated && Session["UserId"] != null)
            {
                int UserId = Int32.Parse(Session["UserId"].ToString());
                int PrId = Int32.Parse(Request["PRId"].ToString());
                int CustId = Int32.Parse(Session["CompanyId"].ToString());
                string CancelType = Request["CancelType"].ToString();
                //try
                //{
                var UpdatePRStatus = db.PurchaseRequisitions.Where(m => m.PRId == PrId).First();
                var getPreparerChildCustId = db.Users.Where(m => m.userId == UpdatePRStatus.PreparedById).First();
                if (CancelType == "CancelPRPhase1" || CancelType == "CancelPR" || CancelType == "CancelPRProcurement")
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

                    if (CancelType == "CancelPR")
                    {
                        return Json(new { success = true, flow = "newPR" });
                    }
                    else if (CancelType == "CancelPRProcurement")
                    {
                        var requestorDone = (from m in db.NotificationMsgs
                                             join n in db.NotiGroups on m.msgId equals n.msgId
                                             where n.toUserId != null && m.PRId == PrId && m.msgType == "Task"
                                             select new PRModel()
                                             {
                                                 MsgId = m.msgId
                                             }).ToList();

                        foreach (var item in requestorDone)
                        {
                            UpdatePRStatus.StatusId = "PR07";
                            NotificationMsg getDone = db.NotificationMsgs.First(m => m.msgId == item.MsgId);
                            getDone.done = true;
                            db.SaveChanges();
                        }
                        
                        return Json(new { success = true, flow = "phase1", message = "Successfully reject the PR." });
                    }
                    else
                    {
                        var requestorDone = (from m in db.NotificationMsgs
                                             join n in db.NotiGroups on m.msgId equals n.msgId
                                             where n.toUserId != null && m.PRId == PrId && m.msgType == "Task"
                                             select new PRModel()
                                             {
                                                 MsgId = m.msgId
                                             }).ToList();

                        foreach (var item in requestorDone)
                        {
                            UpdatePRStatus.StatusId = "PR08";
                            NotificationMsg getDone = db.NotificationMsgs.First(m => m.msgId == item.MsgId);
                            getDone.done = true;
                            db.SaveChanges();
                        }

                        return Json(new { success = true, flow = "phase1", message = "Successfully cancel the PR." });
                    }
                }
                else
                {
                    UpdatePRStatus.StatusId = "PR15";

                    NotificationMsg objNotification = new NotificationMsg()
                    {
                        uuid = Guid.NewGuid(),
                        message = Session["FullName"].ToString() + " has request to cancel the PR No: " + UpdatePRStatus.PRNo + " subject recommendation from HOD. ",
                        fromUserId = Int32.Parse(Session["UserId"].ToString()),
                        msgDate = DateTime.Now,
                        msgType = "Trail",
                        PRId = PrId
                    };
                    db.NotificationMsgs.Add(objNotification);
                    db.SaveChanges();

                    //actual HOD
                    var getHOD = new List<NewPRModel>();
                    if (UpdatePRStatus.CustId == 2 || (UpdatePRStatus.CustId == 3 && getPreparerChildCustId.childCompanyId == 16))
                    {
                        getHOD = (from m in db.Users
                                  join n in db.Users_Roles on m.userId equals n.userId
                                  join o in db.ChildCustomers on m.childCompanyId equals o.childCustId
                                  where (n.roleId == "R02" || n.roleId == "R10") && m.companyId == UpdatePRStatus.CustId && m.childCompanyId == getPreparerChildCustId.childCompanyId
                                  select new NewPRModel()
                                  {
                                      HODApproverId = m.userId,
                                      HODApproverName = m.firstName + " " + m.lastName,
                                      ApproverEmail = m.emailAddress
                                  }).ToList();
                    }
                    else
                    {
                        getHOD = (from m in db.Users
                                  join o in db.Users on m.superiorId equals o.userId
                                  join n in db.Users_Roles on o.userId equals n.userId
                                  where n.roleId == "R02" && m.companyId == UpdatePRStatus.CustId && m.userId == UpdatePRStatus.PreparedById
                                  select new NewPRModel()
                                  {
                                      HODApproverId = o.userId,
                                      HODApproverName = o.firstName + " " + o.lastName,
                                      ApproverEmail = o.emailAddress
                                  }).ToList();
                    }

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

                    int HODUserId = 0;
                    foreach (var item in getHOD)
                    {
                        if (HODUserId != item.HODApproverId)
                        {
                            NotiGroup Task = new NotiGroup()
                            {
                                uuid = Guid.NewGuid(),
                                msgId = objTask.msgId,
                                toUserId = item.HODApproverId
                            };
                            db.NotiGroups.Add(Task);
                            db.SaveChanges();
                        }
                        HODUserId = item.HODApproverId;
                    }
                }

                return Json(new { success = true, flow = "phase2", message = "Successfully request for cancellation to your HOD" });
                //}
                //catch (DbEntityValidationException e)
                //{
                //    StringWriter ExMessage = new StringWriter();
                //    foreach (var eve in e.EntityValidationErrors)
                //    {
                //        ExMessage.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                //            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                //        foreach (var ve in eve.ValidationErrors)
                //        {
                //            ExMessage.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                //                ve.PropertyName, ve.ErrorMessage);
                //        }                      
                //    }
                //    ExMessage.Close();
                //    return Json(new { success = false, exception = true, message = ExMessage.ToString() });
                //}

                //todo noti to expected user
            }
            else
            {
                var redirectUrl = new UrlHelper(Request.RequestContext).Action("Index", "Home", new { /* no params */ });
                return Json(new { success = false, url = redirectUrl });
            }
        }

        public ActionResult RecommendCancel()
        {
            if (User.Identity.IsAuthenticated && Session["UserId"] != null)
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

                var getPRPreparerChildCustId = db.Users.First(m => m.userId == UpdatePRStatus.PreparedById);
                var getProcurement = new List<PRModel>();
                if (UpdatePRStatus.CustId == 3 && getPRPreparerChildCustId.childCompanyId == 16)
                {
                    getProcurement = (from m in db.Users
                                      join n in db.Users_Roles on m.userId equals n.userId
                                      where n.roleId == "R03" && (m.companyId == UpdatePRStatus.CustId || m.companyId == 2) && m.childCompanyId == getPRPreparerChildCustId.childCompanyId
                                      select new PRModel()
                                      {
                                          UserId = m.userId,
                                          FullName = m.firstName + " " + m.lastName,
                                          EmailAddress = m.emailAddress
                                      }).ToList();
                }
                else
                {
                    getProcurement = (from m in db.Users
                                      join n in db.Users_Roles on m.userId equals n.userId
                                      where n.roleId == "R03" && (m.companyId == UpdatePRStatus.CustId && m.childCompanyId != 16)
                                      select new PRModel()
                                      {
                                          UserId = m.userId,
                                          FullName = m.firstName + " " + m.lastName,
                                          EmailAddress = m.emailAddress
                                      }).ToList();
                }

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
            else
            {
                return Redirect("~/Home/Index");
            }
        }

        public ActionResult AcceptCancel()
        {
            if (User.Identity.IsAuthenticated && Session["UserId"] != null)
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
            else
            {
                return Redirect("~/Home/Index");
            }
        }
    }

    internal class DashboardPOModel
    {
        public int quantity { get; set; }
        public string description { get; set; }
    }
}