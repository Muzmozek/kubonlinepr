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
        public bool CheckAmountScenarioOne(decimal amount, int custId)
        {
            if (amount <= 20000 || custId == 1)
                return true;

            if (amount <= 50000 && custId == 4)
                return true;

            return false;
        }

        public bool CheckAmountScenarioTwo(decimal amount, int custId, bool IncludeInBudget)
        {
            if (IncludeInBudget)
            {
                if (custId == 4 && amount >= decimal.Parse("50000.01") && amount <= 500000)
                    return true;
            }
            else
            {
                if (custId == 4 && amount <= 200000)
                    return true;
            }

            if (custId == 2 && amount >= decimal.Parse("20000.01") && amount <= 50000)
                return true;

            return false;
        }

        public bool CheckAmountScenarioThree(decimal amount, int custId, bool IncludeInBudget)
        {
            if (IncludeInBudget)
            {
                if (custId == 4 && amount >= decimal.Parse("500000.01") && amount <= 2000000)
                    return true;
            }
            else
            {
                if (custId == 4 && amount >= decimal.Parse("200000.01") && amount <= 500000)
                    return true;
            }

            if (custId == 2 && amount >= decimal.Parse("50000.01"))
                return true;

            return false;
        }

        public bool CheckAmountScenarioFour(decimal amount, int custId, bool IncludeInBudget)
        {
            if (IncludeInBudget)
            {
                //custId = 4 is refered to Solar Gas Sdn Bhd
                if (custId == 4 && amount >= decimal.Parse("2000000.01"))
                    return true;
            }
            else
            {
                if (custId == 4 && amount >= decimal.Parse("500000.01"))
                    return true;
            }

            return false;
        }

        public bool IncludeInBudget(PurchaseRequisition purchaseRequisition, int custId)
        {
            return (purchaseRequisition.Budgeted || custId == 1) ? true : false;
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
                         }).ToList();

            records.SuperAdmin = SuperAdmin;

            var HOGPSS = (from u in _db.Users
                          join ur in _db.Users_Roles on u.userId equals ur.userId into s                          
                          from t in s.DefaultIfEmpty()
                          join r in _db.Roles on t.roleId equals r.roleId
                          where r.name == "HOGPSS"
                          select new UserViewModel
                          {
                              firstName = u.firstName,
                              lastName = u.lastName,
                              userId = u.userId
                          }).ToList();

            records.HeadOfGPSS = HOGPSS;

            // loop by Customer
            var customers = _db.Customers.Where(x => x.custId != 1 && x.custId != 3 && x.custId != 5 && x.custId != 6 && x.custId != 7 && x.custId != 8).ToList();
           
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
                           where (r.name == "HOC" || r.name == "GMD")
                           && u.Customer.abbreviation == customer.abbreviation
                           select new UserViewModel
                           {
                               firstName = u.firstName,
                               lastName = u.lastName,
                               userId = u.userId,
                               CustId = u.companyId.Value
                           }).ToList();

                var it = (from u in _db.Users
                           join ur in _db.Users_Roles on u.userId equals ur.userId
                           join r in _db.Roles on ur.roleId equals r.roleId
                           where r.name == "IT"
                           && u.Customer.abbreviation == customer.abbreviation
                           select new UserViewModel
                           {
                               firstName = u.firstName,
                               lastName = u.lastName,
                               userId = u.userId
                           }).ToList();

                var pmo = (from u in _db.Users
                           join ur in _db.Users_Roles on u.userId equals ur.userId
                           join r in _db.Roles on ur.roleId equals r.roleId
                           where r.name == "PMO"
                           && u.Customer.abbreviation == customer.abbreviation
                           select new UserViewModel
                           {
                               firstName = u.firstName,
                               lastName = u.lastName,
                               userId = u.userId
                           }).ToList();

                var hse = (from u in _db.Users
                           join ur in _db.Users_Roles on u.userId equals ur.userId
                           join r in _db.Roles on ur.roleId equals r.roleId
                           where r.name == "HSE"
                           && u.Customer.abbreviation == customer.abbreviation
                           select new UserViewModel
                           {
                               firstName = u.firstName,
                               lastName = u.lastName,
                               userId = u.userId
                           }).ToList();

                var admin = (from u in _db.Users
                           join ur in _db.Users_Roles on u.userId equals ur.userId
                           join r in _db.Roles on ur.roleId equals r.roleId
                           where r.name == "Admin"
                           && u.Customer.abbreviation == customer.abbreviation
                           select new UserViewModel
                           {
                               firstName = u.firstName,
                               lastName = u.lastName,
                               userId = u.userId
                           }).ToList();

                var finance = (from u in _db.Users
                             join ur in _db.Users_Roles on u.userId equals ur.userId
                             join r in _db.Roles on ur.roleId equals r.roleId
                             where r.name == "Finance"
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
                    if (r.superiorId != null)
                    {
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
                    Procurements = procurements,
                    Requestors = requestor,
                    ITs = it,
                    Finance = finance,
                    PMOs = pmo,
                    HSEs = hse,
                    Admin = admin
                });

            }



            return records;
        }
    }
}