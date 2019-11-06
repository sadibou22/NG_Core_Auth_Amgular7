using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NG_Core_Auth.Data;
using NG_Core_Auth.Models;

namespace NG_Core_Auth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public ProductController(ApplicationDbContext db)
        {
            _db = db;
        }
        // GET: api/Product
        [HttpGet]
        public IActionResult GetProducts()
        {
            var products = _db.Products.ToList();
            return Ok(products);
        }

        // GET: api/Product/5
        [HttpGet("{id}", Name = "Get")]
        public IActionResult GetProduct(int id)
        {
            var product = _db.Products.Where(x=>x.ProductId == id).FirstOrDefault();
            return Ok(product);
        }

        // POST: api/Product
        [HttpPost]
        public async Task<IActionResult> AddProduct([FromBody] ProductModel prodCommand)
        {
            var prod = new ProductModel
            {
                Name = prodCommand.Name,
                Description = prodCommand.Description,
                Price = prodCommand.Price,
                OutOfStock = prodCommand.OutOfStock,
                ImageUrl = prodCommand.ImageUrl
            };

            await _db.Products.AddAsync(prod);

            await _db.SaveChangesAsync();

            return Ok();
        }

        // PUT: api/Product/5
        [HttpPut("{id}")]
        public async Task<IActionResult> EditProduct(int id, [FromBody] ProductModel prodCmd)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var product = _db.Products.Where(x => x.ProductId == id).FirstOrDefault();
            if (product == null)
            {
                return NotFound();
            }

            product.Name = prodCmd.Name;
            product.Description = prodCmd.Description;
            product.ImageUrl = prodCmd.ImageUrl;
            product.Price = prodCmd.Price;
            product.OutOfStock = prodCmd.OutOfStock;

            _db.Entry(product).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return Ok(new JsonResult("The Product with id "+ id+" is updated"));
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var product = _db.Products.Where(x => x.ProductId == id).FirstOrDefault();
            if (product == null)
            {
                return NotFound();
            }

            _db.Products.Remove(product);

            await _db.SaveChangesAsync();
            return Ok(new JsonResult("The Product with id " + id + " is deleted"));
        }
    }
}
