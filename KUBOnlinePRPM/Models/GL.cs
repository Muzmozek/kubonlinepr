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
        public System.Guid uuid { get; set; }
        public int codeId { get; set; }
        public int itemTypeId { get; set; }
        public string itemCode { get; set; }
        public int custId { get; set; }
        public string Description { get; set; }
        public string UoM { get; set; }
        public Nullable<decimal> budgetAmount { get; set; }
        public Nullable<decimal> budgetUtilized { get; set; }
        public Nullable<decimal> budgetBalance { get; set; }
        public Nullable<decimal> budgetInProgress { get; set; }
        public Nullable<System.DateTime> createDate { get; set; }
        public Nullable<int> createByUserId { get; set; }
    
        public virtual Customer Customer { get; set; }
        public virtual ItemType ItemType { get; set; }
    }
}
