using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        public int ChildCustId { get; set; }
        public int POId { get; set; }
        public int PRId { get; set; }
        public string StatusId { get; set; }
        public NewPOModel NewPOForm { get; set; }
        public POListTable POListTable { get; set; }
        public List<POListTable> POListObject { get; set; }
        public List<POItemsTable> POItemList { get; set; }
        public List<POHeaderTable> POHeaderList { get; set; }
        public List<POLineTable> POLineList { get; set; }
        public decimal OutstandingQty { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class NewPOModel
    {
        public string CompanyName { get; set; }
        public int POId { get; set; }
        public DateTime PRDate { get; set; }
        public DateTime PODate { get; set; }
        public string PONo { get; set; }
        public int ProjectId { get; set; }
        public int CustId { get; set; }
        public int AdminId { get; set; }
        public string ProjectName { get; set; }
        public string PRNo { get; set; }
        public int? VendorId { get; set; }
        public string VendorCode { get; set; }
        public string VendorAddress { get; set; }
        public int? VendorStaffId { get; set; }
        public decimal AmountRequired { get; set; }
        public string VendorName { get; set; }
        public string VendorEmail { get; set; }
        public string VendorContactName { get; set; }
        public string VendorContactNo { get; set; }
        public string VendorQuoteNo { get; set; }
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

        [Required]
        public int? PayToVendorId { get; set; }
        public string PayToVendorName { get; set; }

        [Required]
        public int? PaymentTermsId { get; set; }
        public string PaymentTermsCode { get; set; }
        public List<POItemsTable> POItemListObject { get; set; }
        public decimal DiscountAmount { get; set; }
        public int DiscountPerc { get; set; }
        public decimal? TotalBeforeDisc { get; set; }
        public decimal? TotalExclSST { get; set; }
        public decimal? TotalSST { get; set; }
        public decimal? TotalIncSST { get; set; }
        public string SpecReviewerId { get; set; }

        [Required]
        public int? LocationCodeId { get; set; }
        public string LocationCode { get; set; }
        public string DeliveryTo { get; set; }

        [Required]
        public DateTime? OrderDate { get; set; }

        [Required]
        public int? PurchaserCodeId { get; set; }
        public string PurchaserCode { get; set; }

        [Required]
        public DateTime? DeliveryDate { get; set; }
    }

    public class POListTable
    {
        public int CustId { get; set; }
        public int POId { get; set; }
        public string PONo { get; set; }
        public DateTime PODate { get; set; }
        public string PRNo { get; set; }
        public string VendorCompany { get; set; }
        public string VendorId { get; set; }
        public string Company { get; set; }
        public string RequestedName { get; set; }
        public string POItem { get; set; }
        public decimal Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal? AmountRequired { get; set; }
        public int POAging { get; set; }
        public string Status { get; set; }
        public string POType { get; set; }
        public string LastPONo { get; set; }
        public DateTime? LastPODate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string ProjectName { get; set; }
    }

    public class POItemsTable
    {
        public int ItemsId { get; set; }
        public string ItemType { get; set; }
        public int? ItemTypeId { get; set; }
        public int POId { get; set; }
        public DateTime DateRequired { get; set; }
        public string Description { get; set; }
        public int CodeId { get; set; }
        public string ItemCode { get; set; }
        public string CustPONo { get; set; }
        public decimal Quantity { get; set; }
        public decimal OutStandingQuantity { get; set; }
        public string UOM { get; set; }
        public int? UoMId { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal? UnitPriceIncSST { get; set; }
        public decimal? TotalPriceIncSST { get; set; }
        public string JobNo { get; set; }
        public int? JobNoId { get; set; }
        public string JobTaskNo { get; set; }
        public int? JobTaskNoId { get; set; }
        public decimal? SST { get; set; }
        public int? TaxCodeId { get; set; }
        public string TaxCode { get; set; }
        public int? TaxPerc { get; set; }
        public int? DimProjectId { get; set; }
        public string DimProject { get; set; }
        public int? DimDeptId { get; set; }
        public string DimDept { get; set; }
    }

    public class POHeaderTable
    {
        public string DocumentType { get; set; }
        public string No { get; set; }
        public string BuyFromVendorNo { get; set;}
        public string PayToVendorNo { get; set; }
        public string YourReference { get; set; }
        public DateTime? OrderDate { get; set; }
        public DateTime? PostingDate { get; set; }
        public DateTime? ExpectedReceiptDate { get; set; }
        public string PostingDescription { get; set; }
        public string LocationCode { get; set; }
        public string ShortcutDimension1Code { get; set; }
        public string ShortcutDimension2Code { get; set; }
        public string VendorPostingGroup { get; set; }
        public string CurrencyCode { get; set; }
    }

    public class POLineTable
    {
        public string DocumentType { get; set; }
        public string DocumentNo { get; set; }
        public string LineNo { get; set; }
        public string BuyFromVendorNo { get; set; }
        public string Type { get; set; }
        public string No { get; set; }
        public string LocationCode { get; set; }
        public string PostingGroup { get; set; }
        public DateTime? ExpReceiptDate { get; set; }
        public string Description { get; set; }
        public string UnitofMeasure { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? OutStandingQuantity { get; set; }
        public decimal? DirectUnitCost { get; set; }
        public decimal? Amount { get; set; }
        //public string DimProject { get; set; }
        //public string DimDepartment { get; set; }
    }
}