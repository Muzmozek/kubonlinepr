using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KUBOnlinePRPM.Models
{
    public class POModel
    {
        public int UserId { get; set; }
        public List<RoleList> RoleIdList { get; set; }
        public string Type { get; set; }
        public int CustId { get; set; }
        public int POId { get; set; }
        public int PRId { get; set; }
        public string StatusId { get; set; }
        public NewPOModel NewPOForm { get; set; }
        public List<POListTable> POListObject { get; set; }
        public decimal OutstandingQty { get; set; }
    }
    public class NewPOModel
    {
        public string PONo { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string PRNo { get; set; }
        public int? VendorId { get; set; }
        public string VendorCode { get; set; }
        public int? VendorStaffId { get; set; }
        public decimal AmountRequired { get; set; }
        public string VendorName { get; set; }
        public string VendorEmail { get; set; }
        public string VendorContactName { get; set; }
        public string VendorContactNo { get; set; }
        public string VendorQuoteNo { get; set; }
        public DateTime PreparedDate { get; set; }
        public decimal AmountPOBalance { get; set; }
        public int PreparedById { get; set; }
        //public bool Reviewed { get; set; }
        public bool SelectSave { get; set; }
        public int Saved { get; set; }
        public bool SelectSubmit { get; set; }
        public bool Submited { get; set; }
        public int POAging { get; set; }
        public string StatusId { get; set; }
        public string Status { get; set; }
        public int PayToVendorId { get; set; }
        public string PayToVendorName { get; set; }
        public int PaymentTermsId { get; set; }
        public string PaymentTermsCode { get; set; }
        public List<POItemsTable> POItemListObject { get; set; }
    }
    public class POListTable
    {
        public int POId { get; set; }
        public string PONo { get; set; }
        public DateTime PODate { get; set; }
        public string PRNo { get; set; }
        public string VendorCompany { get; set; }
        public decimal? AmountRequired { get; set; }
        public int POAging { get; set; }
        public string POType { get; set; }
    }
    public class POItemsTable
    {
        public int ItemsId { get; set; }
        public int POId { get; set; }
        public DateTime DateRequired { get; set; }
        public string Description { get; set; }
        public int CodeId { get; set; }
        public string ItemCode { get; set; }
        public string CustPONo { get; set; }
        public int Quantity { get; set; }
        public int OutStandingQuantity { get; set; }
        public string UOM { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}