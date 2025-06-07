using System.ComponentModel.DataAnnotations;

namespace Lab10.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        //konstruktor domyslny
        public Category()
        {
        }

        //konstruktor kopiujacy
        public Category(Category category)
        {
            Id = category.Id;
            Name = category.Name;
        }

    }
}
