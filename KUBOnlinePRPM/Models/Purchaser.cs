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
    
    public partial class Purchaser
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Purchaser()
        {
            this.PurchaseOrders = new HashSet<PurchaseOrder>();
        }
    
        public System.Guid uuid { get; set; }
        public int purchaserId { get; set; }
        public string purchaserCode { get; set; }
        public string purchaserName { get; set; }
        public int custId { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; }
    }
}
