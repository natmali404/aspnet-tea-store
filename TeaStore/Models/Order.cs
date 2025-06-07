using Microsoft.AspNetCore.Identity;

namespace Lab10.Models
{
    public class Order
    {
        public int Id { get; set; }
        //fk
        public string UserId { get; set; }
        public int AddressId { get; set; }
        public string PaymentMethod { get; set; }

    }
}
