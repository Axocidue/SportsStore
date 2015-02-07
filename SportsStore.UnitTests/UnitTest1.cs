using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using Moq;
using SportsStore.Domain.Entities;
using SportsStore.Domain.Abstract;
using SportsStore.Domain.Concrete;
using SportsStore.WebUI.Controllers;
using SportsStore.WebUI.Models;
using SportsStore.WebUI.HtmlHelpers;


namespace SportsStore.UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void Can_Paginate()
        {
            var mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(
                new Product[]
                {
                    new Product { ProductID =1, Name = "P1"},
                    new Product { ProductID =2, Name = "P2"},
                    new Product { ProductID =3, Name = "P3"},
                    new Product { ProductID =4, Name = "P4"},
                    new Product { ProductID =5, Name = "P5"},
                });

            var productController = new ProductController(mock.Object);
            productController.PageSize = 3;

            var result = ((ProductListViewModel)productController.List(null, 2).Model).Products;

            var prodArray = result.ToArray();
            Assert.IsTrue(prodArray.Length == 2);
            Assert.AreEqual("P4", prodArray[0].Name);
            Assert.AreEqual("P5", prodArray[1].Name);
        }

        [TestMethod]
        public void Can_Generate_Page_Links()
        {
            HtmlHelper myHelper = null;

            var pagingInfo = new PagingInfo
            {
                CurrentPage = 2,
                TotalItems = 28,
                ItemsPerPage = 10
            };

            var pageUrlDelegate = new Func<int, string>(i => "Page" + i);

            var result = myHelper.PageLinks(pagingInfo, pageUrlDelegate).ToString();

            var expected = @"<a class=""btn btn-default"" href=""Page1"">1</a>" +
                @"<a class=""btn btn-default btn-primary selected"" href=""Page2"">2</a>" +
                @"<a class=""btn btn-default"" href=""Page3"">3</a>";

            Assert.AreEqual(expected, result);

        }

        [TestMethod]
        public void Can_Send_Pagination_View_Model()
        {
            var mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[]
                {
                    new Product {ProductID = 1, Name="P1"},
                    new Product {ProductID = 2, Name="P2"},
                    new Product {ProductID = 3, Name="P3"},
                    new Product {ProductID = 4, Name="P4"},
                    new Product {ProductID = 5, Name="P5"}
                });

            var controller = new ProductController(mock.Object);
            controller.PageSize = 3;

            var result = (ProductListViewModel)controller.List(null, 2).Model;
            var pagingInfo = result.PagingInfo;

            Assert.AreEqual(2, pagingInfo.CurrentPage);
            Assert.AreEqual(3, pagingInfo.ItemsPerPage);
            Assert.AreEqual(5, pagingInfo.TotalItems);
            Assert.AreEqual(2, pagingInfo.TotalPages);

        }

        [TestMethod]
        public void Can_Filter_Products()
        {
            var mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[]
                {
                    new Product {ProductID = 1, Name="P1", Category="Cat1"},
                    new Product {ProductID = 2, Name="P2", Category="Cat2"},
                    new Product {ProductID = 3, Name="P3", Category="Cat3"},
                    new Product {ProductID = 4, Name="P4", Category="Cat1"},
                    new Product {ProductID = 5, Name="P5", Category="Cat2"}
                });

            var controller = new ProductController(mock.Object);
            controller.PageSize = 3;

            var result = ((ProductListViewModel)controller.List("Cat2", 1).Model).Products.ToArray();

            Assert.IsTrue(result[0].Name == "P2" && result[0].Category == "Cat2");
            Assert.IsTrue(result[1].Name == "P5" && result[1].Category == "Cat2");


        }


        [TestMethod]
        public void Can_Create_Categories()
        {
            var mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[]
                {
                    new Product {ProductID = 1, Name = "P1", Category = "Apples"},
                    new Product {ProductID = 2, Name = "P2", Category = "Apples"},
                    new Product {ProductID = 3, Name = "P3", Category = "Plums"},
                    new Product {ProductID = 4, Name = "P4", Category = "Oranges"}
                });

            var target = new NavController(mock.Object);

            var results = ((IEnumerable<string>)target.Menu().Model).ToArray();

            Assert.AreEqual(3, results.Length);
            Assert.AreEqual("Apples", results[0]);
            Assert.AreEqual("Oranges", results[1]);
            Assert.AreEqual("Plums", results[2]);
        }

        [TestMethod]
        public void Can_Indicate_Selected_Catetory()
        {
            var mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[]
                {
                    new Product {ProductID = 1, Name = "P1", Category = "Apples"},
                    new Product {ProductID = 2, Name = "P2", Category = "Apples"},
                    new Product {ProductID = 3, Name = "P3", Category = "Plums"},
                    new Product {ProductID = 4, Name = "P4", Category = "Oranges"}
                });
            var target = new NavController(mock.Object);
            var categoryToSelect = "Apples";

            var result = target.Menu(categoryToSelect).ViewBag.SelectedCategory as string;

            Assert.AreEqual(categoryToSelect, result);
        }

        [TestMethod]
        public void Can_Generate_Category_Specific_Product_Count()
        {
            var mock = new Mock<IProductRepository>();
            mock.Setup(m => m.Products).Returns(new Product[]
                {
                    new Product {ProductID = 1, Name = "P1", Category = "Cat1"},
                    new Product {ProductID = 2, Name = "P2", Category = "Cat2"},
                    new Product {ProductID = 3, Name = "P3", Category = "Cat1"},
                    new Product {ProductID = 4, Name = "P4", Category = "Cat2"},
                    new Product {ProductID = 5, Name = "P5", Category = "Cat3"}
                });
            var target = new ProductController(mock.Object);
            target.PageSize = 3;

            var result1 = (target.List("Cat1").Model as ProductListViewModel).PagingInfo.TotalItems;
            var result2 = (target.List("Cat2").Model as ProductListViewModel).PagingInfo.TotalItems;
            var result3 = (target.List("Cat3").Model as ProductListViewModel).PagingInfo.TotalItems;
            var resultAll = (target.List((string)null).Model as ProductListViewModel).PagingInfo.TotalItems;

            Assert.AreEqual(2, result1);
            Assert.AreEqual(2, result2);
            Assert.AreEqual(1, result3);
            Assert.AreEqual(5, resultAll);


        }
    }
}
