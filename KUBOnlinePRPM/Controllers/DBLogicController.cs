using KUBOnlinePRPM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace KUBOnlinePRPM.Controllers
{
    public class DBLogicController : Controller
    {
        private static KUBOnlinePREntities db = new KUBOnlinePREntities();
        public static void PRSaveDbLogic(PRModel model)
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
                Unbudgeted = model.NewPRForm.Value,
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
                StatusId = "PR01"
            };
            db.PurchaseRequisitions.Add(_objNewPR);
            db.SaveChanges();

            PR_HOD _objHOD = new PR_HOD
            {
                uuid = Guid.NewGuid(),
                HODId = model.NewPRForm.HODApproverId,
                PRId = _objNewPR.PRId,
                HODApprovedP1 = model.NewPRForm.HODApproverApprovedP1
            };
            db.PR_HOD.Add(_objHOD);
            db.SaveChanges();

            var getAdminList = (from m in db.Users
                                join n in db.Users_Roles on m.userId equals n.userId
                                where n.roleId == "R03"
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

                if (model.NewPRForm.StatusId == "PR07")
                {
                    var getMsgId = (from m in db.NotificationMsgs
                                    join n in db.NotiGroups on m.msgId equals n.msgId
                                    where m.PRId == model.PRId && m.msgType == "Task" && n.resubmit != null
                                    select new
                                    {
                                        MsgId = m.msgId,
                                        MsgDate = m.msgDate
                                    }).OrderByDescending(m => m.MsgDate).First();
                    NotiGroup UpdateReqTask = db.NotiGroups.First(m => m.msgId == getMsgId.MsgId);
                    UpdateReqTask.resubmit = true;
                    _objNewPR.StatusId = "PR09";
                    db.SaveChanges();
                }
                else
                {
                    NotiGroup ApproverTask = new NotiGroup()
                    {
                        uuid = Guid.NewGuid(),
                        msgId = objTask.msgId,
                        toUserId = getApprover.HODApproverId,
                        resubmit = false
                    };
                    db.NotiGroups.Add(ApproverTask);
                }
            }
            db.SaveChanges();
        }
        public static void PRUpdateDbLogic(PRModel x)
        {
            PurchaseRequisition FormerPRDetails = db.PurchaseRequisitions.First(m => m.PRId == x.PRId);
            Project updateProject = db.Projects.First(m => m.projectId == x.NewPRForm.ProjectId);

            if (x.NewPRForm.StatusId == "PR06")
            {
                IssuePO(x);
                FormerPRDetails.StatusId = "PR14";
                db.SaveChanges();
            } else
            {
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
                if (FormerPRDetails.Unbudgeted != x.NewPRForm.Value)
                {
                    NotificationMsg _objDetails_ProjectId = new NotificationMsg
                    {
                        uuid = Guid.NewGuid(),
                        PRId = x.PRId,
                        msgDate = DateTime.Now,
                        fromUserId = x.UserId,
                        msgType = "Trail",
                        message = x.FullName + " change Budgeted value from " + FormerPRDetails.Unbudgeted + " to " + x.NewPRForm.Value + " for PR No: " + FormerPRDetails.PRNo
                    };
                    db.NotificationMsgs.Add(_objDetails_ProjectId);
                    FormerPRDetails.Unbudgeted = x.NewPRForm.Value;
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
                if (x.NewPRForm.SelectSave == true && (x.NewPRForm.StatusId == "PR01" || x.NewPRForm.StatusId == "PR07"))
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
                else if (x.NewPRForm.SelectSave == true && x.NewPRForm.StatusId == "PR02")
                {
                    PR_Admin SaveAdminInfo = db.PR_Admin.First(m => m.PRId == x.PRId);
                    SaveAdminInfo.adminSaved = SaveAdminInfo.adminSaved + 1;
                    SaveAdminInfo.lastModifyDate = DateTime.Now;
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
                if (x.NewPRForm.SelectSubmit == true && (x.NewPRForm.StatusId == "PR01" || x.NewPRForm.StatusId == "PR07"))
                {
                    FormerPRDetails.Submited = FormerPRDetails.Submited + 1;
                    FormerPRDetails.SubmitDate = DateTime.Now;
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
                        message = FormerPRDetails.PRNo + " pending for your reviewal",
                        fromUserId = x.UserId,
                        msgDate = DateTime.Now,
                        msgType = "Task",
                        PRId = x.PRId
                    };
                    db.NotificationMsgs.Add(objTask);
                    db.SaveChanges();

                    if (x.NewPRForm.StatusId == "PR07")
                    {
                        var getMsgId = (from m in db.NotificationMsgs
                                        join n in db.NotiGroups on m.msgId equals n.msgId
                                        where m.PRId == x.PRId && m.msgType == "Task" && n.resubmit != null
                                        select new
                                        {
                                            MsgId = m.msgId,
                                            MsgDate = m.msgDate
                                        }).OrderByDescending(m => m.MsgDate).First();
                        NotiGroup UpdateReqTask = db.NotiGroups.First(m => m.msgId == getMsgId.MsgId);
                        UpdateReqTask.resubmit = true;
                        db.SaveChanges();
                    }
                    else
                    {
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
                    }
                }
                else if (x.NewPRForm.SelectSubmit == true && x.NewPRForm.StatusId == "PR02")
                {
                    PR_Admin SaveAdminInfo = db.PR_Admin.First(m => m.PRId == x.PRId);
                    SaveAdminInfo.adminSubmited = SaveAdminInfo.adminSubmited + 1;
                    SaveAdminInfo.adminSubmitDate = DateTime.Now;
                    db.SaveChanges();

                    if (FormerPRDetails.AmountRequired < 20000)
                    {
                        var getReviewer = (from m in db.Users
                                           join n in db.Users_Roles on m.userId equals n.userId
                                           where n.roleId == "R02"
                                           select new NewPRModel()
                                           {
                                               ReviewerId = m.userId
                                           }).ToList();
                        NotificationMsg objTask = new NotificationMsg()
                        {
                            uuid = Guid.NewGuid(),
                            message = FormerPRDetails.PRNo + " pending for your reviewal",
                            fromUserId = x.UserId,
                            msgDate = DateTime.Now,
                            msgType = "Task",
                            PRId = FormerPRDetails.PRId
                        };
                        db.NotificationMsgs.Add(objTask);
                        FormerPRDetails.StatusId = "PR03";
                        db.SaveChanges();

                        foreach (var item in getReviewer)
                        {
                            NotiGroup Task = new NotiGroup()
                            {
                                uuid = Guid.NewGuid(),
                                msgId = objTask.msgId,
                                toUserId = item.ReviewerId
                            };
                            db.NotiGroups.Add(Task);

                            PR_Reviewer saveReviewer = new PR_Reviewer()
                            {
                                uuid = Guid.NewGuid(),
                                reviewerId = item.ReviewerId,
                                PRId = FormerPRDetails.PRId
                            };
                            db.PR_Reviewer.Add(saveReviewer);
                            db.SaveChanges();
                        }
                    }
                    else if (FormerPRDetails.AmountRequired > 20000)
                    {
                        var getRecommender = (from m in db.Users
                                              join n in db.Users_Roles on m.userId equals n.userId
                                              where n.roleId == "R03"
                                              select new NewPRModel()
                                              {
                                                  ReviewerId = m.userId
                                              }).ToList();
                        NotificationMsg objTask = new NotificationMsg()
                        {
                            uuid = Guid.NewGuid(),
                            message = FormerPRDetails.PRNo + " pending for your recommendal",
                            fromUserId = x.UserId,
                            msgDate = DateTime.Now,
                            msgType = "Task",
                            PRId = FormerPRDetails.PRId
                        };
                        db.NotificationMsgs.Add(objTask);
                        FormerPRDetails.StatusId = "PR12";
                        db.SaveChanges();

                        foreach (var item in getRecommender)
                        {
                            NotiGroup Task = new NotiGroup()
                            {
                                uuid = Guid.NewGuid(),
                                msgId = objTask.msgId,
                                toUserId = item.RecommenderId
                            };
                            db.NotiGroups.Add(Task);

                            PR_Recommender saveRecommender = new PR_Recommender()
                            {
                                uuid = Guid.NewGuid(),
                                recommenderId = item.RecommenderId,
                                PRId = FormerPRDetails.PRId
                            };
                            db.PR_Recommender.Add(saveRecommender);
                            db.SaveChanges();
                        }
                    }

                    var requestorDone = (from m in db.NotificationMsgs
                                         join n in db.NotiGroups on m.msgId equals n.msgId
                                         where n.toUserId == x.UserId && m.PRId == x.PRId
                                         select new PRModel()
                                         {
                                             MsgId = m.msgId
                                         }).First();

                    NotificationMsg getDone = db.NotificationMsgs.First(m => m.msgId == requestorDone.MsgId);
                    getDone.done = true;
                    db.SaveChanges();

                    NotificationMsg _objSubmited = new NotificationMsg
                    {
                        uuid = Guid.NewGuid(),
                        PRId = x.PRId,
                        msgDate = DateTime.Now,
                        fromUserId = x.UserId,
                        msgType = "Trail",
                        message = x.FullName + " has submit PR procurement for PR No. " + FormerPRDetails.PRNo
                    };
                    db.NotificationMsgs.Add(_objSubmited);
                }
                db.SaveChanges();

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
                        if (objPRItemDetails.codeId != value.CodeId)
                        {
                            ItemCode formerItemCode = db.ItemCodes.FirstOrDefault(m => m.codeId == objPRItemDetails.codeId);
                            ItemCode itemCodeChanges = db.ItemCodes.First(m => m.codeId == value.CodeId);

                            NotificationMsg _objDetails_CodeId = new NotificationMsg();
                            _objDetails_CodeId.uuid = Guid.NewGuid();
                            _objDetails_CodeId.PRId = x.PRId;
                            _objDetails_CodeId.msgDate = DateTime.Now;
                            _objDetails_CodeId.fromUserId = x.UserId;
                            _objDetails_CodeId.msgType = "Trail";
                            if (formerItemCode == null)
                            {
                                _objDetails_CodeId.message = x.FullName + " set ItemCode to " + itemCodeChanges.ItemCode1 + " in PRNo : " + FormerPRDetails.PRNo;
                            }
                            else
                            {
                                _objDetails_CodeId.message = x.FullName + " change ItemCode in PRNo : " + FormerPRDetails.PRNo + " items from " + formerItemCode.ItemCode1 + " to " + itemCodeChanges.ItemCode1;
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
        }
        public static void IssuePO(PRModel x)
        {
            PurchaseRequisition objPRDetails = db.PurchaseRequisitions.First(m => m.PRId == x.PRId);

            PurchaseOrder newPO = new PurchaseOrder()
            {
                uuid = Guid.NewGuid(),
                PRId = x.PRId,
                PONo = "dummyset",
                PODate = DateTime.Now,
                projectId = x.NewPRForm.ProjectId,
                vendorId = x.NewPRForm.VendorId.Value,
                vendorStaffId = x.NewPRForm.VendorStaffId,
                StatusId = "PO01",
                POAging = 0
            };
            db.PurchaseOrders.Add(newPO);
            db.SaveChanges();
            newPO.PONo = "PO-" + DateTime.Now.Year + "-" + string.Format("{0}{1}", 0, newPO.POId.ToString("D4"));
            db.SaveChanges();

            NotificationMsg _objNewPO = new NotificationMsg
            {
                uuid = Guid.NewGuid(),
                PRId = x.PRId,
                POId = newPO.POId,
                msgDate = DateTime.Now,
                fromUserId = x.UserId,
                msgType = "Trail",
                message = x.FullName + " has create new PO No. " + newPO.PONo
            };
            db.NotificationMsgs.Add(_objNewPO);

            foreach (var value in x.NewPRForm.PRItemListObject)
            {
                PO_Item newPOItem = new PO_Item()
                {
                    uuid = Guid.NewGuid(),
                    POId = newPO.POId,
                    itemsId = value.ItemsId,
                    dateRequired = value.DateRequired,
                    description = value.Description,
                    codeId = value.CodeId.Value,
                    custPONo = value.CustPONo,
                    quantity = value.Quantity,
                    unitPrice = value.UnitPrice.Value,
                    totalPrice = value.TotalPrice.Value
                };
                db.PO_Item.Add(newPOItem);
                db.SaveChanges();

                NotificationMsg newPOMsg = new NotificationMsg()
                {
                    uuid = Guid.NewGuid(),
                    PRId = x.PRId,
                    POId = newPOItem.POId,
                    msgDate = DateTime.Now,
                    fromUserId = x.UserId,
                    msgType = "Trail",
                    message = "The system create new PO item from PR No. " + objPRDetails.PRNo
                };
                db.NotificationMsgs.Add(newPOMsg);
            }
            //objPRDetails.AmountPOBalance = 0;
            //updateProject.utilizedToDate = updateProject.utilizedToDate + objPRDetails.AmountRequired;
            //updateProject.budgetBalance = updateProject.budgetedAmount - updateProject.utilizedToDate;

            db.SaveChanges();
        }
        public static void POSaveDbLogic(POModel POModel)
        {
            PurchaseOrder newPO = new PurchaseOrder();
            newPO.uuid = Guid.NewGuid();
            newPO.PRId = POModel.PRId;
            newPO.PONo = "dummyset";
            newPO.PODate = DateTime.Now;
            newPO.projectId = POModel.NewPOForm.ProjectId;
            newPO.vendorId = POModel.NewPOForm.VendorId.Value;
            newPO.vendorStaffId = POModel.NewPOForm.VendorStaffId;
            newPO.PreparedById = POModel.UserId;
            newPO.PreparedDate = DateTime.Now;
            newPO.Saved = POModel.NewPOForm.Saved + 1;
            newPO.Submited = POModel.NewPOForm.Submited;
            db.PurchaseOrders.Add(newPO);
            db.SaveChanges();
            newPO.PONo = "PO-" + DateTime.Now.Year + "-" + string.Format("{0}{1}", 0, newPO.POId.ToString("D4"));

            db.SaveChanges();

            foreach (var value in POModel.NewPOForm.POItemListObject)
            {
                PO_Item _objNewPOItem = new PO_Item
                {
                    uuid = Guid.NewGuid(),
                    POId = newPO.POId,
                    itemsId = value.ItemsId,
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
                }
            }
            POModel.POId = newPO.POId;
            //if (PRType == "Generic")
            //{
            //    PurchaseOrder newPO = new PurchaseOrder()
            //    {
            //        uuid = Guid.NewGuid(),
            //        PRId = PRId,
            //        PONo = "dummyset",
            //        PODate = DateTime.Now,
            //        projectId = objPRDetails.ProjectId,
            //        vendorId = objPRDetails.VendorId.Value,
            //        vendorStaffId = objPRDetails.VendorStaffId,
            //        POAging = 0
            //    };
            //    db.PurchaseOrders.Add(newPO);
            //    db.SaveChanges();
            //    newPO.PONo = "PO-" + DateTime.Now.Year + "-" + string.Format("{0}{1}", 0, newPO.POId.ToString("D4"));
            //    db.SaveChanges();

            //    NotificationMsg _objNewPO = new NotificationMsg
            //    {
            //        uuid = Guid.NewGuid(),
            //        PRId = PRId,
            //        POId = newPO.POId,
            //        msgDate = DateTime.Now,
            //        fromUserId = Int32.Parse(Session["UserId"].ToString()),
            //        msgType = "Trail",
            //        message = Session["FullName"].ToString() + " has create new PO No. " + newPO.PONo
            //    };
            //    db.NotificationMsgs.Add(_objNewPO);

            //    foreach (var value in objPRItemList)
            //    {
            //        PO_Item newPOItem = new PO_Item()
            //        {
            //            uuid = Guid.NewGuid(),
            //            POId = newPO.POId,
            //            itemsId = value.itemsId,
            //            dateRequired = value.dateRequired,
            //            description = value.description,
            //            codeId = value.codeId.Value,
            //            custPONo = value.custPONo,
            //            quantity = value.quantity,
            //            unitPrice = value.unitPrice.Value,
            //            totalPrice = value.totalPrice.Value
            //        };
            //        db.PO_Item.Add(newPOItem);
            //        db.SaveChanges();

            //        NotificationMsg newPOMsg = new NotificationMsg()
            //        {
            //            uuid = Guid.NewGuid(),
            //            PRId = PRId,
            //            POId = newPOItem.POId,
            //            msgDate = DateTime.Now,
            //            fromUserId = Int32.Parse(Session["UserId"].ToString()),
            //            msgType = "Trail",
            //            message = "The system create new PO item from PR No. " + objPRDetails.PRNo
            //        };
            //        db.NotificationMsgs.Add(newPOMsg);
            //    }
            //    objPRDetails.AmountPOBalance = 0;
            //    updateProject.utilizedToDate = updateProject.utilizedToDate + objPRDetails.AmountRequired;
            //    updateProject.budgetBalance = updateProject.budgetedAmount - updateProject.utilizedToDate;

            //    db.SaveChanges();
            //}
        }
        public static void POUpdateDbLogic(POModel x)
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
            foreach (var value in x.NewPOForm.POItemListObject)
            {
                PO_Item objPOItemDetails = db.PO_Item.FirstOrDefault(m => m.itemsId == value.ItemsId && m.POId == x.POId);
                if (objPOItemDetails == null)
                {
                    PO_Item _objNewPOItem = new PO_Item
                    {
                        uuid = Guid.NewGuid(),
                        itemsId = value.ItemsId,
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
                }
                db.SaveChanges();

                if (x.NewPOForm.SelectSubmit == true)
                {
                    PR_Items _objUpdatePRItem = db.PR_Items.First(m => m.itemsId == value.ItemsId);
                    _objUpdatePRItem.outStandingQuantity = _objUpdatePRItem.outStandingQuantity - objPOItemDetails.quantity;
                    db.SaveChanges();
                }
                db.SaveChanges();
            }
        }
    }
}