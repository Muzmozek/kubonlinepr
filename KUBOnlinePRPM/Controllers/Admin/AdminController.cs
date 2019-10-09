using KUBOnlinePRPM.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.IO;

namespace KUBOnlinePRPM.Controllers.Admin
{
    public class AdminController : DBLogicController
    {
        private KUBOnlinePREntities db = new KUBOnlinePREntities();
        // GET: Admin
        public ActionResult Index()
        {
            POModel model = new POModel();
            model.StartDate =  DateTime.Parse("2018-01-01"); model.EndDate = DateTime.Now;
            model.CustId = Int32.Parse(Session["CompanyId"].ToString());
            if (model.CustId == 2)
            {
                model.ChildCustId = Int32.Parse(Session["ChildCompanyId"].ToString());
            }            
            model.POHeaderList = GetPOHeaderTable(model.StartDate, model.EndDate, model.CustId, model.ChildCustId);
            model.POLineList = GetPOLineTable(model.StartDate, model.EndDate, model.CustId, model.ChildCustId);

            List<SelectListItem> SubsidiaryList = new List<SelectListItem>();
            SubsidiaryList.Add(new SelectListItem
            {
                Text = "KUB Agro Holding",
                Value = "3"
            });
            SubsidiaryList.Add(new SelectListItem
            {
                Text = "KUB Malua",
                Value = "7"
            });
            SubsidiaryList.Add(new SelectListItem
            {
                Text = "KUB Sepadu",
                Value = "8"
            });

            ViewBag.SubsidiaryList = new SelectList(SubsidiaryList.AsEnumerable(), "Value", "Text");

            return View(model);
        }

        [HttpPost]
        public JsonResult Index(POModel model)
        {
            if (User.Identity.IsAuthenticated && Session["UserId"] != null)
            {
                try
                {
                    //int custId = Int32.Parse(Session["CompanyId"].ToString()); 
                    if (model.CustId == 0)
                    {
                        model.CustId = Int32.Parse(Session["CompanyId"].ToString());
                    }
                    model.POHeaderList = GetPOHeaderTable(model.StartDate, model.EndDate, model.CustId, model.ChildCustId);
                    model.POLineList = GetPOLineTable(model.StartDate, model.EndDate, model.CustId, model.ChildCustId);
                    model.CustId = Int32.Parse(Session["CompanyId"].ToString());
                    if (model.CustId == 2)
                    {
                        model.ChildCustId = Int32.Parse(Session["ChildCompanyId"].ToString());
                    }
                    //EntityToNavHeaderExcel(model.StartDate, model.EndDate, model.ExtractFileLocation);
                    //model.NotiListObject = (from m in db.NotificationMsgs
                    //                        join n in db.Users on m.fromUserId equals n.userId
                    //                        join o in db.NotiGroups on m.msgId equals o.msgId into p
                    //                        join r in db.PurchaseRequisitions on m.PRId equals r.PRId into s
                    //                        from q in p.DefaultIfEmpty()
                    //                        from t in s.DefaultIfEmpty()
                    //                        where m.msgType == "ExportLog"
                    //                        select new NotiListTable()
                    //                        {
                    //                            MsgId = m.msgId,
                    //                            Message = m.message,
                    //                            //PRId = m.PRId,
                    //                            FromFullName = n.firstName + " " + n.lastName,
                    //                            //POId = m.POId,
                    //                            //FromUserId = m.fromUserId,
                    //                            MsgDate = m.msgDate
                    //                            //PRType = t.PRType,
                    //                            //Done = m.done
                    //                        }).OrderByDescending(m => m.MsgDate).ToList();
                    //EntityToNavLineExcel("D:\\TestNavLineTemplate[" + DateTime.Now.ToString("yyyyMMdd") + "].xls", "Purchase Line", startDate, endDate, "Line");
                    List<SelectListItem> SubsidiaryList = new List<SelectListItem>();
                    SubsidiaryList.Add(new SelectListItem
                    {
                        Text = "KUB Agro Holding",
                        Value = "3"
                    });
                    SubsidiaryList.Add(new SelectListItem
                    {
                        Text = "KUB Malua",
                        Value = "7"
                    });
                    SubsidiaryList.Add(new SelectListItem
                    {
                        Text = "KUB Sepadu",
                        Value = "8"
                    });

                    ViewBag.SubsidiaryList = new SelectList(SubsidiaryList.AsEnumerable(), "Value", "Text");

                    return new JsonResult
                    {
                        Data = new
                        {
                            success = true,
                            exception = false,
                            message = "Search success",
                            view = this.RenderPartialView("Index", model)
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
            } else
            {
                var redirectUrl = new UrlHelper(Request.RequestContext).Action("Index", "Home", new { /* no params */ });
                return Json(new { success = false, url = redirectUrl });
            }
        }

        public ActionResult PONumbering()
        {
            POModel POList = new POModel();
                int UserId = Int32.Parse(Session["UserId"].ToString());
                int CustId = Int32.Parse(Session["CompanyId"].ToString());
                    if (CustId == 2)
                    {
                        int ChildCustId = Int32.Parse(Session["ChildCompanyId"].ToString());
                        POList.POListObject = (from m in db.PurchaseOrders
                                               join n in db.Vendors on m.vendorId equals n.vendorId
                                               join o in db.PurchaseRequisitions on m.PRId equals o.PRId
                                               join p in db.Users on m.PreparedById equals p.userId
                                               join q in db.POStatus on m.StatusId equals q.statusId
                                               join t in db.Projects on m.projectId equals t.projectId
                                               join r in db.Customers on t.custId equals r.custId
                                               where m.CustId == CustId && p.childCompanyId == ChildCustId
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
                    else
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

                return View("POList", POList);
        }

        [HttpPost]
        public JsonResult PONumbering(POModel model)
        {
            if (User.Identity.IsAuthenticated && Session["UserId"] != null)
            {
                try
                {
                    //int custId = Int32.Parse(Session["CompanyId"].ToString()); 
                    if (model.CustId == 0)
                    {
                        model.CustId = Int32.Parse(Session["CompanyId"].ToString());
                    }
                    model.POHeaderList = GetPOHeaderTable(model.StartDate, model.EndDate, model.CustId, model.ChildCustId);
                    model.POLineList = GetPOLineTable(model.StartDate, model.EndDate, model.CustId, model.ChildCustId);
                    model.CustId = Int32.Parse(Session["CompanyId"].ToString());
                    if (model.CustId == 2)
                    {
                        model.ChildCustId = Int32.Parse(Session["ChildCompanyId"].ToString());
                    }
                    List<SelectListItem> SubsidiaryList = new List<SelectListItem>();
                    SubsidiaryList.Add(new SelectListItem
                    {
                        Text = "KUB Agro Holding",
                        Value = "3"
                    });
                    SubsidiaryList.Add(new SelectListItem
                    {
                        Text = "KUB Malua",
                        Value = "7"
                    });
                    SubsidiaryList.Add(new SelectListItem
                    {
                        Text = "KUB Sepadu",
                        Value = "8"
                    });

                    ViewBag.SubsidiaryList = new SelectList(SubsidiaryList.AsEnumerable(), "Value", "Text");

                    return new JsonResult
                    {
                        Data = new
                        {
                            success = true,
                            exception = false,
                            message = "Search success",
                            view = this.RenderPartialView("Index", model)
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
    }
}