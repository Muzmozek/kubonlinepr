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
            int custId = Int32.Parse(Session["CompanyId"].ToString());
            model.POHeaderList = GetPOHeaderTable(model.StartDate, model.EndDate, custId);
            model.POLineList = GetPOLineTable(model.StartDate, model.EndDate, custId);

            return View(model);
        }

        [HttpPost]
        public JsonResult Index(POModel model)
        {
            if (User.Identity.IsAuthenticated && Session["UserId"] != null)
            {
                try
                {
                    int custId = Int32.Parse(Session["CompanyId"].ToString());
                    model.POHeaderList = GetPOHeaderTable(model.StartDate, model.EndDate, custId);
                    model.POLineList = GetPOLineTable(model.StartDate, model.EndDate, custId);
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
    }
}