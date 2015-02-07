using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using SportsStore.Domain.Entities;
using Moq;
using SportsStore.Domain.Abstract;
using SportsStore.WebUI.Controllers;
using System.Web.Mvc;
using SportsStore.WebUI.Models;

namespace SportsStore.UnitTests
{
    [TestClass]
    public class CartTests
    {
        [TestMethod]
        public void Can_Add_New_Lines()
        {
            var p1 = new Product { ProductID = 1, Name = "P1" };
            var p2 = new Product { ProductID = 2, Name = "P2" };

            var target = new Cart();
            target.AddItem(p1, 1);
            target.AddItem(p2, 1);

            var result = target.Lines.ToArray();

            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(p1, result[0].Product);
            Assert.AreEqual(1, result[0].Quantity);
            Assert.AreEqual(p2, result[1].Product);
            Assert.AreEqual(1, result[1].Quantity);

        }

        [TestMethod]
        public void Can_Add_Quantity_For_Existing_Lines()
        {
            var p1 = new Product { ProductID = 1, Name = "P1" };
            var p2 = new Product { ProductID = 2, Name = "P2" };

            var target = new Cart();
            target.AddItem(p1, 1);
            target.AddItem(p2, 1);
            target.AddItem(p1, 10);

            var result = target.Lines.OrderBy(c => c.Product.ProductID).ToArray();
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual(p1, result[0].Product);
            Assert.AreEqual(11, result[0].Quantity);
            Assert.AreEqual(p2, result[1].Product);
            Assert.AreEqual(1, result[1].Quantity);

        }

        [TestMethod]
        public void Can_Remove_Line()
        {
            var p1 = new Product { ProductID = 1, Name = "P1" };
            var p2 = new Product { ProductID = 2, Name = "P2" };
            var p3 = new Product { ProductID = 3, Name = "P3" };

            var target = new Cart();
            target.AddItem(p1, 1);
            target.AddItem(p2, 3);
            target.AddItem(p3, 5);
            target.AddItem(p2, 1);

            target.RemoveLine(p2);

            Assert.AreEqual(0, target.Lines.Where(c => c.Product == p2).Count());
            Assert.AreEqual(2, target.Lines.Count());


        }

        [TestMethod]
        public void Can_Calculate_Cart_Total()
        {
            var p1 = new Product { ProductID = 1, Name = "P1", Price = 100M };
            var p2 = new Product { ProductID = 2, Name = "P2", Price = 50M };

            var target = new Cart();
            target.AddItem(p1, 1);
            target.AddItem(p2, 1);
            target.AddItem(p1, 3);
            var result = target.ComputeTotalValue();

            Assert.AreEqual(450M, result);
        }

        [TestMethod]
        public void Can_Clear_Contents()
        {
            var p1 = new Product { ProductID = 1, Name = "P1", Price = 100M };
            var p2 = new Product { ProductID = 2, Name = "P2", Price = 50M };

            var target = new Cart();
            target.AddItem(p1, 1);
            target.AddItem(p2, 1);

            target.Clear();
            var result = target.Lines.Count();

            Assert.AreEqual(0, result);

        }

        [TestMethod]
        public void Can_Add_To_Cart()
        {
            var mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[]
                {
                    new Product {ProductID = 1, Name="P1", Category="Apples"}
                }.AsQueryable());

            var cart = new Cart();

            var target = new CartController(mock.Object, null);

            target.AddToCart(cart, 1, null);

            Assert.AreEqual(1, cart.Lines.Count());
            Assert.AreEqual(1, cart.Lines.ToArray()[0].Product.ProductID);
        }

        [TestMethod]
        public void Adding_Product_To_Cart_Goes_To_Cart_Screen()
        {
            var mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[]
                {
                    new Product {ProductID = 1, Name="P1", Category="Apples"}
                }.AsQueryable());

            var cart = new Cart();

            var target = new CartController(mock.Object, null);

            var result = target.AddToCart(cart, 2, "myUrl");

            Assert.AreEqual("Index", result.RouteValues["action"]);
            Assert.AreEqual("myUrl", result.RouteValues["returnUrl"]);
        }

        [TestMethod]
        public void Can_View_Cart_Contents()
        {
            var cart = new Cart();
            var target = new CartController(null, null);
            var result = target.Index(cart, "myUrl").ViewData.Model as CartIndexViewModel;

            Assert.AreSame(cart, result.Cart);
            Assert.AreEqual("myUrl", result.ReturnUrl);
        }

        [TestMethod]
        public void Cannot_Checkout_Empty_Cart()
        {
            var mock = new Mock<IOrderProcessor>();
            var cart = new Cart();
            var shippingDetials = new ShippingDetails();
            var target = new CartController(null, mock.Object);

            var result = target.Checkout(cart, shippingDetials);
            mock.Verify(m => m.ProcessOrder(It.IsAny<Cart>(), It.IsAny<ShippingDetails>()), Times.Never());

            Assert.AreEqual("", result.ViewName);
            Assert.AreEqual(false, result.ViewData.ModelState.IsValid);
        }

        [TestMethod]
        public void Cannot_Checkout_Invalid_ShippingDetails()
        {
            var mock = new Mock<IOrderProcessor>();
            var cart = new Cart();
            var shippingDetails = new ShippingDetails();
            var target = new CartController(null, mock.Object);
            target.ModelState.AddModelError("error", "error");

            var result = target.Checkout(cart, shippingDetails);
            mock.Verify(m => m.ProcessOrder(It.IsAny<Cart>(), It.IsAny<ShippingDetails>()), Times.Never());

            Assert.AreEqual("", result.ViewName);
            Assert.AreEqual(false, result.ViewData.ModelState.IsValid);
        }

        [TestMethod]
        public void Can_Checkout_And_Submit_Order()
        {
            var mock = new Mock<IOrderProcessor>();
            var cart = new Cart();
            cart.AddItem(new Product(), 1);
            var shippingDetails = new ShippingDetails();
            var target = new CartController(null, mock.Object);

            var result = target.Checkout(cart, shippingDetails);
            mock.Verify(m => m.ProcessOrder(It.IsAny<Cart>(), It.IsAny<ShippingDetails>()), Times.Once());

            Assert.AreEqual("Completed", result.ViewName);
            Assert.AreEqual(true, result.ViewData.ModelState.IsValid);
            
        }


    }
}
