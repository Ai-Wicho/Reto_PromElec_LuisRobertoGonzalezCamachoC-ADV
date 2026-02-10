using Microsoft.AspNetCore.Mvc;
using ProductApi.Models;
using TiendaPromElec.Repositories; // Importante
using Microsoft.AspNetCore.Authorization; 

namespace TiendaPromElec.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly IProductRepository _repository; 
        private readonly ILogger<ProductController> _logger;

        public ProductController(IProductRepository repository, ILogger<ProductController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            return Ok(await _repository.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(long id)
        {
            _logger.LogInformation("Buscando producto ID: {Id}", id);
            var product = await _repository.GetByIdAsync(id);

            if (product == null)
            {
                _logger.LogWarning("Producto {Id} no encontrado", id);
                return NotFound();
            }
            return product;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(long id, Product product)
        {
            if (id != product.Id) return BadRequest();

            try
            {
                await _repository.UpdateAsync(product);
            }
            catch (Exception)
            {
                if (!_repository.Exists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            await _repository.AddAsync(product);
            return CreatedAtAction("GetProduct", new { id = product.Id }, product);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(long id)
        {
            if (!_repository.Exists(id)) return NotFound();
            await _repository.DeleteAsync(id);
            return NoContent();
        }
    }
}