namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateIdentityTables : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.CartDetails", "BookID", "dbo.Books");
            DropForeignKey("dbo.Books", "CategoryID", "dbo.Categories");
            DropForeignKey("dbo.OrderDetails", "BookID", "dbo.Books");
            DropForeignKey("dbo.OrderDetails", "OrderID", "dbo.Orders");
            DropForeignKey("dbo.Payments", "OrderID", "dbo.Orders");
            DropForeignKey("dbo.Orders", "UserID", "dbo.Users");
            DropForeignKey("dbo.CartDetails", "CartID", "dbo.Carts");
            DropForeignKey("dbo.Carts", "UserID", "dbo.Users");
            DropIndex("dbo.Carts", new[] { "UserID" });
            DropIndex("dbo.CartDetails", new[] { "CartID" });
            DropIndex("dbo.CartDetails", new[] { "BookID" });
            DropIndex("dbo.Books", new[] { "CategoryID" });
            DropIndex("dbo.OrderDetails", new[] { "OrderID" });
            DropIndex("dbo.OrderDetails", new[] { "BookID" });
            DropIndex("dbo.Orders", new[] { "UserID" });
            DropIndex("dbo.Payments", new[] { "OrderID" });
            DropTable("dbo.Users");
            DropTable("dbo.Carts");
            DropTable("dbo.CartDetails");
            DropTable("dbo.Books");
            DropTable("dbo.Categories");
            DropTable("dbo.OrderDetails");
            DropTable("dbo.Orders");
            DropTable("dbo.Payments");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Payments",
                c => new
                    {
                        PaymentID = c.Int(nullable: false, identity: true),
                        OrderID = c.Int(),
                        PaymentMethod = c.String(),
                        PaymentDate = c.DateTime(),
                        PaymentStatus = c.String(),
                        CreatedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.PaymentID);
            
            CreateTable(
                "dbo.Orders",
                c => new
                    {
                        OrderID = c.Int(nullable: false, identity: true),
                        UserID = c.Int(),
                        OrderDate = c.DateTime(),
                        TotalAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ShippingAddress = c.String(),
                        Status = c.String(),
                        CreatedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.OrderID);
            
            CreateTable(
                "dbo.OrderDetails",
                c => new
                    {
                        OrderDetailID = c.Int(nullable: false, identity: true),
                        OrderID = c.Int(),
                        BookID = c.Int(),
                        Quantity = c.Int(nullable: false),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CreatedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.OrderDetailID);
            
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        CategoryID = c.Int(nullable: false, identity: true),
                        CategoryName = c.String(),
                        CreatedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.CategoryID);
            
            CreateTable(
                "dbo.Books",
                c => new
                    {
                        BookID = c.Int(nullable: false, identity: true),
                        Title = c.String(),
                        Author = c.String(),
                        Publisher = c.String(),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ISBN = c.String(),
                        Stock = c.Int(nullable: false),
                        CategoryID = c.Int(),
                        ImageURL = c.String(),
                        Description = c.String(),
                        CreatedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.BookID);
            
            CreateTable(
                "dbo.CartDetails",
                c => new
                    {
                        CartDetailID = c.Int(nullable: false, identity: true),
                        CartID = c.Int(),
                        BookID = c.Int(),
                        Quantity = c.Int(nullable: false),
                        CreatedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.CartDetailID);
            
            CreateTable(
                "dbo.Carts",
                c => new
                    {
                        CartID = c.Int(nullable: false, identity: true),
                        UserID = c.Int(),
                        CreatedDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.CartID);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        UserID = c.Int(nullable: false, identity: true),
                        FullName = c.String(),
                        Email = c.String(),
                        Password = c.String(),
                        Phone = c.String(),
                        Address = c.String(),
                        Role = c.String(),
                        CreatedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.UserID);
            
            CreateIndex("dbo.Payments", "OrderID");
            CreateIndex("dbo.Orders", "UserID");
            CreateIndex("dbo.OrderDetails", "BookID");
            CreateIndex("dbo.OrderDetails", "OrderID");
            CreateIndex("dbo.Books", "CategoryID");
            CreateIndex("dbo.CartDetails", "BookID");
            CreateIndex("dbo.CartDetails", "CartID");
            CreateIndex("dbo.Carts", "UserID");
            AddForeignKey("dbo.Carts", "UserID", "dbo.Users", "UserID");
            AddForeignKey("dbo.CartDetails", "CartID", "dbo.Carts", "CartID");
            AddForeignKey("dbo.Orders", "UserID", "dbo.Users", "UserID");
            AddForeignKey("dbo.Payments", "OrderID", "dbo.Orders", "OrderID");
            AddForeignKey("dbo.OrderDetails", "OrderID", "dbo.Orders", "OrderID");
            AddForeignKey("dbo.OrderDetails", "BookID", "dbo.Books", "BookID");
            AddForeignKey("dbo.Books", "CategoryID", "dbo.Categories", "CategoryID");
            AddForeignKey("dbo.CartDetails", "BookID", "dbo.Books", "BookID");
        }
    }
}
