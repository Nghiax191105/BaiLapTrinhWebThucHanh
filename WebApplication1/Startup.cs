using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Owin;
using WebApplication1.Models;

[assembly: OwinStartupAttribute(typeof(WebApplication1.Startup))]
namespace WebApplication1
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            CreateRolesAndUsers(); // Gọi phương thức tạo vai trò và người dùng
        }

        // Phương thức để tạo các vai trò và người dùng mặc định
        private void CreateRolesAndUsers()
        {
            using (var context = new ApplicationDbContext())
            {
                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

                // Kiểm tra và tạo vai trò SupperAdmin nếu chưa tồn tại
                if (!roleManager.RoleExists("SupperAdmin"))
                {
                    var role = new IdentityRole("SupperAdmin");
                    roleManager.Create(role);

                    // Tạo tài khoản SupperAdmin mặc định
                    var user = new ApplicationUser
                    {
                        UserName = "aquasaki75@gmail.com",
                        Email = "aquasaki75@gmail.com",
                        EmailConfirmed = true  // Xác nhận email cho tài khoản mặc định
                    };
                    var result = userManager.Create(user, "@Thaito98");

                    // Gán vai trò SupperAdmin cho tài khoản
                    if (result.Succeeded)
                    {
                        userManager.AddToRole(user.Id, "SupperAdmin");
                    }
                }

                // Kiểm tra và tạo vai trò Editor nếu chưa tồn tại
                if (!roleManager.RoleExists("Editor"))
                {
                    var role = new IdentityRole("Editor");
                    roleManager.Create(role);
                }

                // Kiểm tra và tạo vai trò Sale nếu chưa tồn tại
                if (!roleManager.RoleExists("Sale"))
                {
                    var role = new IdentityRole("Sale");
                    roleManager.Create(role);
                }

                // Không cần gọi context.SaveChanges() vì RoleManager và UserManager đã thực hiện thay đổi trong cơ sở dữ liệu
            }
        }
    }
}
