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
    
    public partial class PR_Admin
    {
        public System.Guid uuid { get; set; }
        public int adminId { get; set; }
        public int PRId { get; set; }
        public int adminSaved { get; set; }
        public int adminSubmited { get; set; }
        public Nullable<System.DateTime> adminSubmitDate { get; set; }
        public Nullable<System.DateTime> lastModifyDate { get; set; }
    
        public virtual PurchaseRequisition PurchaseRequisition { get; set; }
    }
}
