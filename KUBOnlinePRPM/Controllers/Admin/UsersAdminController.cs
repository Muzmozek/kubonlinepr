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
using KUBOnlinePRPM.ViewModel;

namespace KUBOnlinePRPM.Controllers.Admin
{
    public class UsersAdminController : Controller
    {
        private KUBOnlinePREntities db = new KUBOnlinePREntities();

        // GET: UsersAdmin
        public async Task<ActionResult> Index()
        {
            var users = db.Users.Include(u => u.Customer).Include(x => x.Users_Roles);

            foreach(User u in users.ToList())
            {
                Console.WriteLine(u);
            }
            return View(await users.ToListAsync());
        }

        // GET: UsersAdmin/Details/5
        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = await db.Users.FindAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: UsersAdmin/Create
        public ActionResult Create()
        {
            ViewBag.companyId = new SelectList(db.Customers, "custId", "name");
            return View();
        }

        // POST: UsersAdmin/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "uuid,userId,companyId,createByUserId,createDate,modifiedByUserId,modifiedDate,userName,password,passwordReset,passwordModifiedDate,reminderQueryQuestion,reminderQueryAnswer,emailAddress,firstName,lastName,employeeNo,jobTitle,loginDate,loginLatLng,loginAddress,lastLoginDate,lastFailedLoginDate,failedLoginAttempts,lockout,lockoutDate,status,telephoneNo,address,extensionNo,superiorId")] User user)
        {
            if (ModelState.IsValid)
            {
                user.uuid = System.Guid.NewGuid();
                user.password = BCrypt.HashPassword(user.password, BCrypt.GenerateSalt(12));
                db.Users.Add(user);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.companyId = new SelectList(db.Customers, "custId", "name", user.companyId);
            return View(user);
        }

        // GET: UsersAdmin/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = await db.Users.FindAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            ViewBag.companyId = new SelectList(db.Customers, "custId", "name", user.companyId);
            ViewBag.superiorId = new SelectList(db.Users, "userId", "fullName", user.superiorId);
            ViewBag.roleList = db.Roles.Select(x => new UserRoleViewModel{ roleId = x.roleId , roleName = x.name});
            return View(user);
        }

        // POST: UsersAdmin/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "uuid,userId,companyId,createByUserId,createDate,modifiedByUserId,modifiedDate,userName,password,passwordReset,passwordModifiedDate,reminderQueryQuestion,reminderQueryAnswer,emailAddress,firstName,lastName,employeeNo,jobTitle,loginDate,loginLatLng,loginAddress,lastLoginDate,lastFailedLoginDate,failedLoginAttempts,lockout,lockoutDate,status,telephoneNo,address,extensionNo,superiorId")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.companyId = new SelectList(db.Customers, "custId", "name", user.companyId);
            return View(user);
        }

        // GET: UsersAdmin/Delete/5
        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = await db.Users.FindAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: UsersAdmin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            User user = await db.Users.FindAsync(id);
            db.Users.Remove(user);
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
    }
}
