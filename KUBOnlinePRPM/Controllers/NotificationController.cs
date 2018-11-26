using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KUBOnlinePRPM.Models;

namespace KUBOnlinePRPM.Controllers
{
    public class NotificationController : Controller
    {
        private KUBOnlinePREntities db = new KUBOnlinePREntities();
        // GET: Notification
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated && Session["UserId"] != null)
            {
                NotiModel NewNotiList = new NotiModel();
                int userId = Int32.Parse(Session["UserId"].ToString());
                //var haha = db.NotificationMsgs.ToList();
                NewNotiList.NotiListObject = (from m in db.NotificationMsgs
                                              join n in db.Users on m.fromUserId equals n.userId
                                              join o in db.NotiGroups on m.msgId equals o.msgId into p
                                              join r in db.PurchaseRequisitions on m.PRId equals r.PRId into s
                                              from q in p.DefaultIfEmpty()
                                              from t in s.DefaultIfEmpty()
                                              where q.toUserId == userId
                                              select new NotiListTable()
                                              {
                                                  MsgId = m.msgId,
                                                  Message = m.message,
                                                  PRId = m.PRId,
                                                  //RequestorName = n.firstName + " " + n.lastName,
                                                  POId = m.POId,
                                                  FromUserId = m.fromUserId,
                                                  MsgDate = m.msgDate,
                                                  PRType = t.PRType,
                                                  Done = m.done
                                              }).ToList();

                return PartialView(NewNotiList);
            }
            else
            {
                return Redirect("~/Home/Index");
            }
        }

        public ActionResult AuditTrail()
        {
            if (User.Identity.IsAuthenticated && Session["UserId"] != null)
            {
                NotiModel NewNotiList = new NotiModel();
                int userId = Int32.Parse(Session["UserId"].ToString());
                //var haha = db.NotificationMsgs.ToList();
                NewNotiList.NotiListObject = (from m in db.NotificationMsgs
                                              join n in db.Users on m.fromUserId equals n.userId
                                              join o in db.NotiGroups on m.msgId equals o.msgId into p
                                              join r in db.PurchaseRequisitions on m.PRId equals r.PRId into s
                                              from q in p.DefaultIfEmpty()
                                              from t in s.DefaultIfEmpty()
                                              select new NotiListTable()
                                              {
                                                  MsgId = m.msgId,
                                                  Message = m.message,
                                                  PRId = m.PRId,
                                                  //RequestorName = n.firstName + " " + n.lastName,
                                                  POId = m.POId,
                                                  FromUserId = m.fromUserId,
                                                  MsgDate = m.msgDate,
                                                  PRType = t.PRType,
                                                  Done = m.done
                                              }).ToList();

                return View(NewNotiList);
            }
            else
            {
                return Redirect("~/Home/Index");
            }
        }
    }
}