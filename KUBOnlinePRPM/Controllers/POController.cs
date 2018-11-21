﻿using KUBOnlinePRPM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KUBOnlinePRPM.Controllers
{
    public class POController : Controller
    {
        private KUBOnlinePREntities db = new KUBOnlinePREntities();
        // GET: PO
        public ActionResult Index()
        {
            return View();
        }
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
                                       PreparedDate = a.PreparedDate,
                                       Saved = a.Saved,
                                       //TotalAmountRequired = a.AmountRequired.Value
                                       //StatusId = a.StatusId.Trim()
                                   }).FirstOrDefault();
                NewPO.NewPOForm.POItemListObject = (from m in db.PR_Items
                                                    join n in db.PopulateItemLists on m.codeId equals n.codeId into o
                                                    from p in o.DefaultIfEmpty()
                                                    where m.PRId == PRId
                                                    select new POItemsTable()
                                                    {
                                                        ItemsId = m.itemsId,
                                                        DateRequired = m.dateRequired,
                                                        Description = m.description,
                                                        CodeId = p.codeId,
                                                        ItemCode = p.ItemCode,
                                                        OutStandingQuantity = m.outStandingQuantity.Value,
                                                        UnitPrice = m.unitPrice.Value,
                                                        //TotalPrice = m.totalPrice,
                                                        UOM = p.UoM
                                                    }).ToList();

                //TotalBudgetedQuantity
                //foreach (var item in NewPO.NewPOForm.POItemListObject)
                //{

                //}

                ViewBag.ProjectNameList = new SelectList(ProjectNameQuery.AsEnumerable(), "projectId", "projectName", NewPO.NewPOForm.ProjectId);
                ViewBag.VendorList = new SelectList(VendorListQuery.AsEnumerable(), "vendorId", "VendorName", NewPO.NewPOForm.VendorId);
                ViewBag.VendorStaffList = new SelectList(VendorStaffQuery.AsEnumerable(), "staffId", "VendorContactName", NewPO.NewPOForm.VendorStaffId);

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

                if (!ModelState.IsValid)
                {
                    ViewBag.ProjectNameList = new SelectList(ProjectNameQuery.AsEnumerable(), "projectId", "projectName", POModel.NewPOForm.ProjectId);
                    ViewBag.VendorList = new SelectList(VendorListQuery.AsEnumerable(), "vendorId", "VendorName", POModel.NewPOForm.VendorId);
                    ViewBag.VendorStaffList = new SelectList(VendorStaffQuery.AsEnumerable(), "staffId", "VendorContactName", POModel.NewPOForm.VendorStaffId);

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
                DBLogicController.POSaveDbLogic(POModel);


                //var CalcPOBalance = (from m in db.PurchaseRequisitions
                //                     join n in db.Projects on m.ProjectId equals n.projectId
                //                     join o in db.PurchaseOrders on m.PRId equals o.PRId
                //                     where m.PRId == newPO.PRId.Value
                //                     select new NewPOModel()
                //                     {
                //                         AmountPOBalance = m.AmountRequired.Value - POModel.NewPOForm.AmountRequired,
                //                    }).ToList();
                //Project updateProject = db.Projects.First(m => m.projectId == POModel.NewPOForm.ProjectId);
                //PurchaseRequisition updatePR = db.PurchaseRequisitions.First(m => m.PRId == newPO.PRId.Value);
                //updatePR.AmountPOBalance = updatePR.AmountRequired - POModel.NewPOForm.AmountRequired;

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

        public ActionResult POList(string Type)
        {
            if (User.Identity.IsAuthenticated && Session["UserId"] != null)
            {
                POModel POList = new POModel();
                if (Type == "All")
                {
                    POList.POListObject = (from m in db.PurchaseOrders
                                           join n in db.Vendors on m.vendorId equals n.vendorId
                                           join o in db.PurchaseRequisitions on m.PRId equals o.PRId
                                           select new POListTable()
                                           {
                                               POId = m.POId,
                                               PONo = m.PONo,
                                               PODate = m.PODate,
                                               PRNo = o.PRNo,
                                               VendorCompany = n.name,
                                               AmountRequired = o.AmountRequired,
                                               POAging = m.POAging,
                                               POType = o.PRType
                                           }).ToList();
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
                                               PRNo = o.PRNo,
                                               VendorCompany = n.name,
                                               AmountRequired = o.AmountRequired,
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
                int UserId = Int32.Parse(Session["UserId"].ToString());
                int CustId = Int32.Parse(Session["CompanyId"].ToString());
                var getRole = (from m in db.Users
                               join n in db.Users_Roles on m.userId equals n.userId
                               where m.userId == UserId
                               select new RoleList()
                               {
                                   RoleId = n.roleId,
                                   RoleName = n.Role.name
                               }).ToList();
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

                POModel PODetail = new POModel
                {
                    POId = Int32.Parse(Session["POId"].ToString()),
                    UserId = Int32.Parse(Session["UserId"].ToString()),
                    Type = Session["POType"].ToString(),
                    RoleIdList = getRole
                };
                PODetail.NewPOForm = (from a in db.PurchaseOrders
                                      join b in db.Projects on a.projectId equals b.projectId
                                      join c in db.Vendors on a.vendorId equals c.vendorId
                                      //join d in db.VendorStaffs on a.vendorStaffId equals d.staffId
                                      join e in db.PO_Item on a.POId equals e.POId
                                      join f in db.PopulateItemLists on e.codeId equals f.codeId
                                      join g in db.POStatus on a.StatusId equals g.statusId
                                      join h in db.PurchaseRequisitions on a.PRId equals h.PRId
                                      where a.POId == PODetail.POId && f.custId == CustId
                                      select new NewPOModel()
                                      {
                                          PONo = a.PONo,
                                          ProjectName = b.projectName,
                                          PRNo = h.PRNo,
                                          PreparedDate = a.PreparedDate,
                                          VendorName = c.name,
                                          VendorCode = c.vendorNo,
                                          VendorQuoteNo = c.quotationNo,
                                          //VendorContactName = d.vendorContactName,
                                          //VendorEmail = d.vendorEmail,
                                          //VendorStaffId = d.staffId,
                                          //VendorContactNo = d.vendorContactNo,
                                          StatusId = a.StatusId,
                                          Status = g.status
                                          //Submited = a.Submited.Value
                                      }).FirstOrDefault();
                PODetail.NewPOForm.POItemListObject = (from m in db.PO_Item
                                                       join n in db.PopulateItemLists on m.codeId equals n.codeId
                                                       join p in db.PR_Items on m.itemsId equals p.itemsId
                                                       where m.POId == PODetail.POId && n.custId == CustId
                                                       select new POItemsTable()
                                                       {
                                                           ItemsId = m.itemsId,
                                                           DateRequired = m.dateRequired,
                                                           ItemCode = n.ItemCode,
                                                           Description = m.description,                                                          
                                                           CustPONo = m.custPONo,
                                                           Quantity = m.quantity,
                                                           OutStandingQuantity = p.outStandingQuantity.Value,
                                                           UnitPrice = m.unitPrice,
                                                           TotalPrice = m.totalPrice
                                                           //UOM = n.UoM
                                                       }).ToList();

                ViewBag.PayToVendorList = new SelectList(PayToVendorQuery.AsEnumerable(), "VendorId", "VendorName");
                ViewBag.PaymentTermsCodeList = new SelectList(PaymentTermsCodeQuery.AsEnumerable(), "PaymentTermsId", "PaymentDescription");

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
                DBLogicController.POUpdateDbLogic(POModel);
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
        public JsonResult ComfirmPO()
        {
            //var VendorStaffIdInfo = db.VendorStaffs.Select(m => new NewPRModel()
            //{
            //    VendorStaffId = m.staffId,
            //    VendorEmail = m.vendorEmail,
            //    VendorContactNo = m.vendorContactNo
            //}).Where(m => m.VendorStaffId == VendorStaffId).FirstOrDefault();
            if (Request["PayToVendorId"] != "")
            {
                int PayToVendorId = Int32.Parse(Request["PayToVendorId"]);
            }
            if (Request["PaymentTermsId"] != "")
            {
                int PaymentTermsId = Int32.Parse(Request["PaymentTermsId"]);
            }
            //int CustId = Int32.Parse(Session["CompanyId"].ToString());
            //var PayToVendorQuery = (from m in db.Vendors
            //                        where m.custId == CustId
            //                        select new
            //                        {
            //                            VendorId = m.vendorId,
            //                            VendorName = m.vendorNo + " - " + m.name,
            //                            VendorNo = m.vendorNo,
            //                        }).OrderBy(c => c.VendorNo).ThenBy(c => c.VendorName);
            //var PaymentTermsCodeQuery = (from m in db.PaymentTerms
            //                             where m.custId == CustId
            //                             select new
            //                             {
            //                                 PaymentTermsId = m.paymentTermsId,
            //                                 PaymentDescription = m.paymentCode + " - " + m.paymentDescription,
            //                                 PaymentCode = m.paymentCode,
            //                             }).OrderBy(c => c.PaymentCode).ThenBy(c => c.PaymentDescription);
            //ViewBag.PayToVendorList = new SelectList(PayToVendorQuery.AsEnumerable(), "VendorId", "VendorName");
            //ViewBag.PaymentTermsCodeList = new SelectList(PaymentTermsCodeQuery.AsEnumerable(), "PaymentTermsId", "PaymentDescription");

            return Json("The PO has been confirmed");
        }
    }
}