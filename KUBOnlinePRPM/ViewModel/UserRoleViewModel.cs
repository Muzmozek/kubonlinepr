using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KUBOnlinePRPM.ViewModel
{
    public class UserRoleViewModel
    {
        public string userId { get; set; }
        public string roleId { get; set; }
        public string roleName { get; set; }
    }

    public class UserViewModel
    {
        public int userId { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public Nullable<int> superiorId { get; set; }
        public string superiorFirstName { get; set; }
        public string superiorLastName { get; set; }
    }
}