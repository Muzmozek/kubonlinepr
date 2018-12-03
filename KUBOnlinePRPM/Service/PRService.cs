using KUBOnlinePRPM.Models;
using KUBOnlinePRPM.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KUBOnlinePRPM.Service
{
    public class PRService
    {
        private KUBOnlinePREntities _db = new KUBOnlinePREntities();

        public PRService()
        {
        
        }

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

        public HeirarchyModel populateUserList()
        {
            var records = new HeirarchyModel();

            var SuperAdmin = (from u in _db.Users
                         join ur in _db.Users_Roles on u.userId equals ur.userId
                         join r in _db.Roles on ur.roleId equals r.roleId
                         where r.name == "Super Admin"
                              select new UserViewModel
                         {
                             firstName = u.firstName,
                             lastName = u.lastName,
                             userId = u.userId
                         }).First();

            records.SuperAdmin = SuperAdmin;



            var HOGPSS = (from u in _db.Users
                          join ur in _db.Users_Roles on u.userId equals ur.userId
                          join r in _db.Roles on ur.roleId equals r.roleId
                          where r.name == "HOGPSS"
                          select new UserViewModel
                          {
                              firstName = u.firstName,
                              lastName = u.lastName,
                              userId = u.userId
                          }).First();

            records.HeadOfGPSS = HOGPSS;

            // loop by Customer
            var customers = _db.Customers.ToList();

            records.Members = new List<SubsidiaryLevel>();

            foreach (Customer customer in customers)
            {
                var procurements = (from u in _db.Users
                                    join ur in _db.Users_Roles on u.userId equals ur.userId
                                    join r in _db.Roles on ur.roleId equals r.roleId
                                    where r.name == "Procurement"
                                    && u.Customer.abbreviation == customer.abbreviation
                                    select new UserViewModel
                                    {
                                        firstName = u.firstName,
                                        lastName = u.lastName,
                                        userId = u.userId
                                    }).ToList();

                var hoc = (from u in _db.Users
                           join ur in _db.Users_Roles on u.userId equals ur.userId
                           join r in _db.Roles on ur.roleId equals r.roleId
                           where r.name == "HOC"
                           && u.Customer.abbreviation == customer.abbreviation
                           select new UserViewModel
                           {
                               firstName = u.firstName,
                               lastName = u.lastName,
                               userId = u.userId
                           }).ToList();

                var hod = (from u in _db.Users
                           join ur in _db.Users_Roles on u.userId equals ur.userId
                           join r in _db.Roles on ur.roleId equals r.roleId
                           where r.name == "HOD"
                           && u.Customer.abbreviation == customer.abbreviation
                           select new UserViewModel
                           {
                               firstName = u.firstName,
                               lastName = u.lastName,
                               userId = u.userId
                           }).ToList();

                var requestor = (from u in _db.Users
                                 join ur in _db.Users_Roles on u.userId equals ur.userId
                                 join r in _db.Roles on ur.roleId equals r.roleId
                                 where r.name == "Requestor"
                                 && u.Customer.abbreviation == customer.abbreviation
                                 select new UserViewModel
                                 {
                                     firstName = u.firstName,
                                     lastName = u.lastName,
                                     userId = u.userId,
                                     superiorId = u.superiorId
                                    
                                 }).ToList();


                foreach (var r in requestor)
                {
                    if (r.superiorId != null) {
                        var superior = _db.Users.Find(r.superiorId);

                        r.superiorFirstName = superior.firstName;
                        r.superiorLastName = superior.lastName;
                    }
                    
                }

                records.Members.Add(new SubsidiaryLevel()
                {
                    
                    SubsidiaryName = customer.abbreviation,
                    HOCs = hoc,
                    HODs = hod,
                    Requestors = requestor
                });

            }



            return records;
        }
    }
}