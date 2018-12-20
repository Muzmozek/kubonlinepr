﻿using KUBOnlinePRPM.Models;
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

namespace KUBOnlinePRPM.Controllers
{
    public abstract class DBLogicController : Controller
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
                AmountRequired = model.NewPRForm.AmountRequired,
                Justification = model.NewPRForm.Justification,
                //TotalGST = x.PRDetailObject.TotalGST,
                PreparedById = model.NewPRForm.PreparedById,
                PreparedDate = DateTime.Now,
                Saved = model.NewPRForm.Saved + 1,
                Submited = model.NewPRForm.Submited,
                Rejected = model.NewPRForm.Rejected,
                PRType = model.Type,
                Scenario = model.NewPRForm.Scenario,
                StatusId = "PR01"
            };
            db.PurchaseRequisitions.Add(_objNewPR);
            db.SaveChanges();

            Project _updateProject = db.Projects.First(m => m.projectId == model.NewPRForm.ProjectId);
            _updateProject.budgetedAmount = model.NewPRForm.BudgetedAmount;
            _updateProject.utilizedToDate = model.NewPRForm.UtilizedToDate;
            _updateProject.budgetBalance = model.NewPRForm.BudgetBalance;
            db.SaveChanges();

            PurchaseRequisition generatePRNo = db.PurchaseRequisitions.First(m => m.PRId == _objNewPR.PRId);
            generatePRNo.PRNo = "PR-" + DateTime.Now.Year + "-" + string.Format("{0}{1}", 0, _objNewPR.PRId.ToString("D4"));
            NotificationMsg generateMsg = new NotificationMsg();

            model.PRId = _objNewPR.PRId;
            _objNewPR.AmountPOBalance = 0;

            foreach (var value in model.NewPRForm.PRItemListObject)
            {
                PR_Items _objNewPRItem = new PR_Items
                {
                    uuid = Guid.NewGuid(),
                    PRId = _objNewPR.PRId,
                    //itemsId = value.ItemsId,
                    dateRequired = value.DateRequired,
                    description = value.Description,
                    //codeId = value.CodeId,
                    custPONo = value.CustPONo,
                    quantity = value.Quantity,
                    outStandingQuantity = value.Quantity
                    //UOM = value.UOM
                };
                _objNewPR.AmountPOBalance = _objNewPR.AmountPOBalance + _objNewPRItem.outStandingQuantity;
                db.PR_Items.Add(_objNewPRItem);

                generateMsg.uuid = Guid.NewGuid();
                generateMsg.message = model.FullName + " has save new item for PR No. " + _objNewPR.PRNo;
                generateMsg.PRId = _objNewPR.PRId;
                generateMsg.msgDate = DateTime.Now;
                generateMsg.fromUserId = model.NewPRForm.PreparedById;
                generateMsg.msgType = "Trail";
                db.NotificationMsgs.Add(generateMsg);

                db.SaveChanges();
            }

            PR_HOD _objHOD = new PR_HOD
            {
                uuid = Guid.NewGuid(),
                HODId = model.NewPRForm.HODApproverId,
                PRId = _objNewPR.PRId,
                HODApprovedP1 = model.NewPRForm.HODApproverApprovedP1
            };
            db.PR_HOD.Add(_objHOD);
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
                if((model.PaperRefNoFile != null && model.NewPRForm.PaperRefNo != null) || (model.BidWaiverRefNoFile != null && model.NewPRForm.BidWaiverRefNo != null)){
                    generatePRNo.PaperAttachment = true;
                    generatePRNo.BidWaiverRefNo = model.NewPRForm.BidWaiverRefNo;
                    generatePRNo.PaperRefNo = model.NewPRForm.PaperRefNo;
                }                
                generatePRNo.StatusId = "PR09";
                generateMsg.uuid = Guid.NewGuid();
                generateMsg.message = model.FullName + " has submit new PR  No. " + _objNewPR.PRNo;
                generateMsg.PRId = _objNewPR.PRId;
                generateMsg.msgDate = DateTime.Now;
                generateMsg.fromUserId = model.NewPRForm.PreparedById;
                generateMsg.msgType = "Trail";
                db.NotificationMsgs.Add(generateMsg);

                var getApprover = (from m in db.Users
                                   join n in db.PR_HOD on m.userId equals n.HODId
                                   where n.PRId == _objNewPR.PRId
                                   select new NewPRModel()
                                   {
                                       HODApproverId = m.userId,
                                       HODApproverName = m.firstName + " " + m.lastName
                                   }).First();
                NotificationMsg objTask = new NotificationMsg()
                {
                    uuid = Guid.NewGuid(),
                    message = _objNewPR.PRNo + " pending for your approval",
                    fromUserId = model.UserId,
                    msgDate = DateTime.Now,
                    msgType = "Task",
                    PRId = model.PRId
                };
                db.NotificationMsgs.Add(objTask);
                db.SaveChanges();

                var getAdminList = (from m in db.Users
                                    join n in db.Users_Roles on m.userId equals n.userId
                                    where n.roleId == "R03" && m.companyId == model.CustId
                                    select new NewPRModel()
                                    {
                                        AdminId = m.userId,
                                        AdminName = m.firstName + " " + m.lastName
                                    }).ToList();

                foreach (var item in getAdminList)
                {
                    PR_Admin _objSaveAdmin = new PR_Admin
                    {
                        uuid = Guid.NewGuid(),
                        adminId = item.AdminId,
                        PRId = _objNewPR.PRId
                    };
                    db.PR_Admin.Add(_objSaveAdmin);
                    db.SaveChanges();
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
                    NotiGroup ApproverTask = new NotiGroup()
                    {
                        uuid = Guid.NewGuid(),
                        msgId = objTask.msgId,
                        toUserId = getApprover.HODApproverId,
                        resubmit = false
                    };
                    db.NotiGroups.Add(ApproverTask);
                //}
                db.SaveChanges();
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
                //if (FormerPRDetails.Unbudgeted != x.NewPRForm.Unbudgeted)
                //{
                //    string newMsg = "";
                //    if (x.NewPRForm.Unbudgeted == false) {
                //        newMsg = x.FullName + " has untick unbudgeted checkbox for PR No: " + FormerPRDetails.PRNo; } else
                //    {
                //        newMsg = x.FullName + " has tick unbudgeted checkbox for PR No: " + FormerPRDetails.PRNo;
                //    }
                //    NotificationMsg _objDetails_Unbudgeted = new NotificationMsg
                //    {
                //        uuid = Guid.NewGuid(),
                //        PRId = x.PRId,
                //        msgDate = DateTime.Now,
                //        fromUserId = x.UserId,
                //        msgType = "Trail",
                //        message = newMsg
                //    };
                //    db.NotificationMsgs.Add(_objDetails_Unbudgeted);
                //    FormerPRDetails.Unbudgeted = x.NewPRForm.Unbudgeted;
                //}
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
                if (updateProject.budgetedAmount != x.NewPRForm.BudgetedAmount)
                {
                    NotificationMsg _objDetails_BudgetedAmount = new NotificationMsg
                    {
                        uuid = Guid.NewGuid(),
                        PRId = x.PRId,
                        msgDate = DateTime.Now,
                        fromUserId = x.UserId,
                        msgType = "Trail",
                        message = x.FullName + " change Budget Amount from " + updateProject.budgetedAmount + " to " + x.NewPRForm.BudgetedAmount + " for project: " + updateProject.projectName
                    };
                    db.NotificationMsgs.Add(_objDetails_BudgetedAmount);
                    updateProject.budgetedAmount = x.NewPRForm.BudgetedAmount;
                }
                if (FormerPRDetails.AmountRequired != x.NewPRForm.AmountRequired)
                {
                    NotificationMsg _objDetails_AmountRequired = new NotificationMsg
                    {
                        uuid = Guid.NewGuid(),
                        PRId = x.PRId,
                        msgDate = DateTime.Now,
                        fromUserId = x.UserId,
                        msgType = "Trail",
                        message = x.FullName + " change Amount Required from " + FormerPRDetails.AmountRequired + " to " + x.NewPRForm.AmountRequired + " for PR No: " + FormerPRDetails.PRNo
                    };
                    db.NotificationMsgs.Add(_objDetails_AmountRequired);
                    FormerPRDetails.AmountRequired = x.NewPRForm.AmountRequired;
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
                        message = x.FullName + " change Justification from " + FormerPRDetails.Justification + " to " + x.NewPRForm.Justification + " for PR No: " + FormerPRDetails.PRNo
                    };
                    db.NotificationMsgs.Add(_objDetails_Justification);
                    FormerPRDetails.Justification = x.NewPRForm.Justification;
                }
                if (updateProject.utilizedToDate != x.NewPRForm.UtilizedToDate)
                {
                    NotificationMsg _objDetails_UtilizedToDate = new NotificationMsg
                    {
                        uuid = Guid.NewGuid(),
                        PRId = x.PRId,
                        msgDate = DateTime.Now,
                        fromUserId = x.UserId,
                        msgType = "Trail",
                        message = x.FullName + " change Utilized To Date from " + updateProject.utilizedToDate + " to " + x.NewPRForm.UtilizedToDate + " for project: " + updateProject.projectName
                    };
                    db.NotificationMsgs.Add(_objDetails_UtilizedToDate);
                    updateProject.utilizedToDate = x.NewPRForm.UtilizedToDate;
                }
                if (updateProject.budgetBalance != x.NewPRForm.BudgetBalance)
                {
                    NotificationMsg _objDetails_BudgetBalance = new NotificationMsg
                    {
                        uuid = Guid.NewGuid(),
                        PRId = x.PRId,
                        msgDate = DateTime.Now,
                        fromUserId = x.UserId,
                        msgType = "Trail",
                        message = x.FullName + " change Budget Balance from " + updateProject.budgetBalance + " to " + x.NewPRForm.BudgetBalance + " for project: " + updateProject.projectName
                    };
                    db.NotificationMsgs.Add(_objDetails_BudgetBalance);
                    updateProject.budgetBalance = x.NewPRForm.BudgetBalance;
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
                    if ((x.PaperRefNoFile != null && x.NewPRForm.PaperRefNo != null) || (x.BidWaiverRefNoFile != null && x.NewPRForm.BidWaiverRefNo != null))
                    {
                        FormerPRDetails.PaperAttachment = true;
                        FormerPRDetails.BidWaiverRefNo = x.NewPRForm.BidWaiverRefNo;
                        FormerPRDetails.PaperRefNo = x.NewPRForm.PaperRefNo;
                    }
                    FormerPRDetails.StatusId = "PR09";
                    NotificationMsg _objSubmited = new NotificationMsg
                    {
                        uuid = Guid.NewGuid(),
                        PRId = x.PRId,
                        msgDate = DateTime.Now,
                        fromUserId = x.UserId,
                        msgType = "Trail",
                        message = x.FullName + " has submit PR application for PR No. " + FormerPRDetails.PRNo + " subject for approval"
                    };
                    db.NotificationMsgs.Add(_objSubmited);

                    var getApprover = (from m in db.PurchaseRequisitions
                                       join n in db.PR_HOD on m.PRId equals n.PRId
                                       //join o in db.Roles on n.roleId equals o.roleId
                                       where m.PRId == x.PRId
                                       select new PRModel()
                                       {
                                           UserId = n.HODId
                                       }).ToList();
                    NotificationMsg objTask = new NotificationMsg()
                    {
                        uuid = Guid.NewGuid(),
                        message = FormerPRDetails.PRNo + " pending for your approval",
                        fromUserId = x.UserId,
                        msgDate = DateTime.Now,
                        msgType = "Task",
                        PRId = x.PRId
                    };
                    db.NotificationMsgs.Add(objTask);
                    db.SaveChanges();

                var getAdminList = (from m in db.Users
                                        join n in db.Users_Roles on m.userId equals n.userId
                                        where n.roleId == "R03" && m.companyId == x.CustId
                                    select new NewPRModel()
                                        {
                                            AdminId = m.userId,
                                            AdminName = m.firstName + " " + m.lastName
                                        }).ToList();

                    foreach (var item in getAdminList)
                    {
                        PR_Admin _objSaveAdmin = new PR_Admin
                        {
                            uuid = Guid.NewGuid(),
                            adminId = item.AdminId,
                            PRId = x.PRId
                        };
                        db.PR_Admin.Add(_objSaveAdmin);
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
                        foreach (var item in getApprover)
                        {
                            NotiGroup ApproverTask = new NotiGroup()
                            {
                                uuid = Guid.NewGuid(),
                                msgId = objTask.msgId,
                                toUserId = item.UserId,
                                resubmit = false
                            };
                            db.NotiGroups.Add(ApproverTask);
                            db.SaveChanges();
                        }
                    //}
                }
                //else if (x.NewPRForm.SelectSubmit == true && x.NewPRForm.StatusId == "PR02")
                //{
                //    PR_Admin SaveAdminInfo = db.PR_Admin.First(m => m.PRId == x.PRId);
                //    SaveAdminInfo.adminSubmited = SaveAdminInfo.adminSubmited + 1;
                //    SaveAdminInfo.adminSubmitDate = DateTime.Now;
                //    db.SaveChanges();

                //    if (FormerPRDetails.AmountRequired < 20000)
                //    {
                //        var getReviewer = (from m in db.Users
                //                           join n in db.Users_Roles on m.userId equals n.userId
                //                           where n.roleId == "R02"
                //                           select new NewPRModel()
                //                           {
                //                               ReviewerId = m.userId
                //                           }).ToList();
                //        NotificationMsg objTask = new NotificationMsg()
                //        {
                //            uuid = Guid.NewGuid(),
                //            message = FormerPRDetails.PRNo + " pending for your reviewal",
                //            fromUserId = x.UserId,
                //            msgDate = DateTime.Now,
                //            msgType = "Task",
                //            PRId = FormerPRDetails.PRId
                //        };
                //        db.NotificationMsgs.Add(objTask);
                //        FormerPRDetails.StatusId = "PR03";
                //        db.SaveChanges();

                //        foreach (var item in getReviewer)
                //        {
                //            NotiGroup Task = new NotiGroup()
                //            {
                //                uuid = Guid.NewGuid(),
                //                msgId = objTask.msgId,
                //                toUserId = item.ReviewerId
                //            };
                //            db.NotiGroups.Add(Task);

                //            PR_Reviewer saveReviewer = new PR_Reviewer()
                //            {
                //                uuid = Guid.NewGuid(),
                //                reviewerId = item.ReviewerId,
                //                PRId = FormerPRDetails.PRId
                //            };
                //            db.PR_Reviewer.Add(saveReviewer);
                //            db.SaveChanges();
                //        }
                //    }
                //    else if (FormerPRDetails.AmountRequired > 20000)
                //    {
                //        var getRecommender = (from m in db.Users
                //                              join n in db.Users_Roles on m.userId equals n.userId
                //                              where n.roleId == "R03"
                //                              select new NewPRModel()
                //                              {
                //                                  ReviewerId = m.userId
                //                              }).ToList();
                //        NotificationMsg objTask = new NotificationMsg()
                //        {
                //            uuid = Guid.NewGuid(),
                //            message = FormerPRDetails.PRNo + " pending for your recommendal",
                //            fromUserId = x.UserId,
                //            msgDate = DateTime.Now,
                //            msgType = "Task",
                //            PRId = FormerPRDetails.PRId
                //        };
                //        db.NotificationMsgs.Add(objTask);
                //        FormerPRDetails.StatusId = "PR12";
                //        db.SaveChanges();

                //        foreach (var item in getRecommender)
                //        {
                //            NotiGroup Task = new NotiGroup()
                //            {
                //                uuid = Guid.NewGuid(),
                //                msgId = objTask.msgId,
                //                toUserId = item.RecommenderId
                //            };
                //            db.NotiGroups.Add(Task);

                //            PR_Recommender saveRecommender = new PR_Recommender()
                //            {
                //                uuid = Guid.NewGuid(),
                //                recommenderId = item.RecommenderId,
                //                PRId = FormerPRDetails.PRId
                //            };
                //            db.PR_Recommender.Add(saveRecommender);
                //            db.SaveChanges();
                //        }
                //    }

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
                            description = value.Description,
                            codeId = value.CodeId,
                            custPONo = value.CustPONo,
                            quantity = value.Quantity,
                            outStandingQuantity = value.Quantity,
                            unitPrice = value.UnitPrice,
                            totalPrice = value.TotalPrice
                        };
                        if (x.NewPRForm.StatusId == "PR02")
                        {
                            FormerPRDetails.AmountPOBalance = FormerPRDetails.AmountPOBalance + _objNewPRItem.outStandingQuantity;
                        }
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
                            ItemType itemTypeIdChanges = db.ItemTypes.First(m => m.itemTypeId == value.ItemTypeId);

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
                            else
                            {
                                _objDetails_ItemTypeId.message = x.FullName + " change ItemDescription in PRNo : " + FormerPRDetails.PRNo + " items from " + formerItemTypeId.type + " to " + itemTypeIdChanges.type;
                            }
                            db.NotificationMsgs.Add(_objDetails_ItemTypeId);
                            objPRItemDetails.itemTypeId = value.ItemTypeId;
                        }
                        if (objPRItemDetails.codeId != value.CodeId)
                        {
                            var formerItemDescription = db.PopulateItemLists.FirstOrDefault(m => m.codeId == objPRItemDetails.codeId);
                            var ItemDescriptionChanges = db.PopulateItemLists.First(m => m.codeId == value.CodeId);

                            NotificationMsg _objDetails_CodeId = new NotificationMsg();
                            _objDetails_CodeId.uuid = Guid.NewGuid();
                            _objDetails_CodeId.PRId = x.PRId;
                            _objDetails_CodeId.msgDate = DateTime.Now;
                            _objDetails_CodeId.fromUserId = x.UserId;
                            _objDetails_CodeId.msgType = "Trail";
                            if (formerItemDescription == null)
                            {
                                _objDetails_CodeId.message = x.FullName + " set ItemDescription to " + ItemDescriptionChanges.ItemDescription + " in PRNo : " + FormerPRDetails.PRNo;
                            }
                            else
                            {
                                _objDetails_CodeId.message = x.FullName + " change ItemDescription in PRNo : " + FormerPRDetails.PRNo + " items from " + formerItemDescription.ItemDescription + " to " + ItemDescriptionChanges.ItemDescription;
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
                        }
                        //if (objPRItemDetails.outStandingQuantity != value.OutstandingQuantity)
                        //{
                        //    NotificationMsg _objDetails_OutStandingQuantity = new NotificationMsg
                        //    {
                        //        uuid = Guid.NewGuid(),
                        //        PRId = x.PRId,
                        //        msgDate = DateTime.Now,
                        //        fromUserId = x.UserId,
                        //        msgType = "Trail",
                        //        message = x.FullName + " change Outstanding Quantity in PRNo : " + FormerPRDetails.PRNo + " items from " + objPRItemDetails.outStandingQuantity + " to " + value.OutstandingQuantity
                        //    };
                        //    db.NotificationMsgs.Add(_objDetails_OutStandingQuantity);
                        //    objPRItemDetails.outStandingQuantity = value.OutstandingQuantity;
                        //}
                        if (objPRItemDetails.unitPrice != value.UnitPrice)
                        {
                            PR_Items FormerUnitPrice = db.PR_Items.FirstOrDefault(m => m.unitPrice == objPRItemDetails.unitPrice);
                            NotificationMsg _objDetails_NotificationMsg = new NotificationMsg();
                            _objDetails_NotificationMsg.uuid = Guid.NewGuid();
                            _objDetails_NotificationMsg.PRId = x.PRId;
                            _objDetails_NotificationMsg.msgDate = DateTime.Now;
                            _objDetails_NotificationMsg.fromUserId = x.UserId;
                            _objDetails_NotificationMsg.msgType = "Trail";
                            if (FormerUnitPrice.unitPrice == null)
                            {
                                _objDetails_NotificationMsg.message = x.FullName + " set Unit Price to " + value.UnitPrice + " in PRNo : " + FormerPRDetails.PRNo;
                            }
                            else
                            {
                                _objDetails_NotificationMsg.message = x.FullName + " change Unit Price in PRNo : " + FormerPRDetails.PRNo + " items from " + objPRItemDetails.unitPrice + " to " + value.UnitPrice;
                            }
                            db.NotificationMsgs.Add(_objDetails_NotificationMsg);
                            objPRItemDetails.unitPrice = value.UnitPrice;
                        }
                        if (objPRItemDetails.totalPrice != value.TotalPrice)
                        {
                            PR_Items FormerTotalPrice = db.PR_Items.FirstOrDefault(m => m.totalPrice == objPRItemDetails.totalPrice);
                            NotificationMsg _objDetails_NotificationMsg = new NotificationMsg();
                            _objDetails_NotificationMsg.uuid = Guid.NewGuid();
                            _objDetails_NotificationMsg.PRId = x.PRId;
                            _objDetails_NotificationMsg.msgDate = DateTime.Now;
                            _objDetails_NotificationMsg.fromUserId = x.UserId;
                            _objDetails_NotificationMsg.msgType = "Trail";
                            if (FormerTotalPrice.totalPrice == null)
                            {
                                _objDetails_NotificationMsg.message = x.FullName + " set Total PRice to " + value.TotalPrice + " for PRNo L " + FormerPRDetails.PRNo;
                            }
                            else
                            {
                                _objDetails_NotificationMsg.message = x.FullName + " change Total Price in PRNo : " + FormerPRDetails.PRNo + " items from " + objPRItemDetails.unitPrice + " to " + value.UnitPrice;
                            }

                            db.NotificationMsgs.Add(_objDetails_NotificationMsg);
                            objPRItemDetails.totalPrice = value.TotalPrice;
                        }
                        if (x.NewPRForm.StatusId == "PR02")
                        {
                            FormerPRDetails.AmountPOBalance = FormerPRDetails.AmountPOBalance + objPRItemDetails.outStandingQuantity;
                        }
                    }
                    db.SaveChanges();
                }
        }
        protected void SendEmailNotification(PRModel PR, string POFlow)
        {
            var myEmail = db.Users.SingleOrDefault(x => x.userId == PR.UserId).emailAddress;
            var getRequestorDetails = (from m in db.PurchaseRequisitions
                                     join n in db.Users on m.PreparedById equals n.userId
                                     where m.PRId == PR.PRId
                                     select new UserModel()
                                     {
                                         UserId = n.userId,
                                         FullName = n.firstName + " " + n.lastName,
                                         EmailAddress = n.emailAddress
                                     }).FirstOrDefault();
            var getHODDetails = (from m in db.PR_HOD
                               join n in db.Users on m.HODId equals n.userId
                               where m.PRId == PR.PRId
                               select new UserModel()
                               {
                                   UserId = n.userId,
                                   FullName = n.firstName + " " + n.lastName,
                                   EmailAddress = n.emailAddress
                               }).FirstOrDefault();
            var getReviewerDetails = (from m in db.PR_Reviewer
                                      join n in db.Users on m.reviewerId equals n.userId
                                      where m.PRId == PR.PRId
                                      select new UserModel()
                                      {
                                          UserId = n.userId,
                                          FullName = n.firstName + " " + n.lastName,
                                          EmailAddress = n.emailAddress
                                      }).FirstOrDefault();
            var getHOCDetails = (from m in db.PR_Approver
                                 join n in db.Users on m.approverId equals n.userId
                                 where m.PRId == PR.PRId
                                 select new UserModel()
                                 {
                                     UserId = n.userId,
                                     FullName = n.firstName + " " + n.lastName,
                                     EmailAddress = n.emailAddress
                                 }).FirstOrDefault();
            string NotiMessage = ""; string POMessage = "";
            List<UserModel> sentEmailList = new List<UserModel>();
            sentEmailList.Add(getRequestorDetails);
            sentEmailList.Add(getHODDetails);
            sentEmailList.Add(getReviewerDetails);
            sentEmailList.Add(getHOCDetails);

            if (POFlow == "IssuePO")
            {
                POMessage = "PO issuance";
            } else
            {
                POMessage = "PO Confirmation";
            }

            if (PR.NewPRForm.Scenario == 1)
            {                
                NotiMessage = PR.FullName + " has send email notification for " + POMessage + " for PRNo : " + PR.NewPRForm.PRNo + " to " + getRequestorDetails.FullName + " (Requestor), " + getHODDetails.FullName + " (HOD), " + getReviewerDetails.FullName + " (Reviewer), " + getHOCDetails.FullName + " (Approver).";
                                
            } else if (PR.NewPRForm.Scenario == 2)
            {
                var getRecommenderDetails = (from m in db.PR_Recommender
                                          join n in db.Users on m.recommenderId equals n.userId
                                          where m.PRId == PR.PRId
                                          select new UserModel()
                                          {
                                              UserId = n.userId,
                                              FullName = n.firstName + " " + n.lastName,
                                              EmailAddress = n.emailAddress
                                          }).FirstOrDefault();
                sentEmailList.Add(getRecommenderDetails);
                NotiMessage = PR.FullName + " has send email notification for " + POMessage + " for PRNo : " + PR.NewPRForm.PRNo + " to " + getRequestorDetails.FullName + " (Requestor), " + getHODDetails.FullName + " (HOD), " + getRecommenderDetails.FullName + " (Recommender), " + getReviewerDetails.FullName + " (Reviewer), " + getHOCDetails.FullName + " (Approver).";
            } else
            {
                var getRecommenderDetails = (from m in db.PR_Recommender
                                          join n in db.Users on m.recommenderId equals n.userId
                                          where m.PRId == PR.PRId
                                          select new UserModel()
                                          {
                                              UserId = n.userId,
                                              FullName = n.firstName + " " + n.lastName,
                                              EmailAddress = n.emailAddress
                                          }).FirstOrDefault();
                
                var getRecommenderIIDetails = (from m in db.PR_RecommenderII
                                             join n in db.Users on m.recommenderId equals n.userId
                                             where m.PRId == PR.PRId
                                             select new UserModel()
                                             {
                                                 UserId = n.userId,
                                                 FullName = n.firstName + " " + n.lastName,
                                                 EmailAddress = n.emailAddress
                                             }).FirstOrDefault();

                sentEmailList.Add(getRecommenderDetails);
                sentEmailList.Add(getRecommenderIIDetails);
                NotiMessage = PR.FullName + " has send email notification for " + POMessage + " for PRNo : " + PR.NewPRForm.PRNo + " to " + getRequestorDetails.FullName + " (Requestor), " + getHODDetails.FullName + " (HOD), " + getRecommenderDetails.FullName + " (Recommender), " + getRecommenderIIDetails.FullName + " (RecommenderII), " + getReviewerDetails.FullName + " (Reviewer), " + getHOCDetails.FullName + " (Approver).";
            }

            NotificationMsg _objSendMessage = new NotificationMsg
            {
                uuid = Guid.NewGuid(),
                //POId = PR.poi,
                PRId = PR.PRId,
                msgDate = DateTime.Now,
                fromUserId = PR.UserId,
                message = NotiMessage,
                msgType = "Message"
            };
            db.NotificationMsgs.Add(_objSendMessage);
            db.SaveChanges();

            foreach (var item in sentEmailList)
            {
                var POInfo = (from m in db.PurchaseOrders
                              join n in db.Users on m.PreparedById equals n.userId
                              join o in db.Projects on m.projectId equals o.projectId
                              join p in db.Customers on o.custId equals p.custId
                              //join p in db.OpportunityTypes on m.typeId equals p.typeId
                              where m.PRId == PR.PRId
                              select new MailModel()
                              {
                                  POId = m.POId,
                                  CustName = p.name + " (" + p.abbreviation + ")",
                                  //abb = p.abbreviation,
                                  ProjectName = o.projectName,
                                  //Description = o.des,
                                  //PODescription = m.des,
                                  PONo = m.PONo,
                                  //ContractType = p.type,
                                  AssignTo = n.firstName + " " + n.lastName
                              }).FirstOrDefault();

                string templateFile = System.IO.File.ReadAllText(HttpContext.Server.MapPath("~/Views/Shared/PREmailTemplate.cshtml"));
                POInfo.Content = PR.FullName + " has issue new Purchase Order. Please refer the details as below: ";
                POInfo.FromName = PR.FullName;
                POInfo.BackLink = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority,
                    Url.Content("~/PO/ViewPODetails") + "?POId=" + POInfo.POId + "&POType=Generic");
                var result = Engine.Razor.RunCompile(new LoadedTemplateSource(templateFile), "POTemplateKey", null, POInfo);

                MailMessage mail = new MailMessage
                {
                    From = new MailAddress("hr@kubtel.com"),
                    Subject = "OnlinePR: Updates on " + POInfo.ProjectName + " for " + POInfo.CustName,
                    Body = result,
                    IsBodyHtml = true
                };
                mail.To.Add(new MailAddress(item.EmailAddress));

                SmtpClient smtp = new SmtpClient
                {
                    //smtp.Host = "pops.kub.com";
                    Host = "outlook.office365.com",
                    //smtp.Host = "smtp.gmail.com";
                    Port = 587,
                    //smtp.Port = 25;
                    EnableSsl = true,
                    Credentials = new System.Net.NetworkCredential("hr@kubtel.com", "welcome123$")
                };
                //smtp.Credentials = new System.Net.NetworkCredential("ocm@kubtel.com", "welcome123$");
                smtp.Send(mail);

                NotiGroup _objSendToUserId = new NotiGroup
                {
                    uuid = Guid.NewGuid(),
                    msgId = _objSendMessage.msgId,
                    toUserId = item.UserId
                };
                db.NotiGroups.Add(_objSendToUserId);
            }
            db.SaveChanges();
        }
        //NewPO balnket
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
            } else if (POModel.NewPOForm.SelectSubmit == true)
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
                                         where n.toUserId == POModel.UserId && m.PRId == POModel.PRId
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