using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KUBOnlinePRPM.Controllers
{
    public class PMController : Controller
    {
        // GET: PM
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult ProjectDetail()
        {
            return View();
        }
        public ActionResult DODetails()
        {
            return View();
        }
        public ActionResult ProjectOverview()
        {
            return PartialView();
        }
        public ActionResult ProjectSchedules()
        {
            return PartialView();
        }
        public ActionResult ProjectFinance()
        {
            return PartialView();
        }
        public ActionResult ProjectDO()
        {
            return PartialView();
        }
        public ActionResult ProjectIssues()
        {
            return PartialView();
        }
        public ActionResult ProjectReport()
        {
            return PartialView();
        }
        public ActionResult ProjectFiles()
        {
            return PartialView();
        }
        public ActionResult ProjectConversation()
        {
            return PartialView();
        }
    }
}