using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CozyComfortAPI.Models
{
    public class SInventory
    {
        public int SInventoryID { get; set; }
        public int SellerID { get; set; }
        public int SBlanketID { get; set; }
        public int SQuantity { get; set; }
        public DateTime SLastUpdated { get; set; }


        public string Model { get; set; }
        public string Material { get; set; }

        public string Error { get; set; }
    }
}
