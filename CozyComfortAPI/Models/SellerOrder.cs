using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CozyComfortAPI.Models
{
    public class SellerOrder
    {
        public int SOrderID { get; set; }
        public int BlanketID { get; set; }
        public int Quantity { get; set; }
        public string OrderStatus { get; set; }
        public DateTime OrderDate { get; set; }
    }
}