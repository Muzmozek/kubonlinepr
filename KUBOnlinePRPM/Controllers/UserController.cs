using KUBOnlinePRPM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KUBOnlinePRPM.Controllers
{

    public class UserController : Controller
    {
        private KUBOnlinePREntities db = new KUBOnlinePREntities();

        // GET: User
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult MyProfile()
        {
            var userId = Int32.Parse(Session["UserId"].ToString());
            var userDetail = db.Users
                .Where(x => x.userId == userId)
                .First();

            return View(userDetail);
        }
    }
}