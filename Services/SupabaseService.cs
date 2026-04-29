using System.Text.Json;

public class SupabaseService
{
    private readonly HttpClient _client;
    private readonly ILogger<SupabaseService> _logger;

    public SupabaseService(IHttpClientFactory factory, ILogger<SupabaseService> logger)
    {
        _client = factory.CreateClient("SupabaseClient");
        _logger = logger;
    }

    public async Task<List<Product>?> GetAll()
    {
        try
        {
            var response = await _client.GetAsync("products?select=*");
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            var products = JsonSerializer.Deserialize<List<Product>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            return products;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all products");
            return null;
        }
    }

    public async Task<Product?> GetById(long id)
    {
        try
        {
            var response = await _client.GetAsync($"products?select=*&id=eq.{id}");
            response.EnsureSuccessStatusCode();
            
            var content = await response.Content.ReadAsStringAsync();
            var products = JsonSerializer.Deserialize<List<Product>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            return products?.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product {Id}", id);
            return null;
        }
    }

    public async Task<Product?> Create(Product product)
    {
        try
        {
            var productToCreate = new
            {
                name = product.Name,
                description = product.Description
            };
            
            var response = await _client.PostAsJsonAsync("products", productToCreate);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Create failed: {Error}", error);
                return null;
            }
            
            var content = await response.Content.ReadAsStringAsync();
            var createdProducts = JsonSerializer.Deserialize<List<Product>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            return createdProducts?.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return null;
        }
    }

    public async Task<Product?> Update(long id, object updateData)
    {
        try
        {
            var response = await _client.PatchAsJsonAsync($"products?id=eq.{id}", updateData);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Update failed: {Error}", error);
                return null;
            }
            
            var content = await response.Content.ReadAsStringAsync();
            var updatedProducts = JsonSerializer.Deserialize<List<Product>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            return updatedProducts?.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {Id}", id);
            return null;
        }
    }

    public async Task<bool> Delete(long id)
    {
        try
        {
            var response = await _client.DeleteAsync($"products?id=eq.{id}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {Id}", id);
            return false;
        }
    }
}
