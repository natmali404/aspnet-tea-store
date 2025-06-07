using Lab10.Data;
using Lab10.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Lab10.Controllers.Api
{
    [Route("api/categories")]
    [ApiController]
    public class CategoryApiController : ControllerBase
    {
        private readonly IRepository<Category> _repository;

        public CategoryApiController(IRepository<Category> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Category>> GetCategories()
        {
            return Ok(_repository.GetAll());
        }

        [HttpGet("{id}")]
        public ActionResult<Category> GetCategory(int id)
        {
            var category = _repository.GetById(id);
            if (category == null)
                return NotFound();

            return Ok(category);
        }

        [HttpPost]
        public ActionResult<Category> CreateCategory([FromBody] Category category)
        {
            if (category == null)
                return BadRequest();

            _repository.Add(category);
            return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateCategory(int id, [FromBody] Category category)
        {
            if (category == null || category.Id != id)
                return BadRequest();

            var existingCategory = _repository.GetById(id);
            if (existingCategory == null)
                return NotFound();

            _repository.Update(category);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteCategory(int id)
        {
            var category = _repository.GetById(id);
            if (category == null)
                return NotFound();

            _repository.Delete(id);
            return NoContent();
        }

        [HttpGet("{id}/next")]
        public ActionResult<Category> GetNextCategory(int id)
        {
            var nextCategory = _repository.GetNext(id);
            return nextCategory == null ? NotFound() : Ok(nextCategory);
        }

        [HttpGet("{id}/previous")]
        public ActionResult<Category> GetPreviousCategory(int id)
        {
            var prevCategory = _repository.GetPrevious(id);
            return prevCategory == null ? NotFound() : Ok(prevCategory);
        }
    }
}
