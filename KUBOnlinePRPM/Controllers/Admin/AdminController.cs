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

namespace KUBOnlinePRPM.Controllers.Admin
{
    public class AdminController : DBLogicController
    {
        private KUBOnlinePREntities db = new KUBOnlinePREntities();
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(NotiModel model)
        {
            try
            {
                EntityToNavHeaderExcel(model.StartDate, model.EndDate, model.ExtractFileLocation);
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
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return Json(new { success = true });
        }
    }
}