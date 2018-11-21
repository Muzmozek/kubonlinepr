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
    
    public partial class VendorStaff
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public VendorStaff()
        {
            this.PurchaseRequisitions = new HashSet<PurchaseRequisition>();
            this.PurchaseOrders = new HashSet<PurchaseOrder>();
        }
    
        public System.Guid uuid { get; set; }
        public int staffId { get; set; }
        public int vendorId { get; set; }
        public string vendorContactName { get; set; }
        public string vendorEmail { get; set; }
        public string vendorContactNo { get; set; }
        public Nullable<int> createByUserId { get; set; }
        public Nullable<int> lastModifyByUserId { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<System.DateTime> lastModifyDate { get; set; }
    
        public virtual User User { get; set; }
        public virtual User User1 { get; set; }
        public virtual Vendor Vendor { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PurchaseRequisition> PurchaseRequisitions { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; }
    }
}
