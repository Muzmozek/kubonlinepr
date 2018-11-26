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
        public bool CheckAmountScenarioOne(decimal amount)
        {
            if (amount <= 20000)
                return true;

            return false;
        }

        public bool CheckAmountScenarioTwo(decimal amount)
        {
            if (amount >= 20001 && amount <= 50000)
                return true;

            return false;
        }

        public bool CheckAmountScenarioThree(decimal amount)
        {
            if (amount >= 50001 && amount <= 300000)
                return true;

            return false;
        }

        public bool IncludeInBudget(PurchaseRequisition purchaseRequisition)
        {
            return (purchaseRequisition.Budgeted) ? true : false;
        }


    }
}