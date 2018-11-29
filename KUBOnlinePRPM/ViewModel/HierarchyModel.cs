using KUBOnlinePRPM.Models;
using KUBOnlinePRPM.ViewModel;
using System.Collections.Generic;

namespace KUBOnlinePRPM.Service
{
    public class HeirarchyModel
    {
        public UserViewModel SuperAdmin { get; set; }
        public UserViewModel HeadOfGPSS { get; set; }
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
    }

        
    
}