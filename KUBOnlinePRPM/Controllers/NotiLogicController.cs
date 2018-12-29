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
    public class NotiLogicController : Controller
    {
        private static KUBOnlinePREntities db = new KUBOnlinePREntities();
        protected void SendEmailPRNotification(PRModel PR, string PRFlow)
        {
            var myEmail = db.Users.SingleOrDefault(x => x.userId == PR.UserId).emailAddress;
            var getReceipientDetails = new UserModel()
            {
                UserId = PR.NewPRForm.ApproverId,
                FullName = PR.NewPRForm.ApproverName,
                EmailAddress = PR.EmailAddress
            };
            string NotiMessage = "";

            if (PRFlow == "ApprovalInitial")
            {
                NotiMessage = PR.FullName + " has send email notification for initial approval for PRNo : " + PR.NewPRForm.PRNo + " to " + getReceipientDetails.FullName + " (HOD).";
            }
            else if(PRFlow == "ToHOGPSSNoti")
            {
                NotiMessage = PR.FullName + " has send email notification for paper approval for PRNo : " + PR.NewPRForm.PRNo + " to " + getReceipientDetails.FullName + " (HODGPSS).";
            }
            else if (PRFlow == "ToApproverNoti")
            {
                NotiMessage = PR.FullName + " has send email notification for PR approval for PRNo : " + PR.NewPRForm.PRNo + " to " + getReceipientDetails.FullName;
            }
            else if (PRFlow == "ToProcurementNoti")
            {
                NotiMessage = PR.FullName + " has send email notification for PRNo : " + PR.NewPRForm.PRNo + " to " + getReceipientDetails.FullName + " (Procurement)";
            }
            else if (PRFlow == "ToRecommenderNoti")
            {
                NotiMessage = PR.FullName + " has send email notification for PR recommendal for PRNo : " + PR.NewPRForm.PRNo + " to " + getReceipientDetails.FullName;
            }
            else if (PRFlow == "ToRecommenderIINoti")
            {
                NotiMessage = PR.FullName + " has send email notification for PR joint recommendal for PRNo : " + PR.NewPRForm.PRNo + " to " + getReceipientDetails.FullName;
            }
            else if (PRFlow == "ToReviewerNoti")
            {
                NotiMessage = PR.FullName + " has send email notification for PR reviewal for PRNo : " + PR.NewPRForm.PRNo + " to " + getReceipientDetails.FullName;
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

            var PRInfo = (from m in db.PurchaseRequisitions
                          join n in db.Users on m.PreparedById equals n.userId
                          join o in db.Projects on m.ProjectId equals o.projectId
                          join p in db.Customers on o.custId equals p.custId
                          join q in db.Vendors on m.VendorId equals q.vendorId into r
                          from s in r.DefaultIfEmpty()
                          //join p in db.OpportunityTypes on m.typeId equals p.typeId
                          where m.PRId == PR.PRId
                          select new MailModel()
                          {
                              CustName = p.name + " (" + p.abbreviation + ")",
                              //abb = p.abbreviation,
                              ProjectName = o.projectName,
                              //Description = o.des,
                              //PODescription = m.des,
                              PRNo = m.PRNo,
                              PRDate = m.PreparedDate,
                              VendorCompany = s.name,
                              AmountRequired = m.AmountRequired,
                              //ContractType = p.type,
                              Requestor = n.firstName + " " + n.lastName
                          }).FirstOrDefault();

            string templateFile = System.IO.File.ReadAllText(HttpContext.Server.MapPath("~/Views/Shared/PREmailTemplate.cshtml"));
            string Username = db.Users.First(m => m.userId == getReceipientDetails.UserId).userName;
            PRInfo.Content = "~/Home/Index?Username=" + Username + "&PRId=" + PR.PRId + "&PRType=Generic";
            //PRInfo.FromName = PR.FullName;
            PRInfo.BackLink = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority,
                Url.Content(PRInfo.Content));
            var result = Engine.Razor.RunCompile(new LoadedTemplateSource(templateFile), "PRTemplateKey", null, PRInfo);

            MailMessage mail = new MailMessage
            {
                From = new MailAddress("info@kubtel.com"),
                Subject = "[PR Online]  A new PR to be approved - " + PRInfo.PRNo,
                Body = result,
                IsBodyHtml = true
            };
            if (getReceipientDetails.EmailAddress != null)
            {
                mail.To.Add(new MailAddress(getReceipientDetails.EmailAddress));
            } else
            {
                mail.To.Add(new MailAddress("muzhafar@kubtel.com"));
            }
            
            SmtpClient smtp = new SmtpClient
            {
                //smtp.Host = "pops.kub.com";
                Host = "outlook.office365.com",
                //smtp.Host = "smtp.gmail.com";
                Port = 587,
                //smtp.Port = 25;
                EnableSsl = true,
                Credentials = new System.Net.NetworkCredential("info@kubtel.com", "welcome123$")
            };
            //smtp.Credentials = new System.Net.NetworkCredential("ocm@kubtel.com", "welcome123$");
            smtp.Send(mail);

            NotiGroup _objSendToUserId = new NotiGroup
            {
                uuid = Guid.NewGuid(),
                msgId = _objSendMessage.msgId,
                toUserId = getReceipientDetails.UserId
            };
            db.NotiGroups.Add(_objSendToUserId);
            db.SaveChanges();
        }
        protected void SendEmailPONotification(PRModel PR, string POFlow)
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
            }
            else
            {
                POMessage = "PO Confirmation";
            }

            if (PR.NewPRForm.Scenario == 1)
            {
                NotiMessage = PR.FullName + " has send email notification for " + POMessage + " for PRNo : " + PR.NewPRForm.PRNo + " to " + getRequestorDetails.FullName + " (Requestor), " + getHODDetails.FullName + " (HOD), " + getReviewerDetails.FullName + " (Reviewer), " + getHOCDetails.FullName + " (Approver).";

            }
            else if (PR.NewPRForm.Scenario == 2)
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
            }
            else
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
                              join q in db.Vendors on m.vendorId equals q.vendorId
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
                                  PODate = m.PODate,
                                  VendorCompany = q.name,
                                  AmountRequired = m.TotalPrice,
                                  //ContractType = p.type,
                                  Requestor = n.firstName + " " + n.lastName
                              }).FirstOrDefault();

                string templateFile = System.IO.File.ReadAllText(HttpContext.Server.MapPath("~/Views/Shared/POEmailTemplate.cshtml"));
                string Username = db.Users.First(m => m.userId == item.UserId).userName;
                POInfo.Content = "~/Home/Index?Username=" + Username + "&POId=" + POInfo.POId + "&PRType=Generic";
                POInfo.FromName = PR.FullName;
                POInfo.BackLink = string.Format("{0}://{1}{2}", Request.Url.Scheme, Request.Url.Authority,
                    Url.Content(POInfo.Content));
                var result = Engine.Razor.RunCompile(new LoadedTemplateSource(templateFile), "POTemplateKey", null, POInfo);

                MailMessage mail = new MailMessage
                {
                    From = new MailAddress("info@kubtel.com"),
                    Subject = "[PR Online] " + POMessage + " for - " + POInfo.PONo,
                    Body = result,
                    IsBodyHtml = true
                };

                if (item.EmailAddress != null)
                {
                    mail.To.Add(new MailAddress(item.EmailAddress));
                }
                else
                {
                    mail.To.Add(new MailAddress("muzhafar@kubtel.com"));
                }

                SmtpClient smtp = new SmtpClient
                {
                    //smtp.Host = "pops.kub.com";
                    Host = "outlook.office365.com",
                    //smtp.Host = "smtp.gmail.com";
                    Port = 587,
                    //smtp.Port = 25;
                    EnableSsl = true,
                    Credentials = new System.Net.NetworkCredential("info@kubtel.com", "welcome123$")
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
    }
}