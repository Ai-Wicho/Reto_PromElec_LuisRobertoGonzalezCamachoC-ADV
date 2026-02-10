using Moq;
using Xunit;
using TiendaPromElec.Controllers;
using ProductApi.Models;
using TiendaPromElec.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TiendaPromElec.Tests
{
    public class ProductControllerTests
    {
        private readonly Mock<IProductRepository> _mockRepo;
        private readonly Mock<ILogger<ProductController>> _mockLogger;
        private readonly ProductController _controller;

        public ProductControllerTests()
        {
            // Mocks
            _mockRepo = new Mock<IProductRepository>();
            _mockLogger = new Mock<ILogger<ProductController>>();
            
        
            _controller = new ProductController(_mockRepo.Object, _mockLogger.Object);
        }

        // Good Path Tests 

        [Fact]
        public async Task GetProducts_ReturnsOkResult_WithListOfProducts()
        {
            // Arrange
            var products = new List<Product> { new Product { Name = "Laptop", Description = "Gaming", Brand = "Dell" } };
            _mockRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(products);

            // Act
            var result = await _controller.GetProducts();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result); 
            var returnProducts = Assert.IsType<List<Product>>(okResult.Value);
            Assert.Single(returnProducts); 
        }

        [Fact]
        public async Task GetProduct_ReturnsProduct_WhenIdExists()
        {
            // Arrange
            var product = new Product { Id = 1, Name = "Mouse", Description = "Wireless", Brand = "Logitech" };
            _mockRepo.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(product);

            // Act
            var result = await _controller.GetProduct(1);

            // Assert
            Assert.Equal(product, result.Value);
        }

        [Fact]
        public async Task PostProduct_ReturnsCreatedAtAction()
        {
            // Arrange
            var newProduct = new Product { Name = "Teclado", Description = "Mecánico", Brand = "Razer" };
            _mockRepo.Setup(repo => repo.AddAsync(newProduct)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.PostProduct(newProduct);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal("GetProduct", createdResult.ActionName);
        }

        [Fact]
        public async Task PutProduct_ReturnsNoContent_WhenUpdateIsSuccessful()
        {
            // Arrange
            var product = new Product { Id = 1, Name = "Monitor", Description = "4K", Brand = "LG" };
            _mockRepo.Setup(repo => repo.UpdateAsync(product)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.PutProduct(1, product);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteProduct_ReturnsNoContent_WhenProductDeleted()
        {
            // Arrange
            _mockRepo.Setup(repo => repo.Exists(1)).Returns(true);
            _mockRepo.Setup(repo => repo.DeleteAsync(1)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteProduct(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        // Bad Path Tests

        [Fact]
        public async Task GetProduct_ReturnsNotFound_WhenIdDoesNotExist()
        {
            // Arrange
            _mockRepo.Setup(repo => repo.GetByIdAsync(99)).ReturnsAsync((Product)null);

            // Act
            var result = await _controller.GetProduct(99);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task PutProduct_ReturnsBadRequest_WhenIdsDoNotMatch()
        {
            // Arrange
            var product = new Product { Id = 2, Name = "Error", Description = "Test", Brand = "Test" }; 

            // Act
            var result = await _controller.PutProduct(1, product); 

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task PutProduct_ReturnsNotFound_WhenProductToUpdateDoesNotExist()
        {
            // Arrange
            var product = new Product { Id = 1, Name = "Fantasma", Description = "Descripción de prueba", Brand = "Marca de prueba" };

            _mockRepo.Setup(repo => repo.UpdateAsync(product)).ThrowsAsync(new System.Exception());
            _mockRepo.Setup(repo => repo.Exists(1)).Returns(false);

            // Act
            var result = await _controller.PutProduct(1, product);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteProduct_ReturnsNotFound_WhenProductDoesNotExist()
        {
            // Arrange
            _mockRepo.Setup(repo => repo.Exists(99)).Returns(false);

            // Act
            var result = await _controller.DeleteProduct(99);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task PutProduct_ThrowsException_WhenErrorIsNotConcurrency()
        {
             // Arrange
            var product = new Product 
            { 
                Id = 1, 
                Name = "Error Grave", 
                Description = "Descripción de prueba",
                Brand = "Marca de prueba"             
            };
            var exception = new System.Exception("Error de conexión");
            
            _mockRepo.Setup(repo => repo.UpdateAsync(product)).ThrowsAsync(exception);
            _mockRepo.Setup(repo => repo.Exists(1)).Returns(true); 

            // Act & Assert
            await Assert.ThrowsAsync<System.Exception>(() => _controller.PutProduct(1, product));
        }
    }
}