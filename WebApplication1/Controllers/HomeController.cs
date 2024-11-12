using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private DefaultConnectionEntities db = new DefaultConnectionEntities();
        public ActionResult Index()
        {
            // Lấy danh sách tất cả các thể loại từ cơ sở dữ liệu
            var categories = db.Categories.ToList();
            return View(categories); // Truyền danh sách thể loại vào view
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            ViewBag.Categories = db.Categories.ToList();
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            ViewBag.Categories = db.Categories.ToList();
            return View();
        }
    }
}