using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class OrdersController : Controller
    {
        private DefaultConnectionEntities db = new DefaultConnectionEntities();

        // GET: Orders
        public ActionResult Index()
        {

            string userId = User.Identity.GetUserId();
            var orders = db.Orders
                .Include(o => o.OrderDetails.Select(od => od.Book))
                .Where(o => o.UserID == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();
            return View(orders);
        }

        // GET: Orders/Details/5
        public ActionResult Details(int? id)
        {
            ViewBag.ShowFilter = false;
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // GET: Orders/Create
        public ActionResult Create()
        {
            ViewBag.ShowFilter = false;
            ViewBag.UserID = new SelectList(db.AspNetUsers, "Id", "UserName"); // Sử dụng AspNetUsers
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "OrderID,UserID,OrderDate,TotalAmount,ShippingAddress,Status,CreatedAt")] Order order)
        {
            if (ModelState.IsValid)
            {
                db.Orders.Add(order);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.UserID = new SelectList(db.AspNetUsers, "Id", "UserName", order.UserID); // Cập nhật
            return View(order);
        }

        //// GET: Orders/Edit/5
        public ActionResult Edit(int? id)
        {
            ViewBag.ShowFilter = false;
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserID = new SelectList(db.AspNetUsers, "Id", "UserName", order.UserID); // Cập nhật
            return View(order);
        }
        // POST: Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "OrderID,UserID,OrderDate,TotalAmount,ShippingAddress,Status,CreatedAt")] Order order)
        {
            if (ModelState.IsValid)
            {
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.UserID = new SelectList(db.AspNetUsers, "Id", "UserName", order.UserID); // Cập nhật
            return View(order);
        }

        // GET: Orders/Delete/5
        public ActionResult Delete(int? id)
        {
            ViewBag.ShowFilter = false;
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            // Tìm Order cần xóa
            Order order = db.Orders
                            .Include(o => o.OrderDetails) // Bao gồm OrderDetails liên quan
                            .FirstOrDefault(o => o.OrderID == id);

            if (order != null)
            {
                // Xóa các OrderDetails liên quan trước
                db.OrderDetails.RemoveRange(order.OrderDetails);

                // Sau đó xóa Order
                db.Orders.Remove(order);

                // Lưu thay đổi
                db.SaveChanges();
            }

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

        [HttpPost]
        public ActionResult Checkout(string address)
        {
            var userId = GetCurrentUserId();

            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để tiếp tục.";
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrEmpty(address))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập địa chỉ giao hàng.";
                return RedirectToAction("Index", "Carts");
            }

            var cart = db.Carts.Include(c => c.CartDetails.Select(cd => cd.Book))
                                .FirstOrDefault(c => c.UserID == userId);

            if (cart == null || !cart.CartDetails.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn trống.";
                return RedirectToAction("Index", "Carts");
            }

            try
            {
                var order = new Order
                {
                    UserID = userId,
                    OrderDate = DateTime.Now,
                    ShippingAddress = address,
                    Status = "Pending",  // Gán trạng thái mặc định
                    OrderDetails = new List<OrderDetail>()
                };

                decimal totalAmount = 0;
                foreach (var item in cart.CartDetails)
                {
                    totalAmount += item.Book.Price * item.Quantity;
                    order.OrderDetails.Add(new OrderDetail
                    {
                        BookID = item.BookID,
                        Quantity = item.Quantity,
                        Price = item.Book.Price
                    });
                }

                

                db.Orders.Add(order);
                db.SaveChanges();

                db.Carts.Remove(cart);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Đơn hàng của bạn đã được đặt thành công!";
                return RedirectToAction("OrderConfirmation", "Orders", new { orderId = order.OrderID });
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var validationError in ex.EntityValidationErrors.SelectMany(e => e.ValidationErrors))
                {
                    ModelState.AddModelError(validationError.PropertyName, validationError.ErrorMessage);
                }

                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xử lý đơn hàng của bạn.";
                return RedirectToAction("Index", "Carts");
            }
        }
        [HttpPost]
        public JsonResult UpdateAddress(string address)
        {
            // Lưu địa chỉ vào Session thay vì lưu vào cơ sở dữ liệu
            Session["UserAddress"] = address;

            return Json(new { message = "Địa chỉ của bạn đã được cập nhật thành công!" });
        }

        public string GetAddress()
        {
            // Lấy địa chỉ từ Session khi cần thiết
            return Session["UserAddress"] as string;
        }

        private string GetCurrentUserId() // Thay đổi kiểu trả về thành string
        {
            return User.Identity.GetUserId(); // Lấy UserID thực tế
        }
        public ActionResult OrderConfirmation(int orderId)
        {
            var order = db.Orders
                .Include(o => o.OrderDetails.Select(od => od.Book))
                .FirstOrDefault(o => o.OrderID == orderId);

            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
        }
        [Authorize(Roles = "Sale, SupperAdmin")]
        public ActionResult OrderList()
        {
            var orders = db.Orders
                .Include(o => o.OrderDetails.Select(od => od.Book))
                .Include(o => o.AspNetUser)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders); // Đảm bảo tên view là "OrderList.cshtml"
        }
        [HttpPost]
        public ActionResult UpdateStatus(int orderId, string status)
        {
            var order = db.Orders.Find(orderId);
            if (order != null)
            {
                order.Status = status;
                db.SaveChanges();
                TempData["SuccessMessage"] = "Cập nhật trạng thái thành công!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng!";
            }
            return RedirectToAction("OrderList");
        }

    }
}
