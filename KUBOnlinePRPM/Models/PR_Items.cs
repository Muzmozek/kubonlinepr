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
    
    public partial class PR_Items
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PR_Items()
        {
            this.PO_Item = new HashSet<PO_Item>();
        }
    
        public System.Guid uuid { get; set; }
        public int itemsId { get; set; }
        public int PRId { get; set; }
        public System.DateTime dateRequired { get; set; }
        public string description { get; set; }
        public Nullable<int> codeId { get; set; }
        public string custPONo { get; set; }
        public int quantity { get; set; }
        public Nullable<decimal> unitPrice { get; set; }
        public Nullable<decimal> totalPrice { get; set; }
        public Nullable<int> outStandingQuantity { get; set; }
        public Nullable<int> itemTypeId { get; set; }
        public string location { get; set; }
        public Nullable<int> jobNo { get; set; }
        public Nullable<int> jobTaskNo { get; set; }
        public Nullable<System.DateTime> plannedReceiptDate { get; set; }
    
        public virtual ItemType ItemType { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PO_Item> PO_Item { get; set; }
        public virtual PurchaseRequisition PurchaseRequisition { get; set; }
        public virtual FixedAsset FixedAsset { get; set; }
        public virtual GL GL { get; set; }
        public virtual Item Item { get; set; }
    }
}
