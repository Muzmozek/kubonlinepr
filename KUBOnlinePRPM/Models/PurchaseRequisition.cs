//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace KUBOnlinePRPM.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class PurchaseRequisition
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PurchaseRequisition()
        {
            this.PR_Admin = new HashSet<PR_Admin>();
            this.PR_Approver = new HashSet<PR_Approver>();
            this.PR_HOD = new HashSet<PR_HOD>();
            this.PR_Recommender = new HashSet<PR_Recommender>();
            this.PR_RecommenderCFO = new HashSet<PR_RecommenderCFO>();
            this.PR_RecommenderCOO = new HashSet<PR_RecommenderCOO>();
            this.PR_RecommenderHOC = new HashSet<PR_RecommenderHOC>();
            this.PR_Reviewer = new HashSet<PR_Reviewer>();
            this.PRs_FileUpload = new HashSet<PRs_FileUpload>();
            this.PR_Finance = new HashSet<PR_Finance>();
            this.PR_PaperApprover = new HashSet<PR_PaperApprover>();
            this.PR_Items = new HashSet<PR_Items>();
            this.PurchaseOrders = new HashSet<PurchaseOrder>();
        }
    
        public System.Guid uuid { get; set; }
        public int PRId { get; set; }
        public int CustId { get; set; }
        public string PRNo { get; set; }
        public int ProjectId { get; set; }
        public bool Budgeted { get; set; }
        public string PaperRefNo { get; set; }
        public string BidWaiverRefNo { get; set; }
        public bool PaperAttachment { get; set; }
        public int PurchaseTypeId { get; set; }
        public string BudgetDescription { get; set; }
        public string Justification { get; set; }
        public decimal AmountRequired { get; set; }
        public Nullable<int> VendorId { get; set; }
        public Nullable<int> VendorStaffId { get; set; }
        public string VendorCompanyId { get; set; }
        public string VendorQuoteNo { get; set; }
        public string SpecsReviewerId { get; set; }
        public System.DateTime PreparedDate { get; set; }
        public int PreparedById { get; set; }
        public Nullable<System.DateTime> LastModifyDate { get; set; }
        public int Saved { get; set; }
        public int Submited { get; set; }
        public Nullable<System.DateTime> SubmitDate { get; set; }
        public Nullable<System.DateTime> LastSubmitedDate { get; set; }
        public int Rejected { get; set; }
        public string RejectedRemark { get; set; }
        public Nullable<int> RejectedById { get; set; }
        public Nullable<System.DateTime> RejectedDate { get; set; }
        public bool Phase1Completed { get; set; }
        public string PRType { get; set; }
        public Nullable<int> POId { get; set; }
        public Nullable<decimal> AmountPOBalance { get; set; }
        public bool Phase2Completed { get; set; }
        public int Scenario { get; set; }
        public Nullable<int> PRAging { get; set; }
        public string StatusId { get; set; }
        public Nullable<decimal> DiscountAmount { get; set; }
        public Nullable<int> Discount_ { get; set; }
        public Nullable<decimal> TotalExclSST { get; set; }
        public Nullable<decimal> TotalSST { get; set; }
        public Nullable<decimal> TotalIncSST { get; set; }
        public decimal budgetedAmount { get; set; }
        public decimal utilizedToDate { get; set; }
        public decimal budgetBalance { get; set; }
    
        public virtual Customer Customer { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PR_Admin> PR_Admin { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PR_Approver> PR_Approver { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PR_HOD> PR_HOD { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PR_Recommender> PR_Recommender { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PR_RecommenderCFO> PR_RecommenderCFO { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PR_RecommenderCOO> PR_RecommenderCOO { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PR_RecommenderHOC> PR_RecommenderHOC { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PR_Reviewer> PR_Reviewer { get; set; }
        public virtual Project Project { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PRs_FileUpload> PRs_FileUpload { get; set; }
        public virtual PRStatu PRStatu { get; set; }
        public virtual PurchaseType PurchaseType { get; set; }
        public virtual User User { get; set; }
        public virtual Vendor Vendor { get; set; }
        public virtual VendorStaff VendorStaff { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PR_Finance> PR_Finance { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PR_PaperApprover> PR_PaperApprover { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PR_Items> PR_Items { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; }
    }
}
