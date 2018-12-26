using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KUBOnlinePRPM.Models
{
    public class UserModel
    {
        public int UserId { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public int? CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string Role { get; set; }
        public string RoleId { get; set; }
        public string JobTitle { get; set; }
        public string EmailAddress { get; set; }
        public string Department { get; set; }
        public string HOD { get; set; }
        public string HOC { get; set; }
    }
}