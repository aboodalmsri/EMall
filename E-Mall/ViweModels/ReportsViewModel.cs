using System.Collections.Generic;
using E_Mall.Models; // Make sure this namespace is correct

namespace E_Mall.Models.ViewModels
{
    public class ReportsViewModel
    {
        //public decimal TotalSales { get; set; }
        //public decimal TotalProfit { get; set; }
        //public int TotalOrders { get; set; }
        public List<ProductPerformance> ProductPerformances { get; set; } = new List<ProductPerformance>();
        public List<TopStore> TopStores { get; set; } = new List<TopStore>();
    }
    public class ProductPerformance
    {
        public string ProductName { get; set; }
        public int SalesCount { get; set; }
        public double AverageRating { get; set; }
    }

    public class TopStore
    {
        public string StoreName { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}