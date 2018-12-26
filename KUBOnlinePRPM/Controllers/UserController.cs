using KUBOnlinePRPM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KUBOnlinePRPM.Controllers
{

    public class UserController : DBLogicController
    {
        private KUBOnlinePREntities db = new KUBOnlinePREntities();

        // GET: User
        //
        public ActionResult Index()
        {
            return View();
        }

        [CheckSessionOut]
        public ActionResult MyProfile()
        {
            var userId = Int32.Parse(Session["UserId"].ToString());
            int CustId = Int32.Parse(Session["CompanyId"].ToString());
            UserModel newUserModel = (from m in db.Users
                              join n in db.Customers on m.companyId equals n.custId
                              where m.userId == userId
                                      select new UserModel()
                              {
                                  FullName = m.firstName + " " + m.lastName,
                                  JobTitle = m.jobTitle,
                                  Department = m.department,
                                  CompanyName = n.name
                              }).FirstOrDefault();

            newUserModel.HOD = (from m in db.Users
                                              join n in db.Users_Roles on m.userId equals n.userId
                                              where n.roleId == "R02" && m.companyId == CustId
                                              select m.firstName + " " + m.lastName).FirstOrDefault();

            newUserModel.HOC = (from m in db.Users
                                              join n in db.Users_Roles on m.userId equals n.userId
                                              where n.roleId == "R04" && m.companyId == CustId
                                              select m.firstName + " " + m.lastName).FirstOrDefault();

            return View(newUserModel);
        }
    }
}