using KUBOnlinePRPM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KUBOnlinePRPM.Service
{
    public class PRService
    {
        // check project amount
        public bool CheckAmount(int amount)
        {
            if (amount <= 20000)
                return true;

            return false;
        }

        public bool IncludeInBudget(PurchaseRequisition purchaseRequisition)
        {
            return (purchaseRequisition.Unbudgeted) ? false : true;
        }


    }
}