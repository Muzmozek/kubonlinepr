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
    
    public partial class PR_Finance
    {
        public System.Guid uuid { get; set; }
        public int reviewerId { get; set; }
        public int PRId { get; set; }
        public int reviewed { get; set; }
        public Nullable<System.DateTime> reviewedDate { get; set; }
        public Nullable<System.DateTime> lastReviewedDate { get; set; }
    
        public virtual User User { get; set; }
        public virtual PurchaseRequisition PurchaseRequisition { get; set; }
    }
}
