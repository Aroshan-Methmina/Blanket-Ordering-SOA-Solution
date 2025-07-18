using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CozyComfortAPI.Models
{
    [Serializable]
    public class DInventory
    {
        public int DInventoryID { get; set; }
        public int DistributorID { get; set; }
        public int DBlanketID { get; set; }
        public int DQuantity { get; set; }
        public DateTime DLastUpdated { get; set; }
        public string Error { get; set; }
    }

}
