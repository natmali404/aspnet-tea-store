using Microsoft.AspNetCore.Identity;

namespace Lab10.Models
{
    public class Address
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Street { get; set; }
        public string PostalCode { get; set; }
        public string City { get; set; }

        //fk
        public string UserId { get; set; }
    }
}
