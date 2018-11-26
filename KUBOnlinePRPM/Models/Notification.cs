using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KUBOnlinePRPM.Models
{
    public class NotiModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public List<RoleList> RoleIdList { get; set; }
        public string Type { get; set; }
        public int CustId { get; set; }
        public int PRId { get; set; }
        public List<NotiListTable> NotiListObject { get; set; }
        public List<QuestionAnswerModels> MessageListModel { get; set; }
    }
    public class NotiListTable
    {
        public int MsgId { get; set; }
        public string Message { get; set; }
        public int? PRId { get; set; }
        public int? POId { get; set; }
        public int? FromUserId { get; set; }
        public string FromFullName { get; set; }
        public int? ToUserId { get; set; }
        public string ToFullName { get; set; }
        public DateTime MsgDate { get; set; }
        public string PRType { get; set; }
        public bool? Done { get; set; }
    }
    public class QuestionAnswerModels
    {
        public int PRId { get; set; }
        public string PRNo { get; set; }
        public string FromUserName { get; set; }
        public DateTime? MsgDate { get; set; }
        //public int? FromUserId { get; set; }            
        //public int ToUserID { get; set; }
        public string ToUserName { get; set; }
        public string Message { get; set; }
        public string MsgType { get; set; }
    }
    public class NotiMemberList
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}