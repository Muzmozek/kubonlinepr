using KUBOnlinePRPM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KUBOnlinePRPM.Service
{
    public class KUBHelper
    {
        public bool CheckRole(String roleId)
        {
            var result = false;

            var userRoles = (List<Users_Roles>)HttpContext.Current.Session["roles"];
            

            if (userRoles != null)
            {
                if (userRoles.Exists(x => x.roleId.Trim() == roleId))
                {
                    result = true;
                }
            }
            

            return result;
        }
    }
}