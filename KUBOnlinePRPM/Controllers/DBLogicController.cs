using KUBOnlinePRPM.Models;
using RazorEngine;
using RazorEngine.Templating;
using System.Net.Mail;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity.Validation;

using System.Reflection;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Objects;
using System.Data.Common;
using System.IO;
using System.Configuration;

namespace KUBOnlinePRPM.Controllers
{
    public abstract class DBLogicController : NotiLogicController
    {
        private static KUBOnlinePREntities db = new KUBOnlinePREntities();

        protected void PRSaveDbLogic(PRModel model)
        {
            PurchaseRequisition _objNewPR = new PurchaseRequisition
            {
                uuid = Guid.NewGuid(),
                //PRId = x.PRDetailObject.PRId,
                PRNo = "dummyset",
                ProjectId = model.NewPRForm.ProjectId,
                CustId = model.CustId,
                PaperRefNo = model.NewPRForm.PaperRefNo,
                BidWaiverRefNo = model.NewPRForm.BidWaiverRefNo,
                Budgeted = model.NewPRForm.Value,
                PurchaseTypeId = model.NewPRForm.PurchaseTypeId,
                BudgetDescription = model.NewPRForm.BudgetDescription,
                //AmountRequired = model.NewPRForm.AmountRequired,
                Justification = model.NewPRForm.Justification,
                //TotalGST = x.PRDetailObject.TotalGST,
                PreparedById = model.NewPRForm.PreparedById,
                PreparedDate = DateTime.Now,
                Saved = model.NewPRForm.Saved + 1,
                Submited = model.NewPRForm.Submited,
                Rejected = model.NewPRForm.Rejected,
                PRType = model.Type,
                Scenario = model.NewPRForm.Scenario,
                budgetedAmount = model.NewPRForm.BudgetedAmount,
                utilizedToDate = model.NewPRForm.UtilizedToDate,
                budgetBalance = model.NewPRForm.BudgetBalance,
                StatusId = "PR01"
            };
            db.PurchaseRequisitions.Add(_objNewPR);
            db.SaveChanges();

            //Project _updateProject = db.Projects.First(m => m.projectId == model.NewPRForm.ProjectId);
            //_updateProject.budgetedAmount = model.NewPRForm.BudgetedAmount;
            //_updateProject.utilizedToDate = model.NewPRForm.UtilizedToDate;
            //_updateProject.budgetBalance = model.NewPRForm.BudgetBalance;
            //db.SaveChanges();

            PurchaseRequisition generatePRNo = db.PurchaseRequisitions.First(m => m.PRId == _objNewPR.PRId);
            generatePRNo.PRNo = "PR-" + DateTime.Now.Year + "-" + string.Format("{0}{1}", 0, _objNewPR.PRId.ToString("D4"));
            NotificationMsg generateMsg = new NotificationMsg();

            model.PRId = _objNewPR.PRId;
            _objNewPR.AmountPOBalance = 0;

            int InitCodeId = 0;
            foreach (var value in model.NewPRForm.PRItemListObject)
            {
                PR_Items _objNewPRItem = new PR_Items
                {
                    uuid = Guid.NewGuid(),
                    PRId = _objNewPR.PRId,
                    //itemsId = value.ItemsId,
                    dateRequired = value.DateRequired,
                    description = value.Description,
                    itemTypeId = value.ItemTypeId,
                    codeId = value.CodeId,
                    custPONo = value.CustPONo,
                    quantity = value.Quantity,
                    outStandingQuantity = value.Quantity,
                    UoMId = value.UoMId
                };
                _objNewPR.AmountPOBalance = _objNewPR.AmountPOBalance + _objNewPRItem.outStandingQuantity;
                db.PR_Items.Add(_objNewPRItem);

                if (value.CodeId != null && InitCodeId != value.CodeId.Value)
                {
                    Budget createBudget = new Budget();
                    createBudget.uuid = Guid.NewGuid();
                    createBudget.PRId = _objNewPR.PRId;
                    createBudget.codeId = value.CodeId.Value;
                    createBudget.budgetAmount = _objNewPR.budgetedAmount;
                    createBudget.initialUtilized = 0;
                    createBudget.projectId = _objNewPR.ProjectId;
                    createBudget.year = _objNewPR.PreparedDate.Year;
                    db.Budgets.Add(createBudget);
                    db.SaveChanges();

                    if (ConfigurationManager.AppSettings["TestUser"] == "true")
                    {
                        var AmountInProgress = db.Budgets.Select(m => new { CodeId = m.codeId, initialUtilized = m.initialUtilized, utilized = m.utilized }).Where(m => m.CodeId == value.CodeId.Value && m.utilized == false).GroupBy(m => m.CodeId).Select(n => new { total = n.Sum(o => o.initialUtilized) }).SingleOrDefault();
                        var updateProgress = db.Budgets.Where(m => m.codeId == value.CodeId.Value).ToList();

                        if (AmountInProgress != null)
                        {
                            updateProgress.ForEach(m => m.progress = AmountInProgress.total);
                        }
                        db.SaveChanges();
                    }                   
                    InitCodeId = createBudget.codeId;
                }

                generateMsg.uuid = Guid.NewGuid();
                generateMsg.message = model.FullName + " has save new item for PR No. " + _objNewPR.PRNo;
                generateMsg.PRId = _objNewPR.PRId;
                generateMsg.msgDate = DateTime.Now;
                generateMsg.fromUserId = model.NewPRForm.PreparedById;
                generateMsg.msgType = "Trail";
                db.NotificationMsgs.Add(generateMsg);
                db.SaveChanges();
            }

            //if ((model.PaperRefNoFile != null && model.NewPRForm.PaperRefNo != null) || (model.BidWaiverRefNoFile != null && model.NewPRForm.BidWaiverRefNo != null))
            //{
            //    generatePRNo.PaperAttachment = true;
            //    generatePRNo.BidWaiverRefNo = model.NewPRForm.BidWaiverRefNo;
            //    generatePRNo.PaperRefNo = model.NewPRForm.PaperRefNo;
            //}
            if (model.PaperRefNoFile != null && model.NewPRForm.PaperRefNo != null)
            {
                generatePRNo.PaperAttachment = true;
                generatePRNo.PaperRefNo = model.NewPRForm.PaperRefNo;
            }
            if (model.BidWaiverRefNoFile != null && model.NewPRForm.BidWaiverRefNo != null)
            {
                generatePRNo.BidWaiverRefNo = model.NewPRForm.BidWaiverRefNo;
            }
            db.SaveChanges();

            if (model.NewPRForm.SelectSave == true)
            {
                generateMsg.uuid = Guid.NewGuid();
                generateMsg.message = model.FullName + " has save new PR No. " + _objNewPR.PRNo + " for " + _objNewPR.Saved + " time. ";
                generateMsg.PRId = _objNewPR.PRId;
                generateMsg.msgDate = DateTime.Now;
                generateMsg.fromUserId = model.NewPRForm.PreparedById;
                generateMsg.msgType = "Trail";
                db.NotificationMsgs.Add(generateMsg);
            }
            if (model.NewPRForm.SelectSubmit == true)
            {
                generatePRNo.Submited = generatePRNo.Submited + 1;
                generatePRNo.SubmitDate = DateTime.Now;
                generatePRNo.PRAging = model.NewPRForm.PRAging;
                generatePRNo.StatusId = "PR09";
                generateMsg.uuid = Guid.NewGuid();
                generateMsg.message = model.FullName + " has submit new PR  No. " + _objNewPR.PRNo;
                generateMsg.PRId = _objNewPR.PRId;
                generateMsg.msgDate = DateTime.Now;
                generateMsg.fromUserId = model.NewPRForm.PreparedById;
                generateMsg.msgType = "Trail";
                db.NotificationMsgs.Add(generateMsg);

                var getPRPreparerChildCustId = db.Users.First(m => m.userId == model.NewPRForm.PreparedById);
                var getProcurement = new List<PRModel>();
                if (model.CustId == 3 && getPRPreparerChildCustId.childCompanyId == 16)
                {
                    getProcurement = (from m in db.Users
                                      join n in db.Users_Roles on m.userId equals n.userId
                                      where n.roleId == "R03" && (m.companyId == model.CustId || m.companyId == 2) && m.childCompanyId == getPRPreparerChildCustId.childCompanyId
                                      select new PRModel()
                                      {
                                          UserId = m.userId,
                                          FullName = m.firstName + " " + m.lastName,
                                          EmailAddress = m.emailAddress
                                      }).ToList();
                } 
                else
                {
                    getProcurement = (from m in db.Users
                                      join n in db.Users_Roles on m.userId equals n.userId
                                      where n.roleId == "R03" && (m.companyId == model.CustId && (m.childCompanyId != 16 || m.childCompanyId == null))
                                      select new PRModel()
                                      {
                                          UserId = m.userId,
                                          FullName = m.firstName + " " + m.lastName,
                                          EmailAddress = m.emailAddress
                                      }).ToList();
                }

                foreach (var item in getProcurement)
                {
                    PR_Admin _objSaveAdmin = new PR_Admin
                    {
                        uuid = Guid.NewGuid(),
                        adminId = item.UserId,
                        PRId = _objNewPR.PRId
                    };
                    db.PR_Admin.Add(_objSaveAdmin);
                    db.SaveChanges();
                }

                var getFinance = (from m in db.Users
                                  join n in db.Users_Roles on m.userId equals n.userId
                                  where n.roleId == "R13" && m.companyId == model.CustId
                                  select new NewPRModel()
                                  {
                                      ReviewerId = m.userId,
                                      ReviewerName = m.firstName + " " + m.lastName,
                                      ReviewerEmail = m.emailAddress
                                  }).ToList();

                foreach (var item in getFinance)
                {
                    PR_Finance _objReviewer = new PR_Finance
                    {
                        uuid = Guid.NewGuid(),
                        reviewerId = item.ReviewerId.Value,
                        PRId = _objNewPR.PRId,
                        reviewed = model.NewPRForm.Reviewed
                    };
                    db.PR_Finance.Add(_objReviewer);
                    db.SaveChanges();
                    //NotificationMsg objTask = new NotificationMsg()
                    //{
                    //    uuid = Guid.NewGuid(),
                    //    message = _objNewPR.PRNo + " pending for your initial reviewal",
                    //    fromUserId = model.UserId,
                    //    msgDate = DateTime.Now,
                    //    msgType = "Task",
                    //    PRId = model.PRId
                    //};
                    //db.NotificationMsgs.Add(objTask);
                    //db.SaveChanges();

                    //NotiGroup ReviewerTask = new NotiGroup()
                    //{
                    //    uuid = Guid.NewGuid(),
                    //    msgId = objTask.msgId,
                    //    toUserId = item.ReviewerId.Value,
                    //    resubmit = false
                    //};
                    //db.NotiGroups.Add(ReviewerTask);
                    //db.SaveChanges();

                    //model.EmailAddress = item.ReviewerEmail;
                    //model.NewPRForm.PRNo = generatePRNo.PRNo;
                    //model.NewPRForm.ApproverId = item.ReviewerId.Value;
                    //model.NewPRForm.ApproverName = item.ReviewerName;
                    //SendEmailPRNotification(model, "ReviewalInitial");
                }

                var getHOD = new List<NewPRModel>();
                if (model.CustId == 2 || (model.CustId == 3 && getPRPreparerChildCustId.childCompanyId == 16))
                {
                    getHOD = (from m in db.Users
                              join n in db.Users_Roles on m.userId equals n.userId
                              join o in db.ChildCustomers on m.childCompanyId equals o.childCustId
                              where (n.roleId == "R02" || n.roleId == "R10") && m.companyId == model.CustId && m.childCompanyId == getPRPreparerChildCustId.childCompanyId
                              select new NewPRModel()
                              {
                                  HODApproverId = m.userId,
                                  HODApproverName = m.firstName + " " + m.lastName,
                                  ApproverEmail = m.emailAddress
                              }).ToList();
                }
                //else if (model.CustId == 3 && getPRPreparerChildCustId.childCompanyId == 16)
                //{
                //    getHOD = (from m in db.Users
                //              join n in db.Users_Roles on m.userId equals n.userId
                //              where n.roleId == "R08" && m.companyId == 2
                //              //&& m.userId == 172 //en. Shahril
                //              select new NewPRModel()
                //              {
                //                  HODApproverId = m.userId,
                //                  HODApproverName = m.firstName + " " + m.lastName,
                //                  ApproverEmail = m.emailAddress
                //              }).ToList();
                //}
                else
                {
                    getHOD = (from m in db.Users
                              join o in db.Users on m.superiorId equals o.userId
                              join n in db.Users_Roles on o.userId equals n.userId
                              where n.roleId == "R02" && m.companyId == model.CustId && m.userId == model.NewPRForm.PreparedById
                              select new NewPRModel()
                              {
                                  HODApproverId = o.userId,
                                  HODApproverName = o.firstName + " " + o.lastName,
                                  ApproverEmail = o.emailAddress
                              }).ToList();
                }

                int HODApproverId = 0;
                foreach (var item in getHOD)
                {
                    if (HODApproverId != item.HODApproverId)
                    {
                        PR_HOD _objHOD = new PR_HOD
                        {
                            uuid = Guid.NewGuid(),
                            HODId = item.HODApproverId,
                            PRId = _objNewPR.PRId,
                            HODApprovedP1 = model.NewPRForm.HODApproverApprovedP1
                        };
                        db.PR_HOD.Add(_objHOD);
                        db.SaveChanges();

                        NotificationMsg objTask = new NotificationMsg()
                        {
                            uuid = Guid.NewGuid(),
                            message = generatePRNo.PRNo + " pending for your initial approval",
                            fromUserId = model.UserId,
                            msgDate = DateTime.Now,
                            msgType = "Task",
                            PRId = model.PRId
                        };
                        db.NotificationMsgs.Add(objTask);
                        db.SaveChanges();

                        NotiGroup ApproverTask = new NotiGroup()
                        {
                            uuid = Guid.NewGuid(),
                            msgId = objTask.msgId,
                            toUserId = item.HODApproverId,
                            resubmit = false
                        };
                        db.NotiGroups.Add(ApproverTask);
                        db.SaveChanges();

                        model.NewPRForm.PRNo = generatePRNo.PRNo;
                        model.NewPRForm.ApproverId = item.HODApproverId;
                        model.NewPRForm.ApproverName = item.HODApproverName;
                        model.EmailAddress = item.ApproverEmail;
                        SendEmailPRNotification(model, "ApprovalInitial");
                    }
                    HODApproverId = item.HODApproverId;
                }
                //if (model.NewPRForm.StatusId == "PR07")
                //{
                //    var getMsgId = (from m in db.NotificationMsgs
                //                    join n in db.NotiGroups on m.msgId equals n.msgId
                //                    where m.PRId == model.PRId && m.msgType == "Task" && n.resubmit != null
                //                    select new
                //                    {
                //                        MsgId = m.msgId,
                //                        MsgDate = m.msgDate
                //                    }).OrderByDescending(m => m.MsgDate).First();
                //    NotiGroup UpdateReqTask = db.NotiGroups.First(m => m.msgId == getMsgId.MsgId);
                //    UpdateReqTask.resubmit = true;
                //    _objNewPR.StatusId = "PR09";
                //    db.SaveChanges();
                //}
                //else
                //{
            }
        }

        protected void PRUpdateDbLogic(PRModel x)
        {
            PurchaseRequisition FormerPRDetails = db.PurchaseRequisitions.First(m => m.PRId == x.PRId);
            Project updateProject = db.Projects.First(m => m.projectId == x.NewPRForm.ProjectId);

            if (FormerPRDetails.ProjectId != x.NewPRForm.ProjectId)
            {
                Project FormerProjectDetail = db.Projects.First(m => m.projectId == FormerPRDetails.ProjectId);
                Project LatestProjectDetail = db.Projects.First(m => m.projectId == x.NewPRForm.ProjectId);

                NotificationMsg _objDetails_ProjectId = new NotificationMsg
                {
                    uuid = Guid.NewGuid(),
                    PRId = x.PRId,
                    msgDate = DateTime.Now,
                    fromUserId = x.UserId,
                    msgType = "Trail",
                    message = x.FullName + " change ProjectName from " + FormerProjectDetail.projectName + " to " + LatestProjectDetail.projectName + " for PR No: " + FormerPRDetails.PRNo
                };
                db.NotificationMsgs.Add(_objDetails_ProjectId);
                FormerPRDetails.ProjectId = x.NewPRForm.ProjectId;
            }
            if (FormerPRDetails.Budgeted != x.NewPRForm.Value)
            {
                NotificationMsg _objDetails_ProjectId = new NotificationMsg
                {
                    uuid = Guid.NewGuid(),
                    PRId = x.PRId,
                    msgDate = DateTime.Now,
                    fromUserId = x.UserId,
                    msgType = "Trail",
                    message = x.FullName + " change Budgeted value from " + FormerPRDetails.Budgeted + " to " + x.NewPRForm.Value + " for PR No: " + FormerPRDetails.PRNo
                };
                db.NotificationMsgs.Add(_objDetails_ProjectId);
                FormerPRDetails.Budgeted = x.NewPRForm.Value;
            }
            if (FormerPRDetails.PurchaseTypeId != x.NewPRForm.PurchaseTypeId)
            {
                NotificationMsg _objDetails_PurchaseTypeId = new NotificationMsg
                {
                    uuid = Guid.NewGuid(),
                    PRId = x.PRId,
                    msgDate = DateTime.Now,
                    fromUserId = x.UserId,
                    msgType = "Trail",
                    message = x.FullName + " change Purchase type from " + FormerPRDetails.PurchaseType + " to " + x.NewPRForm.PurchaseType + " for PR No: " + FormerPRDetails.PRNo
                };
                db.NotificationMsgs.Add(_objDetails_PurchaseTypeId);
                FormerPRDetails.PurchaseTypeId = x.NewPRForm.PurchaseTypeId;
            }
            if (FormerPRDetails.BudgetDescription != x.NewPRForm.BudgetDescription)
            {
                NotificationMsg _objDetails_BudgetDescription = new NotificationMsg
                {
                    uuid = Guid.NewGuid(),
                    PRId = x.PRId,
                    msgDate = DateTime.Now,
                    fromUserId = x.UserId,
                    msgType = "Trail",
                    message = x.FullName + " change Budget Description from " + FormerPRDetails.BudgetDescription + " to " + x.NewPRForm.BudgetDescription + " for PR No: " + FormerPRDetails.PRNo
                };
                db.NotificationMsgs.Add(_objDetails_BudgetDescription);
                FormerPRDetails.BudgetDescription = x.NewPRForm.BudgetDescription;
            }
            if (FormerPRDetails.budgetedAmount != x.NewPRForm.BudgetedAmount)
            {
                NotificationMsg _objDetails_BudgetedAmount = new NotificationMsg
                {
                    uuid = Guid.NewGuid(),
                    PRId = x.PRId,
                    msgDate = DateTime.Now,
                    fromUserId = x.UserId,
                    msgType = "Trail",
                    message = x.FullName + " change Budget Amount from " + FormerPRDetails.budgetedAmount + " to " + x.NewPRForm.BudgetedAmount + " for project: " + updateProject.projectName
                };
                db.NotificationMsgs.Add(_objDetails_BudgetedAmount);
                FormerPRDetails.budgetedAmount = x.NewPRForm.BudgetedAmount;
            }
            //if (FormerPRDetails.AmountRequired != x.NewPRForm.AmountRequired)
            //{
            //    NotificationMsg _objDetails_AmountRequired = new NotificationMsg
            //    {
            //        uuid = Guid.NewGuid(),
            //        PRId = x.PRId,
            //        msgDate = DateTime.Now,
            //        fromUserId = x.UserId,
            //        msgType = "Trail",
            //        message = x.FullName + " change Amount Required from " + FormerPRDetails.AmountRequired + " to " + x.NewPRForm.AmountRequired + " for PR No: " + FormerPRDetails.PRNo
            //    };
            //    db.NotificationMsgs.Add(_objDetails_AmountRequired);
            //    FormerPRDetails.AmountRequired = x.NewPRForm.AmountRequired;
            //}
            if (ConfigurationManager.AppSettings["TestUser"] == "true")
            {
                Budget FormerBudget = db.Budgets.FirstOrDefault(m => m.PRId == x.PRId);
                if (FormerBudget != null && ConfigurationManager.AppSettings["TestUser"] == "true")
                {
                    FormerBudget.progress = x.NewPRForm.AmountInProgress.Value;
                }
            }                       
            if (FormerPRDetails.Justification != x.NewPRForm.Justification)
            {
                NotificationMsg _objDetails_Justification = new NotificationMsg
                {
                    uuid = Guid.NewGuid(),
                    PRId = x.PRId,
                    msgDate = DateTime.Now,
                    fromUserId = x.UserId,
                    msgType = "Trail",
                    message = x.FullName + " change Remark from " + FormerPRDetails.Justification + " to " + x.NewPRForm.Justification + " for PR No: " + FormerPRDetails.PRNo
                };
                db.NotificationMsgs.Add(_objDetails_Justification);
                FormerPRDetails.Justification = x.NewPRForm.Justification;
            }
            if (FormerPRDetails.utilizedToDate != x.NewPRForm.UtilizedToDate)
            {
                NotificationMsg _objDetails_UtilizedToDate = new NotificationMsg
                {
                    uuid = Guid.NewGuid(),
                    PRId = x.PRId,
                    msgDate = DateTime.Now,
                    fromUserId = x.UserId,
                    msgType = "Trail",
                    message = x.FullName + " change Utilized To Date from " + FormerPRDetails.utilizedToDate + " to " + x.NewPRForm.UtilizedToDate + " for project: " + updateProject.projectName
                };
                db.NotificationMsgs.Add(_objDetails_UtilizedToDate);
                FormerPRDetails.utilizedToDate = x.NewPRForm.UtilizedToDate;
            }
            if (FormerPRDetails.budgetBalance != x.NewPRForm.BudgetBalance)
            {
                NotificationMsg _objDetails_BudgetBalance = new NotificationMsg
                {
                    uuid = Guid.NewGuid(),
                    PRId = x.PRId,
                    msgDate = DateTime.Now,
                    fromUserId = x.UserId,
                    msgType = "Trail",
                    message = x.FullName + " change Budget Balance from " + FormerPRDetails.budgetBalance + " to " + x.NewPRForm.BudgetBalance + " for project: " + updateProject.projectName
                };
                db.NotificationMsgs.Add(_objDetails_BudgetBalance);
                FormerPRDetails.budgetBalance = x.NewPRForm.BudgetBalance;
            }
            if (FormerPRDetails.VendorId != x.NewPRForm.VendorId)
            {
                Vendor FormerVendorDetails = db.Vendors.FirstOrDefault(m => m.vendorId == FormerPRDetails.VendorId);
                Vendor LatestVendorDetails = db.Vendors.First(m => m.vendorId == x.NewPRForm.VendorId);

                NotificationMsg _objDetails_VendorId = new NotificationMsg();
                _objDetails_VendorId.uuid = Guid.NewGuid();
                _objDetails_VendorId.PRId = x.PRId;
                _objDetails_VendorId.msgDate = DateTime.Now;
                _objDetails_VendorId.fromUserId = x.UserId;
                _objDetails_VendorId.msgType = "Trail";
                if (FormerVendorDetails == null)
                {
                    _objDetails_VendorId.message = x.FullName + " set Vendor name to " + LatestVendorDetails.name + " for PR No: " + FormerPRDetails.PRNo;
                }
                else
                {
                    _objDetails_VendorId.message = x.FullName + " set Vendor name from " + FormerVendorDetails.name + " to " + LatestVendorDetails.name + " for PR No: " + FormerPRDetails.PRNo;
                }
                db.NotificationMsgs.Add(_objDetails_VendorId);
                FormerPRDetails.VendorId = x.NewPRForm.VendorId;
            }
            if (FormerPRDetails.VendorStaffId != x.NewPRForm.VendorStaffId)
            {
                VendorStaff FormerVendorStaffDetails = db.VendorStaffs.FirstOrDefault(m => m.staffId == FormerPRDetails.VendorStaffId);
                VendorStaff LatestVendorStaffDetails = db.VendorStaffs.First(m => m.staffId == x.NewPRForm.VendorStaffId);

                NotificationMsg _objDetails_VendorStaffId = new NotificationMsg();
                _objDetails_VendorStaffId.uuid = Guid.NewGuid();
                _objDetails_VendorStaffId.PRId = x.PRId;
                _objDetails_VendorStaffId.msgDate = DateTime.Now;
                _objDetails_VendorStaffId.fromUserId = x.UserId;
                _objDetails_VendorStaffId.msgType = "Trail";
                if (FormerVendorStaffDetails == null)
                {
                    _objDetails_VendorStaffId.message = x.FullName + " set Vendor staff name to " + LatestVendorStaffDetails.vendorContactName + " for PR No: " + FormerPRDetails.PRNo;
                }
                else
                {
                    _objDetails_VendorStaffId.message = x.FullName + " change Vendor staff name from " + FormerVendorStaffDetails.vendorContactName + " to " + LatestVendorStaffDetails.vendorContactName + " for PR No: " + FormerPRDetails.PRNo;
                }
                db.NotificationMsgs.Add(_objDetails_VendorStaffId);
                FormerPRDetails.VendorStaffId = x.NewPRForm.VendorStaffId;
            }

            foreach (var value in x.NewPRForm.PRItemListObject)
            {
                PR_Items objPRItemDetails = db.PR_Items.FirstOrDefault(m => m.itemsId == value.ItemsId);
                if (objPRItemDetails == null)
                {
                    PR_Items _objNewPRItem = new PR_Items
                    {
                        uuid = Guid.NewGuid(),
                        PRId = x.PRId,
                        dateRequired = value.DateRequired,
                        itemTypeId = value.ItemTypeId,
                        codeId = value.CodeId,
                        description = value.Description,                       
                        custPONo = value.CustPONo,
                        quantity = value.Quantity,
                        outStandingQuantity = value.Quantity,
                        UoMId = value.UoMId
                        //unitPrice = value.UnitPrice,
                        //totalPrice = value.TotalPrice,                        
                    };
                    db.PR_Items.Add(_objNewPRItem);

                    NotificationMsg newPRItem = new NotificationMsg()
                    {
                        uuid = Guid.NewGuid(),
                        PRId = x.PRId,
                        msgDate = DateTime.Now,
                        fromUserId = x.UserId,
                        msgType = "Trail",
                        message = x.FullName + " create new item from PR No. " + FormerPRDetails.PRNo
                    };
                    db.NotificationMsgs.Add(newPRItem);
                    db.SaveChanges();
                }
                else
                {
                    if (objPRItemDetails.dateRequired != value.DateRequired)
                    {
                        NotificationMsg _objDetails_dateRequired = new NotificationMsg
                        {
                            uuid = Guid.NewGuid(),
                            PRId = x.PRId,
                            msgDate = DateTime.Now,
                            fromUserId = x.UserId,
                            msgType = "Trail",
                            message = x.FullName + " change dateRequired in PRNo : " + FormerPRDetails.PRNo + " items from " + objPRItemDetails.dateRequired + " to " + value.DateRequired
                        };
                        db.NotificationMsgs.Add(_objDetails_dateRequired);
                        objPRItemDetails.dateRequired = value.DateRequired;
                    }
                    if (objPRItemDetails.description != value.Description)
                    {
                        NotificationMsg _objDetails_Description = new NotificationMsg
                        {
                            uuid = Guid.NewGuid(),
                            PRId = x.PRId,
                            msgDate = DateTime.Now,
                            fromUserId = x.UserId,
                            msgType = "Trail",
                            message = x.FullName + " change description in PRNo : " + FormerPRDetails.PRNo + " items from " + objPRItemDetails.description + " to " + value.Description
                        };
                        db.NotificationMsgs.Add(_objDetails_Description);
                        objPRItemDetails.description = value.Description;
                    }
                    if (objPRItemDetails.itemTypeId != value.ItemTypeId)
                    {

                        ItemType formerItemTypeId = db.ItemTypes.FirstOrDefault(m => m.itemTypeId == objPRItemDetails.itemTypeId);
                        ItemType itemTypeIdChanges = db.ItemTypes.FirstOrDefault(m => m.itemTypeId == value.ItemTypeId);

                        NotificationMsg _objDetails_ItemTypeId = new NotificationMsg();
                        _objDetails_ItemTypeId.uuid = Guid.NewGuid();
                        _objDetails_ItemTypeId.PRId = x.PRId;
                        _objDetails_ItemTypeId.msgDate = DateTime.Now;
                        _objDetails_ItemTypeId.fromUserId = x.UserId;
                        _objDetails_ItemTypeId.msgType = "Trail";
                        if (formerItemTypeId == null)
                        {
                            _objDetails_ItemTypeId.message = x.FullName + " set ItemDescription to " + itemTypeIdChanges.type + " in PRNo : " + FormerPRDetails.PRNo;
                        }
                        else if (itemTypeIdChanges == null)
                        {
                            _objDetails_ItemTypeId.message = x.FullName + " change ItemDescription in PRNo : " + FormerPRDetails.PRNo + " items from " + formerItemTypeId.type + " to " + itemTypeIdChanges;
                        } else
                        {
                            _objDetails_ItemTypeId.message = x.FullName + " change ItemDescription in PRNo : " + FormerPRDetails.PRNo + " items from " + formerItemTypeId.type + " to " + itemTypeIdChanges.type;
                        }
                        db.NotificationMsgs.Add(_objDetails_ItemTypeId);
                        objPRItemDetails.itemTypeId = value.ItemTypeId;
                    }
                    if (objPRItemDetails.codeId != value.CodeId)
                    {
                        
                            var formerCodeId = db.PR_Items.First(m => m.PRId == x.PRId);
                            var FormerItemCode = db.PopulateItemLists.Select(m => new { ItemDescription = m.ItemDescription, CodeId = m.codeId, CustId = m.custId }).Where(m => m.CustId == updateProject.custId).Where(m => m.CodeId == formerCodeId.codeId).FirstOrDefault();
                            var NewItemCode = db.PopulateItemLists.Select(m => new { ItemDescription = m.ItemDescription, CodeId = m.codeId, CustId = m.custId }).Where(m => m.CustId == updateProject.custId).Where(m => m.CodeId == value.CodeId).FirstOrDefault();
                            NotificationMsg _objDetails_CodeId = new NotificationMsg();
                            _objDetails_CodeId.uuid = Guid.NewGuid();
                            _objDetails_CodeId.PRId = x.PRId;
                            _objDetails_CodeId.msgDate = DateTime.Now;
                            _objDetails_CodeId.fromUserId = x.UserId;
                            _objDetails_CodeId.msgType = "Trail";
                        if (FormerItemCode == null)
                        {
                            _objDetails_CodeId.message = x.FullName + " change Item Code to " + NewItemCode.ItemDescription + " for PR No: " + FormerPRDetails.PRNo;
                        }
                        else if (NewItemCode == null)
                        {
                            _objDetails_CodeId.message = x.FullName + " change Item Code from " + FormerItemCode.ItemDescription + " to NULL for PR No: " + FormerPRDetails.PRNo;
                        }
                        else
                        {
                            _objDetails_CodeId.message = x.FullName + " change Item Code from " + FormerItemCode.ItemDescription + " to " + NewItemCode.ItemDescription + " for PR No: " + FormerPRDetails.PRNo;
                        }
                        db.NotificationMsgs.Add(_objDetails_CodeId);
                        objPRItemDetails.codeId = value.CodeId;
                    }
                    if (objPRItemDetails.custPONo != value.CustPONo)
                    {
                        NotificationMsg _objDetails_custPONo = new NotificationMsg
                        {
                            uuid = Guid.NewGuid(),
                            PRId = x.PRId,
                            msgDate = DateTime.Now,
                            fromUserId = x.UserId,
                            msgType = "Trail",
                            message = x.FullName + " change cust PONo in PRNo : " + FormerPRDetails.PRNo + " items from " + objPRItemDetails.custPONo + " to " + value.CustPONo
                        };
                        db.NotificationMsgs.Add(_objDetails_custPONo);
                        objPRItemDetails.custPONo = value.CustPONo;
                    }
                    if (objPRItemDetails.quantity != value.Quantity)
                    {
                        NotificationMsg _objDetails_Quantity = new NotificationMsg
                        {
                            uuid = Guid.NewGuid(),
                            PRId = x.PRId,
                            msgDate = DateTime.Now,
                            fromUserId = x.UserId,
                            msgType = "Trail",
                            message = x.FullName + " change Quantity in PRNo : " + FormerPRDetails.PRNo + " items from " + objPRItemDetails.quantity + " to " + value.Quantity
                        };
                        db.NotificationMsgs.Add(_objDetails_Quantity);
                        objPRItemDetails.quantity = value.Quantity;
                        objPRItemDetails.outStandingQuantity = value.Quantity;
                        //objPRItemDetails.totalPrice = (value.Quantity * value.UnitPrice) + value.SST;
                    }
                    if (objPRItemDetails.UoMId != value.UoMId)
                    {
                        UOM formerUOMs = db.UOMs.FirstOrDefault(m => m.UoMId == objPRItemDetails.UoMId);
                        UOM UOMsChanges = db.UOMs.FirstOrDefault(m => m.UoMId == value.UoMId);
                        NotificationMsg _objDetails_NotificationMsg = new NotificationMsg();
                        _objDetails_NotificationMsg.uuid = Guid.NewGuid();
                        _objDetails_NotificationMsg.PRId = x.PRId;
                        _objDetails_NotificationMsg.msgDate = DateTime.Now;
                        _objDetails_NotificationMsg.fromUserId = x.UserId;
                        _objDetails_NotificationMsg.msgType = "Trail";
                        if (formerUOMs == null)
                        {
                            _objDetails_NotificationMsg.message = x.FullName + " set UOM to " + UOMsChanges.UoMCode + " in PRNo : " + FormerPRDetails.PRNo;
                        }
                        else if (value.UoMId == null)
                        {
                            _objDetails_NotificationMsg.message = x.FullName + " change UOM in PRNo : " + FormerPRDetails.PRNo + " items from " + formerUOMs.UoMCode + " to null";
                        } else
                        {
                            _objDetails_NotificationMsg.message = x.FullName + " change UOM in PRNo : " + FormerPRDetails.PRNo + " items from " + formerUOMs.UoMCode + " to " + UOMsChanges.UoMCode;
                        }
                        db.NotificationMsgs.Add(_objDetails_NotificationMsg);
                        objPRItemDetails.UoMId = value.UoMId;
                    }
                    //if (objPRItemDetails.totalPrice != value.TotalPrice)
                    //{
                    //    PR_Items FormerTotalPrice = db.PR_Items.FirstOrDefault(m => m.totalPrice == objPRItemDetails.totalPrice);
                    //    NotificationMsg _objDetails_NotificationMsg = new NotificationMsg();
                    //    _objDetails_NotificationMsg.uuid = Guid.NewGuid();
                    //    _objDetails_NotificationMsg.PRId = x.PRId;
                    //    _objDetails_NotificationMsg.msgDate = DateTime.Now;
                    //    _objDetails_NotificationMsg.fromUserId = x.UserId;
                    //    _objDetails_NotificationMsg.msgType = "Trail";
                    //    if (FormerTotalPrice.totalPrice == null)
                    //    {
                    //        _objDetails_NotificationMsg.message = x.FullName + " set Total PRice to " + value.TotalPrice + " for PRNo L " + FormerPRDetails.PRNo;
                    //    }
                    //    else
                    //    {
                    //        _objDetails_NotificationMsg.message = x.FullName + " change Total Price in PRNo : " + FormerPRDetails.PRNo + " items from " + objPRItemDetails.unitPrice + " to " + value.UnitPrice;
                    //    }

                    //    db.NotificationMsgs.Add(_objDetails_NotificationMsg);
                    //    objPRItemDetails.totalPrice = value.TotalPrice;
                    //}
                    //if (x.NewPRForm.StatusId == "PR02")
                    //{
                    //    FormerPRDetails.AmountPOBalance = FormerPRDetails.AmountPOBalance + objPRItemDetails.outStandingQuantity;
                    //}
                    db.SaveChanges();
                }
            }
            //if (x.NewPRForm.SelectSave == true && (x.NewPRForm.StatusId == "PR01" || x.NewPRForm.StatusId == "PR07"))
            if (x.NewPRForm.SelectSave == true && x.NewPRForm.StatusId == "PR01")
            {
                FormerPRDetails.Saved = FormerPRDetails.Saved + 1;
                FormerPRDetails.LastModifyDate = DateTime.Now;
                NotificationMsg _objSaved = new NotificationMsg
                {
                    uuid = Guid.NewGuid(),
                    PRId = x.PRId,
                    msgDate = DateTime.Now,
                    fromUserId = x.UserId,
                    msgType = "Trail",
                    message = x.FullName + " has saved PR application for PR No. " + FormerPRDetails.PRNo + " " + FormerPRDetails.Saved + " time."
                };
                db.NotificationMsgs.Add(_objSaved);
                db.SaveChanges();
            }
            //else if (x.NewPRForm.SelectSave == true && x.NewPRForm.StatusId == "PR02")
            //{
            //    PR_Admin SaveAdminInfo = db.PR_Admin.First(m => m.PRId == x.PRId);
            //    SaveAdminInfo.adminSaved = SaveAdminInfo.adminSaved + 1;
            //    SaveAdminInfo.lastModifyDate = DateTime.Now;
            //    NotificationMsg _objSaved = new NotificationMsg
            //    {
            //        uuid = Guid.NewGuid(),
            //        PRId = x.PRId,
            //        msgDate = DateTime.Now,
            //        fromUserId = x.UserId,
            //        msgType = "Trail",
            //        message = x.FullName + " has saved PR application for PR No. " + FormerPRDetails.PRNo + " " + FormerPRDetails.Saved + " time."
            //    };
            //    db.NotificationMsgs.Add(_objSaved);
            //    db.SaveChanges();
            //}
            //if (x.NewPRForm.SelectSubmit == true && (x.NewPRForm.StatusId == "PR01" || x.NewPRForm.StatusId == "PR07"))
            if (x.NewPRForm.SelectSubmit == true && x.NewPRForm.StatusId == "PR01")
            {
                FormerPRDetails.Submited = FormerPRDetails.Submited + 1;
                FormerPRDetails.SubmitDate = DateTime.Now;
                //if ((x.PaperRefNoFile != null && x.NewPRForm.PaperRefNo != null) || (x.BidWaiverRefNoFile != null && x.NewPRForm.BidWaiverRefNo != null))
                //{
                //    FormerPRDetails.PaperAttachment = true;
                //    FormerPRDetails.BidWaiverRefNo = x.NewPRForm.BidWaiverRefNo;
                //    FormerPRDetails.PaperRefNo = x.NewPRForm.PaperRefNo;
                //}
                if (x.PaperRefNoFile != null && x.NewPRForm.PaperRefNo != null)
                {
                    FormerPRDetails.PaperAttachment = true;
                    FormerPRDetails.PaperRefNo = x.NewPRForm.PaperRefNo;
                }
                if (x.BidWaiverRefNoFile != null && x.NewPRForm.BidWaiverRefNo != null)
                {
                    FormerPRDetails.BidWaiverRefNo = x.NewPRForm.BidWaiverRefNo;
                }
                FormerPRDetails.StatusId = "PR09";
                NotificationMsg _objSubmited = new NotificationMsg
                {
                    uuid = Guid.NewGuid(),
                    PRId = x.PRId,
                    msgDate = DateTime.Now,
                    fromUserId = x.UserId,
                    msgType = "Trail",
                    message = x.FullName + " has submit PR application for PR No. " + FormerPRDetails.PRNo + " subject for reviewal"
                };
                db.NotificationMsgs.Add(_objSubmited);

                var getHOD = new List<NewPRModel>();
                var getPRPreparerChildCustId = db.Users.First(m => m.userId == FormerPRDetails.PreparedById);
                if (FormerPRDetails.CustId == 2 || (FormerPRDetails.CustId == 3 && getPRPreparerChildCustId.childCompanyId == 16))
                {                    
                    getHOD = (from m in db.Users
                              join n in db.Users_Roles on m.userId equals n.userId
                              join o in db.ChildCustomers on m.childCompanyId equals o.childCustId
                              where (n.roleId == "R02" || n.roleId == "R10") && m.companyId == x.CustId && m.childCompanyId == getPRPreparerChildCustId.childCompanyId
                              select new NewPRModel()
                              {
                                  HODApproverId = m.userId,
                                  HODApproverName = m.firstName + " " + m.lastName,
                                  ApproverEmail = m.emailAddress
                              }).ToList();
                }
                //else if (FormerPRDetails.CustId == 3 && getPRPreparerChildCustId.childCompanyId == 16)
                //{
                //    getHOD = (from m in db.Users
                //                   join n in db.Users_Roles on m.userId equals n.userId
                //                   where n.roleId == "R08" && m.companyId == 2 
                //                   //&& m.userId == 172 //en. Shahril
                //              select new NewPRModel()
                //              {
                //                  HODApproverId = m.userId,
                //                  HODApproverName = m.firstName + " " + m.lastName,
                //                  ApproverEmail = m.emailAddress
                //              }).ToList();
                //}
                else
                {
                    getHOD = (from m in db.Users
                              join o in db.Users on m.superiorId equals o.userId
                              join n in db.Users_Roles on o.userId equals n.userId
                              where n.roleId == "R02" && m.companyId == FormerPRDetails.CustId && m.userId == FormerPRDetails.PreparedById
                              select new NewPRModel()
                              {
                                  HODApproverId = o.userId,
                                  HODApproverName = o.firstName + " " + o.lastName,
                                  ApproverEmail = o.emailAddress
                              }).ToList();
                }

                int HODApproverId = 0;
                foreach (var item in getHOD)
                {
                    if (HODApproverId != item.HODApproverId)
                    {
                        PR_HOD _objHOD = new PR_HOD
                        {
                            uuid = Guid.NewGuid(),
                            HODId = item.HODApproverId,
                            PRId = FormerPRDetails.PRId,
                            HODApprovedP1 = x.NewPRForm.HODApproverApprovedP1
                        };
                        db.PR_HOD.Add(_objHOD);
                        db.SaveChanges();

                        NotificationMsg objTask = new NotificationMsg()
                        {
                            uuid = Guid.NewGuid(),
                            message = FormerPRDetails.PRNo + " pending for your initial approval",
                            fromUserId = x.UserId,
                            msgDate = DateTime.Now,
                            msgType = "Task",
                            PRId = x.PRId
                        };
                        db.NotificationMsgs.Add(objTask);
                        db.SaveChanges();

                        NotiGroup ApproverTask = new NotiGroup()
                        {
                            uuid = Guid.NewGuid(),
                            msgId = objTask.msgId,
                            toUserId = item.HODApproverId,
                            resubmit = false
                        };
                        db.NotiGroups.Add(ApproverTask);
                        db.SaveChanges();

                        x.NewPRForm.PRNo = FormerPRDetails.PRNo;
                        x.NewPRForm.ApproverId = item.HODApproverId;
                        x.NewPRForm.ApproverName = item.HODApproverName;
                        x.EmailAddress = item.ApproverEmail;
                        SendEmailPRNotification(x, "ApprovalInitial");
                    }
                    HODApproverId = item.HODApproverId;
                }
                
                var getProcurement = new List<PRModel>();
                if (x.CustId == 3 && getPRPreparerChildCustId.childCompanyId == 16)
                {
                    getProcurement = (from m in db.Users
                                      join n in db.Users_Roles on m.userId equals n.userId
                                      where n.roleId == "R03" && (m.companyId == x.CustId || (m.companyId == 2 && m.childCompanyId == getPRPreparerChildCustId.childCompanyId))
                                      select new PRModel()
                                      {
                                          UserId = m.userId,
                                          FullName = m.firstName + " " + m.lastName,
                                          EmailAddress = m.emailAddress
                                      }).ToList();
                }
                else
                {
                    getProcurement = (from m in db.Users
                                      join n in db.Users_Roles on m.userId equals n.userId
                                      where n.roleId == "R03" && (m.companyId == x.CustId && m.childCompanyId != 16)
                                      select new PRModel()
                                      {
                                          UserId = m.userId,
                                          FullName = m.firstName + " " + m.lastName,
                                          EmailAddress = m.emailAddress
                                      }).ToList();
                }

                foreach (var item in getProcurement)
                {
                    PR_Admin _objSaveAdmin = new PR_Admin
                    {
                        uuid = Guid.NewGuid(),
                        adminId = item.UserId,
                        PRId = x.PRId
                    };
                    db.PR_Admin.Add(_objSaveAdmin);
                    db.SaveChanges();
                }

                var getFinance = (from m in db.Users
                                  join n in db.Users_Roles on m.userId equals n.userId
                                  where n.roleId == "R13" && m.companyId == x.CustId
                                  select new NewPRModel()
                                  {
                                      ReviewerId = m.userId,
                                      ReviewerName = m.firstName + " " + m.lastName,
                                      ReviewerEmail = m.emailAddress
                                  }).ToList();

                foreach (var item in getFinance)
                {
                    PR_Finance _objReviewer = new PR_Finance
                    {
                        uuid = Guid.NewGuid(),
                        reviewerId = item.ReviewerId.Value,
                        PRId = FormerPRDetails.PRId,
                        reviewed = x.NewPRForm.Reviewed
                    };
                    db.PR_Finance.Add(_objReviewer);
                    db.SaveChanges();                    
                }
                //if (x.NewPRForm.StatusId == "PR07")
                //{
                //    var getMsgId = (from m in db.NotificationMsgs
                //                    join n in db.NotiGroups on m.msgId equals n.msgId
                //                    where m.PRId == x.PRId && m.msgType == "Task" && n.resubmit != null
                //                    select new
                //                    {
                //                        MsgId = m.msgId,
                //                        MsgDate = m.msgDate
                //                    }).OrderByDescending(m => m.MsgDate).First();
                //    NotiGroup UpdateReqTask = db.NotiGroups.First(m => m.msgId == getMsgId.MsgId);
                //    UpdateReqTask.resubmit = true;
                //    db.SaveChanges();
                //}
                //else
                //{                    
            }
            //}

            //else if (x.NewPRForm.SelectSubmit == true && x.NewPRForm.StatusId == "PR02")
            //{
            //    PR_Admin SaveAdminInfo = db.PR_Admin.First(m => m.PRId == x.PRId);
            //    SaveAdminInfo.adminSubmited = SaveAdminInfo.adminSubmited + 1;
            //    SaveAdminInfo.adminSubmitDate = DateTime.Now;
            //    db.SaveChanges();            
            //    var requestorDone = (from m in db.NotificationMsgs
            //                         join n in db.NotiGroups on m.msgId equals n.msgId
            //                         where n.toUserId == x.UserId && m.PRId == x.PRId
            //                         select new PRModel()
            //                         {
            //                             MsgId = m.msgId
            //                         }).First();

            //    NotificationMsg getDone = db.NotificationMsgs.First(m => m.msgId == requestorDone.MsgId);
            //    getDone.done = true;
            //    db.SaveChanges();

            //    NotificationMsg _objSubmited = new NotificationMsg
            //    {
            //        uuid = Guid.NewGuid(),
            //        PRId = x.PRId,
            //        msgDate = DateTime.Now,
            //        fromUserId = x.UserId,
            //        msgType = "Trail",
            //        message = x.FullName + " has submit PR procurement for PR No. " + FormerPRDetails.PRNo
            //    };
            //    db.NotificationMsgs.Add(_objSubmited);
            //}
            //db.SaveChanges();
        }

        protected void POSaveDbLogic(POModel POModel)
        {
            PurchaseOrder newPO = new PurchaseOrder();
            newPO.uuid = Guid.NewGuid();
            newPO.PRId = POModel.PRId;
            newPO.PONo = "dummyset";
            newPO.PODate = DateTime.Now;
            newPO.CustId = POModel.CustId;
            newPO.SubmitDate = DateTime.Now;
            newPO.projectId = POModel.NewPOForm.ProjectId;
            newPO.vendorId = POModel.NewPOForm.VendorId.Value;
            newPO.vendorStaffId = POModel.NewPOForm.VendorStaffId;
            newPO.PayToVendorId = POModel.NewPOForm.PayToVendorId;
            newPO.PaymentTermsId = POModel.NewPOForm.PaymentTermsId;
            newPO.PreparedById = POModel.UserId;
            newPO.PreparedDate = DateTime.Now;
            if (POModel.NewPOForm.SelectSave == true)
            {
                newPO.Saved = POModel.NewPOForm.Saved + 1;
                newPO.StatusId = "PO01";
            }
            else if (POModel.NewPOForm.SelectSubmit == true)
            {
                newPO.Submited = POModel.NewPOForm.SelectSubmit;
                newPO.StatusId = "PO03";
            }

            db.PurchaseOrders.Add(newPO);
            db.SaveChanges();
            newPO.PONo = "PO-" + DateTime.Now.Year + "-" + string.Format("{0}{1}", 0, newPO.POId.ToString("D4"));
            db.SaveChanges();

            int TotalQuantity = 0; decimal TotalPrice = 0;
            foreach (var value in POModel.NewPOForm.POItemListObject)
            {
                PO_Item _objNewPOItem = new PO_Item
                {
                    uuid = Guid.NewGuid(),
                    POId = newPO.POId,
                    itemsId = value.ItemsId,
                    itemTypeId = value.ItemTypeId,
                    dateRequired = value.DateRequired,
                    description = value.Description,
                    codeId = value.CodeId,
                    custPONo = value.CustPONo,
                    quantity = value.Quantity,
                    unitPrice = value.UnitPrice,
                    totalPrice = value.TotalPrice
                };
                db.PO_Item.Add(_objNewPOItem);
                db.SaveChanges();

                if (POModel.NewPOForm.SelectSubmit == true)
                {
                    PR_Items _objUpdatePRItem = db.PR_Items.First(m => m.itemsId == value.ItemsId);
                    _objUpdatePRItem.outStandingQuantity = _objUpdatePRItem.outStandingQuantity - _objNewPOItem.quantity;
                    newPO.POAging = POModel.NewPOForm.POAging;
                    db.SaveChanges();
                    TotalQuantity = TotalQuantity + _objNewPOItem.quantity;
                    TotalPrice = TotalPrice + _objNewPOItem.totalPrice;
                }
            }
            if (POModel.NewPOForm.SelectSubmit == true)
            {
                newPO.TotalQuantity = TotalQuantity;
                newPO.TotalPrice = TotalPrice;
                db.SaveChanges();

                var SumOutStandingQuantity = (from m in db.PurchaseRequisitions
                                              join n in db.PR_Items on m.PRId equals n.PRId
                                              where m.PRId == POModel.PRId
                                              select n.outStandingQuantity).Sum();
                if (SumOutStandingQuantity == 0)
                {
                    PurchaseRequisition updatePR = db.PurchaseRequisitions.First(m => m.PRId == POModel.PRId);
                    updatePR.AmountPOBalance = 0;

                    var requestorDone = (from m in db.NotificationMsgs
                                         join n in db.NotiGroups on m.msgId equals n.msgId
                                         where n.toUserId == POModel.UserId && m.PRId == POModel.PRId && m.msgType == "Task"
                                         select new PRModel()
                                         {
                                             MsgId = m.msgId
                                         }).First();

                    NotificationMsg getDone = db.NotificationMsgs.First(m => m.msgId == requestorDone.MsgId);
                    getDone.done = true;
                    db.SaveChanges();
                }
            }
            POModel.POId = newPO.POId;
        }

        protected void POUpdateDbLogic(POModel x)
        {
            PurchaseOrder objPODetails = db.PurchaseOrders.First(m => m.POId == x.POId);
            PurchaseRequisition newPR = new PurchaseRequisition();

            if (x.NewPOForm.SelectSave == true)
            {
                objPODetails.Saved = objPODetails.Saved + 1;
                objPODetails.LastModifyDate = DateTime.Now;
            }
            if (x.NewPOForm.SelectSubmit == true)
            {
                objPODetails.Submited = true;
                objPODetails.SubmitDate = DateTime.Now;
            }
            db.SaveChanges();
            newPR.AmountPOBalance = 0;
            int TotalQuantity = 0; decimal TotalPrice = 0;
            foreach (var value in x.NewPOForm.POItemListObject)
            {
                PO_Item objPOItemDetails = db.PO_Item.FirstOrDefault(m => m.itemsId == value.ItemsId && m.POId == x.POId);
                if (objPOItemDetails == null)
                {
                    PO_Item _objNewPOItem = new PO_Item
                    {
                        uuid = Guid.NewGuid(),
                        itemsId = value.ItemsId,
                        itemTypeId = value.ItemTypeId,
                        POId = x.POId,
                        dateRequired = value.DateRequired,
                        description = value.Description,
                        codeId = value.CodeId,
                        custPONo = value.CustPONo,
                        quantity = value.Quantity,
                        unitPrice = value.UnitPrice,
                        totalPrice = value.TotalPrice
                    };
                    newPR.AmountPOBalance = newPR.AmountPOBalance + _objNewPOItem.quantity;
                    db.PO_Item.Add(_objNewPOItem);
                    db.SaveChanges();
                }
                else
                {
                    if (objPOItemDetails.custPONo != value.CustPONo)
                    {
                        //Notification _objDetails_TotalGST = new Notification
                        //{
                        //    uuid = Guid.NewGuid(),
                        //    opportunityId = Int32.Parse(Session["ContractId"].ToString()),
                        //    POId = Int32.Parse(Session["POId"].ToString()),
                        //    createDate = DateTime.Now,
                        //    fromUserId = Int32.Parse(Session["UserId"].ToString()),
                        //    content = "change PO Total GST from " + objPODetaildb.Details_totalGST + " to " + x.POListDetail.Details_totalGST,
                        //    flag = 0
                        //};
                        //db.Notifications.Add(_objDetails_TotalGST);
                        objPOItemDetails.custPONo = value.CustPONo;
                    }
                    if (objPOItemDetails.quantity != value.Quantity)
                    {
                        //Notification _objDetails_TotalGST = new Notification
                        //{
                        //    uuid = Guid.NewGuid(),
                        //    opportunityId = Int32.Parse(Session["ContractId"].ToString()),
                        //    POId = Int32.Parse(Session["POId"].ToString()),
                        //    createDate = DateTime.Now,
                        //    fromUserId = Int32.Parse(Session["UserId"].ToString()),
                        //    content = "change PO Total GST from " + objPODetaildb.Details_totalGST + " to " + x.POListDetail.Details_totalGST,
                        //    flag = 0
                        //};
                        //db.Notifications.Add(_objDetails_TotalGST);
                        objPOItemDetails.quantity = value.Quantity;
                    }
                    if (objPOItemDetails.totalPrice != value.TotalPrice)
                    {
                        //Notification _objDetails_TotalGST = new Notification
                        //{
                        //    uuid = Guid.NewGuid(),
                        //    opportunityId = Int32.Parse(Session["ContractId"].ToString()),
                        //    POId = Int32.Parse(Session["POId"].ToString()),
                        //    createDate = DateTime.Now,
                        //    fromUserId = Int32.Parse(Session["UserId"].ToString()),
                        //    content = "change PO Total GST from " + objPODetaildb.Details_totalGST + " to " + x.POListDetail.Details_totalGST,
                        //    flag = 0
                        //};
                        //db.Notifications.Add(_objDetails_TotalGST);
                        objPOItemDetails.totalPrice = value.TotalPrice;
                    }
                    newPR.AmountPOBalance = newPR.AmountPOBalance + objPOItemDetails.quantity;
                    TotalQuantity = TotalQuantity + objPOItemDetails.quantity;
                    TotalPrice = TotalPrice + objPOItemDetails.totalPrice;
                }
                db.SaveChanges();

                if (x.NewPOForm.SelectSubmit == true)
                {
                    PR_Items _objUpdatePRItem = db.PR_Items.First(m => m.itemsId == value.ItemsId);
                    _objUpdatePRItem.outStandingQuantity = _objUpdatePRItem.outStandingQuantity - objPOItemDetails.quantity;
                    db.SaveChanges();
                }
            }
            objPODetails.TotalQuantity = TotalQuantity;
            objPODetails.TotalPrice = TotalPrice;
            db.SaveChanges();
        }
        //public void EntityToNavHeaderExcel(DateTime startDate, DateTime endDate, string ExtractFileLocation)
        //{
        //    //try
        //    //{
        //    Excel.Application oXL;
        //    Excel.Workbook oWB;
        //    Excel.Worksheet oSheet;
        //    Excel.Range oRange;
        //    // Start Excel and get Application object.
        //    oXL = new Excel.Application();

        //    // Set some properties
        //    oXL.Visible = true;
        //    oXL.DisplayAlerts = false;

        //    // Get a new workbook. 
        //    oWB = oXL.Workbooks.Add(Missing.Value);

        //    // Get the active sheet 
        //    oSheet = (Excel.Worksheet)oWB.ActiveSheet;
        //    oSheet.Name = "Purchase Header";

        //    oSheet.Cells[1, 1] = "OPEN PO TRANSACTION";
        //    oSheet.Cells[1, 2] = "Purchase Header";
        //    oSheet.Cells[1, 3] = "38";
        //    DataTable dt = GetPOHeaderTable(startDate, endDate);

        //    int rowCount = 3;
        //    foreach (DataRow dr in dt.Rows)
        //    {
        //        rowCount += 1;
        //        for (int i = 1; i < dt.Columns.Count + 1; i++)
        //        {
        //            // Add the header the first time through 
        //            if (rowCount == 4)
        //                oSheet.Cells[3, i] = dt.Columns[i - 1].ColumnName;
        //            oSheet.Cells[rowCount, i] = dr[i - 1].ToString();
        //        }
        //    }

        //    oRange = oSheet.Range[oSheet.Cells[1, 1], oSheet.Cells[rowCount, dt.Columns.Count]];
        //    oRange.Columns.AutoFit();

        //    // Save the sheet and close 
        //    oSheet = null;
        //    oRange = null;

        //    ;
        //    oWB.SaveAs(Server.MapPath("~/Documents/TestNavHeaderTemplate-" + startDate.ToString("yyyyMMdd") + "to" + endDate.ToString("yyyyMMdd") + "-" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx"), Excel.XlFileFormat.xlOpenXMLWorkbook, Missing.Value,
        //      Missing.Value, false, false,
        //      Excel.XlSaveAsAccessMode.xlNoChange, Excel.XlSaveConflictResolution.xlUserResolution, true, Missing.Value, Missing.Value, Missing.Value);

        //    //using (MemoryStream ms = new MemoryStream())
        //    //{
        //    //    using (FileStream fileStream = new FileStream("file.bin", FileMode.Open, FileAccess.Read))
        //    //    {
        //    //        fileStream.CopyTo(ms);
        //    //    }
        //    //    //Here you delete the saved file
        //    //    File.Delete(fileName);
        //    //    ms.Position = 0;
        //    //    return File(stream, "attachment;filename=myfile.xls", "myfile.xls");
        //    //}

        //    oWB = null;
        //    oXL.Quit();



        //    Excel.Application oXL1;
        //    Excel.Workbook oWB1;
        //    Excel.Worksheet oSheet1;
        //    Excel.Range oRange1;

        //    oXL1 = new Excel.Application();

        //    // Set some properties
        //    oXL1.Visible = true;
        //    oXL1.DisplayAlerts = false;

        //    // Get a new workbook. 
        //    oWB1 = oXL1.Workbooks.Add(Missing.Value);

        //    // Get the active sheet 
        //    oSheet1 = (Excel.Worksheet)oWB1.ActiveSheet;
        //    oSheet1.Name = "Purchase Line";

        //    oSheet1.Cells[1, 1] = "OPEN TRANSACTION";
        //    oSheet1.Cells[1, 2] = "Purchase Line";
        //    oSheet1.Cells[1, 3] = "39";
        //    DataTable dt1 = GetPOLineTable(startDate, endDate);

        //    int rowCount1 = 3;
        //    foreach (DataRow dr in dt1.Rows)
        //    {
        //        rowCount1 += 1;
        //        for (int i = 1; i < dt1.Columns.Count + 1; i++)
        //        {
        //            // Add the header the first time through 
        //            if (rowCount1 == 4)
        //                oSheet1.Cells[3, i] = dt1.Columns[i - 1].ColumnName;
        //            oSheet1.Cells[rowCount1, i] = dr[i - 1].ToString();
        //        }
        //    }

        //    oRange1 = oSheet1.Range[oSheet1.Cells[1, 1], oSheet1.Cells[rowCount1, dt1.Columns.Count]];
        //    oRange1.Columns.AutoFit();

        //    // Save the sheet and close 
        //    oSheet1 = null;
        //    oRange1 = null;
        //    oWB1.SaveAs(Server.MapPath("~/Documents/TestNavLinerTemplate-" + startDate.ToString("yyyyMMdd") + "to" + endDate.ToString("yyyyMMdd") + "-" + DateTime.Now.ToString("yyyyMMdd") + ".xlsx"), Excel.XlFileFormat.xlOpenXMLWorkbook, Missing.Value,
        //      Missing.Value, false, false,
        //      Excel.XlSaveAsAccessMode.xlNoChange, Excel.XlSaveConflictResolution.xlUserResolution, true, Missing.Value, Missing.Value, Missing.Value);
        //    oWB1.Close(Missing.Value, Missing.Value, Missing.Value);
        //    oWB1 = null;
        //    oXL1.Quit();
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    throw ex;
        //    //}
        //}
        //public void EntityToNavLineExcel(string excelFilePath, string sheetName, DateTime startDate, DateTime endDate, string Type)
        //{

        //    try
        //    {
        //        if (Type == "Header")
        //        {
        //            Excel.Application oXL;
        //            Excel.Workbook oWB;
        //            Excel.Worksheet oSheet;
        //            Excel.Range oRange;
        //            // Start Excel and get Application object.
        //            oXL = new Excel.Application();

        //            // Set some properties
        //            oXL.Visible = true;
        //            oXL.DisplayAlerts = false;

        //            // Get a new workbook. 
        //            oWB = oXL.Workbooks.Add(Missing.Value);

        //            // Get the active sheet 
        //            oSheet = (Excel.Worksheet)oWB.ActiveSheet;
        //            oSheet.Name = sheetName;

        //            oSheet.Cells[1, 1] = "OPEN PO TRANSACTION";
        //            oSheet.Cells[1, 2] = "Purchase Header";
        //            oSheet.Cells[1, 3] = "38";
        //            DataTable dt = GetPOHeaderTable(startDate, endDate);

        //            int rowCount = 3;
        //            foreach (DataRow dr in dt.Rows)
        //            {
        //                rowCount += 1;
        //                for (int i = 1; i < dt.Columns.Count + 1; i++)
        //                {
        //                    // Add the header the first time through 
        //                    if (rowCount == 4)
        //                        oSheet.Cells[3, i] = dt.Columns[i - 1].ColumnName;
        //                    oSheet.Cells[rowCount, i] = dr[i - 1].ToString();
        //                }
        //            }

        //            oRange = oSheet.Range[oSheet.Cells[1, 1], oSheet.Cells[rowCount, dt.Columns.Count]];
        //            oRange.Columns.AutoFit();

        //            // Save the sheet and close 
        //            oSheet = null;
        //            oRange = null;
        //            oWB.SaveAs(excelFilePath, Excel.XlFileFormat.xlWorkbookNormal, Missing.Value,
        //              Missing.Value, Missing.Value, Missing.Value,
        //              Excel.XlSaveAsAccessMode.xlExclusive, Missing.Value,
        //              Missing.Value, Missing.Value, Missing.Value);
        //            oWB.Close(Missing.Value, Missing.Value, Missing.Value);
        //            oWB = null;
        //            oXL.Quit();
        //        }
        //        else
        //        {
        //            Excel.Application oXL;
        //            Excel.Workbook oWB;
        //            Excel.Worksheet oSheet;
        //            Excel.Range oRange;

        //            oXL = new Excel.Application();

        //            // Set some properties
        //            oXL.Visible = true;
        //            oXL.DisplayAlerts = false;

        //            // Get a new workbook. 
        //            oWB = oXL.Workbooks.Add(Missing.Value);

        //            // Get the active sheet 
        //            oSheet = (Excel.Worksheet)oWB.ActiveSheet;
        //            oSheet.Name = sheetName;

        //            oSheet.Cells[1, 1] = "OPEN TRANSACTION";
        //            oSheet.Cells[1, 2] = "Purchase Line";
        //            oSheet.Cells[1, 3] = "39";
        //            DataTable dt1 = GetPOLineTable(startDate, endDate);

        //            int rowCount = 3;
        //            foreach (DataRow dr in dt1.Rows)
        //            {
        //                rowCount += 1;
        //                for (int i = 1; i < dt1.Columns.Count + 1; i++)
        //                {
        //                    // Add the header the first time through 
        //                    if (rowCount == 4)
        //                        oSheet.Cells[3, i] = dt1.Columns[i - 1].ColumnName;
        //                    oSheet.Cells[rowCount, i] = dr[i - 1].ToString();
        //                }
        //            }

        //            oRange = oSheet.Range[oSheet.Cells[1, 1], oSheet.Cells[rowCount, dt1.Columns.Count]];
        //            oRange.Columns.AutoFit();

        //            // Save the sheet and close 
        //            oSheet = null;
        //            oRange = null;
        //            oWB.SaveAs(excelFilePath, Excel.XlFileFormat.xlWorkbookNormal, Missing.Value,
        //              Missing.Value, Missing.Value, Missing.Value,
        //              Excel.XlSaveAsAccessMode.xlExclusive, Missing.Value,
        //              Missing.Value, Missing.Value, Missing.Value);
        //            oWB.Close(Missing.Value, Missing.Value, Missing.Value);
        //            oWB = null;
        //            oXL.Quit();
        //        }

        //        // Process the DataTable                

        //        return;
        //        // Resize the columns 

        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
        //NewPO balnket        
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

        protected List<QuestionAnswerModels> getMessageList(int PRId, int UserId)
        {
            SqlConnection conn = null;
            DataSet returnDS = new DataSet("MessageList");
            string info = "";
            try
            {
                SqlDataAdapter SQLDataADP = new SqlDataAdapter();
                SQLDataADP.TableMappings.Add("Table", "MessageList");
                conn = OpenDBConnection();
                conn.InfoMessage += (object obj, SqlInfoMessageEventArgs e) =>
                {
                    info = e.Message.ToString();
                };
                string sql = "";
                sql = "SELECT DISTINCT [PRId], " +
                                        "[FromUserName], " +
                                        "[msgDate], " +
                                        "[PRNo], " +
                                        "[message], " +
                                        "[msgType], " +
                                        "Stuff((SELECT ',' + [ToUserName] " +
                                               "FROM   messagelist " +
                                               "WHERE  [msgId] = a.[msgId] " +
                                               "FOR xml path ('')), 1, 1, '') AS [ToUserName] " +
                        "FROM   messagelist AS a " +
                        "WHERE PRId = @PRId " +
                        "ORDER by [msgDate] DESC ";

                SqlCommand SQLcmd = new SqlCommand(sql, conn)
                {
                    CommandType = CommandType.Text
                };
                SQLDataADP.SelectCommand = SQLcmd;

                SQLcmd.Parameters.Add("@PRId", SqlDbType.Int);
                SQLcmd.Parameters["@PRId"].Value = PRId;

                SQLDataADP.Fill(returnDS);
            }
            catch (Exception ex)
            {
                Debug.Print("Report - " + ex.Message);
            }
            finally
            {
                CloseDBConnection(conn);
            }

            var MessageList = returnDS.Tables[0].AsEnumerable().Select(
            dataRow => new QuestionAnswerModels
            {
                //Uid = uid,
                //ChatId = dataRow.Field<int>("chatid"),
                PRId = dataRow.Field<int>("PRId"),
                FromUserName = dataRow.Field<string>("fromusername"),
                MsgDate = dataRow.Field<DateTime>("msgDate"),
                PRNo = dataRow.Field<string>("PRNo"),
                ToUserName = dataRow.Field<string>("ToUserName"),
                Message = dataRow.Field<string>("message"),
                MsgType = dataRow.Field<string>("msgType")
            }).ToList();

            return MessageList;
        }

        protected List<PRModel> getHOGPSSList(int CustId)
        {
            SqlConnection conn = null;
            DataSet returnDS = new DataSet("HOGPSSList");
            string info = "";
            try
            {
                SqlDataAdapter SQLDataADP = new SqlDataAdapter();
                SQLDataADP.TableMappings.Add("Table", "HOGPSSList");
                conn = OpenDBConnection();
                conn.InfoMessage += (object obj, SqlInfoMessageEventArgs e) =>
                {
                    info = e.Message.ToString();
                };
                string sql = "";
                sql = "SELECT m.[userid], " +
                               "m.[username] + ' (' + o.[name] + ')' AS [Name] " +
                        "FROM   [user] m " +
                               "LEFT JOIN users_roles n " +
                                      "ON ( m.userid = n.userid ) " +
                               "LEFT JOIN role o " +
                                      "ON ( n.roleid = o.roleid ) " +
                        "WHERE  n.roleid = 'R04' " +
                               "AND m.companyid = @CustId " +
                        "UNION " +
                        "SELECT m.[userid], " +
                               "m.[username] + ' (' + o.[name] + ')' AS [Name] " +
                        "FROM   [user] m " +
                               "LEFT JOIN users_roles n " +
                                      "ON ( m.userid = n.userid ) " +
                               "LEFT JOIN role o " +
                                      "ON ( n.roleid = o.roleid ) " +
                        "WHERE  n.roleid = 'R12' " +
                                "OR n.roleid = 'R14' ";

                SqlCommand SQLcmd = new SqlCommand(sql, conn)
                {
                    CommandType = CommandType.Text
                };
                SQLDataADP.SelectCommand = SQLcmd;
                SQLcmd.Parameters.Add("@CustId", SqlDbType.Int);
                SQLcmd.Parameters["@CustId"].Value = CustId;

                SQLDataADP.Fill(returnDS);
            }
            catch (Exception ex)
            {
                Debug.Print("Report - " + ex.Message);
            }
            finally
            {
                CloseDBConnection(conn);
            }

            var HOGPSSList = returnDS.Tables[0].AsEnumerable().Select(
            dataRow => new PRModel
            {
                //Uid = uid,
                //ChatId = dataRow.Field<int>("chatid"),
                FinalApproverId = dataRow.Field<int>("userid"),
                UserName = dataRow.Field<string>("Name")
            }).ToList();

            return HOGPSSList;
        }

        protected List<POHeaderTable> GetPOHeaderTable(DateTime startDate, DateTime endDate)
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
                SQLcmd.Parameters["@startDate"].Value = startDate.ToShortDateString();
                SQLcmd.Parameters.Add("@endDate", SqlDbType.DateTime);
                SQLcmd.Parameters["@endDate"].Value = endDate.ToShortDateString();

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
                    ExpectedReceiptDate = dataRow.Field<string>("Expected Receipt Date"),
                    PostingDescription = dataRow.Field<string>("Posting Description"),
                    LocationCode = dataRow.Field<string>("Location Code"),
                    ShortcutDimension1Code = dataRow.Field<string>("Shortcut Dimension 1 Code"),
                    ShortcutDimension2Code = dataRow.Field<string>("Shortcut Dimension 2 Code"),
                    VendorPostingGroup = dataRow.Field<string>("Vendor Posting Group"),
                    CurrencyCode = dataRow.Field<string>("Currency Code")
                }).ToList();

            return POHeaderList;
        }

        protected List<POLineTable> GetPOLineTable(DateTime startDate, DateTime endDate)
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
                SQLcmd.Parameters["@startDate"].Value = startDate.ToShortDateString();
                SQLcmd.Parameters.Add("@endDate", SqlDbType.DateTime);
                SQLcmd.Parameters["@endDate"].Value = endDate.ToShortDateString();

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
                    DimProject = dataRow.Field<string>("Dim-Project"),
                    DimDepartment = dataRow.Field<string>("Dim-Department")
                }).ToList();

            return POLineList;
        }

        protected void CloseDBConnection(SqlConnection conn)
        {
            if ((conn != null))
            {
                conn.Close();
            }
            //Debug.Print("Connection Closed")
        }
    }
}