using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using KUBOnlinePRPM.Models;
using System.IO;
using System.Web.Security;
using System.DirectoryServices;
using KUBOnlinePRPM.Service;

namespace KUBOnlinePRPM.Controllers
{
    public static class MvcHelpers
    {
        public static string RenderPartialView(this Controller controller, string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = controller.ControllerContext.RouteData.GetRequiredString("action");

            controller.ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName);
                var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(controller.ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }
    }
    public class HomeController : DBLogicController
    {
        private KUBOnlinePREntities db = new KUBOnlinePREntities();
        private KUBHelper KUBHelper = new KUBHelper();
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(LoginModel model)
        {
            //string hashed = BCrypt.HashPassword(model.Password, BCrypt.GenerateSalt(12));
            //bool matches = BCrypt.CheckPassword(model.Password, hashed);
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var CheckUserPassword = (from m in db.Users
                                     where m.userName == model.Username
                                     select new UserModel()
                                     {
                                         Password = m.password
                                     }).FirstOrDefault();
            bool validate = false;
            if (CheckUserPassword != null)
            {
                validate = BCrypt.CheckPassword(model.Password, CheckUserPassword.Password);
            } 
            
            //var directoryEntry = new DirectoryEntry("LDAP://172.16.0.1/DC=kub,DC=local");
            //directoryEntry.Username = "win2k8";
            //directoryEntry.Password = "Quantum111?";
            if (validate == true)
                //if (Membership.ValidateUser(model.Username, model.Password) || validate == true)
            {
                var CheckUserId = (from m in db.Users
                               join n in db.Users_Roles on m.userId equals n.userId
                               join o in db.Roles on n.roleId equals o.roleId
                               where m.userName == model.Username
                               select new UserModel()
                               {
                                   UserId = m.userId,
                                   CompanyId = m.companyId,
                                   JobTitle = m.jobTitle,
                                   FullName = m.firstName + " " + m.lastName
                               }).FirstOrDefault();

                var getRole = db.Users_Roles.Select(x => new { x.userId, x.roleId }).Where(x => x.userId == CheckUserId.UserId).ToList();

                Session["UserId"] = CheckUserId.UserId;
                Session["Username"] = model.Username;
                Session["ifRequestor"] = getRole.FirstOrDefault(x => x.roleId.Contains("R01"));
                Session["ifHOD"] = getRole.FirstOrDefault(x => x.roleId.Contains("R02"));
                Session["ifProcurement"] = getRole.FirstOrDefault(x => x.roleId.Contains("R03"));
                Session["ifHOC"] = getRole.FirstOrDefault(x => x.roleId.Contains("R04"));
                Session["ifAdmin"] = getRole.FirstOrDefault(x => x.roleId.Contains("R05"));
                Session["ifSuperAdmin"] = getRole.FirstOrDefault(x => x.roleId.Contains("R06"));
                Session["ifIT"] = getRole.FirstOrDefault(x => x.roleId.Contains("R07"));
                Session["ifPMO"] = getRole.FirstOrDefault(x => x.roleId.Contains("R08"));
                Session["ifHSE"] = getRole.FirstOrDefault(x => x.roleId.Contains("R09"));
                Session["ifHOGPSS"] = getRole.FirstOrDefault(x => x.roleId.Contains("R10"));
                Session["roles"] = db.Users_Roles.Where(x => x.userId == CheckUserId.UserId).ToList();
                Session["FullName"] = CheckUserId.FullName;
                Session["CompanyId"] = CheckUserId.CompanyId;
                Session["JobTitle"] = CheckUserId.JobTitle;
                FormsAuthentication.SetAuthCookie(model.Username, true);

                User saveLoginNoti = db.Users.First(m => m.userId == CheckUserId.UserId);
                saveLoginNoti.lastLoginDate = DateTime.Now;                
                NotificationMsg userAuditTrail = new NotificationMsg()
                {
                    uuid = Guid.NewGuid(),
                    message = CheckUserId.FullName + " has login to the Online PR system",
                    msgDate = DateTime.Now,
                    fromUserId = CheckUserId.UserId,
                    msgType = "Trail"
                };
                db.NotificationMsgs.Add(userAuditTrail);
                db.SaveChanges();
                var redirectUrl = new UrlHelper(Request.RequestContext).Action("Index", "PR");

                return Json(new { success = true, url = redirectUrl });
            }
            else
            {
                User saveLoginNoti = db.Users.FirstOrDefault(m => m.userName == model.Username.Trim());
                if (saveLoginNoti != null)
                {
                    saveLoginNoti.lastFailedLoginDate = DateTime.Now;
                }
                NotificationMsg userAuditTrail = new NotificationMsg()
                {
                    uuid = Guid.NewGuid(),
                    message = "Username " + model.Username + " failed to login to the Online PR system on " + DateTime.Now,
                    msgDate = DateTime.Now
                };
                db.NotificationMsgs.Add(userAuditTrail);
                db.SaveChanges();
                ModelState.AddModelError("UserName", "Wrong username or password. Please check again.");
                return new JsonResult
                {
                    Data = new
                    {
                        success = false,
                        view = this.RenderPartialView("Index", model)
                    },
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult SignOut()
        {
            Session.Clear();
            FormsAuthentication.SignOut();
            return Redirect("Index");
        }
    }
}