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
    
    public partial class PR_RecommenderCFO
    {
        public System.Guid uuid { get; set; }
        public int recommenderId { get; set; }
        public int PRId { get; set; }
        public int recommended { get; set; }
        public Nullable<System.DateTime> recommendedDate { get; set; }
        public Nullable<System.DateTime> lastRecommendedDate { get; set; }
    
        public virtual PurchaseRequisition PurchaseRequisition { get; set; }
        public virtual User User { get; set; }
    }
}
