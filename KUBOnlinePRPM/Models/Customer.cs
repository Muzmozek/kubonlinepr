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
    
    public partial class Customer
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Customer()
        {
            this.Users = new HashSet<User>();
            this.PurchaseRequisitions = new HashSet<PurchaseRequisition>();
        }
    
        public System.Guid uuid { get; set; }
        public int custId { get; set; }
        public string name { get; set; }
        public string abbreviation { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<int> createByUserId { get; set; }
        public string custRegNo { get; set; }
        public string type { get; set; }
        public string homeURL { get; set; }
        public string address { get; set; }
        public Nullable<int> telephoneNo { get; set; }
        public Nullable<System.DateTime> modifiedDate { get; set; }
        public Nullable<int> modifiedByUserId { get; set; }
    
        public virtual User User { get; set; }
        public virtual User User1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<User> Users { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PurchaseRequisition> PurchaseRequisitions { get; set; }
    }
}
