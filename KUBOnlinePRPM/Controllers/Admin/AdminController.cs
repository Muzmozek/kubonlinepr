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

            POList.POListTable = (from m in db.PurchaseOrders
                                   join n in db.Projects on m.projectId equals n.projectId
                                   join o in db.Vendors on m.vendorId equals o.vendorId
                                   where m.CustId == CustId
                                   select new POListTable()
                                   {
                                       POId = m.POId,
                                       PODate = m.PODate,
                                       CustId = n.custId,
                                       PONo = m.PONo,
                                       ProjectName = n.projectName,
                                       VendorCompany = o.name
                                   }).OrderByDescending(m => m.PODate).FirstOrDefault();

            var POEditHistory = db.PONoEditHistories.Where(m => m.custId == CustId).OrderByDescending(m => m.modifiedDate).FirstOrDefault();

            if (POEditHistory != null)
            {
                POList.POListTable.LastPONo = POEditHistory.newPONo;
                POList.POListTable.LastPODate = POEditHistory.modifiedDate;
            }

            return View(POList);
        }

        [HttpPost]
        public JsonResult PONumbering(POModel POList)
        {
            if (User.Identity.IsAuthenticated && Session["UserId"] != null)
            {
                try
                {
                    string NewPONo = POList.NewPOForm.PONo; int CustId = Int32.Parse(Session["CompanyId"].ToString());
                    int UserId = Int32.Parse(Session["UserId"].ToString()); int POId = POList.POListTable.POId;
                    PurchaseOrder editPO = db.PurchaseOrders.First(m => m.POId == POId);

                    PONoEditHistory editPONO = new PONoEditHistory
                    {
                        uuid = Guid.NewGuid(),
                        POId = editPO.POId,
                        custId = CustId,
                        lastPODate = editPO.PODate,
                        lastPONo = editPO.PONo,
                        newPONo = NewPONo,
                        modifiedDate = DateTime.Now,
                        modifiedByUserId = UserId
                    };
                    db.PONoEditHistories.Add(editPONO);
                    db.SaveChanges();

                    return new JsonResult
                    {
                        Data = new
                        {
                            success = true,
                            exception = false
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