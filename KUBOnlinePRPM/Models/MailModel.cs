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
        public int PRId { get; set; }
        public int POId { get; set; }
        public string PONo { get; set; }
        public DateTime PRDate { get; set; }
        public DateTime PODate { get; set; }
        public string PRNo { get; set; }
        public string PRFlow { get; set; }
        public string Messages { get; set; }
        public string PODescription { get; set; }
        public string VendorCompany { get; set; }
        public decimal AmountRequired { get; set; }
        public string CustAbbreviation { get; set; }
        public string Requestor { get; set; }
        public string FromName { get; set; }
        public string Content { get; set; }
        public string BackLink { get; set; }
    }
}