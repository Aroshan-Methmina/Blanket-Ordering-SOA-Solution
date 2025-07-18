using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CozyComfortAPI.Models
{
    public class UOrder
    {
        public int OrderID { get; set; }
        public int SellerID { get; set; }
        public string User_name { get; set; }
        public string User_contact { get; set; }
        public int BlanketID { get; set; }
        public int Quantity { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }

 
        public string BlanketModel { get; set; }
        public string SellerName { get; set; }
        public string ErrorMessage { get; set; }
    }
}