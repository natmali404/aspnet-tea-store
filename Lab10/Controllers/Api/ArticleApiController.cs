using Lab10.Data;
using Lab10.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Lab10.Controllers.Api
{
    [Route("api/articles")]
    [ApiController]
    public class ArticleApiController : ControllerBase
    {
        private readonly IRepository<Article> _repository;

        public ArticleApiController(IRepository<Article> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Article>> GetArticles()
        {
            return Ok(_repository.GetAll());
        }

        [HttpGet("{id}")]
        public ActionResult<Article> GetArticle(int id)
        {
            var article = _repository.GetById(id);
            if (article == null)
                return NotFound();

            return Ok(article);
        }

        [HttpPost]
        public ActionResult<Article> CreateArticle([FromBody] Article article)
        {
            if (article == null)
                return BadRequest();

            _repository.Add(article);
            return CreatedAtAction(nameof(GetArticle), new { id = article.Id }, article);
        }

        //[HttpPut("{id}")]
        //public IActionResult UpdateArticle(int id, [FromBody] Article article)
        //{
        //    if (article == null || article.Id != id)
        //        return BadRequest();

        //    var existingArticle = _repository.GetById(id);
        //    if (existingArticle == null)
        //        return NotFound();

        //    _repository.Update(article);
        //    return NoContent();
        //}
        [HttpPut("{id}")]
        public IActionResult UpdateArticle(int id, [FromBody] Article updatedArticle)
        {
            if (updatedArticle == null || updatedArticle.Id != id)
                return BadRequest("Invalid request data.");

            var existingArticle = _repository.GetById(id);
            if (existingArticle == null)
                return NotFound();

            existingArticle.Name = updatedArticle.Name;
            existingArticle.Price = updatedArticle.Price;
            existingArticle.ImagePath = updatedArticle.ImagePath;
            existingArticle.CategoryId = updatedArticle.CategoryId;

            _repository.Update(existingArticle);
            return NoContent();
        }



        [HttpDelete("{id}")]
        public IActionResult DeleteArticle(int id)
        {
            var article = _repository.GetById(id);
            if (article == null)
                return NotFound();

            _repository.Delete(id);
            return NoContent();
        }

        [HttpGet("{id}/next")]
        public ActionResult<Article> GetNextArticle(int id)
        {
            var nextArticle = _repository.GetNext(id);
            return nextArticle == null ? NotFound() : Ok(nextArticle);
        }

        [HttpGet("{id}/previous")]
        public ActionResult<Article> GetPreviousArticle(int id)
        {
            var prevArticle = _repository.GetPrevious(id);
            return prevArticle == null ? NotFound() : Ok(prevArticle);
        }

        //AJAX 
        [HttpGet]
        //[Route("lazy/{startAfterId}/{limit}/{categoryId?}")]
        [Route("lazy")]
        public ActionResult<IEnumerable<Article>> GetArticles(int startAfterId, int limit, int? categoryId)
        {
            var articles = _repository.GetAll();

            if (startAfterId > 0)
            {
                articles = articles.Where(a => a.Id > startAfterId).ToList();
            }

            if (categoryId.HasValue)
            {
                articles = articles.Where(a => a.CategoryId == categoryId.Value).ToList();
            }

            articles = articles.Take(limit).ToList();

            return Ok(articles);
        }



    }
}
