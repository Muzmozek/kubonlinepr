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
    
    public partial class PR_PaperApprover
    {
        public System.Guid uuid { get; set; }
        public int approverId { get; set; }
        public int PRId { get; set; }
        public int approverApproved { get; set; }
        public Nullable<System.DateTime> approverApprovedDate { get; set; }
        public Nullable<int> finalApproverUserId { get; set; }
    
        public virtual PurchaseRequisition PurchaseRequisition { get; set; }
        public virtual User User { get; set; }
    }
}
