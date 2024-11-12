using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class BooksController : Controller
    {
        private DefaultConnectionEntities db = new DefaultConnectionEntities();

        // GET: Books
        public ActionResult Index(int? categoryId, int? page, string searchString, string sortOrder, decimal? min, decimal? max)
        {
            // Lấy danh sách sách kèm theo thông tin danh mục
            var books = db.Books.Include(b => b.Category);
            ViewBag.Categories = db.Categories.ToList(); // Load danh sách categories

            // Lưu các giá trị để hiển thị lại trên view
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.PriceSortParm = sortOrder == "price" ? "price_desc" : "price";
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentCategory = categoryId;

            // Chỉ gán ViewBag.MinPrice và ViewBag.MaxPrice khi có giá trị
            if (min.HasValue)
                ViewBag.MinPrice = min.Value;
            if (max.HasValue)
                ViewBag.MaxPrice = max.Value;

            // Lấy danh sách categories cho dropdown
            ViewBag.Categories = db.Categories.ToList();

            bool hasFilter = false; // Biến kiểm tra xem có bộ lọc nào được áp dụng không

            // Tìm kiếm theo category
            if (categoryId.HasValue)
            {
                books = books.Where(x => x.CategoryID == categoryId);
                hasFilter = true;
            }

            // Tìm kiếm theo tên sách
            if (!String.IsNullOrEmpty(searchString))
            {
                books = books.Where(s => s.Title.Contains(searchString.Trim())
                                     || s.Author.Contains(searchString.Trim())
                                     || s.Publisher.Contains(searchString.Trim()));
                hasFilter = true;
            }

            // Lọc theo giá
            if (min.HasValue || max.HasValue)
            {
                if (min.HasValue)
                {
                    books = books.Where(p => p.Price >= min.Value);
                }
                if (max.HasValue)
                {
                    books = books.Where(p => p.Price <= max.Value);
                }
                hasFilter = true;
            }

            // Sắp xếp
            switch (sortOrder)
            {
                case "name_desc":
                    books = books.OrderByDescending(s => s.Title);
                    break;
                case "price":
                    books = books.OrderBy(s => s.Price);
                    break;
                case "price_desc":
                    books = books.OrderByDescending(s => s.Price);
                    break;
                default:
                    books = books.OrderBy(s => s.Title);
                    break;
            }

            // Chỉ phân trang khi có bộ lọc
            if (hasFilter)
            {
                int pageSize = 4; // Số sách mỗi trang
                int pageNumber = (page ?? 1);
                ViewBag.HasPaging = true;
                return View(books.ToList().ToPagedList(pageNumber, pageSize));
            }

            // Nếu không có bộ lọc, trả về toàn bộ danh sách
            ViewBag.HasPaging = false;
            return View(books.ToList());
        }

        // GET: Books/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            return View(book);
        }

        // GET: Books/Create
        public ActionResult Create()
        {
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName");
            return View();
        }

        // POST: Books/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "BookID,Title,Author,Publisher,Price,ISBN,Stock,CategoryID,ImageURL,Description,CreatedAt")] Book book, HttpPostedFileBase ImageURL)
        {
            // Kiểm tra xem ISBN có tồn tại trước khi thêm vào cơ sở dữ liệu
            if (db.Books.Any(b => b.ISBN == book.ISBN))
            {
                ModelState.AddModelError("ISBN", "ISBN đã tồn tại. Vui lòng nhập mã ISBN khác.");
                ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", book.CategoryID);
                return View(book);
            }

            if (ModelState.IsValid)
            {
                if (ImageURL != null && ImageURL.ContentLength > 0)
                {
                    // Lưu file vào thư mục Images
                    var fileName = Path.GetFileName(ImageURL.FileName);
                    var path = Path.Combine(Server.MapPath("~/Content/images"), fileName);
                    ImageURL.SaveAs(path);

                    // Gán tên file cho thuộc tính ImageURL của đối tượng Book
                    book.ImageURL = fileName;
                }

                db.Books.Add(book);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", book.CategoryID);
            return View(book);
        }


        // GET: Books/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", book.CategoryID);
            return View(book);
        }

        // POST: Books/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "BookID,Title,Author,Publisher,Price,ISBN,Stock,CategoryID,ImageURL,Description,CreatedAt")] Book book)
        {
            if (ModelState.IsValid)
            {
                db.Entry(book).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", book.CategoryID);
            return View(book);
        }

        // GET: Books/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Book book = db.Books.Find(id);
            if (book == null)
            {
                return HttpNotFound();
            }
            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Book book = db.Books.Find(id);
            db.Books.Remove(book);
            db.SaveChanges();
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

        


        //[ChildActionOnly]
        //public PartialViewResult BooksFilterPartial()
        //{
        //    try
        //    {
        //        var books = db.Books.ToList(); // Đảm bảo db đã được khởi tạo
        //        if (books == null)
        //        {
        //            books = new List<Book>(); // Tránh null bằng cách tạo list rỗng
        //        }
        //        ViewBag.Message = TempData["Message"];
        //        return PartialView("BooksFilter", books);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Ghi log lỗi
        //        System.Diagnostics.Debug.WriteLine($"Error in BooksFilterPartial: {ex.Message}");
        //        // Hoặc có thể lưu thông báo lỗi vào ViewBag
        //        ViewBag.Error = "Có lỗi xảy ra khi tải danh sách sách";
        //        return PartialView("BooksFilter", new List<Book>()); // Trả về list rỗng nếu có lỗi
        //    }
        //}

    }
}
