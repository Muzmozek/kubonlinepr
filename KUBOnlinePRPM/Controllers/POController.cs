﻿using KUBOnlinePRPM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Serialization;
using KUBOnlinePRPM.Controllers;
using System.Data.Entity.Validation;
using System.IO;
using System.Configuration;

namespace KUBOnlinePRPM.Controllers
{
    public class POController : DBLogicController
    {
        private KUBOnlinePREntities db = new KUBOnlinePREntities();
        // GET: PO
        [CheckSessionOut]
        public ActionResult Index()
        {
            return View();
        }

        [CheckSessionOut]
        public ActionResult NewPO(string type)
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
                var VendorListQuery = (from m in db.Vendors
                                       where m.custId == CustId
                                       select new
                                       {
                                           VendorId = m.vendorId,
                                           VendorName = m.vendorNo + " - " + m.name,
                                           VendorNo = m.vendorNo,
                                       }).OrderBy(c => c.VendorNo).ThenBy(c => c.VendorName);
                var VendorStaffQuery = db.VendorStaffs.Select(c => new { StaffId = c.staffId, VendorContactName = c.vendorContactName }).OrderBy(c => c.VendorContactName);
                var PayToVendorQuery = (from m in db.Vendors
                                        where m.custId == CustId
                                        select new
                                        {
                                            VendorId = m.vendorId,
                                            VendorName = m.vendorNo + " - " + m.name,
                                            VendorNo = m.vendorNo,
                                        }).OrderBy(c => c.VendorNo).ThenBy(c => c.VendorName);
                var PaymentTermsCodeQuery = (from m in db.PaymentTerms
                                             where m.custId == CustId
                                             select new
                                             {
                                                 PaymentTermsId = m.paymentTermsId,
                                                 PaymentDescription = m.paymentCode + " - " + m.paymentDescription,
                                                 PaymentCode = m.paymentCode,
                                             }).OrderBy(c => c.PaymentCode).ThenBy(c => c.PaymentDescription);

                int UserId = Int32.Parse(Session["UserId"].ToString());
                var getRole = (from m in db.Users
                               join n in db.Users_Roles on m.userId equals n.userId
                               where m.userId == UserId
                               select new RoleList()
                               {
                                   RoleId = n.roleId,
                                   RoleName = n.Role.name
                               }).ToList();

                POModel NewPO = new POModel
                {
                    UserId = Int32.Parse(Session["UserId"].ToString()),
                    Type = type,
                    RoleIdList = getRole
                };
                int PRId = Int32.Parse(Session["PRId"].ToString());
                NewPO.NewPOForm = (from a in db.PurchaseRequisitions
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
                                   where a.PRId == PRId
                                   select new NewPOModel()
                                   {
                                       //PRNo = a.PRNo,
                                       ProjectId = a.ProjectId,
                                       VendorId = a.VendorId,
                                       VendorEmail = w.vendorEmail,
                                       VendorStaffId = w.staffId,
                                       VendorContactNo = w.vendorContactNo,
                                       PreparedById = a.PreparedById,
                                       PODate = a.PreparedDate,
                                       Saved = a.Saved,
                                       //TotalAmountRequired = a.AmountRequired.Value
                                       //StatusId = a.StatusId.Trim()
                                   }).FirstOrDefault();
                NewPO.NewPOForm.POItemListObject = (from m in db.PR_Items
                                                    from n in db.PopulateItemLists.Where(x => m.codeId == x.codeId && m.itemTypeId == x.itemTypeId).DefaultIfEmpty()
                                                        //from p in o.DefaultIfEmpty()
                                                    where m.PRId == PRId && n.custId == CustId && m.outStandingQuantity > 0
                                                    select new POItemsTable()
                                                    {
                                                        ItemsId = m.itemsId,
                                                        ItemTypeId = m.itemTypeId,
                                                        DateRequired = m.dateRequired,
                                                        Description = m.description,
                                                        CodeId = n.codeId,
                                                        ItemCode = n.ItemDescription,
                                                        OutStandingQuantity = m.outStandingQuantity.Value,
                                                        UnitPrice = m.unitPrice.Value,
                                                        //TotalPrice = m.totalPrice,
                                                        UOM = n.UoM
                                                    }).ToList();

                //TotalBudgetedQuantity
                //foreach (var item in NewPO.NewPOForm.POItemListObject)
                //{

                //}

                ViewBag.ProjectNameList = new SelectList(ProjectNameQuery.AsEnumerable(), "projectId", "description", NewPO.NewPOForm.ProjectId);
                ViewBag.VendorList = new SelectList(VendorListQuery.AsEnumerable(), "vendorId", "VendorName", NewPO.NewPOForm.VendorId);
                ViewBag.VendorStaffList = new SelectList(VendorStaffQuery.AsEnumerable(), "staffId", "VendorContactName", NewPO.NewPOForm.VendorStaffId);
                ViewBag.PayToVendorList = new SelectList(PayToVendorQuery.AsEnumerable(), "VendorId", "VendorName", NewPO.NewPOForm.PayToVendorId);
                ViewBag.PaymentTermsCodeList = new SelectList(PaymentTermsCodeQuery.AsEnumerable(), "PaymentTermsId", "PaymentDescription", NewPO.NewPOForm.PaymentTermsId);

                return View(NewPO);
            }
            else
            {
                return Redirect("~/Home/Index");
            }
        }

        [HttpPost]
        public ActionResult NewPO(POModel POModel)
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
                var VendorListQuery = (from m in db.Vendors
                                       where m.custId == CustId
                                       select new
                                       {
                                           VendorId = m.vendorId,
                                           VendorName = m.vendorNo + " - " + m.name,
                                           VendorNo = m.vendorNo,
                                       }).OrderBy(c => c.VendorNo).ThenBy(c => c.VendorName);
                var VendorStaffQuery = db.VendorStaffs.Select(c => new { StaffId = c.staffId, VendorContactName = c.vendorContactName }).OrderBy(c => c.VendorContactName);
                var PayToVendorQuery = (from m in db.Vendors
                                        where m.custId == CustId
                                        select new
                                        {
                                            VendorId = m.vendorId,
                                            VendorName = m.vendorNo + " - " + m.name,
                                            VendorNo = m.vendorNo,
                                        }).OrderBy(c => c.VendorNo).ThenBy(c => c.VendorName);
                var PaymentTermsCodeQuery = (from m in db.PaymentTerms
                                             where m.custId == CustId
                                             select new
                                             {
                                                 PaymentTermsId = m.paymentTermsId,
                                                 PaymentDescription = m.paymentCode + " - " + m.paymentDescription,
                                                 PaymentCode = m.paymentCode,
                                             }).OrderBy(c => c.PaymentCode).ThenBy(c => c.PaymentDescription);

                if (!ModelState.IsValid)
                {
                    ViewBag.ProjectNameList = new SelectList(ProjectNameQuery.AsEnumerable(), "projectId", "projectName", POModel.NewPOForm.ProjectId);
                    ViewBag.VendorList = new SelectList(VendorListQuery.AsEnumerable(), "vendorId", "VendorName", POModel.NewPOForm.VendorId);
                    ViewBag.VendorStaffList = new SelectList(VendorStaffQuery.AsEnumerable(), "staffId", "VendorContactName", POModel.NewPOForm.VendorStaffId);
                    ViewBag.PayToVendorList = new SelectList(PayToVendorQuery.AsEnumerable(), "VendorId", "VendorName", POModel.NewPOForm.PayToVendorId);
                    ViewBag.PaymentTermsCodeList = new SelectList(PaymentTermsCodeQuery.AsEnumerable(), "PaymentTermsId", "PaymentDescription", POModel.NewPOForm.PaymentTermsId);

                    return new JsonResult
                    {
                        Data = new
                        {
                            success = false,
                            exception = true,
                            type = POModel.Type,
                            POId = POModel.POId,
                            Saved = POModel.NewPOForm.SelectSave,
                            Submited = POModel.NewPOForm.SelectSubmit,
                            NewPO = true,
                            view = this.PartialView("NewPO", POModel)
                        },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                POModel.PRId = Int32.Parse(Session["PRId"].ToString());
                POModel.UserId = Int32.Parse(Session["UserId"].ToString());
                POModel.CustId = Int32.Parse(Session["CompanyId"].ToString());
                POSaveDbLogic(POModel);

                return new JsonResult
                {
                    Data = new
                    {
                        success = true,
                        exception = false,
                        type = POModel.Type,
                        POId = POModel.POId,
                        Saved = POModel.NewPOForm.SelectSave,
                        Submited = POModel.NewPOForm.SelectSubmit,
                        NewPO = true,
                        view = this.PartialView("NewPO", POModel)
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            else
            {
                return Redirect("~/Home/Index");
            }
        }

        [CheckSessionOut]
        public ActionResult POList(string Type)
        {
            if (User.Identity.IsAuthenticated && Session["UserId"] != null)
            {
                POModel POList = new POModel();
                if (Type == "All")
                {
                    int UserId = Int32.Parse(Session["UserId"].ToString());
                    int CustId = Int32.Parse(Session["CompanyId"].ToString());
                    if (Session["ifSuperAdmin"] != null || Session["ifHOGPSS"] != null || Session["ifCFO"] != null || Session["ifGMD"] != null)
                    {
                        POList.POListObject = (from m in db.PurchaseOrders
                                               join n in db.Vendors on m.vendorId equals n.vendorId
                                               join o in db.PurchaseRequisitions on m.PRId equals o.PRId
                                               join p in db.Users on m.PreparedById equals p.userId
                                               join q in db.POStatus on m.StatusId equals q.statusId
                                               join t in db.Projects on m.projectId equals t.projectId
                                               join r in db.Customers on t.custId equals r.custId
                                               select new POListTable()
                                               {
                                                   POId = m.POId,
                                                   PONo = m.PONo,
                                                   PODate = m.PODate,
                                                   PRNo = o.PRNo,
                                                   Company = r.name,
                                                   RequestedName = p.userName,
                                                   VendorId = n.vendorNo,
                                                   VendorCompany = n.name,
                                                   TotalPrice = m.TotalPrice,
                                                   POAging = m.POAging,
                                                   Status = q.status,
                                                   POType = o.PRType
                                               }).ToList();
                    }
                    else if (UserId == 89)
                    {
                        POList.POListObject = (from m in db.PurchaseOrders
                                               join n in db.Vendors on m.vendorId equals n.vendorId
                                               join o in db.PurchaseRequisitions on m.PRId equals o.PRId
                                               join p in db.Users on m.PreparedById equals p.userId
                                               join q in db.POStatus on m.StatusId equals q.statusId
                                               join t in db.Projects on m.projectId equals t.projectId
                                               join r in db.Customers on t.custId equals r.custId
                                               where m.CustId == 3 &&  p.childCompanyId == 16
                                               select new POListTable()
                                               {
                                                   POId = m.POId,
                                                   PONo = m.PONo,
                                                   PODate = m.PODate,
                                                   PRNo = o.PRNo,
                                                   Company = r.name,
                                                   RequestedName = p.userName,
                                                   VendorId = n.vendorNo,
                                                   VendorCompany = n.name,
                                                   TotalPrice = m.TotalPrice,
                                                   POAging = m.POAging,
                                                   Status = q.status,
                                                   POType = o.PRType
                                               }).ToList();
                    } else
                    {
                        POList.POListObject = (from m in db.PurchaseOrders
                                               join n in db.Vendors on m.vendorId equals n.vendorId
                                               join o in db.PurchaseRequisitions on m.PRId equals o.PRId
                                               join p in db.Users on m.PreparedById equals p.userId
                                               join q in db.POStatus on m.StatusId equals q.statusId
                                               join t in db.Projects on m.projectId equals t.projectId
                                               join r in db.Customers on t.custId equals r.custId
                                               where m.CustId == CustId
                                               select new POListTable()
                                               {
                                                   POId = m.POId,
                                                   PONo = m.PONo,
                                                   PODate = m.PODate,
                                                   PRNo = o.PRNo,
                                                   Company = r.name,
                                                   RequestedName = p.userName,
                                                   VendorId = n.vendorNo,
                                                   VendorCompany = n.name,
                                                   TotalPrice = m.TotalPrice,
                                                   POAging = m.POAging,
                                                   Status = q.status,
                                                   POType = o.PRType
                                               }).ToList();
                    }
                    
                    POList.Type = Type;

                    return View("POList", POList);
                }
                else if (Type == "Blanket")
                {
                    int PRId = Int32.Parse(Session["PRId"].ToString());
                    POList.POListObject = (from m in db.PurchaseOrders
                                           join n in db.Vendors on m.vendorId equals n.vendorId
                                           join o in db.PurchaseRequisitions on m.PRId equals o.PRId
                                           where o.PRType == "Blanket" && o.PRId == PRId
                                           select new POListTable()
                                           {
                                               POId = m.POId,
                                               PONo = m.PONo,
                                               PODate = m.PODate,
                                               POItem = o.PRNo,
                                               Quantity = m.TotalQuantity,
                                               TotalPrice = m.TotalPrice,
                                               POAging = m.POAging,
                                               POType = o.PRType
                                           }).ToList();
                    PurchaseRequisition getPOBalance = db.PurchaseRequisitions.First(m => m.PRId == PRId);
                    POList.OutstandingQty = getPOBalance.AmountPOBalance.Value;
                    POList.StatusId = getPOBalance.StatusId;
                    POList.Type = Type;

                    return PartialView("POList", POList);
                }
                //if (ifRequestor != null)
                //{
                //    POList.PRListObject = (from m in db.PurchaseRequisitions
                //                           join n in db.Users on m.PreparedById equals n.userId
                //                           join o in db.Vendors on m.VendorId equals o.vendorId into q
                //                           join p in db.PRStatus on m.StatusId equals p.statusId
                //                           from r in q.DefaultIfEmpty()
                //                           where m.PRType == type && m.PreparedById == UserId
                //                           select new POListTable()
                //                           {
                //                               PRId = m.PRId,
                //                               PRNo = m.PRNo,
                //                               PRDate = m.PreparedDate,
                //                               RequestorName = n.firstName + " " + n.lastName,
                //                               VendorCompany = r.name,
                //                               AmountRequired = m.AmountRequired,
                //                               //PRAging = m.aging,
                //                               Status = p.status
                //                           }).ToList();
                //}
                return Redirect("~/Home/Index");
            }
            else
            {
                return Redirect("~/Home/Index");
            }
        }

        [HttpPost]
        public JsonResult ViewPODetails(int POId, string POType)
        {
            Session["POId"] = POId; Session["POType"] = POType;
            var redirectUrl = new UrlHelper(Request.RequestContext).Action("PODetails", "PO", new { /* no params */ });
            return Json(new { success = true, url = redirectUrl });
        }

        public ActionResult PODetails()
        {
            if (User.Identity.IsAuthenticated && Session["UserId"] != null)
            {
                POModel PODetail = new POModel();
                if (Session["POId"] == null)
                {
                    PODetail.POId = Int32.Parse(Request["POId"].ToString());
                    PODetail.Type = Request["POType"].ToString();
                    Session["POId"] = PODetail.POId; Session["POType"] = PODetail.Type;
                }
                else
                {
                    PODetail.POId = Int32.Parse(Session["POId"].ToString());
                    PODetail.Type = Session["POType"].ToString();
                }

                int UserId = Int32.Parse(Session["UserId"].ToString());
                int PRId = db.PurchaseOrders.First(m => m.POId == PODetail.POId).PRId.Value;
                int PRCustId = (from m in db.PurchaseRequisitions
                                join n in db.Projects on m.ProjectId equals n.projectId
                                where m.PRId == PRId
                                select new
                                {
                                    CustId = n.custId
                                }).First().CustId;

                var getRole = (from m in db.Users
                               join n in db.Users_Roles on m.userId equals n.userId
                               where m.userId == UserId
                               select new RoleList()
                               {
                                   RoleId = n.roleId,
                                   RoleName = n.Role.name
                               }).ToList();
                var PayToVendorQuery = (from m in db.Vendors
                                        where m.custId == PRCustId
                                        select new
                                        {
                                            VendorId = m.vendorId,
                                            VendorName = m.vendorNo + " - " + m.name,
                                            VendorNo = m.vendorNo,
                                        }).OrderBy(c => c.VendorNo).ThenBy(c => c.VendorName);
                var PaymentTermsCodeQuery = (from m in db.PaymentTerms
                                             where m.custId == PRCustId
                                             select new
                                             {
                                                 PaymentTermsId = m.paymentTermsId,
                                                 PaymentDescription = m.paymentCode + " - " + m.paymentDescription,
                                                 PaymentCode = m.paymentCode,
                                             }).OrderBy(c => c.PaymentCode).ThenBy(c => c.PaymentDescription);
                var LocationQuery = db.Locations.Select(m => new { LocationCodeId = m.locationId, LocationCode = m.locationCode, Address = m.locationAddress + m.locationCity }).OrderBy(m => m.LocationCode);
                var SpecReviewerQuery = db.Groups.Select(c => new GroupList { GroupId = c.groupId, GroupName = c.groupName }).OrderBy(c => c.GroupName);
                var PurchaserCodeQuery = db.Purchasers.Select(m => new { PurchaserCodeId = m.purchaserId, PurchaserCode = m.purchaserCode }).OrderBy(m => m.PurchaserCode);
                PODetail.UserId = Int32.Parse(Session["UserId"].ToString());
                PODetail.NewPOForm = (from a in db.PurchaseOrders
                                      join b in db.Projects on a.projectId equals b.projectId
                                      join c in db.Vendors on a.vendorId equals c.vendorId
                                      //join d in db.VendorStaffs on a.vendorStaffId equals d.staffId
                                      //join e in db.Users on a.SpecsReviewerId equals e.userId
                                      //join f in db.PopulateItemLists on e.codeId equals f.codeId
                                      join g in db.POStatus on a.StatusId equals g.statusId
                                      join h in db.PurchaseRequisitions on a.PRId equals h.PRId
                                      where a.POId == PODetail.POId /*&& a.CustId == CustId*/
                                      select new NewPOModel()
                                      {
                                          PONo = a.PONo,
                                          ProjectName = b.projectCode,
                                          PRNo = h.PRNo,
                                          PODate = a.PreparedDate,
                                          VendorName = c.name,
                                          VendorCode = c.vendorNo,
                                          VendorQuoteNo = a.VendorQuoteNo,
                                          //VendorContactName = d.vendorContactName,
                                          //VendorEmail = d.vendorEmail,
                                          //VendorStaffId = d.staffId,
                                          //VendorContactNo = d.vendorContactNo,
                                          DiscountAmount = a.DiscountAmount,
                                          DiscountPerc = a.Discount_,
                                          TotalExclSST = a.TotalExcSST,
                                          TotalSST = a.TotalSST,
                                          TotalIncSST = a.TotalIncSST,
                                          PayToVendorId = a.PayToVendorId,
                                          PaymentTermsId = a.PaymentTermsId,
                                          SpecReviewerId = a.SpecsReviewerId,
                                          LocationCodeId = a.LocationCodeId,
                                          OrderDate = a.OrderDate,
                                          PurchaserCodeId = a.PurchaserId,
                                          DeliveryDate = a.DeliveryDate,
                                          StatusId = a.StatusId,
                                          Status = g.status,
                                          Submited = a.Submited
                                      }).FirstOrDefault();             
                PODetail.NewPOForm.POItemListObject = (from m in db.PO_Item
                                                       from n in db.PopulateItemLists.Where(x => m.codeId == x.codeId && m.itemTypeId == x.itemTypeId).DefaultIfEmpty()
                                                       join p in db.PR_Items on m.itemsId equals p.itemsId
                                                       join o in db.Projects on m.jobNoId equals o.projectId into s
                                                       join r in db.ItemTypes on m.itemTypeId equals r.itemTypeId
                                                       join u in db.JobTasks on m.jobTaskNoId equals u.jobTaskNoId into v
                                                       join x in db.TaxCodes on m.taxCodeId equals x.TaxCodeId into b
                                                       join a in db.UOMs on m.UoMId equals a.UoMId into d
                                                       from t in s.DefaultIfEmpty()
                                                       from w in v.DefaultIfEmpty()
                                                       from c in b.DefaultIfEmpty()
                                                       from e in d.DefaultIfEmpty()
                                                       where m.POId == PODetail.POId && n.custId == PRCustId
                                                       select new POItemsTable()
                                                       {
                                                           ItemsId = m.itemsId,
                                                           ItemTypeId = m.itemTypeId,
                                                           DateRequired = m.dateRequired,
                                                           ItemCode = n.ItemDescription,
                                                           Description = m.description,
                                                           CustPONo = m.custPONo,
                                                           Quantity = m.quantity,
                                                           OutStandingQuantity = p.outStandingQuantity.Value,                                                           
                                                           UoMId = m.UoMId,
                                                           UOM = e.UoMCode,
                                                           JobNoId = t.projectId,
                                                           JobNo = t.projectCode,
                                                           JobTaskNoId = m.jobTaskNoId,
                                                           JobTaskNo = w.jobTaskNo,
                                                           UnitPrice = m.unitPrice,
                                                           TaxCodeId = m.taxCodeId,
                                                           TaxCode = c.Code,
                                                           TotalPrice = m.totalPrice,
                                                           UnitPriceIncSST = m.unitPriceIncSST,
                                                           TotalPriceIncSST = m.totalPriceIncSST,
                                                           DimProjectId = m.dimProjectId,
                                                           DimDeptId = m.dimDeptId
                                                       }).ToList();

                ViewBag.PayToVendorList = new SelectList(PayToVendorQuery.AsEnumerable(), "VendorId", "VendorName", PODetail.NewPOForm.PayToVendorId);
                ViewBag.PaymentTermsCodeList = new SelectList(PaymentTermsCodeQuery.AsEnumerable(), "PaymentTermsId", "PaymentDescription", PODetail.NewPOForm.PaymentTermsId);
                ViewBag.SpecReviewerList = new SelectList(SpecReviewerQuery.AsEnumerable(), "GroupId", "GroupName", PODetail.NewPOForm.SpecReviewerId);
                ViewBag.PurchaserCodeList = new SelectList(PurchaserCodeQuery.AsEnumerable(), "PurchaserCodeId", "PurchaserCode", PODetail.NewPOForm.PurchaserCodeId);
                ViewBag.LocationCodeList = new SelectList(LocationQuery.AsEnumerable(), "LocationCodeId", "LocationCode", PODetail.NewPOForm.LocationCodeId);

                return View(PODetail);
            }
            else
            {
                return Redirect("~/Home/Index");
            }
        }

        [HttpPost]
        public ActionResult PODetails(POModel POModel)
        {
            if (User.Identity.IsAuthenticated && Session["UserId"] != null)
            {
                //int i = 1;
                if (!ModelState.IsValid)
                {
                    return new JsonResult
                    {
                        Data = new
                        {
                            success = false,
                            exception = true,
                            type = POModel.Type,
                            PRId = POModel.PRId,
                            Saved = POModel.NewPOForm.SelectSave,
                            Submited = POModel.NewPOForm.SelectSubmit,
                            view = this.RenderPartialView("PODetails", POModel)
                        },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                int UserId = Int32.Parse(Session["UserId"].ToString());
                //var getRole = (from m in db.Users
                //               join n in db.Users_Roles on m.userId equals n.userId
                //               where m.userId == UserId
                //               select new RoleList()
                //               {
                //                   RoleId = n.roleId,
                //                   RoleName = n.Role.name
                //               }).ToList();
                POUpdateDbLogic(POModel);
                POModel.PRId = Int32.Parse(Session["PRId"].ToString());
                return new JsonResult
                {
                    Data = new
                    {
                        success = true,
                        exception = false,
                        type = POModel.Type,
                        PRId = POModel.PRId,
                        Saved = POModel.NewPOForm.SelectSave,
                        Submited = POModel.NewPOForm.SelectSubmit,
                        view = this.PartialView("PODetails", POModel)
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            else
            {
                return Redirect("~/Home/Index");
            }
        }

        [HttpPost]
        public JsonResult IssuePO(PRModel PRModel)
        {
            if (User.Identity.IsAuthenticated && Session["UserId"] != null)
            {
                try
                {
                    int PRCustId = (from m in db.PurchaseRequisitions
                                    join n in db.Projects on m.ProjectId equals n.projectId
                                    where m.PRId == PRModel.PRId
                                    select new
                                    {
                                        CustId = n.custId
                                    }).First().CustId;
                    PurchaseRequisition updatePR = db.PurchaseRequisitions.First(m => m.PRId == PRModel.PRId);
                    var updatePRItem = db.PR_Items.ToList<PR_Items>().Where(m => m.PRId == PRModel.PRId);
                    updatePR.StatusId = "PR14";
                    db.SaveChanges();

                    int UserId = Int32.Parse(Session["UserId"].ToString());
                    if (PRModel.Type == "Blanket")
                    {

                    }
                    else
                    {
                        PurchaseOrder newPO = new PurchaseOrder();
                        newPO.uuid = Guid.NewGuid();
                        newPO.PRId = updatePR.PRId;

                        string CustName = "";
                        switch (PRCustId)
                        {
                            case 1:
                                CustName = "KUBTelsequence";
                                break;
                            case 2:
                                CustName = "KUBMsequence";
                                break;
                            case 3:
                                CustName = "KUBAHsequence";
                                break;
                            case 4:
                                CustName = "KUBMGazsequence";
                                break;
                            case 5:
                                CustName = "KUBPowersequence";
                                break;
                            case 6:
                                CustName = "KUBMasequence";
                                break;
                            case 7:
                                CustName = "KUBMaluasequence";
                                break;
                            case 8:
                                CustName = "KUBSsequence";
                                break;
                        }

                        var checkPOforCust = (from m in db.PurchaseOrders
                                              join n in db.Projects on m.projectId equals n.projectId
                                              where n.custId == PRCustId
                                              select new
                                              {
                                                  PONo = m.PONo,
                                                  POId = m.POId
                                              }).OrderByDescending(m => m.POId).FirstOrDefault();

                        if (checkPOforCust != null)
                        {
                            int NewPOsequence =  Int32.Parse(checkPOforCust.PONo.Split('-')[2]) + 1;
                            if (ConfigurationManager.AppSettings[CustName] != null)
                            {
                                NewPOsequence = Int32.Parse(ConfigurationManager.AppSettings[CustName].ToString()) + 1;
                            } 
                            newPO.PONo = "PO-" + DateTime.Now.Year.ToString().Substring(2) + "-" + string.Format("{0}{1}", 0, NewPOsequence.ToString("D4"));
                        }
                        else
                        {
                            int NewPOsequence = 1;
                            if (ConfigurationManager.AppSettings[CustName] != null)
                            {
                                NewPOsequence = Int32.Parse(ConfigurationManager.AppSettings[CustName].ToString());
                            }
                            newPO.PONo = "PO-" + DateTime.Now.Year.ToString().Substring(2) + "-" + string.Format("{0}{1}", 0, NewPOsequence.ToString("D4"));
                        }
                        //newPO.PONo = "dummyset";
                        newPO.CustId = updatePR.CustId;
                        newPO.PODate = DateTime.Now;
                        newPO.projectId = updatePR.ProjectId;
                        newPO.vendorId = updatePR.VendorId.Value;
                        newPO.VendorQuoteNo = updatePR.VendorQuoteNo;
                        //newPO.vendorStaffId = PRModel.NewPRForm.VendorStaffId;
                        newPO.PreparedById = UserId;
                        newPO.PreparedDate = DateTime.Now;
                        newPO.Saved = 0;
                        newPO.Submited = true;
                        newPO.SubmitDate = DateTime.Now;
                        newPO.POAging = 0;
                        newPO.TotalPrice = updatePR.AmountRequired;
                        newPO.DiscountAmount = updatePR.DiscountAmount;
                        newPO.Discount_ = updatePR.Discount_;
                        newPO.TotalExcSST = updatePR.TotalExclSST;
                        newPO.TotalSST = updatePR.TotalSST;
                        newPO.TotalIncSST = updatePR.TotalIncSST;
                        newPO.SpecsReviewerId = updatePR.SpecsReviewerId;
                        newPO.StatusId = "PO01";
                        db.PurchaseOrders.Add(newPO);
                        db.SaveChanges();

                        db.SaveChanges();

                        int TotalQuantity = 0;
                        foreach (var value in updatePRItem)
                        {
                            PO_Item _objNewPOItem = new PO_Item
                            {
                                uuid = Guid.NewGuid(),
                                POId = newPO.POId,
                                itemsId = value.itemsId,
                                itemTypeId = value.itemTypeId,
                                dateRequired = value.dateRequired,
                                description = value.description,
                                codeId = value.codeId.Value,
                                custPONo = value.custPONo,
                                quantity = value.quantity,
                                UoMId = value.UoMId,
                                unitPrice = value.unitPrice.Value,
                                unitPriceIncSST = value.unitPriceIncSST,
                                totalPrice = value.totalPrice.Value,
                                totalPriceIncSST = value.totalPriceIncSST,
                                jobNoId = value.jobNoId,
                                jobTaskNoId = value.jobTaskNoId,
                                taxCodeId = value.taxCodeId,
                                dimProjectId = value.dimProjectId,
                                dimDeptId = value.dimDeptId
                            };
                            db.PO_Item.Add(_objNewPOItem);
                            db.SaveChanges();

                            PR_Items _objUpdatePRItem = db.PR_Items.First(m => m.itemsId == value.itemsId);
                            _objUpdatePRItem.outStandingQuantity = _objUpdatePRItem.outStandingQuantity - _objNewPOItem.quantity;

                            newPO.POAging = 0;
                            db.SaveChanges();

                            TotalQuantity = TotalQuantity + _objNewPOItem.quantity;
                        }

                        newPO.TotalQuantity = TotalQuantity;
                        NotificationMsg _objSubmited = new NotificationMsg
                        {
                            uuid = Guid.NewGuid(),
                            POId = newPO.POId,
                            msgDate = DateTime.Now,
                            fromUserId = UserId,
                            msgType = "Trail",
                            message = Session["FullName"].ToString() + " has issue new PO No. " + newPO.PONo + " subject for confirmation"
                        };
                        db.NotificationMsgs.Add(_objSubmited);

                        var requestorDone = (from m in db.NotificationMsgs
                                             where m.PRId == updatePR.PRId && m.msgType == "Task"
                                             select new PRModel()
                                             {
                                                 MsgId = m.msgId
                                             }).OrderByDescending(m => m.MsgId).First();

                        NotificationMsg getDone = db.NotificationMsgs.First(m => m.msgId == requestorDone.MsgId);
                        getDone.done = true;
                        db.SaveChanges();
                    }
                    PRModel.UserId = UserId;
                    PRModel.FullName = Session["FullName"].ToString();
                    PRModel.CustId = updatePR.CustId;
                    PRModel.NewPRForm.PRNo = updatePR.PRNo;
                    PRModel.NewPRForm.Scenario = updatePR.Scenario;
                    SendEmailPONotification(PRModel, "IssuePO");

                    return Json(new { success = true, message = "The PO has been issued" });
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
        public JsonResult ComfirmPO()
        {
            if (User.Identity.IsAuthenticated && Session["UserId"] != null)
            {
                try
                {
                    int? PayToVendorId = null; int? PaymentTermsId = null; int? LocationCodeId = null; DateTime? OrderDate = null; int? PurchaserCodeId = null;
                    DateTime? DeliveryDate = null;

                    if (Request["PayToVendorId"] != "")
                    {
                        PayToVendorId = Int32.Parse(Request["PayToVendorId"]);
                    }                    
                    if (Request["LocationCodeId"] != "")
                    {
                        LocationCodeId = Int32.Parse(Request["LocationCodeId"]);
                    }
                    if (Request["OrderDate"] != "")
                    {
                        OrderDate = DateTime.Parse(Request["OrderDate"]);
                    }
                    if (Request["PaymentTermsId"] != "")
                    {
                        PaymentTermsId = Int32.Parse(Request["PaymentTermsId"]);
                    }
                    if (Request["PurchaserCodeId"] != "")
                    {
                        PurchaserCodeId = Int32.Parse(Request["PurchaserCodeId"]);
                    }
                    if (Request["DeliveryDate"] != "")
                    {
                        DeliveryDate = DateTime.Parse(Request["DeliveryDate"]);
                    }
                    int POId = Int32.Parse(Request["POId"]);                   
                    int UserId = Int32.Parse(Session["UserId"].ToString());

                    PurchaseOrder updatePO = db.PurchaseOrders.First(m => m.POId == POId);
                    var getScenario = db.PurchaseRequisitions.First(m => m.PRId == updatePO.PRId);
                    int CustId = db.Projects.First(m => m.projectId == getScenario.ProjectId).custId;

                    updatePO.PayToVendorId = PayToVendorId;
                    updatePO.LocationCodeId = LocationCodeId;
                    updatePO.OrderDate = OrderDate;
                    updatePO.PaymentTermsId = PaymentTermsId;
                    updatePO.PurchaserId = PurchaserCodeId;
                    updatePO.DeliveryDate = DeliveryDate;
                    updatePO.StatusId = "PO03";
                    //var POItemList = (from m in db.PO_Item
                    //                  from n in db.PopulateItemLists.Where(x => m.codeId == x.codeId && m.itemTypeId == x.itemTypeId).DefaultIfEmpty()
                    //                  join p in db.PR_Items on m.itemsId equals p.itemsId
                    //                  where m.POId == POId && n.custId == CustId
                    //                  select new POItemsTable()
                    //                  {
                    //                      ItemsId = m.itemsId,
                    //                      ItemTypeId = m.itemTypeId,
                    //                      DateRequired = m.dateRequired,
                    //                      ItemCode = n.ItemDescription,
                    //                      Description = m.description,
                    //                      CustPONo = m.custPONo,
                    //                      Quantity = m.quantity,
                    //                      OutStandingQuantity = p.outStandingQuantity.Value,
                    //                      UnitPrice = m.unitPrice,
                    //                      TotalPrice = m.totalPrice
                    //                      UOM = n.UoM
                    //                  }).ToList();

                    //Project UpdateBudgetBalance = db.Projects.First(m => m.projectId == updatePO.projectId);
                    //decimal getAmountRequired = db.PurchaseRequisitions.First(m => m.PRId == updatePO.PRId).AmountRequired;
                    //UpdateBudgetBalance.utilizedToDate = UpdateBudgetBalance.utilizedToDate + getAmountRequired;
                    //UpdateBudgetBalance.budgetBalance = UpdateBudgetBalance.budgetedAmount - UpdateBudgetBalance.utilizedToDate;

                    NotificationMsg _objConfirmed = new NotificationMsg
                    {
                        uuid = Guid.NewGuid(),
                        POId = POId,
                        msgDate = DateTime.Now,
                        fromUserId = UserId,
                        msgType = "Trail",
                        message = Session["FullName"].ToString() + " has confirmed new PO No. " + updatePO.PONo
                    };
                    db.NotificationMsgs.Add(_objConfirmed);
                    db.SaveChanges();

                    PRModel PRModel = new PRModel();
                    PRModel.FullName = Session["FullName"].ToString();
                    PRModel.PRId = updatePO.PRId.Value;
                    PRModel.UserId = UserId;
                    PRModel.CustId = updatePO.CustId;
                    PRModel.NewPRForm = new NewPRModel();
                    PRModel.NewPRForm.Scenario = getScenario.Scenario;
                    SendEmailPONotification(PRModel, "ConfirmPO");
                    //XmlWriterSettings setting = new XmlWriterSettings();
                    //setting.ConformanceLevel = ConformanceLevel.Auto;
                    //using (XmlWriter writer = XmlWriter.Create(@"D:\test.xml", setting))
                    //{
                    //    writer.WriteStartElement("PurchaseOrderDetails");
                    //    writer.WriteElementString("PONo", updatePO.PONo);
                    //    writer.WriteElementString("CustId", updatePO.CustId.ToString());
                    //    writer.WriteElementString("PODate", updatePO.PODate.ToString());
                    //    writer.WriteElementString("ProjectId", updatePO.projectId.ToString());
                    //    writer.WriteElementString("VendorId", updatePO.vendorId.ToString());
                    //    writer.WriteElementString("StatusId", updatePO.StatusId.ToString());

                    //    foreach (var item in POItemList)
                    //    {
                    //        writer.WriteStartElement("POItemFor-" + updatePO.PONo);
                    //        writer.WriteElementString("DateRequired", item.DateRequired.ToString());
                    //        writer.WriteElementString("ItemCode", item.ItemCode);
                    //        writer.WriteElementString("Description", item.Description);
                    //        writer.WriteElementString("CustPONo", item.CustPONo);
                    //        writer.WriteElementString("Quantity", item.Quantity.ToString());
                    //        writer.WriteElementString("UnitPrice", item.UnitPrice.ToString());
                    //        writer.WriteElementString("TotalPrice", item.TotalPrice.ToString());
                    //        writer.WriteEndElement();
                    //    }
                    //    writer.WriteEndElement();
                    //    writer.Flush();
                    //}
                    return Json(new { success = true, message = "The PO has been confirmed" });
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
        public ActionResult CancelPO()
        {
            if (User.Identity.IsAuthenticated && Session["UserId"] != null)
            {
                int UserId = Int32.Parse(Session["UserId"].ToString());
                int POId = Int32.Parse(Request["POId"].ToString());
                int CustId = Int32.Parse(Session["CompanyId"].ToString());
                var UpdatePOStatus = db.PurchaseOrders.Where(m => m.POId == POId).First();
                UpdatePOStatus.StatusId = "PO05";
                NotificationMsg objNotification = new NotificationMsg()
                {
                    uuid = Guid.NewGuid(),
                    message = Session["FullName"].ToString() + " has cancel the PO No: " + UpdatePOStatus.PONo + " application. ",
                    fromUserId = Int32.Parse(Session["UserId"].ToString()),
                    msgDate = DateTime.Now,
                    msgType = "Trail",
                    POId = POId
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

        public ActionResult POForm()
        {
            if (User.Identity.IsAuthenticated && Session["UserId"] != null)
            {
                POModel PODetail = new POModel
                {
                    POId = Int32.Parse(Session["POId"].ToString()),
                    UserId = Int32.Parse(Session["UserId"].ToString()),                   
                };

                PODetail.NewPOForm = (from a in db.PurchaseOrders
                                      join b in db.Users on a.PreparedById equals b.userId
                                      join c in db.POStatus on a.StatusId equals c.statusId
                                      join d in db.Projects on a.projectId equals d.projectId
                                      join e in db.Vendors on a.vendorId equals e.vendorId into f
                                      //join g in db.VendorStaffs on a.VendorStaffId equals g.staffId into l
                                      join g in db.Projects on a.projectId equals g.projectId
                                      join h in db.PO_Item on a.POId equals h.POId
                                      join i in db.PopulateItemLists on h.codeId equals i.codeId into j
                                      join k in db.PaymentTerms on a.PaymentTermsId equals k.paymentTermsId
                                      join l in db.Purchasers on a.PurchaserId equals l.purchaserId into m
                                      join o in db.Locations on a.LocationCodeId equals o.locationId into p
                                      from n in m.DefaultIfEmpty()
                                          //from v in r.DefaultIfEmpty()
                                      from q in p.DefaultIfEmpty()
                                      from y in f.DefaultIfEmpty()
                                      from z in j.DefaultIfEmpty()
                                      where a.POId == PODetail.POId
                                      select new NewPOModel()
                                      {
                                          PONo = a.PONo,
                                          PODate = a.PreparedDate,
                                          OrderDate = a.OrderDate,
                                          ProjectId = a.projectId,
                                          ProjectName = g.projectName,
                                          CustId = d.custId,
                                          VendorId = a.vendorId,
                                          VendorCode = y.vendorNo,
                                          VendorName = y.name,
                                          PayToVendorId = a.PayToVendorId,
                                          PayToVendorName = y.name,
                                          PaymentTermsId = a.PaymentTermsId,
                                          PaymentTermsCode = k.paymentDescription,
                                          VendorQuoteNo = a.VendorQuoteNo,
                                          //VendorEmail = x.vendorEmail,
                                          //VendorStaffId = x.staffId,
                                          //VendorContactName = x.vendorContactName,
                                          //VendorContactNo = x.vendorContactNo,
                                          DeliveryDate = a.DeliveryDate,
                                          DiscountAmount = a.DiscountAmount,
                                          PurchaserCode = n.purchaserCode,
                                          DeliveryTo = q.locationAddress + " " + q.locationCity + " " + q.PostCode + " " + q.State + " " + q.CountryRegionCode,
                                          AmountRequired = a.TotalPrice,
                                          TotalBeforeDisc = a.TotalExcSST + a.DiscountAmount,
                                          TotalExclSST = a.TotalExcSST,
                                          TotalSST = a.TotalSST,
                                          TotalIncSST = a.TotalIncSST,
                                          PreparedById = a.PreparedById.Value,
                                          Saved = a.Saved,
                                          Submited = a.Submited,
                                          StatusId = a.StatusId.Trim()
                                      }).FirstOrDefault();

                var getUserDetails = (from m in db.PurchaseOrders
                                      join o in db.PurchaseRequisitions on m.PRId equals o.PRId
                                      join n in db.Users on o.PreparedById equals n.userId
                                      where m.POId == PODetail.POId
                                      select new
                                      {
                                          CustId = m.CustId,
                                          ChildCustId = n.childCompanyId
                                      }).First();
                int PRCustId = (from m in db.PurchaseOrders
                                join n in db.Projects on m.projectId equals n.projectId
                                where m.POId == PODetail.POId
                                select new
                                {
                                    CustId = n.custId
                                }).First().CustId;

                PODetail.POItemList = (from m in db.PO_Item
                                       from n in db.PopulateItemLists.Where(x => m.codeId == x.codeId && m.itemTypeId == x.itemTypeId)
                                       join o in db.TaxCodes on m.taxCodeId equals o.TaxCodeId into p
                                       join r in db.UOMs on m.UoMId equals r.UoMId into s
                                       from q in p.DefaultIfEmpty()
                                       from t in s.DefaultIfEmpty()
                                       where m.POId == PODetail.POId && n.custId == PRCustId
                                       select new POItemsTable()
                                       {
                                           ItemsId = m.itemsId,
                                           DateRequired = m.dateRequired,
                                           Description = m.description,
                                           ItemCode = n.ItemCode,
                                           CustPONo = m.custPONo,
                                           Quantity = m.quantity,
                                           UnitPrice = m.unitPrice,
                                           //SST = m.sst,
                                           TotalPrice = m.totalPrice,
                                           DimDeptId = m.dimDeptId,
                                           DimProjectId = m.dimProjectId,
                                           TotalPriceIncSST = m.totalPriceIncSST,
                                           TaxCode = q.Code,
                                           TaxPerc = q.GST,
                                           UOM = t.UoMCode
                                       }).ToList();

                return new RazorPDF.PdfActionResult("POForm", PODetail);
            }
            else
            {
                return Redirect("~/Home/Index");
            }
        }
    }
}