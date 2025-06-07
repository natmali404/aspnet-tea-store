namespace Lab10.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        //fk
        public int OrderId { get; set; }
        public int ArticleId { get; set; }


        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice { get; set; }
    }

}
