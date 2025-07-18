using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CozyComfortAPI.Models
{

    public class SellerOperationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int SellerId { get; set; }
    }
    public class Seller
    {
        public int SellerId { get; set; }
        public string StoreName { get; set; }
        public string ContactNumber { get; set; }
        public string StoreLocation { get; set; }
        public string Website { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Error { get; set; }
    }
}