﻿using KUBOnlinePRPM.Controllers;
using System.Web;
using System.Web.Mvc;

namespace KUBOnlinePRPM
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new CheckSessionOutAttribute());
        }
    }
}
