using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly SupabaseService _service;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(SupabaseService service, ILogger<ProductsController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var products = await _service.GetAll();
        
        if (products == null)
            return StatusCode(500, "Failed to retrieve products");
        
        return Ok(products);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get(long id)
    {
        var product = await _service.GetById(id);

        if (product == null)
            return NotFound($"Product with ID {id} not found");

        return Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDto createDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        var product = new Product
        {
            Name = createDto.Name,
            Description = createDto.Description
        };
        
        var createdProduct = await _service.Create(product);

        if (createdProduct == null)
            return BadRequest("Failed to create product");

        return CreatedAtAction(nameof(Get), new { id = createdProduct.Id }, createdProduct);
    }

    [HttpPatch("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateProductDto updateDto)
    {
        var existingProduct = await _service.GetById(id);
        if (existingProduct == null)
            return NotFound($"Product with ID {id} not found");
        
        var updateData = new Dictionary<string, object>();
        
        if (!string.IsNullOrEmpty(updateDto.Name))
            updateData["name"] = updateDto.Name;
        
        if (updateDto.Description != null)
            updateData["description"] = updateDto.Description;
        
        if (updateData.Count == 0)
            return BadRequest("No valid fields to update");
        
        var updatedProduct = await _service.Update(id, updateData);
        
        if (updatedProduct == null)
            return StatusCode(500, "Failed to update product");
        
        return Ok(updatedProduct);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var existingProduct = await _service.GetById(id);
        if (existingProduct == null)
            return NotFound($"Product with ID {id} not found");
        
        var success = await _service.Delete(id);

        if (!success)
            return StatusCode(500, "Failed to delete product");

        return NoContent();
    }
}
