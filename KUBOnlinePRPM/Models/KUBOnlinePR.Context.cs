﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class KUBOnlinePREntities : DbContext
    {
        public KUBOnlinePREntities()
            : base("name=KUBOnlinePREntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Customer> Customers { get; set; }
        public virtual DbSet<FileUpload> FileUploads { get; set; }
        public virtual DbSet<PR_Items> PR_Items { get; set; }
        public virtual DbSet<Project> Projects { get; set; }
        public virtual DbSet<PRs_FileUpload> PRs_FileUpload { get; set; }
        public virtual DbSet<PRStatu> PRStatus { get; set; }
        public virtual DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public virtual DbSet<PurchaseType> PurchaseTypes { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<Users_Roles> Users_Roles { get; set; }
        public virtual DbSet<Vendor> Vendors { get; set; }
        public virtual DbSet<VendorStaff> VendorStaffs { get; set; }
        public virtual DbSet<ItemType> ItemTypes { get; set; }
        public virtual DbSet<PO_Item> PO_Item { get; set; }
        public virtual DbSet<NotificationMsg> NotificationMsgs { get; set; }
        public virtual DbSet<NotiGroup> NotiGroups { get; set; }
        public virtual DbSet<PR_Admin> PR_Admin { get; set; }
        public virtual DbSet<PR_HOD> PR_HOD { get; set; }
        public virtual DbSet<PR_Recommender> PR_Recommender { get; set; }
        public virtual DbSet<PR_Reviewer> PR_Reviewer { get; set; }
        public virtual DbSet<PR_Approver> PR_Approver { get; set; }
        public virtual DbSet<POStatu> POStatus { get; set; }
        public virtual DbSet<PR_PaperApprover> PR_PaperApprover { get; set; }
        public virtual DbSet<PurchaseRequisition> PurchaseRequisitions { get; set; }
        public virtual DbSet<FixedAsset> FixedAssets { get; set; }
        public virtual DbSet<GL> GLs { get; set; }
        public virtual DbSet<Item> Items { get; set; }
        public virtual DbSet<JobTask> JobTasks { get; set; }
        public virtual DbSet<PopulateItemList> PopulateItemLists { get; set; }
        public virtual DbSet<PaymentTerm> PaymentTerms { get; set; }
    }
}
