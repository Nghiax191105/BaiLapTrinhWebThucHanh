using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;
using Microsoft.AspNet.Identity;
using System.Data.Entity.Validation;

namespace WebApplication1.Controllers
{
    public class CartsController : Controller
    {
        private DefaultConnectionEntities db = new DefaultConnectionEntities();
       
        // GET: Carts
        public ActionResult Index()
        {
            // Kiểm tra đăng nhập
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.ShowFilter = false;

            string userId = User.Identity.GetUserId(); // Sử dụng phương thức có sẵn của Identity

                // Lấy giỏ hàng của người dùng hiện tại kèm thông tin sách
                var cart = db.Carts
                    .Include(c => c.CartDetails.Select(cd => cd.Book))
                    .FirstOrDefault(c => c.UserID == userId);

                // Nếu giỏ hàng chưa tồn tại, tạo mới
                if (cart == null)
                {
                    cart = new Cart
                    {
                        UserID = userId,
                        CreatedDate = DateTime.Now
                    };
                    db.Carts.Add(cart);
                    db.SaveChanges();
                }

                // Tính tổng tiền và lưu vào ViewBag
                decimal totalAmount = 0;
                if (cart.CartDetails != null && cart.CartDetails.Any())
                {
                    totalAmount = cart.CartDetails.Sum(cd => cd.Quantity * cd.Book.Price);
                }
                ViewBag.TotalAmount = totalAmount;

                return View(cart);
            }



        // GET: Carts/Details/5
        public ActionResult Details(int? id)
        {
            ViewBag.ShowFilter = false;
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Cart cart = db.Carts.Find(id);
            if (cart == null)
            {
                return HttpNotFound();
            }
            return View(cart);
        }

        // GET: Carts/Create
        public ActionResult Create()
        {
            ViewBag.ShowFilter = false;
            ViewBag.UserId = new SelectList(db.AspNetUsers, "Id", "UserName"); // Thay đổi thành AspNetUsers
            return View();
        }

        // POST: Carts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CartID,UserID,CreatedDate")] Cart cart)
        {
            if (ModelState.IsValid)
            {
                db.Carts.Add(cart);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.UserID = new SelectList(db.AspNetUsers, "Id", "UserName", cart.UserID);
            return View(cart);
        }

        // GET: Carts/Edit/5
        public ActionResult Edit(int? id)
        {
            ViewBag.ShowFilter = false;
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Cart cart = db.Carts.Find(id);
            if (cart == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserID = new SelectList(db.AspNetUsers, "Id", "UserName", cart.UserID);
            return View(cart);
        }

        // POST: Carts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CartID,UserID,CreatedDate")] Cart cart)
        {
            if (ModelState.IsValid)
            {
                db.Entry(cart).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.UserID = new SelectList(db.AspNetUsers, "Id", "UserName", cart.UserID);
            return View(cart);
        }

        // GET: Carts/Delete/5
        public ActionResult Delete(int? id)
        {
            ViewBag.ShowFilter = false;
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Cart cart = db.Carts.Find(id);
            if (cart == null)
            {
                return HttpNotFound();
            }
            return View(cart);
        }

        // POST: Carts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Cart cart = db.Carts.Find(id);
            db.Carts.Remove(cart);
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

        public ActionResult BuyNow(int bookId, int quantity = 1)
        {
            return AddToCart(bookId, quantity, true);
        }

        public ActionResult AddToCart(int bookId, int quantity = 1, bool buyNow = false)
        {
            try
            {
                string userId = GetCurrentUserId();
                var cart = db.Carts.FirstOrDefault(c => c.UserID == userId);

                if (cart == null)
                {
                    cart = new Cart { UserID = userId, CreatedDate = DateTime.Now };
                    db.Carts.Add(cart);
                    db.SaveChanges();
                }

                var book = db.Books.Find(bookId);
                if (book == null)
                {
                    if (Request.IsAjaxRequest())
                    {
                        return Json(new { success = false, message = "Sản phẩm không tồn tại" }, JsonRequestBehavior.AllowGet);
                    }
                    TempData["ErrorMessage"] = "Sản phẩm không tồn tại.";
                    return RedirectToAction("Index", "Books");
                }

                var cartDetail = db.CartDetails.FirstOrDefault(cd => cd.CartID == cart.CartID && cd.BookID == bookId);

                if (cartDetail == null)
                {
                    cartDetail = new CartDetail { CartID = cart.CartID, BookID = bookId, Quantity = quantity };
                    db.CartDetails.Add(cartDetail);
                }
                else
                {
                    cartDetail.Quantity += quantity;
                }

                db.SaveChanges();

                // Tính tổng số lượng trong giỏ hàng
                var cartCount = db.CartDetails.Where(cd => cd.CartID == cart.CartID).Sum(cd => cd.Quantity);

                if (Request.IsAjaxRequest())
                {
                    return Json(new { success = true, cartCount = cartCount }, JsonRequestBehavior.AllowGet);
                }

                if (buyNow)
                {
                    return RedirectToAction("Index");
                }

                TempData["SuccessMessage"] = "Đã thêm sản phẩm vào giỏ hàng!";

                if (Request.UrlReferrer != null)
                {
                    return Redirect(Request.UrlReferrer.ToString());
                }

                return RedirectToAction("Index", "Books");
            }
            catch (Exception ex)
            {
                // Ghi log lỗi
                System.Diagnostics.Debug.WriteLine($"Error in AddToCart: {ex.Message}");
                // hoặc
                // Logger.Error($"Error in AddToCart: {ex.Message}", ex);

                if (Request.IsAjaxRequest())
                {
                    return Json(new
                    {
                        success = false,
                        message = "Có lỗi xảy ra: " + ex.Message
                    }, JsonRequestBehavior.AllowGet);
                }

                TempData["ErrorMessage"] = $"Có lỗi xảy ra khi thêm vào giỏ hàng: {ex.Message}";
                return RedirectToAction("Index", "Books");
            }
        }



        // Phương thức giả để lấy UserID của người dùng hiện tại
        private string GetCurrentUserId()
        {
            return User.Identity.GetUserId(); // Lấy UserId kiểu string
        }
        // Phương thức dành cho admin để xem danh sách giỏ hàng của tất cả người dùng
        [Authorize(Roles = "Admin")]
        public ActionResult AdminIndex()
        {
            var carts = db.Carts.Include(c => c.AspNetUser).Include(c => c.CartDetails.Select(cd => cd.Book)).ToList();
            return View(carts);
        }
        public ActionResult ConfirmPurchase()
        {
            string userId = GetCurrentUserId(); // Lấy ID người dùng hiện tại (kiểu string)
            var cart = db.Carts.Include(c => c.CartDetails)
                               .FirstOrDefault(c => c.UserID == userId); // Sử dụng UserID kiểu string

            if (cart != null && cart.CartDetails.Any())
            {
                // Xử lý logic thanh toán (có thể chuyển qua bảng Orders hoặc trạng thái đã thanh toán)
                // Xóa các mục trong giỏ hàng sau khi thanh toán
                db.CartDetails.RemoveRange(cart.CartDetails);
                db.SaveChanges(); // Lưu thay đổi
            }

            return RedirectToAction("Index"); // Quay lại trang Index
        }
        [HttpPost]
        public ActionResult UpdateQuantity(int bookId, int quantity)
        {
            try
            {
                string userId = User.Identity.GetUserId();
                var cart = db.Carts.FirstOrDefault(c => c.UserID == userId);

                if (cart != null)
                {
                    var cartDetail = db.CartDetails.FirstOrDefault(cd => cd.CartID == cart.CartID && cd.BookID == bookId);
                    if (cartDetail != null)
                    {
                        cartDetail.Quantity = quantity;
                        db.SaveChanges();
                    }
                }
                return RedirectToAction("Index");
            }
            catch
            {
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult RemoveFromCart(int bookId)
        {
            try
            {
                string userId = User.Identity.GetUserId();
                var cart = db.Carts.FirstOrDefault(c => c.UserID == userId);

                if (cart != null)
                {
                    var cartDetail = db.CartDetails.FirstOrDefault(cd => cd.CartID == cart.CartID && cd.BookID == bookId);
                    if (cartDetail != null)
                    {
                        db.CartDetails.Remove(cartDetail);
                        db.SaveChanges();
                    }
                }
                return RedirectToAction("Index");
            }
            catch
            {
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult Checkout()
        {
            try
            {
                string shippingAddress = TempData["ShippingAddress"] as string;

                if (string.IsNullOrEmpty(shippingAddress))
                {
                    TempData["Error"] = "Vui lòng nhập địa chỉ giao hàng!";
                    return RedirectToAction("Index");
                }

                string userId = User.Identity.GetUserId();
                var cart = db.Carts
                    .Include(c => c.CartDetails.Select(cd => cd.Book))
                    .FirstOrDefault(c => c.UserID == userId);

                if (cart == null || !cart.CartDetails.Any())
                {
                    TempData["Error"] = "Giỏ hàng trống!";
                    return RedirectToAction("Index");
                }

                // Tạo đơn hàng mới
                var order = new Order
                {
                    UserID = userId,
                    OrderDate = DateTime.Now,
                    Status = "Pending",
                    ShippingAddress = shippingAddress,
                    TotalAmount = cart.CartDetails.Sum(cd => cd.Book.Price * cd.Quantity),
                    CreatedAt = DateTime.Now
                };

                db.Orders.Add(order);

                // Tạo chi tiết đơn hàng từ giỏ hàng
                foreach (var cartDetail in cart.CartDetails)
                {
                    var orderDetail = new OrderDetail
                    {
                        OrderID = order.OrderID,
                        BookID = cartDetail.BookID,
                        Quantity = cartDetail.Quantity,
                        Price = cartDetail.Book.Price,
                        CreatedAt = DateTime.Now
                    };
                    db.OrderDetails.Add(orderDetail);
                }

                // Xóa giỏ hàng
                db.CartDetails.RemoveRange(cart.CartDetails);

                db.SaveChanges();

                TempData["Success"] = "Đặt hàng thành công!";
                return RedirectToAction("OrderConfirmation", "Orders", new { orderId = order.OrderID });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi đặt hàng: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult TemporaryShippingAddress(string shippingAddress)
        {
            if (string.IsNullOrEmpty(shippingAddress))
            {
                TempData["Error"] = "Vui lòng nhập địa chỉ giao hàng!";
            }
            else
            {
                TempData["ShippingAddress"] = shippingAddress; // Lưu vào TempData để giữ giá trị qua yêu cầu HTTP
            }

            return RedirectToAction("Index");
        }






    }
}
