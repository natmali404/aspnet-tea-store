using System.ComponentModel.DataAnnotations;

namespace Lab10.Models
{
    public class Article
    {

        [Display(Name = "Product ID")]
        public int Id { get; set; }

        [Display(Name = "Product name")]
        public string Name { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Price must be a positive number.")]
        public decimal Price { get; set; }

        [Display(Name = "Image")]
        public string? ImagePath { get; set; }

        [Display(Name = "Category")]
        public int? CategoryId { get; set; }


        //konstruktor domyslny
        public Article()
        {
        }

        //konstruktor kopiujacy
        public Article(Article article)
        {
            Id = article.Id;
            Name = article.Name;
            Price = article.Price;
            ImagePath = article.ImagePath;
            CategoryId = article.CategoryId;
        }

    }
}
