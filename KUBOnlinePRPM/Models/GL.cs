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
    
    public partial class GL
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public GL()
        {
            this.PO_Item = new HashSet<PO_Item>();
            this.PR_Items = new HashSet<PR_Items>();
        }
    
        public System.Guid uuid { get; set; }
        public int codeId { get; set; }
        public int itemTypeId { get; set; }
        public string itemCode { get; set; }
        public int custId { get; set; }
        public string Description { get; set; }
        public string UoM { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<int> createByUserId { get; set; }
    
        public virtual Customer Customer { get; set; }
        public virtual ItemType ItemType { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PO_Item> PO_Item { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PR_Items> PR_Items { get; set; }
    }
}