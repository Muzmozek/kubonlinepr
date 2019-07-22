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
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;

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
                                      //AmountRequired = a.AmountRequired,
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
            var getReqName = db.Users.First(m => m.userId == 278).userName;
            var getLeader = (from m in db.Users
                             join n in db.Users_Roles on m.userId equals n.userId
                             join o in db.ChildCustomers on m.childCompanyId equals o.childCustId
                             where (n.roleId == "R02" || n.roleId == "R10") && m.companyId == 3
                             && m.childCompanyId == 17
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
                    getLeaderName += item.HODApproverName + ", ";
                }
                LeaderUserId = item.HODApproverId.Value;
            }

            var getHOD = (from m in db.Users
                          join o in db.Users on m.superiorId equals o.userId
                          join n in db.Users_Roles on o.userId equals n.userId
                          where (n.roleId == "R02" || n.roleId == "R11" || n.roleId == "R14") && m.companyId == 4 && m.userId == 278
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

            var getFinance = (from m in db.Users
                              join n in db.Users_Roles on m.userId equals n.userId
                              where n.roleId == "R13" && m.companyId == 3
                              select new NewPRModel()
                              {
                                  ReviewerId = m.userId,
                                  ReviewerName = m.firstName + " " + m.lastName,
                                  ReviewerEmail = m.emailAddress
                              }).ToList();
            int FinanceUserId = 0; string getFinanceName = "";
            foreach (var item in getFinance)
            {
                if (FinanceUserId != item.ReviewerId)
                {
                    getFinanceName += item.ReviewerName + ", ";
                }
                FinanceUserId = item.ReviewerId.Value;
            }

            var getProcList = (from m in db.Users
                               join n in db.Users_Roles on m.userId equals n.userId
                               where n.roleId == "R03" && m.companyId == 3
                               select new NewPRModel()
                               {
                                   AdminId = m.userId,
                                   AdminName = m.firstName + " " + m.lastName
                               }).ToList();

            int ProcUserId = 0; string getProcName = "";
            foreach (var item in getProcList)
            {
                if (ProcUserId != item.AdminId)
                {
                    getProcName += item.AdminName + ", ";
                }
                ProcUserId = item.AdminId;
            }
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

        [TestMethod]
        public void testPOnumbering()
        {
            string PONumber = "PO-19-00022";
            var getListPONo = Int32.Parse(PONumber.Split('-')[2]);
        }

        [TestMethod]
        public void GetPOHeaderTable()
        {
            SqlConnection conn = null;
            DataTable returnDT = new DataTable("POHeaderTable");
            string info = "";
            try
            {
                SqlDataAdapter SQLDataADP = new SqlDataAdapter();
                SQLDataADP.TableMappings.Add("Table", "POHeaderTable");
                conn = OpenDBConnection();
                conn.InfoMessage += (object obj, SqlInfoMessageEventArgs e) =>
                {
                    info = e.Message.ToString();
                };
                string sql = "";
                sql = "SELECT 'Order'                       AS [Document Type], " +
                               "m.pono                        AS [No.], " +
                               "n.vendorno                    AS [Buy-from Vendor No.], " +
                               "n.vendorno                    AS [Pay-to Vendor No.], " +
                               "''                            AS [Your Reference], " +
                               "m.prepareddate                AS [Order Date], " +
                               "m.submitdate                  AS [Posting Date], " +
                               "''                            AS [Expected Receipt Date], " +
                               "Concat ('Order', ' ', m.pono) AS [Posting Description], " +
                               "o.abbreviation                AS [Location Code], " +
                               "''                            AS [Shortcut Dimension 1 Code], " +
                               "''                            AS [Shortcut Dimension 2 Code], " +
                               "''                            AS [Vendor Posting Group], " +
                               "''                            AS [Currency Code] " +
                        "FROM   purchaseorder m " +
                               "LEFT JOIN vendor n " +
                                      "ON ( m.paytovendorid = n.vendorid ) " +
                               "LEFT JOIN customer o " +
                                      "ON ( m.custid = o.custid ) " +
                        "WHERE m.SubmitDate between @startDate and @endDate and n.vendorNo IS NOT NULL";

                SqlCommand SQLcmd = new SqlCommand(sql, conn)
                {
                    CommandType = CommandType.Text
                };
                SQLDataADP.SelectCommand = SQLcmd;

                SQLcmd.Parameters.Add("@startDate", SqlDbType.DateTime);
                SQLcmd.Parameters["@startDate"].Value = DateTime.Parse("2018-01-01").ToShortDateString();
                SQLcmd.Parameters.Add("@endDate", SqlDbType.DateTime);
                SQLcmd.Parameters["@endDate"].Value = DateTime.Now.ToShortDateString();

                SQLDataADP.Fill(returnDT);
            }
            catch (Exception ex)
            {
                Debug.Print("Report - " + ex.Message);
            }
            finally
            {
                CloseDBConnection(conn);
            }

            var POHeaderList = returnDT.AsEnumerable().Select(
                dataRow => new POHeaderTable
                {
                    DocumentType = dataRow.Field<string>("Document Type"),
                    No = dataRow.Field<string>("No."),
                    BuyFromVendorNo = dataRow.Field<string>("Buy-from Vendor No."),
                    PayToVendorNo = dataRow.Field<string>("Pay-to Vendor No."),
                    YourReference = dataRow.Field<string>("Your Reference"),
                    OrderDate = dataRow.Field<DateTime>("Order Date"),
                    PostingDate = dataRow.Field<DateTime>("Posting Date"),
                    ExpectedReceiptDate = dataRow.Field<DateTime?>("Expected Receipt Date"),
                    PostingDescription = dataRow.Field<string>("Posting Description"),
                    LocationCode = dataRow.Field<string>("Location Code"),
                    ShortcutDimension1Code = dataRow.Field<string>("Shortcut Dimension 1 Code"),
                    ShortcutDimension2Code = dataRow.Field<string>("Shortcut Dimension 2 Code"),
                    VendorPostingGroup = dataRow.Field<string>("Vendor Posting Group"),
                    CurrencyCode = dataRow.Field<string>("Currency Code")
                }).ToList();

            //return POHeaderList;
        }

        [TestMethod]
        public void GetPOLineTable()
        {
            SqlConnection conn = null;
            DataTable returnDT = new DataTable("POLineTable");
            string info = "";
            try
            {
                SqlDataAdapter SQLDataADP = new SqlDataAdapter();
                SQLDataADP.TableMappings.Add("Table", "POLineTable");
                conn = OpenDBConnection();
                conn.InfoMessage += (object obj, SqlInfoMessageEventArgs e) =>
                {
                    info = e.Message.ToString();
                };
                string sql = "";
                sql = "SELECT 'Order'        AS [Document Type], " +
                               "m.pono         AS [Document No.], " +
                               "''             AS [Line No.], " +
                               "n.vendorno     AS [Buy-from Vendor No.], " +
                               "q.type         AS [Type], " +
                               "r.itemcode     AS [No.], " +
                               "o.abbreviation AS [Location Code], " +
                               "r.description  AS [Description], " +
                               "r.uom          AS [Unit of Measure], " +
                               "p.quantity     AS [Quantity], " +
                               "p.[unitprice]  AS [Direct Unit Cost], " +
                               "p.[totalprice] AS [Amount], " +
                               "u.projectCode AS [Dim-Project], " +
                               "v.projectcode  AS [Dim-Department] " +
                        "FROM   purchaseorder m " +
                               "LEFT JOIN vendor n " +
                                      "ON ( m.paytovendorid = n.vendorid ) " +
                               "LEFT JOIN customer o " +
                                      "ON ( m.custid = o.custid ) " +
                               "LEFT JOIN po_item p " +
                                      "ON ( m.poid = p.poid ) " +
                               "LEFT JOIN itemtype q " +
                                      "ON ( p.itemtypeid = q.itemtypeid ) " +
                               "LEFT JOIN populateitemlist r " +
                                      "ON ( p.itemtypeid = r.itemtypeid " +
                                           "AND p.codeid = r.codeid ) " +
                               "LEFT JOIN project u " +
                                      "ON ( m.projectid = u.projectid and u.dimension = 'PROJECT' ) " +
                               "LEFT JOIN project v " +
                                      "ON ( m.projectid = v.projectid and v.dimension = 'DEPARTMENT' ) " +
                        "WHERE m.SubmitDate between @startDate and @endDate and n.vendorNo IS NOT NULL";

                SqlCommand SQLcmd = new SqlCommand(sql, conn)
                {
                    CommandType = CommandType.Text
                };
                SQLDataADP.SelectCommand = SQLcmd;

                SQLcmd.Parameters.Add("@startDate", SqlDbType.DateTime);
                SQLcmd.Parameters["@startDate"].Value = DateTime.Parse("2018-01-01").ToShortDateString();
                SQLcmd.Parameters.Add("@endDate", SqlDbType.DateTime);
                SQLcmd.Parameters["@endDate"].Value = DateTime.Now.ToShortDateString();

                SQLDataADP.Fill(returnDT);

                string DocNo = ""; int LineNo = 0;
                foreach (DataRow row in returnDT.Rows)
                {
                    if (row["Document No."].ToString() != DocNo)
                    {
                        row["Line No."] = 10000;
                        LineNo = 10000;
                    }
                    else
                    {
                        LineNo = LineNo + 10000;
                        row["Line No."] = LineNo;
                    }
                    DocNo = row["Document No."].ToString();
                }
            }
            catch (Exception ex)
            {
                Debug.Print("Report - " + ex.Message);
            }
            finally
            {
                CloseDBConnection(conn);
            }

            var POLineList = returnDT.AsEnumerable().Select(
                dataRow => new POLineTable
                {
                    DocumentType = dataRow.Field<string>("Document Type"),
                    DocumentNo = dataRow.Field<string>("Document No."),
                    LineNo = dataRow.Field<string>("Line No."),
                    BuyFromVendorNo = dataRow.Field<string>("Buy-from Vendor No."),
                    Type = dataRow.Field<string>("Type"),
                    No = dataRow.Field<string>("No."),
                    LocationCode = dataRow.Field<string>("Location Code"),
                    Description = dataRow.Field<string>("Description"),
                    UnitofMeasure = dataRow.Field<string>("Unit of Measure"),
                    Quantity = dataRow.Field<int?>("Quantity"),
                    DirectUnitCost = dataRow.Field<decimal?>("Direct Unit Cost"),
                    Amount = dataRow.Field<decimal?>("Amount"),
                    //DimProject = dataRow.Field<string>("Dim-Project"),
                    //DimDepartment = dataRow.Field<string>("Dim-Department")
                }).ToList();
        }

        protected SqlConnection OpenDBConnection()
        {
            SqlConnection conn = null;
            try
            {

                conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["MainConnectionString"].ConnectionString);
                //conn = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["ADConnectionString"].ConnectionString);
                conn.Open();
                //Debug.Print("Connected")
            }
            catch
            {
                if (conn.State == System.Data.ConnectionState.Open)
                {
                    conn.Close();
                }

                conn = null;
                Debug.Print("Failed");
            }
            return conn;
        }

        protected void CloseDBConnection(SqlConnection conn)
        {
            if ((conn != null))
            {
                conn.Close();
            }
            //Debug.Print("Connection Closed")
        }

        [TestMethod]
        public void TestRecommender()
        {
            //var PR = db.PurchaseRequisitions.First(x => x.PRId == 161);
            int CustId = 3;
            var getReqChildCustId = db.Users.First(m => m.userId == 203);
            var getRecommenderIII = new List<PRModel>();
            if (CustId == 2 || (CustId == 3 && getReqChildCustId.childCompanyId == 16) || CustId == 4)
            /*if (PR.CustId == 2 || (PR.CustId == 3 && getReqChildCustId.childCompanyId == 16) || PR.CustId == 4)*/  //childCompanyId = kubma or customer = kubgaz
            {
                //PR.StatusId = "PR18";
                //db.SaveChanges();

                //PR_RecommenderCOO SaveRecommenderInfo = db.PR_RecommenderCOO.First(m => m.PRId == PR.PRId);
                //SaveRecommenderInfo.recommendedDate = DateTime.Now;
                //SaveRecommenderInfo.recommended = 1;
                //db.SaveChanges();

                //getRecommenderIII = (from m in db.Users
                //                     join n in db.Users_Roles on m.userId equals n.userId
                //                     //join o in db.Roles on n.roleId equals o.roleId
                //                     where n.roleId == "R12" && m.companyId == 2
                //                     select new PRModel()
                //                     {
                //                         UserId = n.userId,
                //                         FullName = m.firstName + " " + m.lastName,
                //                         EmailAddress = m.emailAddress
                //                     }).ToList();
            }
        }
    }
}