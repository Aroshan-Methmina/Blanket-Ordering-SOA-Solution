using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CozyComfortAPI.Models
{
    public class MInventory
    {
        public int MInventoryID { get; set; }
        public int ManufacturerID { get; set; }
        public int MBlanketID { get; set; }
        public int MQuantity { get; set; }
        public DateTime MLastUpdated { get; set; }
        public string Error { get; set; }
    }
}