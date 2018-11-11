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
        public bool CheckAmountScenarioOne(int amount)
        {
            if (amount <= 20000)
                return true;

            return false;
        }

        public bool CheckAmountScenarioTwo(int amount)
        {
            if (amount >= 20001 && amount <= 50000)
                return true;

            return false;
        }

        public bool CheckAmountScenarioThree(int amount)
        {
            if (amount >= 50001 && amount <= 300000)
                return true;

            return false;
        }

        public bool IncludeInBudget(PurchaseRequisition purchaseRequisition)
        {
            return (purchaseRequisition.Unbudgeted) ? false : true;
        }


    }
}