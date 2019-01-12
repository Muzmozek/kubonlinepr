using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KUBOnlinePRPM.Models;

namespace KUBOnlinePRPM.Controllers
{
    public class NotificationController : DBLogicController
    {
        private KUBOnlinePREntities db = new KUBOnlinePREntities();
        // GET: Notification
        [CheckSessionOut]
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
                                              where q.toUserId == userId && m.msgType == "Task" && m.done == null
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
                                              }).OrderByDescending(m => m.MsgDate).ToList();

                return PartialView(NewNotiList);
            }
            else
            {
                return Redirect("~/Home/Index");
            }
        }

        [CheckSessionOut]
        public ActionResult AuditTrail()
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
                                              join u in db.Users on q.toUserId equals u.userId into v
                                              from w in v.DefaultIfEmpty()
                                              select new NotiListTable()
                                              {
                                                  MsgId = m.msgId,
                                                  Message = m.message,
                                                  PRId = m.PRId,
                                                  //RequestorName = n.firstName + " " + n.lastName,
                                                  POId = m.POId,
                                                  FromFullName = n.firstName + " " + n.lastName,
                                                  ToFullName = w.firstName + " " + w.lastName,
                                                  MsgDate = m.msgDate,
                                                  PRType = t.PRType,
                                                  Done = m.done
                                              }).OrderByDescending(m => m.MsgDate).ToList();

                return View(NewNotiList);
        }
    }
}