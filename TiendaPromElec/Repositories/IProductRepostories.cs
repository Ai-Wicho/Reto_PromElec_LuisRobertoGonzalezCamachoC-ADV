using ProductApi.Models;

namespace TiendaPromElec.Repositories // Asegúrate que el namespace coincida
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(long id);
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(long id);
        bool Exists(long id);
    }
}