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
    
    public partial class Budget
    {
        public System.Guid uuid { get; set; }
        public int budgetId { get; set; }
        public int codeId { get; set; }
        public decimal budgetAmount { get; set; }
        public decimal initialUtilized { get; set; }
        public decimal utilized { get; set; }
        public Nullable<int> progress { get; set; }
        public int projectId { get; set; }
        public Nullable<int> year { get; set; }
    }
}