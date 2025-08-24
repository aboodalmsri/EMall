namespace E_Mall.Models
{
    public class Category
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string? Image { get; set; }
        public int StoreCount { get; set; } = 0;

        // Navigation properties
        public virtual ICollection<Store> Stores { get; set; }
    }
}
