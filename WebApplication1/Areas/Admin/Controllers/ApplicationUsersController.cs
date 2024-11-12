using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;

namespace WebApplication1.Areas.Admin.Controllers
{
    [Authorize(Roles = "SupperAdmin")]
    public class ApplicationUsersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        private ApplicationRoleManager _roleManager;
        public ApplicationRoleManager RoleManager
        {
            get
            {
                return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>();
            }
            private set
            {
                _roleManager = value;
            }
        }


        public ActionResult Index()
        {
            if (UserManager == null || RoleManager == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "UserManager hoặc RoleManager chưa được khởi tạo.");
            }

            var users = UserManager.Users.ToList();
            var userRoles = new Dictionary<string, string>();

            foreach (var user in users)
            {
                var roles = UserManager.GetRoles(user.Id);
                userRoles[user.Id] = roles.Count > 0 ? string.Join(", ", roles) : "Chưa có vai trò";
            }

            ViewBag.UserRoles = userRoles;
            ViewBag.RoleList = new SelectList(RoleManager.Roles.ToList(), "Name", "Name");

            return View(users);
        }

        // GET: Admin/ApplicationUsers/Details/5
        public ActionResult Details(string id)
        {
            if (UserManager == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "UserManager is not initialized.");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ApplicationUser applicationUser = UserManager.FindById(id);
            if (applicationUser == null)
            {
                return HttpNotFound();
            }

            return View(applicationUser);
        }


        // GET: Admin/ApplicationUsers/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Admin/ApplicationUsers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Email,Password")] RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");
                }
                AddErrors(result);
            }
            return View(model);
        }

        // GET: Admin/ApplicationUsers/Edit/5
        public ActionResult Edit(string id)
        {
            if (UserManager == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "UserManager is not initialized.");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ApplicationUser applicationUser = UserManager.FindById(id);
            if (applicationUser == null)
            {
                return HttpNotFound();
            }

            return View(applicationUser);
        }

        // POST: Admin/ApplicationUsers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Email,EmailConfirmed,PasswordHash,SecurityStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEndDateUtc,LockoutEnabled,AccessFailedCount,UserName")] ApplicationUser applicationUser)
        {
            if (ModelState.IsValid)
            {
                db.Entry(applicationUser).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(applicationUser);
        }

        // GET: Admin/ApplicationUsers/Delete/5
        public ActionResult Delete(string id)
        {
            if (UserManager == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "UserManager is not initialized.");
            }

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ApplicationUser applicationUser = UserManager.FindById(id);
            if (applicationUser == null)
            {
                return HttpNotFound();
            }

            return View(applicationUser);
        }

        // POST: Admin/ApplicationUsers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            if (UserManager == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "UserManager is not initialized.");
            }

            ApplicationUser applicationUser = await UserManager.FindByIdAsync(id);
            if (applicationUser == null)
            {
                return HttpNotFound();
            }

            var result = await UserManager.DeleteAsync(applicationUser);
            if (result.Succeeded)
            {
                return RedirectToAction("Index");
            }

            AddErrors(result);
            return View(applicationUser);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }
        public ActionResult EditRole(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var user = UserManager.FindById(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            // Lấy danh sách tất cả vai trò
            ViewBag.RoleList = new SelectList(RoleManager.Roles.ToList(), "Name", "Name");

            // Lấy vai trò hiện tại của người dùng
            var userRoles = UserManager.GetRoles(id);
            ViewBag.UserRoles = userRoles;

            return View(user);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditRole(string id, string selectedRole)
        {
            var user = UserManager.FindById(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            // Xóa tất cả vai trò hiện tại của người dùng
            var currentRoles = UserManager.GetRoles(id);
            UserManager.RemoveFromRoles(id, currentRoles.ToArray());

            // Gán vai trò mới cho người dùng
            if (!string.IsNullOrEmpty(selectedRole))
            {
                UserManager.AddToRole(id, selectedRole);
            }

            return RedirectToAction("Index");
        }

    }
    public class ApplicationRoleManager : RoleManager<IdentityRole>
    {
        public ApplicationRoleManager(IRoleStore<IdentityRole, string> roleStore)
            : base(roleStore)
        {
        }

        public static ApplicationRoleManager Create(IdentityFactoryOptions<ApplicationRoleManager> options, IOwinContext context)
        {
            var roleStore = new RoleStore<IdentityRole>(context.Get<ApplicationDbContext>());
            return new ApplicationRoleManager(roleStore);
        }
    }

}
