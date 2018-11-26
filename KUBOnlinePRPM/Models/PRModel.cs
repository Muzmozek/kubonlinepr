using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Web;

namespace KUBOnlinePRPM.Models
{
    public class PRModel
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public List<RoleList> RoleIdList { get; set; }
        public string Type { get; set; }
        public int CustId { get; set; }
        public int PRId { get; set; }
        public int MsgId { get; set; }
        public bool Done { get; set; }
        public NewPRModel NewPRForm { get; set; }
        public HttpPostedFileBase PaperRefNoFile { get; set; }
        public string PaperRefNoFileName { get; set; }
        public HttpPostedFileBase BidWaiverRefNoFile { get; set; }
        public string BidWaiverRefNoFileName { get; set; }
        public List<NotiListTable> NotiListObject { get; set; }
        public List<PRListTable> PRListObject { get; set; }
        public List<PRDocListTable> PRDocListObject { get; set; }
    }
    public class NewPRModel
    {
        public string PRNo { get; set; }

        [Required]
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }

        [Required]
        public bool Value { get; set; }
        public bool Unbudgeted { get; set; }
        public string PaperRefNo { get; set; }
        public string BidWaiverRefNo { get; set; }
        public bool PaperAttachment { get; set; }
        public bool PaperVerified { get; set; }

        [Required]
        public int PurchaseTypeId { get; set; }
        public string PurchaseType { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "* Must be at least {2} characters long and {1} characters max.", MinimumLength = 3)]
        public string BudgetDescription { get; set; }
        public decimal AmountRequired { get; set; }

        [Required]
        public string Justification { get; set; }

        [Required]
        [Range(typeof(decimal), "0", "999999999999999999.99", ErrorMessage = "* Must be at least {2} integer long and {1} integer max.")]
        public decimal BudgetedAmount { get; set; }

        [Required]
        [Range(typeof(decimal), "0", "999999999999999999.99", ErrorMessage = "* Must be at least {2} integer long and {1} integer max.")]
        public decimal UtilizedToDate { get; set; }
        public decimal BudgetBalance { get; set; }
        public int? VendorId { get; set; }
        public int? VendorStaffId { get; set; }
        public string CompanyName { get; set; }
        public string VendorName { get; set; }
        public string VendorEmail { get; set; }
        public string VendorContactName { get; set; }
        public string VendorContactNo { get; set; }
        public string VendorQuoteNo { get; set; }
        public DateTime PreparedDate { get; set; }
        public int PreparedById { get; set; }
        public int HODApproverId { get; set; }
        public string HODApproverName { get; set; }
        public DateTime HODApprovedDate1 { get; set; }
        public DateTime HODApprovedDate2 { get; set; }
        public int ApproverId { get; set; }
        public string ApproverName { get; set; }
        public DateTime ApprovedDate { get; set; }
        public int ReviewerId { get; set; }
        public string ReviewerName { get; set; }
        public DateTime ReviewedDate { get; set; }
        public int RecommenderId { get; set; }
        public string RecommenderName { get; set; }
        public DateTime RecommendDate { get; set; }
        public int AdminId { get; set; }
        public string AdminName { get; set; }
        public bool SelectSave { get; set; }
        public int Saved { get; set; }
        public bool SelectSubmit { get; set; }
        public int Submited { get; set; }
        public int Rejected { get; set; }
        public int Reviewed { get; set; }
        public int HODApproverApprovedP1 { get; set; }
        public int HODApproverApprovedP2 { get; set; }
        public int ApproverApproved { get; set; }
        public int AdminApproved { get; set; }
        public string StatusId { get; set; }
        public string PRStatus { get; set; }
        public DateTime LastModifyDate { get; set; }
        public DateTime SubmitDate { get; set; }
        public DateTime AdminApprovedDate { get; set; }
        public int PRAging { get; set; }
        public string RequestorName { get; set; }
        public string Designation { get; set; }
        public int? AmountPOBalance { get; set; }
        public List<PRItemsTable> PRItemListObject { get; set; }
        public int? Scenario { get; set; }
    }
    public class PRDocListTable
    {
        public int fileEntryId { get; set; }
        public string uploadUserName { get; set; }
        public DateTime uploadDate { get; set; }
        public string FileName { get; set; }
        public byte[] File { get; set; }
        public string Extension { get; set; }
    }
    public class PRListTable
    {
        public int PRId { get; set; }
        public string PRNo { get; set; }
        public DateTime PRDate { get; set; }
        public string RequestorName { get; set; }
        public string VendorCompany { get; set; }
        public decimal? AmountRequired { get; set; }
        public int PRAging { get; set; }
        public string Status { get; set; }
    }
    public class PRItemsTable
    {
        public int CustId { get; set; }
        public int ItemsId { get; set; }
        public int? ItemTypeId { get; set; }
        public int PRId { get; set; }

        [Required(ErrorMessage = "Date and time cannot be empty")]
        [DataType(DataType.DateTime)]
        [Range(typeof(DateTime), "1/1/1966", "1/1/2020")]
        public DateTime DateRequired { get; set; }

        [Required]
        [StringLength(300, ErrorMessage = "* Must be at least {2} characters long and {1} characters max.", MinimumLength = 3)]
        public string Description { get; set; }
        public int? CodeId { get; set; }
        public string ItemCode { get; set; }
        public string CustPONo { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Please enter valid integer Number")]
        public int Quantity { get; set; }
        public int? OutstandingQuantity { get; set; }
        public string UOM { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? TotalPrice { get; set; }
        public string location { get; set; }
        public int JobNo { get; set; }
        public int JobTaskNo { get; set; }
    }
    public class RoleList
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
    }
}