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
    
    public partial class Project
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Project()
        {
            this.PRs_FileUpload = new HashSet<PRs_FileUpload>();
            this.PurchaseOrders = new HashSet<PurchaseOrder>();
            this.PurchaseRequisitions = new HashSet<PurchaseRequisition>();
        }
    
        public System.Guid uuid { get; set; }
        public int projectId { get; set; }
        public int custId { get; set; }
        public string dimension { get; set; }
        public string projectCode { get; set; }
        public string projectName { get; set; }
        public bool paperVerified { get; set; }
        public decimal budgetedAmount { get; set; }
        public decimal utilizedToDate { get; set; }
        public decimal budgetBalance { get; set; }
    
        public virtual Customer Customer { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PRs_FileUpload> PRs_FileUpload { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PurchaseRequisition> PurchaseRequisitions { get; set; }
    }
}
