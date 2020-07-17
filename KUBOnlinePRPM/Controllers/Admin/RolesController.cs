using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using KUBOnlinePRPM.Models;

namespace KUBOnlinePRPM.Controllers.Admin
{
    public class RolesController : DBLogicController
    {
        private KUBOnlinePREntities db = new KUBOnlinePREntities();

        // GET: Roles
        public async Task<ActionResult> Index()
        {
            return View(await db.Roles.ToListAsync());
        }

        // GET: Roles/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Role role = await db.Roles.FindAsync(id);
            if (role == null)
            {
                return HttpNotFound();
            }
            return View(role);
        }

        // GET: Roles/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Roles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "roleId,name,description,canViewAllOppo,canAddUser,canAddCustomer")] Role role)
        {
            if (ModelState.IsValid)
            {
                role.uuid = System.Guid.NewGuid();
                db.Roles.Add(role);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            return View(role);
        }

        // GET: Roles/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Role role = await db.Roles.FindAsync(id);
            if (role == null)
            {
                return HttpNotFound();
            }
            return View(role);
        }

        // POST: Roles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "uuid,roleId,name,description,canViewAllOppo,canAddUser,canAddCustomer")] Role role)
        {
            if (ModelState.IsValid)
            {
                db.Entry(role).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(role);
        }

        // GET: Roles/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Role role = await db.Roles.FindAsync(id);
            if (role == null)
            {
                return HttpNotFound();
            }
            return View(role);
        }

        // POST: Roles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            Role role = await db.Roles.FindAsync(id);
            db.Roles.Remove(role);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        // assign Roles to user
        // ajax call
        [HttpPost]
        public async Task<JsonResult> UpdateRoles(int userId, String roleId, int? childCompanyId, String action)
        {
            var checkRoleExist = db.Users_Roles.Where(x => x.userId == userId).Where(x => x.roleId == roleId).Any();
            var status = false;
            if (checkRoleExist)
            {
                if (action == "assign")
                {

                }
                else if (action == "remove")
                {
                    if (childCompanyId == null)
                    {
                        var rolesToBeRemove = db.Users_Roles.Where(x => x.userId == userId).Where(x => x.roleId == roleId);
                        db.Users_Roles.RemoveRange(rolesToBeRemove);
                        db.SaveChanges();
                        
                    } else
                    {
                        var removeChildCompId = db.Users.Where(x => x.userId == userId).FirstOrDefault();
                        removeChildCompId.childCompanyId = null;
                        db.SaveChanges();
                    }
                    status = true;
                }
            }
            else
            {
                if (action == "assign")
                {
                    if (childCompanyId == null)
                    {
                        db.Users_Roles.Add(new Users_Roles
                        {
                            uuid = System.Guid.NewGuid(),
                            userId = userId,
                            roleId = roleId
                        });

                        db.SaveChanges();
                    } else
                    {
                        var addChildCompId = db.Users.Where(x => x.userId == userId).FirstOrDefault();
                        addChildCompId.childCompanyId = childCompanyId;
                        db.SaveChanges();
                    }
                    status = true;
                    
                }
                else if (action == "remove")
                {

                }
            }

            return Json(new { success = status});
        }
    }
}
