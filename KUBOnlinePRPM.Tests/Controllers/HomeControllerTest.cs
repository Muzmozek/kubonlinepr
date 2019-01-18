using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KUBOnlinePRPM;
using KUBOnlinePRPM.Controllers;
using System.Net.Mail;
using KUBOnlinePRPM.Models;

namespace KUBOnlinePRPM.Tests.Controllers
{
    [TestClass]
    public class HomeControllerTest
    {
        private KUBOnlinePREntities db = new KUBOnlinePREntities();

        private TestContext testContextInstance;

        /// <summary>
        ///  Gets or sets the test context which provides
        ///  information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        [TestMethod]
        public void Index()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void About()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.About() as ViewResult;

            // Assert
            Assert.AreEqual("Your contact page.", result.ViewBag.Message);
        }

        [TestMethod]
        public void Contact()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.Contact() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void testEmail()
        {
            MailMessage mail = new MailMessage
            {
                //From = new MailAddress("info@kubtel.com"),
                From = new MailAddress("pr-admin@kub.com"),
                Subject = "[PR Online]  A new PR to be approved ",
                Body = "test",
                IsBodyHtml = true
            };
            //mail.To.Add(new MailAddress("muzhafar@kubtel.com"));
            mail.To.Add(new MailAddress("syafiq.khairil@kub.com"));

            SmtpClient smtp = new SmtpClient
            {
                //Host = "relay.kub.com",
                //Host = "mail.kub.local",
                Host = "mail.kub.com",
                //Host = "outlook.office365.com",
                //Host = "smtp.gmail.com";
                Port = 587,
                //DeliveryMethod = SmtpDeliveryMethod.Network,
                //Port = 110,
                //Port = 25,
                EnableSsl = false,

                Credentials = new System.Net.NetworkCredential("pr-admin@kub.com", "welcome123$")
                //Credentials = new System.Net.NetworkCredential("info@kubtel.com", "welcome123$")
            };
            //smtp.Credentials = new System.Net.NetworkCredential("ocm@kubtel.com", "welcome123$");
            smtp.Send(mail);
        }

        [TestMethod]
        public void testLinq()
        {
            NewPRModel NewPRForm = new NewPRModel();

            NewPRForm.PRItemListObject = (from m in db.PR_Items.DefaultIfEmpty()
                                          from n in db.PopulateItemLists.Where(x => m.codeId == x.codeId && m.itemTypeId == x.itemTypeId).DefaultIfEmpty()
                                          where m.PRId == 120 && n.custId == 1
                                          select new PRItemsTable()
                                          {
                                              ItemsId = m.itemsId,
                                              DateRequired = m.dateRequired,
                                              ItemTypeId = m.itemTypeId,
                                              CodeId = m.codeId,
                                              ItemCode = n.ItemCode,
                                              Description = m.description,
                                              CustPONo = m.custPONo,
                                              Quantity = m.quantity,
                                              UnitPrice = m.unitPrice,
                                              TotalPrice = m.totalPrice
                                          }).ToList();
        }

        [TestMethod]
        public void tesPRForm()
        {
            PRModel PRDetail = new PRModel();
            PRDetail.NewPRForm = (from a in db.PurchaseRequisitions
                                  join b in db.Users on a.PreparedById equals b.userId
                                  join c in db.PRStatus on a.StatusId equals c.statusId
                                  join d in db.Projects on a.ProjectId equals d.projectId
                                  join e in db.Vendors on a.VendorId equals e.vendorId into f
                                  //join g in db.VendorStaffs on a.VendorStaffId equals g.staffId into l
                                  join h in db.PR_Items on a.PRId equals h.PRId
                                  join j in db.PopulateItemLists on h.codeId equals j.codeId into k
                                  join m in db.PurchaseTypes on a.PurchaseTypeId equals m.purchaseTypeId
                                  join n in db.Customers on b.companyId equals n.custId
                                  from o in db.PR_Reviewer.Where(x => x.PRId == PRDetail.PRId && x.reviewed == 1)
                                  from q in db.PR_Approver.Where(x => x.PRId == PRDetail.PRId && x.approverApproved == 1).DefaultIfEmpty()
                                  from s in db.PR_HOD.Where(x => x.PRId == PRDetail.PRId && x.HODApprovedP1 == 1).DefaultIfEmpty()
                                  join t in db.Users on s.HODId equals t.userId
                                  from u in db.PR_Recommender.Where(x => x.PRId == PRDetail.PRId && x.recommended == 1).DefaultIfEmpty()
                                      //from v in r.DefaultIfEmpty()
                                      //from w in p.DefaultIfEmpty()
                                      //from x in l.DefaultIfEmpty()
                                  from y in f.DefaultIfEmpty()
                                      //from z in k.DefaultIfEmpty()
                                      //from ab in aa.DefaultIfEmpty()
                                  where a.PRId == 144
                                  select new NewPRModel()
                                  {
                                      CompanyName = n.name,
                                      RequestorName = b.firstName + " " + b.lastName,
                                      Designation = b.jobTitle,
                                      //DivDept = b.
                                      PRNo = a.PRNo,
                                      ProjectId = a.ProjectId,
                                      PaperRefNo = a.PaperRefNo,
                                      Budgeted = a.Budgeted,
                                      VendorId = a.VendorId,
                                      VendorName = y.name,
                                      //VendorEmail = x.vendorEmail,
                                      //VendorStaffId = x.staffId,
                                      //VendorContactName = x.vendorContactName,
                                      //VendorContactNo = x.vendorContactNo,
                                      PurchaseTypeId = a.PurchaseTypeId,
                                      PurchaseType = m.purchaseType1,
                                      BudgetDescription = a.BudgetDescription,
                                      BudgetedAmount = d.budgetedAmount,
                                      Justification = a.Justification,
                                      UtilizedToDate = d.utilizedToDate,
                                      AmountRequired = a.AmountRequired,
                                      BudgetBalance = d.budgetBalance,
                                      PreparedById = a.PreparedById,
                                      PreparedDate = a.PreparedDate,
                                      HODApproverId = s.HODId,
                                      HODApproverName = t.firstName + " " + t.lastName,
                                      HODApprovedDate1 = s.HODApprovedDate1.Value,
                                      //HODApprovedDate2 = s.HODApprovedDate2.Value,
                                      ApproverId = q.approverId,
                                      ApprovedDate = q.approverApprovedDate.Value,
                                      ReviewerId = o.reviewerId,
                                      ReviewedDate = o.reviewedDate.Value,
                                      RecommenderId = u.recommenderId,
                                      RecommendDate = u.recommendedDate,
                                      Saved = a.Saved,
                                      Submited = a.Submited,
                                      Rejected = a.Rejected,
                                      Scenario = a.Scenario,
                                      StatusId = a.StatusId.Trim()
                                  }).FirstOrDefault();
        }

        [TestMethod]
        public void testRolesforAllforKUBM()
        {
            var getFinance = (from m in db.Users
                              join n in db.Users_Roles on m.userId equals n.userId
                              where n.roleId == "R13" && m.companyId == 2
                              select new NewPRModel()
                              {
                                  ReviewerId = m.userId,
                                  ReviewerName = m.firstName + " " + m.lastName,
                                  ReviewerEmail = m.emailAddress
                              }).ToList();

            var getLeader = (from m in db.Users
                          join n in db.Users_Roles on m.userId equals n.userId
                          join o in db.ChildCustomers on m.childCompanyId equals o.childCustId
                          where (n.roleId == "R02" || n.roleId == "R10") && m.companyId == 2 && m.childCompanyId == 2
                          select new NewPRModel()
                          {
                              HODApproverId = m.userId,
                              HODApproverName = m.firstName + " " + m.lastName,
                              ApproverEmail = m.emailAddress
                          }).ToList();
            int LeaderUserId = 0; string getLeaderName = "";
            foreach (var item in getLeader)
            {
                if (LeaderUserId != item.HODApproverId)
                {
                    getLeaderName += item.HODApproverName;
                }
                LeaderUserId = item.HODApproverId;
            }

            var getHOD = (from m in db.Users
                          join o in db.Users on m.superiorId equals o.userId
                          join n in db.Users_Roles on o.userId equals n.userId
                          where (n.roleId == "R02" || n.roleId == "R11" || n.roleId == "R14") && m.companyId == 2 && m.userId == 180
                          select new PRModel()
                          {
                              UserId = o.userId,
                              FullName = o.firstName + " " + o.lastName,
                              EmailAddress = o.emailAddress
                          }).ToList();
            int HODUserId = 0; string getHODName = "";
            foreach (var item in getHOD)
            {
                if (HODUserId != item.UserId)
                {
                    getHODName += item.FullName;
                }
                HODUserId = item.UserId;
            }
            
            var getProcList = (from m in db.Users
                               join n in db.Users_Roles on m.userId equals n.userId
                               where n.roleId == "R03" && m.companyId == 2
                               select new NewPRModel()
                               {
                                   AdminId = m.userId,
                                   AdminName = m.firstName + " " + m.lastName
                               }).ToList();
            //var getHODDetails = (from m in db.PR_HOD
            //                     join n in db.Users on m.HODId equals n.userId
            //                     join o in db.ChildCustomers on n.childCompanyId equals o.childCustId
            //                     where m.PRId == 160 && o.childCustId == 2
            //                     select new UserModel()
            //                     {
            //                         UserId = n.userId,
            //                         FullName = n.firstName + " " + n.lastName,
            //                         EmailAddress = n.emailAddress
            //                     }).FirstOrDefault();
        }

        [TestMethod]
        public void testDate()
        {
            int POId = 44;
            var PONo = "PO-" + DateTime.Now.Year + "-" + string.Format("{0}{1}", 0, POId.ToString("D4"));
        }
    }
}