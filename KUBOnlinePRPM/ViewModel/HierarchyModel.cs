using KUBOnlinePRPM.Models;
using KUBOnlinePRPM.ViewModel;
using System.Collections.Generic;

namespace KUBOnlinePRPM.Service
{
    public class HeirarchyModel
    {
        public List<UserViewModel> SuperAdmin { get; set; }
        public List<UserViewModel> HeadOfGPSS { get; set; }
        public List<SubsidiaryLevel> Members { get; set; }       
    }

    public class SubsidiaryLevel
    {
        public string SubsidiaryName { get; set; }
        public List<UserViewModel> Procurements { get; set; }
        public List<UserViewModel> HOCs { get; set; }
        public List<UserViewModel> SpecVerifiers { get; set; }
        public List<UserViewModel> HODs { get; set; }
        public List<UserViewModel> Requestors { get; set; }
        public List<UserViewModel> Admin { get; set; }
        public List<UserViewModel> ITs { get; set; }
        public List<UserViewModel> PMOs { get; set; }
        public List<UserViewModel> HSEs { get; set; }
    }

        
    
}