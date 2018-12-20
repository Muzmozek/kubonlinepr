using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KUBOnlinePRPM.Models
{
    public class MailModel
    {
        public string CustName { get; set; }
        public string ProjectName { get; set; }
        public int POId { get; set; }
        public string PONo { get; set; }
        public string PRNo { get; set; }
        public string POFlow { get; set; }
        public string IOFlow { get; set; }
        public string PRFlow { get; set; }
        public string DOFlow { get; set; }
        public string Description { get; set; }
        public string PODescription { get; set; }
        public string CustAbbreviation { get; set; }
        public string AssignTo { get; set; }
        public string FromName { get; set; }
        public string Content { get; set; }
        public string BackLink { get; set; }
    }
}