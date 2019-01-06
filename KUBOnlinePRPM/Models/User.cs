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
    
    public partial class User
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public User()
        {
            this.Customers = new HashSet<Customer>();
            this.Customers1 = new HashSet<Customer>();
            this.FileUploads = new HashSet<FileUpload>();
            this.Users_Roles = new HashSet<Users_Roles>();
            this.Vendors = new HashSet<Vendor>();
            this.Vendors1 = new HashSet<Vendor>();
            this.VendorStaffs = new HashSet<VendorStaff>();
            this.VendorStaffs1 = new HashSet<VendorStaff>();
            this.NotiGroups = new HashSet<NotiGroup>();
            this.PR_HOD = new HashSet<PR_HOD>();
            this.PR_Recommender = new HashSet<PR_Recommender>();
            this.PR_Reviewer = new HashSet<PR_Reviewer>();
            this.PR_Approver = new HashSet<PR_Approver>();
            this.PR_PaperApprover = new HashSet<PR_PaperApprover>();
            this.FixedAssets = new HashSet<FixedAsset>();
            this.PR_RecommenderCFO = new HashSet<PR_RecommenderCFO>();
            this.PR_RecommenderCOO = new HashSet<PR_RecommenderCOO>();
            this.PR_RecommenderHOC = new HashSet<PR_RecommenderHOC>();
            this.PurchaseRequisitions = new HashSet<PurchaseRequisition>();
        }
    
        public System.Guid uuid { get; set; }
        public int userId { get; set; }
        public Nullable<int> companyId { get; set; }
        public Nullable<int> createByUserId { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<int> modifiedByUserId { get; set; }
        public Nullable<System.DateTime> modifiedDate { get; set; }
        public string userName { get; set; }
        public string password { get; set; }
        public Nullable<bool> passwordReset { get; set; }
        public Nullable<System.DateTime> passwordModifiedDate { get; set; }
        public string reminderQueryQuestion { get; set; }
        public string reminderQueryAnswer { get; set; }
        public string emailAddress { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string employeeNo { get; set; }
        public string jobTitle { get; set; }
        public Nullable<System.DateTime> loginDate { get; set; }
        public string loginLatLng { get; set; }
        public string loginAddress { get; set; }
        public Nullable<System.DateTime> lastLoginDate { get; set; }
        public Nullable<System.DateTime> lastFailedLoginDate { get; set; }
        public Nullable<int> failedLoginAttempts { get; set; }
        public Nullable<bool> lockout { get; set; }
        public Nullable<System.DateTime> lockoutDate { get; set; }
        public Nullable<bool> status { get; set; }
        public string telephoneNo { get; set; }
        public string address { get; set; }
        public Nullable<int> extensionNo { get; set; }
        public Nullable<int> superiorId { get; set; }
        public string department { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Customer> Customers { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Customer> Customers1 { get; set; }
        public virtual Customer Customer { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FileUpload> FileUploads { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Users_Roles> Users_Roles { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Vendor> Vendors { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Vendor> Vendors1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<VendorStaff> VendorStaffs { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<VendorStaff> VendorStaffs1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<NotiGroup> NotiGroups { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PR_HOD> PR_HOD { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PR_Recommender> PR_Recommender { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PR_Reviewer> PR_Reviewer { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PR_Approver> PR_Approver { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PR_PaperApprover> PR_PaperApprover { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<FixedAsset> FixedAssets { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PR_RecommenderCFO> PR_RecommenderCFO { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PR_RecommenderCOO> PR_RecommenderCOO { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PR_RecommenderHOC> PR_RecommenderHOC { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PurchaseRequisition> PurchaseRequisitions { get; set; }
    }
}
