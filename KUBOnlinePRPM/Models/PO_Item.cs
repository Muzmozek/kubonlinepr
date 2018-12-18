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
    
    public partial class PO_Item
    {
        public System.Guid uuid { get; set; }
        public int itemsId { get; set; }
        public int POId { get; set; }
        public System.DateTime dateRequired { get; set; }
        public string description { get; set; }
        public int codeId { get; set; }
        public string custPONo { get; set; }
        public int quantity { get; set; }
        public decimal unitPrice { get; set; }
        public decimal totalPrice { get; set; }
        public Nullable<int> itemTypeId { get; set; }
    
        public virtual FixedAsset FixedAsset { get; set; }
        public virtual GL GL { get; set; }
        public virtual Item Item { get; set; }
        public virtual PurchaseOrder PurchaseOrder { get; set; }
        public virtual PR_Items PR_Items { get; set; }
    }
}
