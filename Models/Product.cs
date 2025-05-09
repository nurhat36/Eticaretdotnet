namespace ETicaret.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }  // Resim URL'si
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
