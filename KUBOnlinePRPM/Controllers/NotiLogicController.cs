using KUBOnlinePRPM.Models;
using RazorEngine;
using RazorEngine.Templating;
using System.Net.Mail;
using System;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
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
            var myEmail = db.Users.Select(x =>  new { Email = x.emailAddress, UserId = x.userId }).Where(x => x.UserId == PR.UserId).FirstOrDefault();
            var getReceipientDetails = new UserModel()
            {
                UserId = PR.NewPRForm.ApproverId,
                FullName = PR.NewPRForm.ApproverName,
                EmailAddress = PR.EmailAddress
            };
            string NotiMessage = "";

            if (PRFlow == "ApprovalInitial")
            {
                NotiMessage = PR.FullName + " has send email notification for initial approval for PRNo : " + PR.NewPRForm.PRNo + " to " + getReceipientDetails.FullName;
            }
            else if (PRFlow == "ReviewalInitial")
            {
                NotiMessage = PR.FullName + " has send email notification for PR reviewal for PRNo : " + PR.NewPRForm.PRNo + " to " + getReceipientDetails.FullName;
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
                //From = new MailAddress("pr@kub.com"),
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
                //mail.To.Add(new MailAddress("syafiq.khairil@kub.com"));
            }
            
            SmtpClient smtp = new SmtpClient
            {
                //Host = "relay.kub.com",
                //Host = "mail.kub.local",
                //Host = "mail.kub.com",
                Host = "outlook.office365.com",
                //Host = "smtp.gmail.com";
                Port = 587,
                //DeliveryMethod = SmtpDeliveryMethod.Network,
                //Port = 110,
                //Port = 25,
                EnableSsl = true,
                Credentials = new System.Net.NetworkCredential("info@kubtel.com", "welcome123$")
                //Credentials = new System.Net.NetworkCredential("pr@kub.com", "welcome123$")
            };
            if (ConfigurationManager.AppSettings["SentEmail"] == "true")
            {
                smtp.Send(mail);
            }

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
            var myEmail = db.Users.Select(x => new { Email = x.emailAddress, UserId = x.userId }).Where(x => x.UserId == PR.UserId).FirstOrDefault();
            var getRequestorDetails = (from m in db.PurchaseRequisitions
                                       join n in db.Users on m.PreparedById equals n.userId
                                       where m.PRId == PR.PRId
                                       select new UserModel()
                                       {
                                           UserId = n.userId,
                                           FullName = n.firstName + " " + n.lastName,
                                           EmailAddress = n.emailAddress,
                                           ChildCompanyId = n.childCompanyId
                                       }).FirstOrDefault();
            var getHODDetails = new UserModel();
            if (PR.CustId == 2)
            {
                getHODDetails = (from m in db.PR_HOD
                                 join n in db.Users on m.HODId equals n.userId
                                 join o in db.ChildCustomers on n.childCompanyId equals o.childCustId
                                 where m.PRId == PR.PRId && o.childCustId == getRequestorDetails.ChildCompanyId
                                 select new UserModel()
                                 {
                                     UserId = n.userId,
                                     FullName = n.firstName + " " + n.lastName,
                                     EmailAddress = n.emailAddress
                                 }).FirstOrDefault();
            } else if (PR.CustId == 3 && getRequestorDetails.ChildCompanyId == 16) {
                getHODDetails = (from m in db.PR_HOD
                                 join n in db.Users on m.HODId equals n.userId
                                 where m.PRId == PR.PRId
                                 select new UserModel()
                                 {
                                     UserId = n.userId,
                                     FullName = n.firstName + " " + n.lastName,
                                     EmailAddress = n.emailAddress
                                 }).FirstOrDefault();
            }
            else
            {
                getHODDetails = (from m in db.PR_HOD
                                 join n in db.Users on m.HODId equals n.userId
                                 join o in db.Users on n.userId equals o.superiorId
                                 where m.PRId == PR.PRId && o.userId == getRequestorDetails.UserId
                                 select new UserModel()
                                 {
                                     UserId = n.userId,
                                     FullName = n.firstName + " " + n.lastName,
                                     EmailAddress = n.emailAddress
                                 }).FirstOrDefault();
            }
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
            if (getReviewerDetails != null)
            {
                sentEmailList.Add(getReviewerDetails);
            }
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
            {   if (getReviewerDetails != null)
                {
                    NotiMessage = PR.FullName + " has send email notification for " + POMessage + " for PRNo : " + PR.NewPRForm.PRNo + " to " + getRequestorDetails.FullName + " (Requestor), " + getHODDetails.FullName + " , " + getReviewerDetails.FullName + " (Reviewer), " + getHOCDetails.FullName + " (Approver).";
                } else
                {
                    NotiMessage = PR.FullName + " has send email notification for " + POMessage + " for PRNo : " + PR.NewPRForm.PRNo + " to " + getRequestorDetails.FullName + " (Requestor), " + getHODDetails.FullName + " , " + getHOCDetails.FullName + " (Approver).";
                }
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
                if (getRecommenderDetails != null && getReviewerDetails != null)
                {
                    sentEmailList.Add(getRecommenderDetails);
                    NotiMessage = PR.FullName + " has send email notification for " + POMessage + " for PRNo : " + PR.NewPRForm.PRNo + " to " + getRequestorDetails.FullName + " (Requestor), " + getHODDetails.FullName + " (HOD), " + getRecommenderDetails.FullName + " (Recommender), " + getReviewerDetails.FullName + " (Reviewer), " + getHOCDetails.FullName + " (Approver).";
                }
                else if (getReviewerDetails != null)
                {
                    NotiMessage = PR.FullName + " has send email notification for " + POMessage + " for PRNo : " + PR.NewPRForm.PRNo + " to " + getRequestorDetails.FullName + " (Requestor), " + getHODDetails.FullName + " (HOD), " + getReviewerDetails.FullName + " (Reviewer), " + getHOCDetails.FullName + " (Approver).";
                }
                else
                {
                    NotiMessage = PR.FullName + " has send email notification for " + POMessage + " for PRNo : " + PR.NewPRForm.PRNo + " to " + getRequestorDetails.FullName + " (Requestor), " + getHODDetails.FullName + " (HOD), " + getHOCDetails.FullName + " (Approver).";
                }
                
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
                var getRecommenderIIDetails = new UserModel();
                if (PR.CustId == 1 || (PR.CustId == 3 && getRequestorDetails.ChildCompanyId != 16) || PR.CustId == 5)
                {
                    getRecommenderIIDetails = (from m in db.PR_RecommenderHOC
                                                    join n in db.Users on m.recommenderId equals n.userId
                                                    where m.PRId == PR.PRId
                                                    select new UserModel()
                                                    {
                                                        UserId = n.userId,
                                                        FullName = n.firstName + " " + n.lastName,
                                                        EmailAddress = n.emailAddress
                                                    }).FirstOrDefault();
                    sentEmailList.Add(getRecommenderIIDetails);
                } 

                var getRecommenderIIIDetails = (from m in db.PR_RecommenderCFO
                                                join n in db.Users on m.recommenderId equals n.userId
                                               where m.PRId == PR.PRId
                                               select new UserModel()
                                               {
                                                   UserId = n.userId,
                                                   FullName = n.firstName + " " + n.lastName,
                                                   EmailAddress = n.emailAddress
                                               }).FirstOrDefault();

                if (getRecommenderDetails != null)
                {
                    sentEmailList.Add(getRecommenderDetails);
                }
                sentEmailList.Add(getRecommenderIIIDetails);

                if (PR.CustId == 1 || (PR.CustId == 3 && getRequestorDetails.ChildCompanyId != 16) || PR.CustId == 5)
                {
                    if (getRecommenderDetails != null && getReviewerDetails != null)
                    {
                        NotiMessage = PR.FullName + " has send email notification for " + POMessage + " for PRNo : " + PR.NewPRForm.PRNo + " to " + getRequestorDetails.FullName + " (Requestor), " + getHODDetails.FullName + " , " + getRecommenderDetails.FullName + " (Recommender), " + getRecommenderIIDetails.FullName + " , " + getRecommenderIIIDetails.FullName + " (CFO), " + getReviewerDetails.FullName + " (Reviewer), " + getHOCDetails.FullName + " (Approver).";
                    }
                    else if (getReviewerDetails != null)
                    {
                        NotiMessage = PR.FullName + " has send email notification for " + POMessage + " for PRNo : " + PR.NewPRForm.PRNo + " to " + getRequestorDetails.FullName + " (Requestor), " + getHODDetails.FullName + " , " + getRecommenderIIDetails.FullName + " , " + getRecommenderIIIDetails.FullName + " (CFO), " + getReviewerDetails.FullName + " (Reviewer), " + getHOCDetails.FullName + " (Approver).";
                    } else
                    {
                        NotiMessage = PR.FullName + " has send email notification for " + POMessage + " for PRNo : " + PR.NewPRForm.PRNo + " to " + getRequestorDetails.FullName + " (Requestor), " + getHODDetails.FullName + " , " + getRecommenderIIDetails.FullName + " , " + getRecommenderIIIDetails.FullName + " (CFO), " + getHOCDetails.FullName + " (Approver).";
                    }
                        
                } else {
                    if (getRecommenderDetails != null && getReviewerDetails != null)
                    {
                        NotiMessage = PR.FullName + " has send email notification for " + POMessage + " for PRNo : " + PR.NewPRForm.PRNo + " to " + getRequestorDetails.FullName + " (Requestor), " + getHODDetails.FullName + " , " + getRecommenderDetails.FullName + " (Recommender), " + getRecommenderIIIDetails.FullName + " (CFO), " + getReviewerDetails.FullName + " (Reviewer), " + getHOCDetails.FullName + " (Approver).";
                    }
                    else if (getReviewerDetails != null)
                    {
                        NotiMessage = PR.FullName + " has send email notification for " + POMessage + " for PRNo : " + PR.NewPRForm.PRNo + " to " + getRequestorDetails.FullName + " (Requestor), " + getHODDetails.FullName + " , " + getRecommenderIIIDetails.FullName + " (CFO), " + getReviewerDetails.FullName + " (Reviewer), " + getHOCDetails.FullName + " (Approver).";
                    } else
                    {
                        NotiMessage = PR.FullName + " has send email notification for " + POMessage + " for PRNo : " + PR.NewPRForm.PRNo + " to " + getRequestorDetails.FullName + " (Requestor), " + getHODDetails.FullName + " , " + getRecommenderIIIDetails.FullName + " (CFO), " + getHOCDetails.FullName + " (Approver).";
                    }                     
                }               
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
                    //From = new MailAddress("pr-admin@kub.com"),
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
                    //mail.To.Add(new MailAddress("syafiq.khairil@kub.com")); 
                }

                SmtpClient smtp = new SmtpClient
                {
                    //Host = "relay.kub.com",
                    //Host = "mail.kub.local",
                    Host = "outlook.office365.com",
                    //Host = "smtp.gmail.com";
                    Port = 587,
                    //Port = 110,
                    //smtp.Port = 25;
                    EnableSsl = true,
                    Credentials = new System.Net.NetworkCredential("info@kubtel.com", "welcome123$")
                    //Credentials = new System.Net.NetworkCredential("pr-admin@kub.com", "welcome123$")
                };
                if (ConfigurationManager.AppSettings["SentEmail"] == "true")
                {
                    smtp.Send(mail);
                }

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