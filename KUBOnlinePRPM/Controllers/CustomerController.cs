using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KUBOnlinePRPM.Controllers
{
    public class CustomerController : DBLogicController
    {
        // GET: Customer
        public ActionResult Index()
        {
            return View();
        }
    }
}